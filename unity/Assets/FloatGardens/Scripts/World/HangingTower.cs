// Torre de terrazas colgantes (guía de diseño v2, references/torre-colgante-2089.md).
// La forma de los Jardines Colgantes de Babilonia hecha en la maqueta blanca de la web:
// terrazas escalonadas con arcadas, café en los pretiles, enredaderas que cuelgan y guamos.
// Todo va a los MeshBuilder de City (pocas llamadas de dibujo); el café se agrega como mallas bajas.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public static class HangingTower
    {
        public struct Settings
        {
            public int terraces;      // 7 (barrio) a 13 (torre central)
            public float floorHeight; // 6 a 7,2 m
            public float baseSize;    // 20 a 44 m por lado
            public float setback;     // retroceso por lado y por terraza
            public float plinth;      // basamento con arcadas de piso a techo (0 = sin basamento)
            public bool sealed_;      // sellada por Arcadia: sin café, caja azul encima
        }

        // colores de la guía (FGTokens): yeso, núcleo, jardinera, agua, hojas, cerezas
        const string Plaster = "#f4f5f3", Core = "#e3e7e4", Planter = "#f2f4f2", Water = "#8e9c9d", Trunk = "#6b4a2e";
        static readonly string[] Leaves = { "#1f5f27", "#2c7a2f", "#357f30", "#24682a" };
        static readonly string[] Canopy = { "#2c7a2f", "#357f30", "#3f8a33", "#4c9a3a" };

        static Mesh blob;
        /// <summary>Icosaedro de 12 vértices: hojas, enredaderas y copas sin gastar polígonos.</summary>
        public static Mesh Blob
        {
            get
            {
                if (blob != null) return blob;
                float t = (1f + Mathf.Sqrt(5f)) / 2f;
                var v = new[] {
                    new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                    new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                    new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1) };
                int[] f = { 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                    3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 };
                // caras planas: cada triángulo con sus propios vértices y normal
                var verts = new List<Vector3>(); var norms = new List<Vector3>(); var tris = new List<int>();
                for (int i = 0; i < f.Length; i += 3)
                {
                    Vector3 a = v[f[i]].normalized, b = v[f[i + 1]].normalized, c = v[f[i + 2]].normalized;
                    Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                    if (Vector3.Dot(n, a + b + c) < 0) { (b, c) = (c, b); n = -n; }
                    int k = verts.Count; verts.Add(a); verts.Add(b); verts.Add(c);
                    norms.Add(n); norms.Add(n); norms.Add(n);
                    tris.Add(k); tris.Add(k + 2); tris.Add(k + 1); // Unity: horario visto desde afuera
                }
                blob = new Mesh { name = "Blob" };
                blob.SetVertices(verts); blob.SetNormals(norms); blob.SetUVs(0, new List<Vector2>(new Vector2[verts.Count])); blob.SetTriangles(tris, 0);
                return blob;
            }
        }

        /// <summary>Construye la torre en c (centro de la base, y = 0). Devuelve la altura de la corona.</summary>
        public static float Build(Func<string, Material, MeshBuilder> B, Action<Vector3, Vector3> collider, Transform parent,
            Vector3 c, Settings s, System.Random rng)
        {
            float R() => (float)rng.NextDouble();
            float RR(float a, float b) => a + R() * (b - a);
            string Pick(string[] a) => a[rng.Next(a.Length)];
            MeshBuilder Lit(string hex, float smooth = 0.1f) => B("torre" + hex, FGMaterials.Lit(hex, smooth));
            void Blob(string hex, Vector3 p, Vector3 scale) => Lit(hex, 0.35f).Add(HangingTower.Blob, Matrix4x4.TRS(p, Quaternion.Euler(0, R() * 360, 0), scale));

            float TH = s.floorHeight, S0 = s.baseSize, y = 0.25f;
            var white = Lit(Plaster);
            if (s.plinth > 0)
            {
                Lit(Core).Box(c.x, c.z, y, S0 - 2, s.plinth, S0 - 2);
                for (int f = 0; f < 4; f++) ArchWall(white, c, f, S0 + 0.6f, y, s.plinth, Mathf.RoundToInt(S0 / 5));
                collider(new Vector3(c.x, y + s.plinth / 2, c.z), new Vector3(S0, s.plinth, S0));
                y += s.plinth;
            }
            float last = S0;
            for (int i = 0; i < s.terraces; i++)
            {
                float w = S0 - i * s.setback * 2;
                if (w < 6) break;
                last = w;
                float inner = w - 3.2f;
                int n = Mathf.Max(3, Mathf.RoundToInt(w / 4.6f));
                Lit(Core).Box(c.x, c.z, y, inner, TH, inner);
                for (int f = 0; f < 4; f++)
                {
                    ArchWall(white, c, f, w, y, TH, n);
                    // barra de luz blanca de cultivo detrás de la arcada y café adentro
                    Vector3 o = Out(f), r = Right(f);
                    Vector3 bar = c + o * (inner / 2 + 0.9f);
                    Lit("#ffffff", 0.5f).Solid(bar.x, bar.z, y + TH - 0.95f, Mathf.Abs(r.x) * (w - 2) + 0.12f, 0.08f, Mathf.Abs(r.z) * (w - 2) + 0.12f);
                    if (!s.sealed_)
                        for (float t = -w / 2 + 1.6f; t < w / 2 - 1.6f; t += 1.4f)
                            Blob(Pick(Leaves), c + o * (inner / 2 + 0.6f) + r * t + Vector3.up * (y + 0.7f), new Vector3(0.45f, 0.55f, 0.45f));
                }
                collider(new Vector3(c.x, y + TH / 2 + 0.22f, c.z), new Vector3(w, TH + 0.44f, w));
                y += TH;
                white.Solid(c.x, c.z, y, w + 0.8f, 0.44f, w + 0.8f);
                float next = S0 - (i + 1) * s.setback * 2;
                for (int f = 0; f < 4; f++)
                {
                    Vector3 o = Out(f), r = Right(f);
                    Vector3 p = c + o * (w / 2 - 0.1f), ch = c + o * (w / 2 - 0.9f);
                    float ax = Mathf.Abs(r.x), az = Mathf.Abs(r.z);
                    Lit(Planter).Solid(p.x, p.z, y + 0.42f, ax * (w + 0.8f) + az * 1.1f, 0.85f, az * (w + 0.8f) + ax * 1.1f);
                    Lit(Water, 0.92f).Top(ch.x, ch.z, y + 0.5f, ax * (w - 1) + az * 0.35f, az * (w - 1) + ax * 0.35f);
                    if (s.sealed_) continue; // Arcadia seca las terrazas que sella
                    // café en la jardinera, con cerezas
                    for (float t = -w / 2 + 0.7f; t < w / 2 - 0.4f; t += 0.75f)
                    {
                        Vector3 q = p + r * t + Vector3.up * (y + 1.65f);
                        Blob(Pick(Leaves), q, new Vector3(RR(0.38f, 0.5f), RR(0.4f, 0.55f), RR(0.38f, 0.5f)));
                        if (R() < 0.3f) Blob("#b3121a", q + new Vector3(RR(-0.2f, 0.2f), -0.15f, RR(-0.2f, 0.2f)), new Vector3(0.08f, 0.09f, 0.08f));
                    }
                    // enredaderas que cuelgan del pretil: 70 % cortas, 30 % largas
                    for (float t = -w / 2 + 0.9f; t < w / 2 - 0.5f; t += RR(1.3f, 2.8f))
                    {
                        Vector3 v0 = c + o * (w / 2 + 0.42f) + r * t;
                        float len = R() < 0.3f ? RR(2.6f, TH * 0.7f) : RR(0.8f, 2f);
                        int k = Mathf.RoundToInt(len / 0.32f);
                        for (int j = 0; j < k; j++)
                        {
                            float fr = (float)j / k;
                            Blob(fr > 0.75f ? "#79b83c" : Pick(Leaves), v0 + r * RR(-0.15f, 0.15f) + o * RR(-0.1f, 0.15f) + Vector3.up * (y + 0.9f - j * 0.32f),
                                new Vector3(0.3f * (1 - fr * 0.5f), 0.26f, 0.28f * (1 - fr * 0.5f)));
                        }
                    }
                    // guamos de sombra en la terraza
                    if (next >= 6)
                    {
                        float mid = (w / 2 + next / 2) / 2 + 0.2f;
                        foreach (float t in new[] { -w / 2 + 2.6f, 0f, w / 2 - 2.6f })
                        {
                            if (R() > 0.7f) continue;
                            Vector3 tp = c + o * mid + r * (t + RR(-0.5f, 0.5f));
                            Lit(Trunk).Box(tp.x, tp.z, y + 0.44f, 0.2f, 2.4f, 0.2f);
                            for (int j = 0; j < 7; j++)
                                Blob(Pick(Canopy), tp + new Vector3(RR(-1, 1), y + RR(2.6f, 3.6f), RR(-1, 1)), new Vector3(RR(0.6f, 0.9f), RR(0.45f, 0.65f), RR(0.6f, 0.9f)));
                        }
                    }
                }
            }
            float top = y + 0.44f;
            if (s.sealed_) Seal(B, c, top, last + 1.4f);
            else Crown(B, parent, c, top, last);
            return top;
        }

        // caras: 0 = +z (norte), 1 = +x, 2 = -z, 3 = -x
        static Vector3 Out(int f) => f switch { 0 => Vector3.forward, 1 => Vector3.right, 2 => Vector3.back, _ => Vector3.left };
        static Vector3 Right(int f) => f switch { 0 => Vector3.left, 1 => Vector3.forward, 2 => Vector3.right, _ => Vector3.back };

        /// <summary>
        /// Muro de arcos de medio punto en la cara f de una terraza de lado w: un arco cada ~4,6 m, vano = 62 % del paso,
        /// antepecho de 0,35 m y 0,7 m de espesor. Pilares de piso a techo y franjas verticales sobre cada arco.
        /// </summary>
        public static void ArchWall(MeshBuilder mb, Vector3 c, int f, float w, float y0, float h, int n, float depth = 0.7f)
        {
            Vector3 o = Out(f), r = Right(f);
            Vector3 mid = c + o * (w / 2 - depth / 2);
            Vector3 P(float x, float y, float z) => mid + r * x + o * z + Vector3.up * (y0 + y);
            void Face(float x0, float ya, float x1, float yb, float xa1, float xb1)
            {
                // cuadrilátero en el plano del muro (frente y fondo); x0/x1 abajo, xa1/xb1 arriba
                foreach (float side in new[] { 1f, -1f })
                {
                    float z = side * depth / 2;
                    mb.Quad(P(x0, ya, z), P(x1, ya, z), P(xb1, yb, z), P(xa1, yb, z), o * side, Vector2.zero, Vector2.one);
                }
            }
            float pitch = w / n, rad = pitch * 0.31f, ys = h - 0.55f - rad, sill = 0.35f;
            const int Seg = 8;
            Face(-w / 2, 0, w / 2, sill, -w / 2, w / 2); // antepecho corrido
            float prev = -w / 2;
            for (int k = 0; k < n; k++)
            {
                float cx = -w / 2 + pitch * (k + 0.5f);
                Face(prev, sill, cx - rad, h, prev, cx - rad); // pilar de piso a techo
                prev = cx + rad;
                for (int i = 0; i < Seg; i++)
                {
                    float a0 = Mathf.PI * (1 - (float)i / Seg), a1 = Mathf.PI * (1 - (float)(i + 1) / Seg);
                    float x0 = cx + rad * Mathf.Cos(a0), x1 = cx + rad * Mathf.Cos(a1);
                    float y0a = ys + rad * Mathf.Sin(a0), y1a = ys + rad * Mathf.Sin(a1);
                    // franja vertical del arco al techo
                    foreach (float side in new[] { 1f, -1f })
                    {
                        float z = side * depth / 2;
                        mb.Quad(P(x0, y0a, z), P(x1, y1a, z), P(x1, h, z), P(x0, h, z), o * side, Vector2.zero, Vector2.one);
                    }
                    // intradós: la cara de abajo del arco mira hacia el centro del vano
                    Vector3 nIn = -(r * Mathf.Cos((a0 + a1) / 2) + Vector3.up * Mathf.Sin((a0 + a1) / 2));
                    mb.Quad(P(x0, y0a, depth / 2), P(x1, y1a, depth / 2), P(x1, y1a, -depth / 2), P(x0, y0a, -depth / 2), nIn, Vector2.zero, Vector2.one);
                }
                // jambas y repisa del vano
                mb.Quad(P(cx - rad, sill, depth / 2), P(cx - rad, ys, depth / 2), P(cx - rad, ys, -depth / 2), P(cx - rad, sill, -depth / 2), r, Vector2.zero, Vector2.one);
                mb.Quad(P(cx + rad, sill, depth / 2), P(cx + rad, ys, depth / 2), P(cx + rad, ys, -depth / 2), P(cx + rad, sill, -depth / 2), -r, Vector2.zero, Vector2.one);
                mb.Quad(P(cx - rad, sill, depth / 2), P(cx + rad, sill, depth / 2), P(cx + rad, sill, -depth / 2), P(cx - rad, sill, -depth / 2), Vector3.up, Vector2.zero, Vector2.one);
            }
            Face(prev, sill, w / 2, h, prev, w / 2); // pilar del extremo
            // canto superior y extremos
            mb.Quad(P(-w / 2, h, depth / 2), P(w / 2, h, depth / 2), P(w / 2, h, -depth / 2), P(-w / 2, h, -depth / 2), Vector3.up, Vector2.zero, Vector2.one);
            foreach (float e in new[] { -1f, 1f })
                mb.Quad(P(e * w / 2, 0, depth / 2), P(e * w / 2, h, depth / 2), P(e * w / 2, h, -depth / 2), P(e * w / 2, 0, -depth / 2), r * e, Vector2.zero, Vector2.one);
        }

        /// <summary>Corona: invernadero de vidrio claro y la baldosa blanca con la hoja tinta (el logo de la web).</summary>
        static void Crown(Func<string, Material, MeshBuilder> B, Transform parent, Vector3 c, float top, float last)
        {
            float gw = Mathf.Min(9, last - 1), gd = Mathf.Min(6, last - 1.5f);
            var frame = B("torre#f3f5f4", FGMaterials.Lit("#f3f5f4", 0.2f));
            foreach (var p in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                frame.Box(c.x + p.x * gw / 2, c.z - 0.5f + p.y * gd / 2, top, 0.08f, 2.3f, 0.08f);
            frame.Solid(c.x, c.z - 0.5f, top + 2.3f, gw, 0.08f, gd);
            var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(glass.GetComponent<Collider>());
            glass.name = "InvernaderoCorona";
            glass.transform.SetParent(parent, false);
            glass.transform.position = new Vector3(c.x, top + 1.15f, c.z - 0.5f);
            glass.transform.localScale = new Vector3(gw, 2.3f, gd);
            glass.GetComponent<MeshRenderer>().sharedMaterial = FGMaterials.Transparent("vidrioInvernadero", new Color(1, 1, 1, 0.12f), true);
            // baldosa con la hoja: tile blanco, hoja tinta (dos icosaedros aplanados) y nervio blanco
            Vector3 t = new Vector3(c.x, top + 3.2f, c.z - last / 2 + 0.9f);
            B("torre#3a403d", FGMaterials.Lit("#3a403d", 0.35f)).Box(t.x, t.z, top, 0.08f, 1.6f, 0.08f);
            B("torre#ffffff", FGMaterials.Lit("#ffffff", 0.5f)).Solid(t.x, t.z, top + 1.6f, 3.2f, 3.2f, 0.08f);
            var ink = B("torre#2c302e", FGMaterials.Lit("#2c302e", 0.3f));
            foreach (float side in new[] { -1f, 1f })
                ink.Add(Blob, Matrix4x4.TRS(t + new Vector3(0, 0, side * 0.06f), Quaternion.identity, new Vector3(0.9f, 1.25f, 0.02f)));
            foreach (float side in new[] { -1f, 1f })
                B("torre#ffffff", FGMaterials.Lit("#ffffff", 0.5f)).Solid(t.x, t.z + side * 0.09f, top + 2.35f, 0.08f, 1.7f, 0.01f);
        }

        /// <summary>Sello de Arcadia: caja de vidrio azul al 9 % con aristas azules y líneas cada 0,7 m.</summary>
        static void Seal(Func<string, Material, MeshBuilder> B, Vector3 c, float top, float w)
        {
            var glass = B("selloVidrio", FGMaterials.Transparent("selloArcadia", new Color(0.165f, 0.133f, 0.961f, 0.09f)));
            glass.Walls(c.x, c.z, top, w, 7, w);
            glass.Top(c.x, c.z, top + 7, w, w);
            var edge = B("selloArista", FGMaterials.Emissive("arcadia", FGMaterials.Hex("#2a22f5"), 1.2f));
            foreach (var p in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                edge.Solid(c.x + p.x * w / 2, c.z + p.y * w / 2, top, 0.06f, 7, 0.06f);
            for (float yy = 0.7f; yy <= 7.01f; yy += 0.7f)
                foreach (var (dx, dz, sx, sz) in new[] { (0f, 1f, w, 0.04f), (0f, -1f, w, 0.04f), (1f, 0f, 0.04f, w), (-1f, 0f, 0.04f, w) })
                    edge.Solid(c.x + dx * w / 2, c.z + dz * w / 2, top + yy - 0.02f, sx, 0.04f, sz);
        }
    }
}
