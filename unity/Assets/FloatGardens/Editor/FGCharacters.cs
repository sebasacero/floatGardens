// Menú "Float Gardens > Personajes": convierte archivos de Mixamo en personajes del juego con un clic.
// Selecciona en el panel Project el FBX del personaje (con skin) y los FBX de sus animaciones (Idle, Walking,
// Running, Rifle Aiming Idle, Death...), y elige a quién reemplaza. El menú configura el rig Humanoid, arma el
// Animator Controller con los parámetros que usa el juego (Speed, Aiming, Dead) y guarda el prefab en
// Assets/FloatGardens/Resources/FloatGardens/Modelos, donde el juego lo carga solo.
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FloatGardens.EditorTools
{
    public static class FGCharacters
    {
        const string Folder = "Assets/FloatGardens/Resources/" + FGBootstrap.ModelsFolder;
        const string Menu = "Float Gardens/Personajes/";

        [MenuItem(Menu + "Crear Barista con lo seleccionado", priority = 60)] static void Barista() => Create("Barista");
        [MenuItem(Menu + "Crear Enemigo con lo seleccionado", priority = 61)] static void Enemigo() => Create("Enemigo");
        [MenuItem(Menu + "Crear Jefe con lo seleccionado", priority = 62)] static void Jefe() => Create("Jefe");
        [MenuItem(Menu + "Crear Peatón con lo seleccionado", priority = 63)] static void Peaton() => Create("Peaton");

        [MenuItem(Menu + "Crear Barista con lo seleccionado", true)]
        [MenuItem(Menu + "Crear Enemigo con lo seleccionado", true)]
        [MenuItem(Menu + "Crear Jefe con lo seleccionado", true)]
        [MenuItem(Menu + "Crear Peatón con lo seleccionado", true)]
        static bool HasModels() => SelectedModels().Count > 0;

        static List<string> SelectedModels()
        {
            var list = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is ModelImporter) list.Add(path);
            }
            return list;
        }

        static void Create(string name)
        {
            var paths = SelectedModels();
            string character = null;
            var clips = new Dictionary<string, AnimationClip>();

            foreach (var path in paths)
            {
                // Rig humanoide y clips en bucle, sin que la animación desplace al personaje (el juego lo mueve)
                var imp = (ModelImporter)AssetImporter.GetAtPath(path);
                imp.animationType = ModelImporterAnimationType.Human;
                imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                string kind = Classify(Path.GetFileNameWithoutExtension(path));
                var anims = imp.defaultClipAnimations;
                foreach (var a in anims)
                {
                    string k = kind ?? Classify(a.name);
                    a.loopTime = k != "death";
                    a.loopPose = k != "death";
                    a.lockRootRotation = true;
                    a.lockRootHeightY = true;
                    a.lockRootPositionXZ = true;
                    a.keepOriginalOrientation = true;
                    a.keepOriginalPositionY = true;
                    a.keepOriginalPositionXZ = true;
                }
                imp.clipAnimations = anims;
                imp.SaveAndReimport();

                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        string k = kind ?? Classify(clip.name);
                        if (k != null && !clips.ContainsKey(k)) clips[k] = clip;
                    }
                }
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (character == null && go != null && go.GetComponentInChildren<SkinnedMeshRenderer>() != null) character = path;
            }

            if (character == null)
            {
                EditorUtility.DisplayDialog("Float Gardens", "Entre lo seleccionado no hay un personaje con malla (descárgalo de Mixamo con la opción \"With Skin\").", "OK");
                return;
            }
            if (!clips.ContainsKey("idle") && !clips.ContainsKey("run"))
                Debug.LogWarning("Float Gardens: no encontré animaciones Idle ni Running entre lo seleccionado; el personaje quedará quieto.");

            Directory.CreateDirectory(Folder);
            var controller = BuildController(Folder + "/" + name + ".controller", clips);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(character));
            var animator = instance.GetComponentInChildren<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            foreach (var r in instance.GetComponentsInChildren<SkinnedMeshRenderer>()) r.updateWhenOffscreen = false;
            string prefabPath = Folder + "/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();

            var found = new List<string>(clips.Keys);
            EditorUtility.DisplayDialog("Float Gardens",
                "Listo: " + name + " quedó en " + prefabPath + ".\nAnimaciones: " + (found.Count > 0 ? string.Join(", ", found) : "ninguna") +
                ".\n\nDale Play: el juego lo usa automáticamente.", "¡Vamos!");
        }

        /// <summary>Reconoce la animación por el nombre que le pone Mixamo.</summary>
        static string Classify(string n)
        {
            n = n.ToLowerInvariant();
            // "\bdie\b" y no "die" a secas: "Soldier" no es una animación de muerte
            if (n.Contains("death") || n.Contains("dying") || Regex.IsMatch(n, @"\bdie\b")) return "death";
            if (n.Contains("aim") || n.Contains("shoot") || n.Contains("firing")) return "aim";
            if (n.Contains("run") || n.Contains("sprint") || n.Contains("jog")) return "run";
            if (n.Contains("walk")) return "walk";
            if (n.Contains("idle")) return "idle";
            return null;
        }

        static AnimatorController BuildController(string path, Dictionary<string, AnimationClip> clips)
        {
            AssetDatabase.DeleteAsset(path);
            var c = AnimatorController.CreateAnimatorControllerAtPath(path);
            c.AddParameter("Speed", AnimatorControllerParameterType.Float);
            c.AddParameter("Aiming", AnimatorControllerParameterType.Bool);
            c.AddParameter("Dead", AnimatorControllerParameterType.Bool);
            var sm = c.layers[0].stateMachine;

            // Caminar/correr según Speed (0 quieto, ~0.55 caminando, 1 corriendo)
            var loco = c.CreateBlendTreeInController("Locomocion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            if (clips.TryGetValue("idle", out var idle)) tree.AddChild(idle, 0f);
            if (clips.TryGetValue("walk", out var walk)) tree.AddChild(walk, 0.55f);
            if (clips.TryGetValue("run", out var run)) tree.AddChild(run, 1f);
            sm.defaultState = loco;

            if (clips.TryGetValue("aim", out var aim))
            {
                var aimState = sm.AddState("Apuntar");
                aimState.motion = aim;
                var toAim = loco.AddTransition(aimState);
                toAim.AddCondition(AnimatorConditionMode.If, 0, "Aiming");
                toAim.hasExitTime = false;
                toAim.duration = 0.12f;
                var back = aimState.AddTransition(loco);
                back.AddCondition(AnimatorConditionMode.IfNot, 0, "Aiming");
                back.hasExitTime = false;
                back.duration = 0.15f;
            }

            if (clips.TryGetValue("death", out var death))
            {
                var dead = sm.AddState("Muerte");
                dead.motion = death;
                var toDead = sm.AddAnyStateTransition(dead);
                toDead.AddCondition(AnimatorConditionMode.If, 0, "Dead");
                toDead.canTransitionToSelf = false;
                toDead.hasExitTime = false;
                toDead.duration = 0.1f;
                var revive = dead.AddTransition(loco);
                revive.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
                revive.hasExitTime = false;
                revive.duration = 0.1f;
            }
            return c;
        }
    }
}
