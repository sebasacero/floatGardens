// Súper torres de Kennedy 2089 (guía de diseño v2, references/torre-colgante-2089.md, "Súper torres y acentos").
// Rascacielos de 40 a 110 pisos en la maqueta blanca: retícula de losas y montantes sobre vidrio claro,
// dos retrocesos, jardín en el cielo cada 18 pisos y corona con máquinas, tanque y módulo de la red.
// El color entra solo como ACENTO con los colores de la cereza de café (pintón, amarilla, madura).
// El azul sigue siendo solo de Arcadia. Mismo algoritmo que js3d/supertower.js.
using System;
using UnityEngine;

namespace FloatGardens
{
    public static class SuperTower
    {
        public const float FloorHeight = 3.4f;
        public const string Pinton = "#e0471c", Amarilla = "#e8a31f", Madura = "#b3121a";
        const string Glass = "#b3bdbe", Metal = "#3a403d", SkyCore = "#e3e7e4";
        static readonly string[] Tones = { "#f3f3f0", "#eeebe6", "#f1efe9", "#e9ece8", "#efe8e1", "#f5f4f1", "#ebe7e0", "#ece2cf", "#e9e0cf" };
        static readonly string[] Leaves = { "#2c7a2f", "#357f30", "#24682a" };

        public struct Settings
        {
            public float width, depth;  // planta de la base (m)
            public int floors;          // 40 a 110
            public string accent;       // color de toda la piel (torre de acento); null = blanca
            public string crown;        // color solo del último tramo; null = sin corona de color
            public int stripe;          // cara con una franja de montantes de color (-1 = ninguna)
            public string stripeColor;
            public int slabEvery;       // una losa cada n pisos (1 = todas; 2 en el juego para ahorrar vértices)
        }

        /// <summary>
        /// Regla de acento por manzana: como máximo una torre de color entera (35 % de las manzanas).
        /// Las demás: 12 % con corona de color, 10 % con una franja vertical; el resto blancas.
        /// </summary>
        public static Settings[] BlockStyles(int n, System.Random rng)
        {
            float R() => (float)rng.NextDouble();
            int accK = R() < 0.35f ? rng.Next(n) : -1;
            string accC = R() < 0.6f ? Pinton : Amarilla;
            var outS = new Settings[n];
            for (int k = 0; k < n; k++)
            {
                float u = R();
                var s = new Settings { stripe = -1, slabEvery = 1 };
                if (k == accK) s.accent = accC;
                else if (u < 0.12f) s.crown = new[] { Pinton, Amarilla, Madura }[rng.Next(3)];
                else if (u < 0.22f) { s.stripe = rng.Next(4); s.stripeColor = R() < 0.5f ? Pinton : Amarilla; }
                outS[k] = s;
            }
            return outS;
        }

        /// <summary>Construye la torre en c (centro de la base). Devuelve la altura de la cubierta.</summary>
        public static float Build(Func<string, Material, MeshBuilder> B, Action<Vector3, Vector3> collider, Vector3 c, Settings s, System.Random rng)
        {
            float R() => (float)rng.NextDouble();
            float RR(float a, float b) => a + R() * (b - a);
            string Pick(string[] a) => a[rng.Next(a.Length)];
            MeshBuilder Lit(string hex, float smooth = 0.1f) => B("super" + hex, FGMaterials.Lit(hex, smooth));
            string Darker(string hex) => "#" + ColorUtility.ToHtmlStringRGB(Color.Lerp(FGMaterials.Hex(hex), FGMaterials.Hex(Metal), 0.45f)).ToLowerInvariant();

            const float FH = FloorHeight;
            float y0 = c.y + 0.25f;
            int fl = Mathf.Max(20, s.floors), every = Mathf.Max(1, s.slabEvery);
            string tone = s.accent ?? Pick(Tones);
            int f1 = Mathf.RoundToInt(fl * RR(0.55f, 0.7f)), f2 = Mathf.RoundToInt(fl * RR(0.86f, 0.94f));
            float w2 = s.width - RR(3, 5), d2 = s.depth - RR(3, 5), w3 = w2 - RR(3, 5), d3 = d2 - RR(3, 5);
            var segs = new[] { (0, f1, s.width, s.depth), (f1, f2, w2, d2), (f2, fl, w3, d3) };
            var top = segs[0];
            for (int si = 0; si < 3; si++)
            {
                var (a, b, sw, sd) = segs[si];
                if (b <= a || sw < 6 || sd < 6) continue;
                top = segs[si];
                bool crown = s.crown != null && si == 2;
                string skin = crown ? s.crown : tone;
                string glass = s.accent != null || crown ? Darker(skin) : Glass;
                float h = (b - a) * FH, yb = y0 + a * FH;
                Lit(glass, 0.85f).Box(c.x, c.z, yb, sw - 1.4f, h, sd - 1.4f);
                var skinB = Lit(skin);
                for (int f = a; f <= b; f++)
                {
                    if ((f - a) % every == 0 || f == b) skinB.Solid(c.x, c.z, y0 + f * FH, sw + 0.3f, 0.5f, sd + 0.3f);
                    if (f > a && f < b - 2 && f % 18 == 0)
                    {
                        // jardín en el cielo: piso hundido con jardinera verde en los cuatro lados (lo vivo tiene color)
                        Lit(SkyCore).Box(c.x, c.z, y0 + f * FH, sw - 3, FH, sd - 3);
                        var leaf = Lit(Pick(Leaves), 0.35f);
                        leaf.Solid(c.x, c.z + sd / 2 - 0.6f, y0 + f * FH + 0.35f, sw - 0.6f, 1.1f, 0.9f);
                        leaf.Solid(c.x, c.z - sd / 2 + 0.6f, y0 + f * FH + 0.35f, sw - 0.6f, 1.1f, 0.9f);
                        leaf.Solid(c.x + sw / 2 - 0.6f, c.z, y0 + f * FH + 0.35f, 0.9f, 1.1f, sd - 0.6f);
                        leaf.Solid(c.x - sw / 2 + 0.6f, c.z, y0 + f * FH + 0.35f, 0.9f, 1.1f, sd - 0.6f);
                    }
                }
                // montantes verticales: la retícula de ventanas
                for (int q = 0; q < 4; q++)
                {
                    bool alongX = q % 2 == 0;
                    float L = alongX ? sw : sd, D = alongX ? sd : sw, sign = q < 2 ? 1 : -1;
                    int n = Mathf.Max(3, Mathf.RoundToInt(L / 3.1f));
                    for (int k = 0; k <= n; k++)
                    {
                        bool edge = k == 0 || k == n;
                        float t = -L / 2 + L * k / n, fw = edge ? 1f : 0.6f;
                        string col = !edge && s.stripe == q ? s.stripeColor : skin;
                        float px = alongX ? c.x + t : c.x + sign * (D / 2 - 0.1f);
                        float pz = alongX ? c.z + sign * (D / 2 - 0.1f) : c.z + t;
                        Lit(col).Box(px, pz, yb, alongX ? fw : 0.6f, h, alongX ? 0.6f : fw);
                    }
                }
                collider(new Vector3(c.x, yb + h / 2, c.z), new Vector3(sw, h, sd));
            }
            // corona: losa, cuarto de máquinas, tanque, antena y jardinera del módulo de la red
            var (_, tb, tw, td) = top;
            float yt = y0 + tb * FH;
            Lit(s.crown ?? tone).Solid(c.x, c.z, yt, tw + 0.6f, 1, td + 0.6f);
            Lit("#eef0ed").Box(c.x + tw * 0.2f, c.z - td * 0.15f, yt + 0.5f, tw * 0.35f, 3.4f, td * 0.35f);
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            Lit("#e8ebe9").Add(cyl, Matrix4x4.TRS(new Vector3(c.x - tw * 0.25f, yt + 2.4f, c.z + td * 0.2f), Quaternion.identity, new Vector3(3.2f, 1.9f, 3.2f)));
            if (R() < 0.5f) Lit(Metal, 0.35f).Box(c.x + tw * 0.3f, c.z + td * 0.3f, yt + 1, 0.24f, 14, 0.24f);
            Lit("#f4f6f5").Solid(c.x - tw * 0.1f, c.z, yt + 1, Mathf.Min(6, tw - 4), 0.7f, Mathf.Min(9, td - 3));
            Lit("#357f30", 0.35f).Box(c.x - tw * 0.1f, c.z, yt + 1.7f, Mathf.Min(5, tw - 5), 0.4f, Mathf.Min(8, td - 4));
            return yt + 1;
        }
    }
}
