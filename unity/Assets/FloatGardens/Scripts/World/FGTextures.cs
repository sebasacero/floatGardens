// Texturas generadas por código: ladrillo bogotano, fachadas coloniales, vidrio, asfalto, empedrado...
// Un "módulo" de fachada mide 4 m de ancho x 3 m de alto (un piso con una ventana).
using UnityEngine;

namespace FloatGardens
{
    public static class FGTextures
    {
        public static Texture2D Brick, Stucco, Glass, Concrete, Warehouse, Roof, Asphalt, Sidewalk, Cobble, Grass, Water, Circle;
        public static Texture2D[] Murals;
        static System.Random rng = new System.Random(2600);

        class Painter
        {
            public readonly int w, h;
            public readonly Color32[] px;
            public Painter(int w, int h, Color32 bg)
            {
                this.w = w; this.h = h;
                px = new Color32[w * h];
                for (int i = 0; i < px.Length; i++) px[i] = bg;
            }
            // (x, y) desde la esquina superior izquierda, como en un canvas
            public void Rect(float x, float y, float rw, float rh, Color32 c)
            {
                int x0 = Mathf.Max(0, Mathf.FloorToInt(x)), x1 = Mathf.Min(w, Mathf.CeilToInt(x + rw));
                int y0 = Mathf.Max(0, Mathf.FloorToInt(y)), y1 = Mathf.Min(h, Mathf.CeilToInt(y + rh));
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++) px[(h - 1 - yy) * w + xx] = c;
            }
            public void Gradient(float x, float y, float rw, float rh, Color a, Color b)
            {
                int x0 = Mathf.Max(0, (int)x), x1 = Mathf.Min(w, (int)(x + rw));
                int y0 = Mathf.Max(0, (int)y), y1 = Mathf.Min(h, (int)(y + rh));
                for (int yy = y0; yy < y1; yy++)
                    for (int xx = x0; xx < x1; xx++)
                    {
                        float t = ((xx - x) / rw + (yy - y) / rh) * 0.5f;
                        px[(h - 1 - yy) * w + xx] = Color.Lerp(a, b, t);
                    }
            }
            public void Circle(float cx, float cy, float r, Color32 c)
            {
                for (int yy = Mathf.Max(0, (int)(cy - r)); yy < Mathf.Min(h, (int)(cy + r + 1)); yy++)
                    for (int xx = Mathf.Max(0, (int)(cx - r)); xx < Mathf.Min(w, (int)(cx + r + 1)); xx++)
                        if ((xx - cx) * (xx - cx) + (yy - cy) * (yy - cy) <= r * r) px[(h - 1 - yy) * w + xx] = c;
            }
            public void Noise(int count, float alpha)
            {
                for (int i = 0; i < count; i++)
                {
                    int idx = rng.Next(px.Length);
                    float k = (float)rng.NextDouble() * alpha;
                    Color c = px[idx];
                    px[idx] = rng.Next(2) == 0 ? Color.Lerp(c, Color.black, k) : Color.Lerp(c, Color.white, k);
                }
            }
            public Texture2D Build(string name, bool repeat = true)
            {
                var t = new Texture2D(w, h, TextureFormat.RGBA32, true) { name = name };
                t.SetPixels32(px);
                t.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                t.filterMode = FilterMode.Trilinear;
                t.anisoLevel = 4;
                t.Apply(true, false);
                return t;
            }
        }

        static Color32 C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        static float R() => (float)rng.NextDouble();

        static void Window(Painter p, float x, float y, float w, float h, Color32 frame, string g0, string g1)
        {
            p.Rect(x - 3, y - 3, w + 6, h + 6, frame);
            p.Gradient(x, y, w, h, C(g0), C(g1));
            p.Rect(x + w / 2 - 1.5f, y, 3, h, frame);
            p.Rect(x, y + h * 0.4f, w, 3, frame);
        }

        public static void Init()
        {
            if (Brick != null) return;

            var p = new Painter(128, 96, C("#9a4a2f"));
            for (int y = 0; y < 96; y += 8)
            {
                int off = (y / 8) % 2 == 1 ? 8 : 0;
                for (int x = -off; x < 128; x += 16)
                {
                    float s = 0.85f + R() * 0.3f;
                    p.Rect(x + 1, y + 1, 14, 6, new Color(0.59f * s, 0.27f * s, 0.18f * s));
                }
            }
            Window(p, 34, 22, 60, 50, C("#e8e2d6"), "#2d3d4f", "#6b8299");
            Brick = p.Build("Ladrillo");

            p = new Painter(128, 96, C("#f4f0e8"));
            p.Noise(600, 0.08f);
            p.Rect(0, 88, 128, 8, C("#d9d2c4"));
            Window(p, 40, 18, 48, 54, C("#5a3a22"), "#3b3226", "#6b5a44");
            p.Rect(30, 70, 68, 6, C("#4a2e1a"));
            for (int x = 32; x < 98; x += 8) p.Rect(x, 58, 3, 12, C("#4a2e1a"));
            Stucco = p.Build("Colonial");

            p = new Painter(128, 96, C("#5b8fb8"));
            p.Gradient(0, 0, 128, 96, C("#5b8fb8"), C("#a8cbe6"));
            p.Rect(0, 0, 128, 4, C("#c9d6e0"));
            p.Rect(0, 0, 3, 96, C("#c9d6e0"));
            p.Rect(64, 0, 2, 96, C("#c9d6e0"));
            Glass = p.Build("Vidrio");

            p = new Painter(128, 96, C("#b9b5ad"));
            p.Noise(800, 0.12f);
            p.Rect(0, 0, 128, 6, C("#a19c93"));
            Window(p, 22, 22, 84, 46, C("#7d7a74"), "#34404c", "#71879a");
            Concrete = p.Build("Concreto");

            p = new Painter(128, 128, C("#8e969c"));
            for (int x = 0; x < 128; x += 8) p.Rect(x, 0, 4, 128, x % 16 == 0 ? C("#7c848a") : C("#9aa2a8"));
            p.Noise(400, 0.1f);
            Warehouse = p.Build("Bodega");

            p = new Painter(64, 64, C("#8a8680"));
            p.Noise(300, 0.15f);
            Roof = p.Build("Techo");

            p = new Painter(256, 256, C("#41444a"));
            p.Noise(6000, 0.12f);
            Asphalt = p.Build("Asfalto");

            p = new Painter(128, 128, C("#b4b0a8"));
            for (int i = 0; i <= 128; i += 32) { p.Rect(i - 1, 0, 2, 128, C("#97938b")); p.Rect(0, i - 1, 128, 2, C("#97938b")); }
            p.Noise(500, 0.08f);
            Sidewalk = p.Build("Anden");

            p = new Painter(128, 128, C("#6f675e"));
            for (int y = 0; y < 128; y += 12)
                for (int x = (y / 12) % 2 == 1 ? -6 : 0; x < 128; x += 12)
                {
                    float s = 0.8f + R() * 0.4f;
                    p.Circle(x + 6, y + 6, 5, new Color(0.59f * s, 0.55f * s, 0.5f * s));
                }
            Cobble = p.Build("Empedrado");

            p = new Painter(128, 128, C("#4f8f3f"));
            for (int i = 0; i < 1500; i++)
            {
                float s = 0.7f + R() * 0.6f;
                p.Rect(R() * 128, R() * 128, 2, 3, new Color(0.27f * s, 0.55f * s, 0.23f * s));
            }
            Grass = p.Build("Pasto");

            p = new Painter(128, 128, C("#3f8fb3"));
            for (int i = 0; i < 18; i++) p.Rect(R() * 128, R() * 128, 12, 2, C("#7fbfdc"));
            Water = p.Build("Agua");

            p = new Painter(128, 128, new Color32(0, 0, 0, 0));
            p.Circle(64, 64, 62, new Color32(255, 255, 255, 255));
            Circle = p.Build("Circulo", false);

            // Murales (Bogotá es famosa por su arte urbano)
            string[] palette = { "#ff4f6d", "#ffd23f", "#2fbf71", "#47b5e0", "#9b5de5", "#ff8c42", "#00bbf9" };
            Murals = new Texture2D[6];
            for (int m = 0; m < Murals.Length; m++)
            {
                p = new Painter(256, 128, C(palette[(m * 7) % palette.Length]));
                for (int i = 0; i < 7; i++) p.Circle(R() * 256, R() * 128, 20 + R() * 50, C(palette[(m * 7 + i * 3 + 1) % palette.Length]));
                p.Rect(170, 50, 46, 40, C("#ffffff"));
                p.Rect(174, 54, 38, 10, C("#4a2c1d"));
                p.Rect(216, 60, 10, 6, C("#ffffff"));
                p.Rect(216, 74, 10, 6, C("#ffffff"));
                p.Rect(222, 60, 6, 20, C("#ffffff"));
                Murals[m] = p.Build("Mural" + m);
            }
        }
    }
}
