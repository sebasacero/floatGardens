// Menú "Float Gardens" del editor: configura el proyecto y crea la escena de Bogotá en un clic.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloatGardens.EditorTools
{
    public static class FGSetup
    {
        const string Root = "Assets/FloatGardens";
        const string ScenePath = Root + "/Scenes/Bogota.unity";
        const string ResourcesPath = Root + "/Resources/" + FGMaterials.ResourceFolder;

        [MenuItem("Float Gardens/Configurar todo y crear escena", priority = 0)]
        public static void SetupAll()
        {
            ConfigureProject();
            CreateScene();
            string urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null
                ? "\n\nOjo: este proyecto no tiene URP activo y el juego se verá rosado o sin efectos. " +
                  "Crea un proyecto nuevo en Unity Hub con la plantilla 'Universal 3D' e instala ahí Float Gardens."
                : "";
            EditorUtility.DisplayDialog("Float Gardens",
                "Listo. La escena Bogota quedó abierta y agregada al build.\n\nDale Play para jugar." + urp, "¡Vamos!");
        }

        [MenuItem("Float Gardens/1. Configurar proyecto", priority = 20)]
        public static void ConfigureProject()
        {
            // Plantillas de material: garantizan que el build incluya los shaders y variantes que usa el juego
            Directory.CreateDirectory(ResourcesPath);
            foreach (var name in FGMaterials.TemplateNames)
            {
                string path = ResourcesPath + "/" + name + ".mat";
                var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null) continue;
                AssetDatabase.CreateAsset(FGMaterials.CreateTemplate(name), path);
            }
            AssetDatabase.SaveAssets();

            // Ajustes de jugador para celular
            PlayerSettings.productName = "Float Gardens Bogotá";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null)
                Debug.LogWarning("Float Gardens: este proyecto no usa URP. Crea el proyecto con la plantilla 'Universal 3D' para tener sombras, bloom y posprocesado.");
            Debug.Log("Float Gardens: proyecto configurado (materiales plantilla en " + ResourcesPath + ").");
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
