// Tabla que dice qué perfil visual usa cada plataforma. Vive en Resources/FloatGardens/FG_StyleSet.asset.
using UnityEngine;

namespace FloatGardens
{
    [CreateAssetMenu(fileName = "FG_StyleSet", menuName = "Float Gardens/Estilos por plataforma", order = 1)]
    public class FGStyleSet : ScriptableObject
    {
        [Tooltip("Android e iOS.")] public FGStyleProfile mobile;
        [Tooltip("WebGL.")] public FGStyleProfile web;
        [Tooltip("PlayStation, Xbox y Switch.")] public FGStyleProfile console;
        [Tooltip("Qué estilo usa la versión de computador (Windows, Mac, Linux).")]
        public FGPlatform desktopUses = FGPlatform.Console;

        public FGStyleProfile Get(FGPlatform platform)
        {
            switch (platform)
            {
                case FGPlatform.Mobile: return mobile;
                case FGPlatform.Web: return web;
                default: return console;
            }
        }
    }
}
