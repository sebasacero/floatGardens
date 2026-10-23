// Menú "Float Gardens > Estilo visual": crea los perfiles por plataforma y permite ver la escena con cualquiera
// de ellos sin cambiar de plataforma en Build Profiles.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FloatGardens.EditorTools
{
    public static class FGStyleMenu
    {
        const string Folder = "Assets/FloatGardens/Resources/" + FGMaterials.ResourceFolder;
        const string SetPath = Folder + "/FG_StyleSet.asset";
        const string MenuRoot = "Float Gardens/Estilo visual/";

        [MenuItem(MenuRoot + "Crear perfiles por plataforma", priority = 30)]
        public static void CreateProfiles()
        {
            Directory.CreateDirectory(Folder);
            var mobile = LoadOrCreate(FGPlatform.Mobile);
            var web = LoadOrCreate(FGPlatform.Web);
            var console = LoadOrCreate(FGPlatform.Console);

            var set = AssetDatabase.LoadAssetAtPath<FGStyleSet>(SetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<FGStyleSet>();
                AssetDatabase.CreateAsset(set, SetPath);
            }
            if (set.mobile == null) set.mobile = mobile;
            if (set.web == null) set.web = web;
            if (set.console == null) set.console = console;
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            Debug.Log("Float Gardens: perfiles visuales listos en " + Folder + ".");
        }

        // No pisa un perfil que ya exista: así se conservan los ajustes hechos a mano en el inspector.
        static FGStyleProfile LoadOrCreate(FGPlatform platform)
        {
            var profile = FGStyleProfile.CreateDefault(platform);
            string path = Folder + "/" + profile.name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<FGStyleProfile>(path);
            if (existing != null)
            {
                Object.DestroyImmediate(profile);
                return existing;
            }
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        [MenuItem(MenuRoot + "Vista previa: según plataforma activa", priority = 50)]
        static void PreviewAuto() => SetPreview(-1);
        [MenuItem(MenuRoot + "Vista previa: Móvil (MOBA estilo LoL)", priority = 51)]
        static void PreviewMobile() => SetPreview((int)FGPlatform.Mobile);
        [MenuItem(MenuRoot + "Vista previa: Web (cómic/grafiti 2.5D)", priority = 52)]
        static void PreviewWeb() => SetPreview((int)FGPlatform.Web);
        [MenuItem(MenuRoot + "Vista previa: Consola (pintura)", priority = 53)]
        static void PreviewConsole() => SetPreview((int)FGPlatform.Console);

        [MenuItem(MenuRoot + "Vista previa: según plataforma activa", true)]
        static bool CheckAuto() => Check(-1, MenuRoot + "Vista previa: según plataforma activa");
        [MenuItem(MenuRoot + "Vista previa: Móvil (MOBA estilo LoL)", true)]
        static bool CheckMobile() => Check((int)FGPlatform.Mobile, MenuRoot + "Vista previa: Móvil (MOBA estilo LoL)");
        [MenuItem(MenuRoot + "Vista previa: Web (cómic/grafiti 2.5D)", true)]
        static bool CheckWeb() => Check((int)FGPlatform.Web, MenuRoot + "Vista previa: Web (cómic/grafiti 2.5D)");
        [MenuItem(MenuRoot + "Vista previa: Consola (pintura)", true)]
        static bool CheckConsole() => Check((int)FGPlatform.Console, MenuRoot + "Vista previa: Consola (pintura)");

        static void SetPreview(int value)
        {
            if (value < 0) EditorPrefs.DeleteKey(FGStyle.PreviewPref);
            else EditorPrefs.SetInt(FGStyle.PreviewPref, value);
            FGStyle.Use(null);
            Debug.Log("Float Gardens: vista previa de estilo = " + (value < 0 ? "según plataforma activa" : ((FGPlatform)value).ToString())
                + ". Dale Play (o reinícialo) para verla.");
        }

        static bool Check(int value, string item)
        {
            UnityEditor.Menu.SetChecked(item, EditorPrefs.GetInt(FGStyle.PreviewPref, -1) == value);
            return !EditorApplication.isPlaying;
        }
    }
}
