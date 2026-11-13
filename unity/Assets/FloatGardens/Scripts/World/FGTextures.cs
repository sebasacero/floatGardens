// Texturas PBR generadas por código: ladrillo bogotano, fachadas coloniales, vidrio, asfalto, empedrado...
// Cada textura trae tres mapas: color, relieve (normal map) y máscara (metal en R, brillo en A). Así la luz rasante
// marca el ladrillo y la pega, y el vidrio de las ventanas brilla y refleja mientras la pared queda mate.
// Un "módulo" de fachada mide 4 m de ancho x 3 m de alto (un piso con una ventana). Se dibuja en coordenadas
// de 128 x 96 y se escala a la resolución real (128 px por metro en fachadas).
// Se pintan en paralelo en hilos de fondo (es C# puro) y solo la subida a la GPU ocurre en el hilo principal.
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FloatGardens
{
    public static class FGTextures
    {
        public static Texture2D Brick, Stucco, Glass, Concrete, ConcreteBare, Warehouse, Roof, Asphalt, Sidewalk, Cobble, Grass, Water, Circle;
        public static Texture2D[] Murals;
        static readonly Dictionary<Texture2D, Texture2D> normals = new Dictionary<Texture2D, Texture2D>();
        static readonly Dictionary<Texture2D, Texture2D> masks = new Dictionary<Texture2D, Texture2D>();
        static readonly Dictionary<Texture2D, Texture2D> emissions = new Dictionary<Texture2D, Texture2D>();
        static readonly Dictionary<Texture2D, Vector2> tiling = new Dictionary<Texture2D, Vector2>();

        /// <summary>Mapa de relieve de una textura de color (o null si no tiene).</summary>
        public static Texture2D NormalOf(Texture2D albedo) => albedo != null && normals.TryGetValue(albedo, out var t) ? t : null;
        /// <summary>Mapa de metal (R) y brillo (A) de una textura de color (o null si no tiene).</summary>
        public static Texture2D MaskOf(Texture2D albedo) => albedo != null && masks.TryGetValue(albedo, out var t) ? t : null;
        /// <summary>Mapa de luz propia (ventanas encendidas de noche) o null si no tiene.</summary>
        public static Texture2D EmissionOf(Texture2D albedo) => albedo != null && emissions.TryGetValue(albedo, out var t) ? t : null;
        /// <summary>Escala de repetición del material: las fachadas traen dos módulos por textura.</summary>
        public static Vector2 TilesOf(Texture2D albedo) => albedo != null && tiling.TryGetValue(albedo, out var v) ? v : Vector2.one;

        class Painter
        {
            public readonly int w, h;
            public readonly System.Random rng;
            public float R() => (float)rng.NextDouble();
            readonly float k; // escala: las coordenadas de dibujo son de la textura base (p. ej. 128 x 96)
            public readonly Color[] px;
            readonly float[] hgt, smooth, metal;
            Color[] emit; // luz propia (solo si alguien llama Glow)
            // "Pincel" de material: lo que escriben Rect, Circle y Gradient además del color
            public float H = 0.5f, S = 0.1f, M = 0f;

            public Painter(int seed, int w, int h, Color bg, float scale = 1f)
            {
                this.w = w; this.h = h; k = scale;
                rng = new System.Random(seed);
                px = new Color[w * h];
                hgt = new float[w * h];
                smooth = new float[w * h];
                metal = new float[w * h];
                for (int i = 0; i < px.Length; i++) { px[i] = bg; hgt[i] = H; smooth[i] = S; }
            }

            int Idx(int xx, int yy) => (h - 1 - yy) * w + xx;

            void Put(int i, Color c)
            {
                px[i] = c; hgt[i] = H; smooth[i] = S; metal[i] = M;
            }

            /// <summary>Zona que emite luz propia (vidrio de ventanas encendidas). Se ve de noche.</summary>
            public void Glow(float x, float y, float rw, float rh, Color c)
            {
                if (emit == null) emit = new Color[px.Length];
                x *= k; y *= k; rw *= k; rh *= k;
                int x0 = Mathf.Max(0, Mathf.FloorToInt(x)), x1 = Mathf.Min(w, Mathf.CeilToInt(x + rw));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(y)), y1 = Mathf.Min(h, Mathf.CeilToInt(y + rh));
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++) emit[Idx(xx, yy)] = c;
            }

            // (x, y) desde la esquina superior izquierda, como en un canvas
            public void Rect(float x, float y, float rw, float rh, Color c)
            {
                x *= k; y *= k; rw *= k; rh *= k;
                int x0 = Mathf.Max(0, Mathf.FloorToInt(x)), x1 = Mathf.Min(w, Mathf.CeilToInt(x + rw));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(y)), y1 = Mathf.Min(h, Mathf.CeilToInt(y + rh));
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++) Put(Idx(xx, yy), c);
            }

            /// <summary>Rectángulo con bordes biselados en el relieve (ladrillos, adoquines, losas).</summary>
            public void Bevel(float x, float y, float rw, float rh, Color c, float bevel)
            {
                x *= k; y *= k; rw *= k; rh *= k; bevel *= k;
                int x0 = Mathf.Max(0, Mathf.FloorToInt(x)), x1 = Mathf.Min(w, Mathf.CeilToInt(x + rw));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(y)), y1 = Mathf.Min(h, Mathf.CeilToInt(y + rh));
                float baseH = H;
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++)
                    {
                        float e = Mathf.Min(Mathf.Min(xx - x, x + rw - 1 - xx), Mathf.Min(yy - y, y + rh - 1 - yy));
                        float t = Mathf.Clamp01(e / Mathf.Max(1, bevel));
                        int i = Idx(xx, yy);
                        Put(i, c);
                        hgt[i] = Mathf.Lerp(0.5f, baseH, Mathf.SmoothStep(0, 1, t));
                    }
            }

            public void Gradient(float x, float y, float rw, float rh, Color a, Color b)
            {
                x *= k; y *= k; rw *= k; rh *= k;
                int x0 = Mathf.Max(0, (int)x), x1 = Mathf.Min(w, (int)(x + rw));
                int y0 = Mathf.Max(0, (int)y), y1 = Mathf.Min(h, (int)(y + rh));
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++)
                    {
                        float t = ((xx - x) / rw + (yy - y) / rh) * 0.5f;
                        Put(Idx(xx, yy), Color.Lerp(a, b, t));
                    }
            }

            /// <summary>Círculo; con dome el relieve sube hacia el centro (piedras, gotas).</summary>
            public void Circle(float cx, float cy, float r, Color c, bool dome = false)
            {
                cx *= k; cy *= k; r *= k;
                float baseH = H;
                for (int yy = Mathf.Max(0, (int)(cy - r)); yy < Mathf.Min(h, (int)(cy + r + 1)); yy++)
                    for (int xx = Mathf.Max(0, (int)(cx - r)); xx < Mathf.Min(w, (int)(cx + r + 1)); xx++)
                    {
                        float d2 = (xx - cx) * (xx - cx) + (yy - cy) * (yy - cy);
                        if (d2 > r * r) continue;
                        int i = Idx(xx, yy);
                        float prev = hgt[i];
                        Put(i, c);
                        // con domo se queda la altura mayor: dos círculos solapados forman una sola piedra
                        if (dome) hgt[i] = Mathf.Max(prev, Mathf.Lerp(0.5f, baseH, Mathf.SmoothStep(0, 1, (1 - Mathf.Sqrt(d2) / r) * 3f)));
                    }
            }

            /// <summary>Variación natural con ruido fractal que se repite sin costuras. freq = manchas por textura.</summary>
            public void Grime(float freq, float colorAmt, float heightAmt, float smoothAmt = 0, int seed = 0)
            {
                for (int yy = 0; yy < h; yy++)
                    for (int xx = 0; xx < w; xx++)
                    {
                        int i = Idx(xx, yy);
                        float n = Fbm(xx * freq / w, yy * freq / h, Mathf.RoundToInt(freq), seed) - 0.5f;
                        px[i] = px[i] * (1 + n * colorAmt);
                        px[i].a = 1;
                        hgt[i] += n * heightAmt;
                        smooth[i] = Mathf.Clamp01(smooth[i] + n * smoothAmt);
                    }
            }

            /// <summary>Granos finos (poros del concreto, grava del asfalto).</summary>
            public void Grain(int count, float colorAmt, float heightAmt, float size = 1)
            {
                count = Mathf.RoundToInt(count * k * k);
                int s = Mathf.Max(1, Mathf.RoundToInt(size * k));
                for (int n = 0; n < count; n++)
                {
                    int gx = rng.Next(w), gy = rng.Next(h);
                    float v = (float)rng.NextDouble() * 2 - 1;
                    for (int yy = gy; yy < Mathf.Min(h, gy + s); yy++)
                        for (int xx = gx; xx < Mathf.Min(w, gx + s); xx++)
                        {
                            int i = Idx(xx, yy);
                            px[i] = Color.Lerp(px[i], v > 0 ? Color.white : Color.black, Mathf.Abs(v) * colorAmt);
                            hgt[i] += v * heightAmt;
                        }
                }
            }

            /// <summary>Grieta irregular (asfalto, andenes viejos).</summary>
            public void Crack(float x, float y, int steps, Color c)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2;
                float oldH = H;
                H = 0.15f;
                for (int n = 0; n < steps; n++)
                {
                    Rect(x, y, 1.2f, 1.2f, c);
                    a += ((float)rng.NextDouble() - 0.5f) * 0.9f;
                    x += Mathf.Cos(a) * 1.5f; y += Mathf.Sin(a) * 1.5f;
                }
                H = oldH;
            }

            /// <summary>Mancha de humedad que escurre desde arriba (fachadas bogotanas después de la lluvia).</summary>
            public void Streaks(int count, float strength)
            {
                for (int n = 0; n < count; n++)
                {
                    int sx = rng.Next(w), len = (int)(h * (0.3f + (float)rng.NextDouble() * 0.7f));
                    int sw = Mathf.Max(1, (int)(k * (1 + (float)rng.NextDouble() * 3)));
                    for (int yy = 0; yy < len; yy++)
                        for (int xx = sx; xx < Mathf.Min(w, sx + sw); xx++)
                        {
                            int i = Idx(xx, yy);
                            float f = strength * (1 - (float)yy / len);
                            px[i] = Color.Lerp(px[i], px[i] * 0.6f, f);
                            px[i].a = 1;
                        }
                }
            }

            // Resultado de Bake: listo para subir a la GPU
            Color32[] albedo32, normal32, mask32, emit32;
            bool repeat;

            /// <summary>Calcula los tres mapas (sin tocar la API de Unity: se puede llamar desde otro hilo).</summary>
            public void Bake(bool repeat, bool pbr, float bump)
            {
                this.repeat = repeat;
                albedo32 = new Color32[px.Length];
                for (int i = 0; i < px.Length; i++) albedo32[i] = To32(px[i]);
                if (emit != null)
                {
                    emit32 = new Color32[px.Length];
                    for (int i = 0; i < px.Length; i++) emit32[i] = To32(emit[i]);
                }
                if (!pbr) return;

                // Relieve: derivadas del mapa de altura (con wrap si la textura se repite)
                normal32 = new Color32[px.Length];
                mask32 = new Color32[px.Length];
                float strength = bump * k;
                for (int r = 0; r < h; r++)
                    for (int c = 0; c < w; c++)
                    {
                        int i = r * w + c;
                        float hl = hgt[r * w + Wrap(c - 1, w, repeat)], hr = hgt[r * w + Wrap(c + 1, w, repeat)];
                        float hd = hgt[Wrap(r - 1, h, repeat) * w + c], hu = hgt[Wrap(r + 1, h, repeat) * w + c];
                        float nx = (hl - hr) * strength, ny = (hd - hu) * strength;
                        float inv = 1f / Mathf.Sqrt(nx * nx + ny * ny + 1f);
                        normal32[i] = To32(new Color(nx * inv * 0.5f + 0.5f, ny * inv * 0.5f + 0.5f, inv * 0.5f + 0.5f, 1f));
                        mask32[i] = To32(new Color(metal[i], 1f, 0f, smooth[i]));
                    }
            }

            static Color32 To32(Color c) => new Color32(B(c.r), B(c.g), B(c.b), B(c.a));
            static byte B(float v) => (byte)(Mathf.Clamp01(v) * 255f + 0.5f);

            Texture2D NewTex(string name, bool linear, Color32[] data)
            {
                var t = new Texture2D(w, h, TextureFormat.RGBA32, true, linear) { name = name };
                t.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Trilinear;
                t.anisoLevel = 8;
                t.SetPixels32(data);
                t.Apply(true, true); // genera mipmaps y libera la copia en RAM
                return t;
            }

            /// <summary>Sube a la GPU lo calculado en Bake (solo en el hilo principal).</summary>
            public Texture2D ToTextures(string name)
            {
                var t = NewTex(name, false, albedo32);
                if (normal32 != null)
                {
                    normals[t] = NewTex(name + "_N", true, normal32);
                    masks[t] = NewTex(name + "_M", true, mask32);
                }
                if (emit32 != null) emissions[t] = NewTex(name + "_E", false, emit32);
                return t;
            }

            static int Wrap(int v, int n, bool repeat) => repeat ? (v % n + n) % n : Mathf.Clamp(v, 0, n - 1);
        }

        // ---------- Ruido de valor que se repite (periodo entero) ----------
        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int n = x * 374761393 + y * 668265263 + seed * 1442695041;
                n = (n ^ (n >> 13)) * 1274126177;
                return ((n ^ (n >> 16)) & 0xffff) / 65535f;
            }
        }

        static float ValueNoise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            int P(int v) => period > 0 ? (v % period + period) % period : v;
            float a = Hash(P(x0), P(y0), seed), b = Hash(P(x0 + 1), P(y0), seed);
            float c = Hash(P(x0), P(y0 + 1), seed), d = Hash(P(x0 + 1), P(y0 + 1), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        static float Fbm(float x, float y, int period, int seed)
        {
            float sum = 0, amp = 0.5f, norm = 0;
            for (int o = 0; o < 4; o++)
            {
                sum += ValueNoise(x, y, period, seed + o * 31) * amp;
                norm += amp;
                x *= 2; y *= 2; period *= 2; amp *= 0.5f;
            }
            return sum / norm;
        }

        // Lector de "#rrggbb" propio: ColorUtility es nativo y no se debe usar desde otro hilo
        static Color C(string hex)
        {
            int v = System.Convert.ToInt32(hex.Substring(1, 6), 16);
            return new Color(((v >> 16) & 255) / 255f, ((v >> 8) & 255) / 255f, (v & 255) / 255f, 1f);
        }

        /// <summary>Ventana con marco en relieve, vidrio hundido y brillante, y repisa. lit: cuánto se enciende de noche.</summary>
        static void Window(Painter p, float x, float y, float w, float h, Color frame, string g0, string g1, float frameMetal = 0, float lit = 0, string litHex = "#ffd08a")
        {
            if (lit > 0) p.Glow(x, y, w, h, C(litHex) * lit);
            // Repisa (sobresale) y vano (hundido)
            p.H = 0.85f; p.S = 0.25f; p.M = 0;
            p.Rect(x - 6, y + h + 3, w + 12, 4, Color.Lerp(frame, Color.white, 0.3f));
            p.H = 0.3f; p.S = 0.15f;
            p.Rect(x - 3, y - 3, w + 6, h + 6, frame * 0.55f);
            // Vidrio: casi espejo, el reflection probe pone la ciudad encima
            p.H = 0.3f; p.S = 0.96f; p.M = 0.1f;
            p.Gradient(x, y, w, h, C(g0), C(g1));
            // Marco y travesaños
            p.H = 0.7f; p.S = frameMetal > 0 ? 0.55f : 0.25f; p.M = frameMetal;
            p.Rect(x - 3, y - 3, w + 6, 3, frame);
            p.Rect(x - 3, y + h, w + 6, 3, frame);
            p.Rect(x - 3, y - 3, 3, h + 6, frame);
            p.Rect(x + w, y - 3, 3, h + 6, frame);
            p.Rect(x + w / 2 - 1.5f, y, 3, h, frame);
            p.Rect(x, y + h * 0.4f, w, 3, frame);
            p.H = 0.5f; p.S = 0.1f; p.M = 0;
        }

        // Una textura por trabajo: se pinta en un hilo de fondo y se sube a la GPU en el hilo principal
        class Job
        {
            public string name;
            public System.Func<Painter> paint;
            public bool repeat = true, pbr = true;
            public float bump = 4f;
            public int tiles = 1; // módulos de 4 m por textura (las fachadas traen 2 para variar las ventanas)
            public Painter result;
        }

        public static void Init()
        {
            if (Brick != null) return;
            const int F = 4; // fachadas: 512 x 384 por módulo de 4 x 3 m
            var jobs = new List<Job>();
            void Add(string name, float bump, System.Func<Painter> paint, bool repeat = true, bool pbr = true, int tiles = 1) =>
                jobs.Add(new Job { name = name, bump = bump, paint = paint, repeat = repeat, pbr = pbr, tiles = tiles });

            // Ladrillo bogotano: piezas con bisel, pega hundida, color variable y humedad
            Add("Ladrillo", 3.5f, () =>
            {
                var p = new Painter(1, 256 * F, 96 * F, C("#8a8074"), F);
                p.H = 0.35f; p.S = 0.05f;
                p.Rect(0, 0, 256, 96, C("#8a8074"));
                for (int y = 0; y < 96; y += 8)
                {
                    int off = (y / 8) % 2 == 1 ? 8 : 0;
                    for (int x = -off; x < 256; x += 16)
                    {
                        float s = 0.82f + p.R() * 0.32f;
                        var col = new Color(0.6f * s, 0.29f * s * (0.92f + p.R() * 0.16f), 0.19f * s);
                        p.H = 0.72f + p.R() * 0.08f; p.S = 0.12f + p.R() * 0.08f;
                        p.Bevel(x + 0.8f, y + 0.8f, 14.4f, 6.4f, col, 1.4f);
                    }
                }
                p.Grime(6, 0.18f, 0.04f, 0.05f, 1);
                p.Grain(700, 0.07f, 0.02f);
                p.Streaks(16, 0.25f);
                Window(p, 34, 22, 60, 50, C("#e8e2d6"), "#1e2a36", "#4f6478", 0, 0.9f);
                Window(p, 162, 22, 60, 50, C("#e8e2d6"), "#1e2a36", "#4f6478", 0, 0.25f);
                return p;
            }, tiles: 2);

            // Colonial de La Candelaria: pañete con textura, marcos y balcón de madera
            Add("Colonial", 4f, () =>
            {
                var p = new Painter(2, 256 * F, 96 * F, C("#f4f0e8"), F);
                p.H = 0.55f; p.S = 0.12f;
                p.Grime(10, 0.12f, 0.12f, 0.04f, 2);
                p.Grain(2400, 0.05f, 0.03f);
                p.H = 0.7f; p.S = 0.1f;
                p.Rect(0, 86, 256, 10, C("#d2c8b6"));
                p.Streaks(14, 0.2f);
                Window(p, 40, 18, 48, 54, C("#5a3a22"), "#2b241c", "#5f5141", 0, 0.8f, "#ffc777");
                Window(p, 168, 18, 48, 54, C("#5a3a22"), "#2b241c", "#5f5141", 0, 0.3f, "#ffc777");
                foreach (int ox in new[] { 0, 128 })
                {
                    p.H = 0.9f; p.S = 0.35f;
                    p.Rect(ox + 30, 70, 68, 6, C("#4a2e1a"));
                    for (int x = 32; x < 98; x += 8) p.Rect(ox + x, 58, 3, 12, C("#4a2e1a"));
                }
                return p;
            }, tiles: 2);

            // Muro cortina: vidrio espejado con perfiles de aluminio
            Add("Vidrio", 4f, () =>
            {
                var p = new Painter(3, 256 * F, 96 * F, C("#3d5d78"), F);
                p.H = 0.4f; p.S = 0.97f; p.M = 0.35f;
                p.Gradient(0, 0, 256, 96, C("#2e4a63"), C("#7d9fbd"));
                p.Grime(4, 0.08f, 0f, 0.03f, 3);
                // oficinas encendidas detrás del vidrio: un módulo más que el otro
                p.Glow(3, 5, 125, 90, C("#bfe3ff") * 0.75f);
                p.Glow(131, 5, 125, 90, C("#bfe3ff") * 0.3f);
                p.H = 0.8f; p.S = 0.6f; p.M = 0.9f;
                p.Rect(0, 0, 256, 5, C("#b8c4cc"));
                p.Glow(0, 0, 256, 5, Color.black);
                foreach (int x in new[] { 0, 64, 128, 192 })
                {
                    p.Rect(x, 0, x % 128 == 0 ? 3 : 2, 96, C("#b8c4cc"));
                    p.Glow(x, 0, 3, 96, Color.black);
                }
                p.Rect(0, 60, 256, 2, C("#9aa6ae"));
                p.Glow(0, 60, 256, 2, Color.black);
                return p;
            }, tiles: 2);

            // Concreto liso (cornisas, balcones, postes): poros y manchas, sin ventana
            Add("ConcretoLiso", 4f, () =>
            {
                var p = new Painter(4, 128 * F, 96 * F, C("#b9b5ad"), F);
                p.H = 0.6f; p.S = 0.08f;
                p.Grime(8, 0.16f, 0.06f, 0.03f, 4);
                p.Grain(2000, 0.07f, 0.03f);
                p.Streaks(6, 0.2f);
                return p;
            });

            // Concreto de oficinas: paneles con juntas y ventana con marco de aluminio
            Add("Concreto", 4f, () =>
            {
                var p = new Painter(5, 256 * F, 96 * F, C("#b9b5ad"), F);
                p.H = 0.6f; p.S = 0.08f;
                p.Grime(8, 0.16f, 0.06f, 0.03f, 4);
                p.Grain(4000, 0.07f, 0.03f);
                p.H = 0.35f;
                p.Rect(0, 0, 256, 3, C("#8f8a82"));
                p.Rect(0, 0, 2, 96, C("#8f8a82"));
                p.Rect(128, 0, 2, 96, C("#8f8a82"));
                p.H = 0.8f; p.S = 0.1f;
                p.Rect(0, 3, 256, 5, C("#a9a49b"));
                p.Streaks(20, 0.3f);
                Window(p, 22, 22, 84, 46, C("#6d6a64"), "#1c2630", "#4d6274", 0.6f, 0.85f, "#dff1ff");
                Window(p, 150, 22, 84, 46, C("#6d6a64"), "#1c2630", "#4d6274", 0.6f, 0.2f, "#dff1ff");
                return p;
            }, tiles: 2);

            // Bodega: lámina corrugada metálica
            Add("Bodega", 3f, () =>
            {
                var p = new Painter(6, 512, 512, C("#8e969c"), 4);
                for (int x = 0; x < 128; x++)
                {
                    float wave = Mathf.Sin(x / 8f * Mathf.PI * 2) * 0.5f + 0.5f;
                    p.H = 0.3f + wave * 0.5f; p.S = 0.45f; p.M = 0.75f;
                    p.Rect(x, 0, 1, 128, Color.Lerp(C("#7a8288"), C("#a4acb2"), wave));
                }
                p.Grime(5, 0.25f, 0.02f, 0.25f, 5);
                p.Streaks(14, 0.35f);
                return p;
            });

            // Techo: placa con grava e impermeabilizante
            Add("Techo", 3f, () =>
            {
                var p = new Painter(7, 256, 256, C("#7d7a74"), 4);
                p.H = 0.5f; p.S = 0.1f;
                p.Grime(4, 0.2f, 0.1f, 0.05f, 6);
                p.Grain(900, 0.15f, 0.2f);
                return p;
            });

            // Asfalto: grava, parches, grietas y aceite
            Add("Asfalto", 2.5f, () =>
            {
                var p = new Painter(8, 512, 512, C("#3f4247"), 4);
                // parches de pavimento nuevo primero, para que también les caiga grava y ruido
                for (int i = 0; i < 4; i++)
                {
                    p.H = 0.45f; p.S = 0.2f;
                    p.Rect(p.R() * 100, p.R() * 100, 14 + p.R() * 20, 10 + p.R() * 16, C("#36393d"));
                }
                p.H = 0.5f; p.S = 0.12f;
                p.Grime(5, 0.22f, 0.04f, 0.06f, 7);
                p.Grain(3000, 0.1f, 0.2f);
                for (int i = 0; i < 7; i++) p.Crack(p.R() * 128, p.R() * 128, 14 + p.rng.Next(18), C("#25272a"));
                for (int i = 0; i < 3; i++) { p.H = 0.5f; p.S = 0.55f; p.Circle(p.R() * 128, p.R() * 128, 3 + p.R() * 5, C("#2c2d30")); }
                return p;
            });

            // Andén: losas con junta y desgaste
            Add("Anden", 4f, () =>
            {
                var p = new Painter(9, 512, 512, C("#8c8880"), 4);
                p.H = 0.3f; p.S = 0.08f;
                p.Rect(0, 0, 128, 128, C("#8c8880"));
                for (int y = 0; y < 128; y += 32)
                    for (int x = 0; x < 128; x += 32)
                    {
                        float s = 0.9f + p.R() * 0.15f;
                        p.H = 0.7f; p.S = 0.1f;
                        p.Bevel(x + 1, y + 1, 30, 30, new Color(0.7f * s, 0.69f * s, 0.66f * s), 1.5f);
                    }
                p.Grime(6, 0.14f, 0.05f, 0.04f, 8);
                p.Grain(1500, 0.06f, 0.04f);
                for (int i = 0; i < 3; i++) p.Crack(p.R() * 128, p.R() * 128, 10, C("#6f6b64"));
                return p;
            });

            // Empedrado: piedras irregulares de tamaño y tono variable, con tope plano
            Add("Empedrado", 4f, () =>
            {
                var p = new Painter(10, 512, 512, C("#3e3934"), 4);
                p.H = 0.2f; p.S = 0.05f;
                p.Rect(0, 0, 128, 128, C("#3e3934"));
                for (int y = 0; y < 128; y += 10)
                    for (int x = (y / 10) % 2 == 1 ? -5 : 0; x < 133; x += 10)
                    {
                        float s = 0.72f + p.R() * 0.5f;
                        float warm = p.R() * 0.06f;
                        p.H = 0.75f + p.R() * 0.15f; p.S = 0.15f + p.R() * 0.2f;
                        float r = 4.3f + p.R() * 1.1f;
                        var col = new Color((0.55f + warm) * s, 0.51f * s, (0.46f - warm) * s);
                        // dos círculos solapados dan una piedra ovalada e irregular
                        float jx = (p.R() - 0.5f) * 1.6f, jy = (p.R() - 0.5f) * 1.6f;
                        p.Circle(x + 5 + jx, y + 5 + jy, r, col, true);
                        p.Circle(x + 5 + jx + (p.R() - 0.5f) * 2.4f, y + 5 + jy + (p.R() - 0.5f) * 1.2f, r * 0.85f, col, true);
                    }
                p.Grime(5, 0.2f, 0.03f, 0.08f, 9);
                p.Grain(1200, 0.08f, 0.03f);
                return p;
            });

            // Pasto: hojas en relieve con variación de color
            Add("Pasto", 3f, () =>
            {
                var p = new Painter(11, 512, 512, C("#3f7a33"), 4);
                p.H = 0.4f; p.S = 0.08f;
                p.Grime(6, 0.3f, 0.1f, 0f, 10);
                for (int i = 0; i < 4000; i++)
                {
                    float s = 0.7f + p.R() * 0.6f;
                    p.H = 0.5f + p.R() * 0.4f; p.S = 0.15f;
                    p.Rect(p.R() * 128, p.R() * 128, 0.6f, 2.5f, new Color(0.27f * s, 0.55f * s, 0.21f * s));
                }
                return p;
            });

            // Agua: ondas suaves (el material es casi espejo)
            Add("Agua", 2f, () =>
            {
                var p = new Painter(12, 256, 256, C("#2f6f8f"), 2);
                p.H = 0.5f; p.S = 0.95f;
                p.Grime(6, 0.15f, 0.35f, 0f, 11);
                return p;
            });

            Add("Circulo", 0f, () =>
            {
                var p = new Painter(13, 128, 128, new Color(0, 0, 0, 0));
                p.Circle(64, 64, 62, Color.white);
                return p;
            }, false, false);

            // Murales (Bogotá es famosa por su arte urbano)
            string[] palette = { "#ff4f6d", "#ffd23f", "#2fbf71", "#47b5e0", "#9b5de5", "#ff8c42", "#00bbf9" };
            for (int m = 0; m < 6; m++)
            {
                int mm = m;
                Add("Mural" + m, 3f, () =>
                {
                    var p = new Painter(20 + mm, 512, 256, C(palette[(mm * 7) % palette.Length]), 2);
                    p.H = 0.6f; p.S = 0.2f;
                    for (int i = 0; i < 7; i++) p.Circle(p.R() * 256, p.R() * 128, 20 + p.R() * 50, C(palette[(mm * 7 + i * 3 + 1) % palette.Length]));
                    p.Rect(170, 50, 46, 40, C("#ffffff"));
                    p.Rect(174, 54, 38, 10, C("#4a2c1d"));
                    p.Rect(216, 60, 10, 6, C("#ffffff"));
                    p.Rect(216, 74, 10, 6, C("#ffffff"));
                    p.Rect(222, 60, 6, 20, C("#ffffff"));
                    p.Grime(6, 0.12f, 0.15f, 0.05f, 20 + mm);
                    p.Grain(800, 0.08f, 0.06f);
                    return p;
                });
            }

            // Pintar en paralelo; si la plataforma no tiene hilos (WebGL), uno por uno
            System.Action<Job> run = j => { j.result = j.paint(); j.result.Bake(j.repeat, j.pbr, j.bump); };
            if (Application.platform == RuntimePlatform.WebGLPlayer) foreach (var j in jobs) run(j);
            else Parallel.ForEach(jobs, run);

            var tex = new Dictionary<string, Texture2D>();
            foreach (var j in jobs)
            {
                tex[j.name] = j.result.ToTextures(j.name);
                if (j.tiles > 1) tiling[tex[j.name]] = new Vector2(1f / j.tiles, 1f);
            }
            Brick = tex["Ladrillo"]; Stucco = tex["Colonial"]; Glass = tex["Vidrio"];
            ConcreteBare = tex["ConcretoLiso"]; Concrete = tex["Concreto"]; Warehouse = tex["Bodega"];
            Roof = tex["Techo"]; Asphalt = tex["Asfalto"]; Sidewalk = tex["Anden"]; Cobble = tex["Empedrado"];
            Grass = tex["Pasto"]; Water = tex["Agua"]; Circle = tex["Circulo"];
            Murals = new Texture2D[6];
            for (int m = 0; m < Murals.Length; m++) Murals[m] = tex["Mural" + m];
        }
    }
}
