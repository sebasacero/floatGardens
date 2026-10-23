// Perfil visual de una plataforma: paleta de la interfaz, look de materiales, luz y cielo, posprocesado y cámara.
// Hay uno por plataforma (móvil, web, consola). La misma escena se ve distinta según el perfil que se cargue.
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FloatGardens
{
    public enum FGPlatform { Mobile = 0, Web = 1, Console = 2 }

    public enum FGTextEffect { None, Shadow, Outline }

    [CreateAssetMenu(fileName = "FG_Style", menuName = "Float Gardens/Perfil visual", order = 0)]
    public class FGStyleProfile : ScriptableObject
    {
        [Serializable]
        public class Palette
        {
            [Tooltip("Títulos, botones principales, dinero (antes amarillo).")] public Color primary = new Color32(0xff, 0xc2, 0x1a, 255);
            [Tooltip("Avisos secundarios (antes naranja).")] public Color accent = new Color32(0xff, 0x7a, 0x1a, 255);
            [Tooltip("Vida, zonas propias, confirmar (antes verde).")] public Color good = new Color32(0x2f, 0xbf, 0x71, 255);
            [Tooltip("Resaltados, Puntos Float (antes menta).")] public Color highlight = new Color32(0x7d, 0xff, 0xb0, 255);
            [Tooltip("Daño, enemigos, cancelar.")] public Color danger = new Color32(0xff, 0x4b, 0x3a, 255);
            [Tooltip("Texto oscuro sobre botones claros.")] public Color ink = new Color32(0x0b, 0x1a, 0x14, 255);
            [Tooltip("Fondo de paneles y menús.")] public Color panel = new Color(0.03f, 0.07f, 0.055f, 0.85f);
            [Tooltip("Texto normal.")] public Color text = new Color32(0xf3, 0xf1, 0xea, 255);
        }

        [Serializable]
        public class UITheme
        {
            [Range(0.7f, 1.5f)] public float fontScale = 1f;
            public FGTextEffect textEffect = FGTextEffect.Shadow;
            public Color textEffectColor = new Color(0, 0, 0, 0.75f);
            public Vector2 textEffectDistance = new Vector2(1.5f, -1.5f);
        }

        [Serializable]
        public class MaterialLook
        {
            [Tooltip("Multiplica la saturación de todos los colores de la ciudad y los personajes.")]
            [Range(0f, 2f)] public float saturation = 1f;
            [Tooltip("Tinte que se multiplica sobre todos los materiales.")] public Color tint = Color.white;
            [Tooltip("Multiplica el brillo especular: alto = plástico/juguete, bajo = mate/pintado.")]
            [Range(0f, 3f)] public float smoothnessScale = 1f;
            [Tooltip("Multiplica la intensidad del neón.")] [Range(0f, 3f)] public float emissionScale = 1f;
        }

        [Serializable]
        public class Lighting
        {
            public Color sunColor = new Color32(0xff, 0xf1, 0xdc, 255);
            public float sunIntensity = 1.25f;
            public LightShadows sunShadows = LightShadows.Soft;
            [Range(0f, 1f)] public float shadowStrength = 0.75f;
            public Color ambientSky = new Color32(0xc9, 0xd8, 0xe6, 255);
            public Color ambientEquator = new Color32(0x9a, 0xa6, 0xae, 255);
            public Color ambientGround = new Color32(0x4a, 0x4f, 0x45, 255);
            public Color skyTint = new Color32(0x9d, 0xb4, 0xc8, 255);
            public Color skyGround = new Color32(0x55, 0x5a, 0x55, 255);
            public float skyExposure = 1.15f;
            public float atmosphereThickness = 1.15f;
            public Color fogDry = new Color32(0xa9, 0xbc, 0xcb, 255);
            public Color fogWet = new Color32(0x8e, 0x9b, 0xa6, 255);
            public float fogStart = 40;
            public float fogEnd = 430;
        }

        [Serializable]
        public class Post
        {
            public float bloomIntensity = 0.8f;
            public float bloomThreshold = 1f;
            [Range(0f, 1f)] public float bloomScatter = 0.65f;
            public Color bloomTint = Color.white;
            public TonemappingMode tonemapping = TonemappingMode.ACES;
            public float postExposure = 0.25f;
            [Range(-100f, 100f)] public float contrast = 12f;
            [Range(-100f, 100f)] public float saturation = 10f;
            public Color colorFilter = Color.white;
            [Range(0f, 1f)] public float vignette = 0.22f;
            public Color vignetteColor = Color.black;
            [Tooltip("0 = apagado.")] [Range(0f, 1f)] public float filmGrain = 0f;
            [Tooltip("0 = apagado.")] [Range(0f, 1f)] public float chromaticAberration = 0f;
            [Tooltip("Gris = apagado.")] public Color splitShadows = Color.gray;
            [Tooltip("Gris = apagado.")] public Color splitHighlights = Color.gray;
            [Range(-100f, 100f)] public float splitBalance = 0f;
            public AntialiasingMode antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        }

        [Serializable]
        public class CameraLook
        {
            [Tooltip("Menos de 1 aplana la perspectiva (look 2.5D).")] [Range(0.4f, 1.3f)] public float fovScale = 1f;
            [Tooltip("Aleja la cámara del personaje.")] [Range(0.6f, 2.5f)] public float distanceScale = 1f;
            [Tooltip("Cámara alta en diagonal, estilo MOBA (League of Legends). Disparos en la dirección del personaje.")]
            public bool isometric;
            [Range(30f, 80f)] public float isoPitch = 56f;
            [Range(8f, 35f)] public float isoDistance = 18f;
            [Range(20f, 70f)] public float isoFov = 38f;
        }

        public FGPlatform platform = FGPlatform.Console;
        [TextArea] public string notes;
        public Palette palette = new Palette();
        public UITheme ui = new UITheme();
        public MaterialLook materials = new MaterialLook();
        public Lighting lighting = new Lighting();
        public Post post = new Post();
        public CameraLook cameraLook = new CameraLook();

        /// <summary>Aplica el tinte y la saturación del perfil a un color de material.</summary>
        public Color Grade(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            var g = Color.HSVToRGB(h, Mathf.Clamp01(s * materials.saturation), v, true) * materials.tint;
            g.a = c.a;
            return g;
        }

        /// <summary>Perfil por defecto de cada plataforma. Sirve si no existen los assets (los crea el menú del editor).</summary>
        public static FGStyleProfile CreateDefault(FGPlatform platform)
        {
            var p = CreateInstance<FGStyleProfile>();
            p.platform = platform;
            switch (platform)
            {
                case FGPlatform.Mobile: MobileLook(p); break;
                case FGPlatform.Web: WebLook(p); break;
                default: ConsoleLook(p); break;
            }
            return p;
        }

        // Móvil: estilo League of Legends. Fantasía estilizada pintada a mano: formas y siluetas claras, colores vivos
        // pero controlados, materiales casi mates (el sombreado va "pintado", no brilla), sombras suaves y poco oscuras,
        // luz cálida, cero neón y poco bloom. Cámara alta en diagonal e interfaz dorada sobre azul noche.
        static void MobileLook(FGStyleProfile p)
        {
            p.name = "FG_Style_Movil";
            p.notes = "Móvil: estilo League of Legends (fantasía pintada a mano, cámara isométrica, sin neón).";
            var c = p.palette;
            c.primary = Hex("#f0c75e"); c.accent = Hex("#0ac8b9"); c.good = Hex("#4ec96b"); c.highlight = Hex("#5ee3ff");
            c.danger = Hex("#e84057"); c.ink = Hex("#010a13"); c.panel = new Color(0.004f, 0.04f, 0.075f, 0.88f); c.text = Hex("#f0e6d2");
            p.ui.fontScale = 1.1f;
            p.ui.textEffect = FGTextEffect.Outline;
            p.ui.textEffectColor = new Color(0.004f, 0.04f, 0.075f, 0.95f);
            p.ui.textEffectDistance = new Vector2(2f, -2f);
            p.materials.saturation = 1.12f;
            p.materials.tint = new Color(1.02f, 1f, 0.97f);
            p.materials.smoothnessScale = 0.2f;
            p.materials.emissionScale = 0.5f;
            var l = p.lighting;
            l.sunColor = Hex("#fff1d6"); l.sunIntensity = 1.35f; l.sunShadows = LightShadows.Soft; l.shadowStrength = 0.45f;
            l.ambientSky = Hex("#9fc8ff"); l.ambientEquator = Hex("#c9b48a"); l.ambientGround = Hex("#4f6a3a");
            l.skyTint = Hex("#7fb6ff"); l.skyGround = Hex("#3e4a36"); l.skyExposure = 1.2f; l.atmosphereThickness = 1.1f;
            l.fogDry = Hex("#a8d0e6"); l.fogWet = Hex("#7f97a8"); l.fogStart = 60; l.fogEnd = 520;
            var f = p.post;
            f.bloomIntensity = 0.45f; f.bloomThreshold = 1.2f; f.bloomScatter = 0.6f; f.bloomTint = Hex("#fff0d0");
            f.tonemapping = TonemappingMode.Neutral; f.postExposure = 0.3f; f.contrast = 10f; f.saturation = 15f;
            f.vignette = 0.2f; f.vignetteColor = Hex("#01101c");
            f.filmGrain = 0f; f.chromaticAberration = 0f;
            f.splitShadows = Hex("#2c4f8a"); f.splitHighlights = Hex("#f2c66d"); f.splitBalance = 0f;
            f.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            p.cameraLook.fovScale = 1f;
            p.cameraLook.distanceScale = 1f;
            p.cameraLook.isometric = true;
            p.cameraLook.isoPitch = 56f;
            p.cameraLook.isoDistance = 18f;
            p.cameraLook.isoFov = 38f;
        }

        // Web: cómic y grafiti 2.5D. Colores planos y mates, sombras duras como tinta, contraste alto, bordes gruesos en
        // el texto y una cámara de lente larga que aplana la perspectiva.
        static void WebLook(FGStyleProfile p)
        {
            p.name = "FG_Style_Web";
            p.notes = "Web: cómic/grafiti 2.5D (historia, comunidad, cupones).";
            var c = p.palette;
            c.primary = Hex("#ffd400"); c.accent = Hex("#ff5a1f"); c.good = Hex("#00c46a"); c.highlight = Hex("#00e5ff");
            c.danger = Hex("#ff1744"); c.ink = Hex("#111111"); c.panel = new Color(0.07f, 0.07f, 0.07f, 0.92f); c.text = Hex("#fffbea");
            p.ui.fontScale = 1.05f;
            p.ui.textEffect = FGTextEffect.Outline;
            p.ui.textEffectColor = Color.black;
            p.ui.textEffectDistance = new Vector2(3f, -3f);
            p.materials.saturation = 1.2f;
            p.materials.smoothnessScale = 0.25f;
            p.materials.emissionScale = 0.8f;
            var l = p.lighting;
            l.sunColor = Hex("#fff6e0"); l.sunIntensity = 1.45f; l.sunShadows = LightShadows.Hard; l.shadowStrength = 1f;
            l.ambientSky = Hex("#e6e2d6"); l.ambientEquator = Hex("#d2cbb8"); l.ambientGround = Hex("#8a8270");
            l.skyTint = Hex("#7fc8ff"); l.skyGround = Hex("#6b655a"); l.skyExposure = 1.3f; l.atmosphereThickness = 0.8f;
            l.fogDry = Hex("#efe6cf"); l.fogWet = Hex("#b9b3a3"); l.fogStart = 60; l.fogEnd = 480;
            var f = p.post;
            f.bloomIntensity = 0.3f; f.bloomThreshold = 1.3f; f.bloomScatter = 0.4f;
            f.tonemapping = TonemappingMode.None; f.postExposure = 0.1f; f.contrast = 35f; f.saturation = 20f;
            f.vignette = 0.35f; f.filmGrain = 0.35f; f.chromaticAberration = 0f;
            f.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            p.cameraLook.fovScale = 0.62f;
            p.cameraLook.distanceScale = 1.6f;
        }

        // Consola: pintura (estilo Arcane/Spider-Verse). Luz cálida, sombras suaves, tonos partidos ámbar/verde azulado,
        // materiales mates, grano fino y un toque de aberración. Es el más artístico y el más caro.
        static void ConsoleLook(FGStyleProfile p)
        {
            p.name = "FG_Style_Consola";
            p.notes = "Consola: pintura (mundo abierto, el más artístico).";
            var c = p.palette;
            c.primary = Hex("#f2b544"); c.accent = Hex("#e0663a"); c.good = Hex("#5fb58a"); c.highlight = Hex("#9fe3d0");
            c.danger = Hex("#d9433b"); c.ink = Hex("#1a1410"); c.panel = new Color(0.08f, 0.06f, 0.05f, 0.82f); c.text = Hex("#f5ead6");
            p.ui.fontScale = 1f;
            p.ui.textEffect = FGTextEffect.Shadow;
            p.ui.textEffectColor = new Color(0.1f, 0.05f, 0.02f, 0.7f);
            p.ui.textEffectDistance = new Vector2(2f, -2f);
            p.materials.saturation = 0.9f;
            p.materials.tint = new Color(1.03f, 0.99f, 0.94f);
            p.materials.smoothnessScale = 0.6f;
            p.materials.emissionScale = 1.1f;
            var l = p.lighting;
            l.sunColor = Hex("#ffd9a8"); l.sunIntensity = 1.35f; l.sunShadows = LightShadows.Soft; l.shadowStrength = 0.7f;
            l.ambientSky = Hex("#a9c4d6"); l.ambientEquator = Hex("#b79c86"); l.ambientGround = Hex("#4d4136");
            l.skyTint = Hex("#c7a98f"); l.skyGround = Hex("#4f4a42"); l.skyExposure = 1.1f; l.atmosphereThickness = 1.35f;
            l.fogDry = Hex("#c9b8a3"); l.fogWet = Hex("#8f9599"); l.fogStart = 25; l.fogEnd = 380;
            var f = p.post;
            f.bloomIntensity = 0.9f; f.bloomThreshold = 0.95f; f.bloomScatter = 0.85f; f.bloomTint = Hex("#ffe2c0");
            f.tonemapping = TonemappingMode.ACES; f.postExposure = 0.3f; f.contrast = 8f; f.saturation = 4f;
            f.colorFilter = Hex("#fff3e3");
            f.vignette = 0.28f; f.vignetteColor = Hex("#1a0e08");
            f.filmGrain = 0.18f; f.chromaticAberration = 0.08f;
            f.splitShadows = Hex("#3f6f78"); f.splitHighlights = Hex("#e9a35a"); f.splitBalance = -10f;
            f.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            p.cameraLook.fovScale = 1f;
            p.cameraLook.distanceScale = 1f;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
