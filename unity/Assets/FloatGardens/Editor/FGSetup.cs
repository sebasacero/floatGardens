// Menú "Float Gardens" del editor: configura el proyecto (URP con calidad de cine) y crea la escena de Bogotá en un clic.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FloatGardens.EditorTools
{
    public static class FGSetup
    {
        const string Root = "Assets/FloatGardens";
        const string ScenePath = Root + "/Scenes/Bogota.unity";
        const string ResourcesPath = Root + "/Resources/" + FGMaterials.ResourceFolder;
        const string RenderPath = Root + "/Render";
        const string PostProcessDataPath = "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";

        [MenuItem("Float Gardens/Configurar todo y crear escena", priority = 0)]
        public static void SetupAll()
        {
            ConfigureProject();
            CreateScene();
            string urp = GraphicsSettings.currentRenderPipeline == null
                ? "\n\nOjo: no se pudo activar URP y el juego se verá sin efectos. " +
                  "Crea un proyecto nuevo en Unity Hub con la plantilla 'Universal 3D' e instala ahí Float Gardens."
                : "";
            EditorUtility.DisplayDialog("Float Gardens",
                "Listo. La escena Bogota quedó abierta y agregada al build.\n\nDale Play para jugar." + urp, "¡Vamos!");
        }

        [MenuItem("Float Gardens/1. Configurar proyecto", priority = 20)]
        public static void ConfigureProject()
        {
            // Primero URP: las plantillas eligen sus shaders según el pipeline activo
            ConfigureGraphics();

            // Plantillas de material: garantizan que el build incluya los shaders y variantes que usa el juego
            Directory.CreateDirectory(ResourcesPath);
            foreach (var name in FGMaterials.TemplateNames)
            {
                string path = ResourcesPath + "/" + name + ".mat";
                var template = FGMaterials.CreateTemplate(name);
                var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null && existing.shader == template.shader) continue;
                if (existing != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(template, path);
            }
            AssetDatabase.SaveAssets();

            // Ajustes de jugador para celular
            PlayerSettings.productName = "Float Gardens Bogotá";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            if (GraphicsSettings.currentRenderPipeline == null)
                Debug.LogWarning("Float Gardens: no se pudo activar URP. Crea el proyecto con la plantilla 'Universal 3D' para tener sombras, bloom y posprocesado.");
            Debug.Log("Float Gardens: proyecto configurado (materiales plantilla en " + ResourcesPath + ").");
        }

        /// <summary>
        /// Activa URP si el proyecto no lo tiene (por ejemplo, si se creó con la plantilla 3D clásica) y lo ajusta para
        /// verse como cine: HDR, MSAA, sombras suaves de alta resolución a 150 m, reflejos combinados y oclusión ambiental (SSAO).
        /// </summary>
        static void ConfigureGraphics()
        {
            if (!(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset))
            {
                GraphicsSettings.defaultRenderPipeline = LoadOrCreatePipeline();
                Debug.Log("Float Gardens: URP activado con " + RenderPath + "/FG_URP.asset.");
            }
            // El que se usa de verdad puede ser el del nivel de calidad (la plantilla Universal 3D trae uno para PC y otro para celular)
            var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset == null) return;

            asset.supportsHDR = true;
            asset.msaaSampleCount = 4;
            asset.renderScale = 1f;
            asset.supportsCameraDepthTexture = true;
            asset.supportsCameraOpaqueTexture = true;
            asset.shadowDistance = 150f;
            asset.shadowCascadeCount = 4;
            asset.mainLightShadowmapResolution = 4096;
            asset.colorGradingMode = ColorGradingMode.HighDynamicRange;
            asset.maxAdditionalLightsCount = 8;
            // Ajustes sin propiedad pública
            var so = new SerializedObject(asset);
            SetInt(so, "m_AdditionalLightsRenderingMode", 1); // PerPixel: postes y neones iluminan de verdad
            SetBool(so, "m_MainLightShadowsSupported", true);
            SetBool(so, "m_SoftShadowsSupported", true);
            SetInt(so, "m_SoftShadowQuality", 3); // High
            SetBool(so, "m_ReflectionProbeBlending", true);
            SetBool(so, "m_ReflectionProbeBoxProjection", true);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            var list = so.FindProperty("m_RendererDataList");
            var renderer = list != null && list.arraySize > 0
                ? list.GetArrayElementAtIndex(0).objectReferenceValue as UniversalRendererData
                : null;
            if (renderer == null) return;
            renderer.renderingMode = RenderingMode.ForwardPlus; // muchas luces en pantalla (noche con neones)
            if (renderer.postProcessData == null)
                renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(PostProcessDataPath);
            if (!renderer.rendererFeatures.Exists(f => f is ScreenSpaceAmbientOcclusion))
            {
                var ssao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ssao.name = "SSAO";
                AssetDatabase.AddObjectToAsset(ssao, renderer);
                renderer.rendererFeatures.Add(ssao);
            }
            renderer.SetDirty();
            EditorUtility.SetDirty(renderer);
            AssetDatabase.SaveAssets();
        }

        static UniversalRenderPipelineAsset LoadOrCreatePipeline()
        {
            string assetPath = RenderPath + "/FG_URP.asset";
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
            if (asset != null) return asset;
            Directory.CreateDirectory(RenderPath);
            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(PostProcessDataPath);
            AssetDatabase.CreateAsset(renderer, RenderPath + "/FG_URP_Renderer.asset");
            asset = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static void SetBool(SerializedObject so, string name, bool value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.boolValue = value;
        }

        static void SetInt(SerializedObject so, string name, int value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.intValue = value;
        }

        [MenuItem("Float Gardens/2. Crear escena de Bogotá", priority = 21)]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 900;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0, 3, -6);

            new GameObject("FloatGardens").AddComponent<FGBootstrap>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == ScenePath);
            list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("Float Gardens: escena creada en " + ScenePath + " y agregada al build.");
        }

        [MenuItem("Float Gardens/Borrar progreso guardado", priority = 40)]
        public static void ClearSave()
        {
            if (!EditorUtility.DisplayDialog("Float Gardens", "¿Borrar el progreso guardado (zonas, mejoras, Puntos Float, cupones)?", "Borrar", "Cancelar")) return;
            PlayerPrefs.DeleteKey(FGSave.Key);
            PlayerPrefs.Save();
        }
    }
}
