// Materiales PBR de URP creados en código.
// Se clonan de "plantillas" guardadas en Resources/FloatGardens (las crea el menú Float Gardens > Configurar proyecto),
// así el build incluye los shaders y variantes necesarias. Si no existen, se crean al vuelo (sirve en el editor).
// Si el proyecto no tiene URP activo se usan los shaders clásicos (Standard) para que nada se vea rosado.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FloatGardens
{
    public static class FGMaterials
    {
        public const string ResourceFolder = "FloatGardens";
        public static readonly string[] TemplateNames = { "FG_Lit", "FG_LitEmissive", "FG_LitTransparent", "FG_UnlitTransparent", "FG_Particles", "FG_Sky" };

        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
        static readonly Dictionary<string, Material> templates = new Dictionary<string, Material>();

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        /// <summary>¿Hay un render pipeline (URP) activo? Sin él, los shaders de URP se ven rosados.</summary>
        public static bool Urp => GraphicsSettings.currentRenderPipeline != null;

        static Shader Find(params string[] names)
        {
            if (!Urp && names.Length > 1) names = new[] { names[names.Length - 1] };
            foreach (var n in names)
            {
                var s = Shader.Find(n);
                if (s != null) return s;
            }
            Debug.LogError("Float Gardens: no se encontró el shader " + names[0] + ". ¿El proyecto usa URP? Corre Float Gardens > Configurar proyecto.");
            return Shader.Find("Hidden/InternalErrorShader");
        }

        /// <summary>Crea una plantilla desde cero (lo usan el editor para guardarlas y el juego si faltan).</summary>
        public static Material CreateTemplate(string name)
        {
            Material m;
            switch (name)
            {
                case "FG_LitEmissive":
                    m = new Material(Find("Universal Render Pipeline/Lit", "Standard"));
                    m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    m.SetColor("_EmissionColor", Color.white);
                    break;
                case "FG_LitTransparent":
                    m = new Material(Find("Universal Render Pipeline/Lit", "Standard"));
                    MakeTransparent(m);
                    break;
                case "FG_UnlitTransparent":
                    m = new Material(Find("Universal Render Pipeline/Unlit", "Unlit/Transparent"));
                    MakeTransparent(m);
                    break;
                case "FG_Particles":
                    m = new Material(Find("Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit"));
                    MakeTransparent(m);
                    break;
                case "FG_Sky":
                    m = new Material(Find("Skybox/Procedural"));
                    break;
                default:
                    m = new Material(Find("Universal Render Pipeline/Lit", "Standard"));
                    break;
            }
            m.name = name;
            m.enableInstancing = name != "FG_Sky";
            return m;
        }

        static void MakeTransparent(Material m)
        {
            // URP
            m.SetFloat("_Surface", 1);
            m.SetFloat("_Blend", 0);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0);
            m.SetFloat("_Cull", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            // Standard (sin URP): modo Fade
            m.SetFloat("_Mode", 2);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        public static Material Template(string name)
        {
            if (templates.TryGetValue(name, out var t) && t != null) return t;
            // las plantillas guardadas usan shaders de URP: sin URP se crean con los clásicos
            if (Urp) t = Resources.Load<Material>(ResourceFolder + "/" + name);
            if (t == null) t = CreateTemplate(name);
            templates[name] = t;
            return t;
        }

        public static Material New(string template, string name)
        {
            return new Material(Template(template)) { name = name };
        }

        /// <summary>
        /// Material opaco PBR. smooth: 0 mate, 1 espejo. Si la textura trae mapas de FGTextures se usan su relieve y,
        /// con pbrMask, su máscara de metal/brillo (entonces el brillo sale de la máscara y no de smooth).
        /// </summary>
        public static Material Lit(string key, Color color, Texture2D tex = null, float smooth = 0.15f, float metallic = 0f, bool pbrMask = true)
        {
            if (cache.TryGetValue(key, out var m) && m != null) return m;
            m = New("FG_Lit", key);
            m.color = color;
            if (tex != null) m.mainTexture = tex;
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Glossiness", smooth);
            m.SetFloat("_Metallic", metallic);
            ApplyMaps(m, tex, pbrMask);
            cache[key] = m;
            return m;
        }

        /// <summary>Relieve (normal map) y máscara de metal/brillo, con los nombres de URP Lit y de Standard.</summary>
        static void ApplyMaps(Material m, Texture2D tex, bool pbrMask)
        {
            var normal = FGTextures.NormalOf(tex);
            if (normal != null)
            {
                m.SetTexture("_BumpMap", normal);
                m.SetFloat("_BumpScale", 1f);
                m.EnableKeyword("_NORMALMAP");
            }
            var mask = pbrMask ? FGTextures.MaskOf(tex) : null;
            if (mask != null)
            {
                m.SetTexture("_MetallicGlossMap", mask);
                m.SetFloat("_Smoothness", 1f);
                m.SetFloat("_GlossMapScale", 1f);
                m.EnableKeyword("_METALLICSPECGLOSSMAP"); // URP
                m.EnableKeyword("_METALLICGLOSSMAP");     // Standard
            }
        }

        public static Material Lit(string hex, float smooth = 0.15f) => Lit("c" + hex + smooth, Hex(hex), null, smooth);

        /// <summary>Material con brillo propio (neón, corona de la torre): resplandece con el bloom.</summary>
        public static Material Emissive(string key, Color color, float intensity = 2.5f)
        {
            if (cache.TryGetValue(key, out var m) && m != null) return m;
            m = New("FG_LitEmissive", key);
            m.color = color * 0.4f;
            m.SetColor("_EmissionColor", color * intensity);
            cache[key] = m;
            return m;
        }

        public static void SetEmission(Material m, Color color, float intensity)
        {
            m.SetColor("_EmissionColor", color * intensity);
        }

        /// <summary>Material transparente (anillos y columnas de luz de las zonas, vidrio de estaciones).</summary>
        public static Material Transparent(string key, Color color, bool lit = false)
        {
            if (cache.TryGetValue(key, out var m) && m != null) return m;
            m = New(lit ? "FG_LitTransparent" : "FG_UnlitTransparent", key);
            m.color = color;
            cache[key] = m;
            return m;
        }

        public static Material Particles(string name) => New("FG_Particles", name);

        /// <summary>Copia independiente (para cambiar color por instancia, p. ej. zonas).</summary>
        public static Material Instance(Material source) => new Material(source);
    }
}
