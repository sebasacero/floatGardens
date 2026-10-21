// Menú del editor para el modo historia: crea la escena "TierraDeCafe" en un clic.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FloatGardens.Historia.EditorTools
{
    public static class HistoriaSetup
    {
        const string ScenePath = "Assets/FloatGardens/Historia/Scenes/TierraDeCafe.unity";

        [MenuItem("Float Gardens/Historia: crear escena Tierra de Café", priority = 2)]
        public static void SetupAll()
        {
            FloatGardens.EditorTools.FGSetup.ConfigureProject();
            PlayerSettings.productName = HConfig.Title;
            CreateScene();
            EditorUtility.DisplayDialog(HConfig.Title,
                "Listo. La escena TierraDeCafe quedó abierta y es la primera del build.\n\nDale Play para empezar la historia en Bogotá.", "¡Vamos!");
        }

        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 1200;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0, 3, -6);

            new GameObject("TierraDeCafe").AddComponent<HistoriaBootstrap>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == ScenePath);
            list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("Float Gardens: escena de la historia creada en " + ScenePath + " y puesta de primera en el build.");
        }

        [MenuItem("Float Gardens/Historia: borrar progreso", priority = 41)]
        public static void ClearSave()
        {
            if (!EditorUtility.DisplayDialog(HConfig.Title, "¿Borrar el progreso de la historia? (Los Puntos Float ya ganados se conservan.)", "Borrar", "Cancelar")) return;
            HState.Delete();
        }
    }
}
