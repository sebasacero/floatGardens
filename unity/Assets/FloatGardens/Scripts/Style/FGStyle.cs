// Elige el perfil visual según la plataforma de compilación. En el editor sigue la plataforma activa de
// Build Profiles (los símbolos UNITY_ANDROID, UNITY_WEBGL... cambian con ella), así que la misma escena cambia
// de estilo al cambiar de plataforma. El menú Float Gardens > Estilo visual permite forzar uno para probar.
using UnityEngine;

namespace FloatGardens
{
    public static class FGStyle
    {
        public const string SetResource = FGMaterials.ResourceFolder + "/FG_StyleSet";
        public const string PreviewPref = "FloatGardens.StylePreview";

        static FGStyleProfile current;

        public static FGStyleProfile Current => current != null ? current : current = Resolve();

        /// <summary>Fuerza un perfil (p. ej. desde un menú de opciones). Afecta lo que se construya después.</summary>
        public static void Use(FGStyleProfile profile) { current = profile; }

        public static FGPlatform Platform
        {
            get
            {
#if UNITY_EDITOR
                int preview = UnityEditor.EditorPrefs.GetInt(PreviewPref, -1);
                if (preview >= 0) return (FGPlatform)preview;
#endif
#if UNITY_ANDROID || UNITY_IOS
                return FGPlatform.Mobile;
#elif UNITY_WEBGL
                return FGPlatform.Web;
#elif UNITY_PS4 || UNITY_PS5 || UNITY_GAMECORE || UNITY_XBOXONE || UNITY_SWITCH
                return FGPlatform.Console;
#else
                var set = Resources.Load<FGStyleSet>(SetResource);
                return set != null ? set.desktopUses : FGPlatform.Console;
#endif
            }
        }

        static FGStyleProfile Resolve()
        {
            var platform = Platform;
            var set = Resources.Load<FGStyleSet>(SetResource);
            var profile = set != null ? set.Get(platform) : null;
            if (profile == null) profile = FGStyleProfile.CreateDefault(platform);
            Debug.Log("Float Gardens: estilo visual " + profile.name + " (" + platform + ").");
            return profile;
        }
    }
}
