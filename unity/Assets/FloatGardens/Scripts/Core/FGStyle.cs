// Estilo visual de la versión móvil: "Kennedy en miniatura". La ciudad se ve como una maqueta: cámara de
// teleobjetivo desde lo alto, desenfoque tilt-shift alrededor del barista, bruma verde-gris, el convoy de costales
// dorados, el cóndor holográfico y la Virgen colosal en Guadalupe (ver la sección 4b de unity/README.md).
using UnityEngine;

namespace FloatGardens
{
    public static class FGStyle
    {
        /// <summary>Preferencia guardada: -1 automático (solo en celular), 0 nunca, 1 siempre.</summary>
        public const string PrefKey = "fg_estilo_miniatura";
        static bool? miniatura;

        /// <summary>¿Se usa el look de maqueta? En celular sí; en el editor también si el destino es Android o iOS.</summary>
        public static bool Miniatura
        {
            get
            {
                if (miniatura == null)
                {
                    int pref = PlayerPrefs.GetInt(PrefKey, -1);
                    miniatura = pref < 0 ? MobileTarget : pref == 1;
                }
                return miniatura.Value;
            }
        }

        public static void SetPreference(int pref)
        {
            PlayerPrefs.SetInt(PrefKey, pref);
            PlayerPrefs.Save();
            miniatura = null;
        }

        static bool MobileTarget
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return true;
#else
                return Application.isMobilePlatform;
#endif
            }
        }
    }
}
