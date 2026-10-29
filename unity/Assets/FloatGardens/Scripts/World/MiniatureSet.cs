// Escenografía de la maqueta (versión móvil, FGStyle.Miniatura), inspirada en la ciudad en miniatura del video de
// referencia: un convoy de costales de café dorados colgados de drones que da vueltas sobre la ciudad (las linternas),
// un cóndor holográfico de malla azul sobre el HQ de Float Gardens (el dragón de alambre) y la Virgen de Guadalupe
// colosal sobre su cerro, con manto verde pátina y rayos dorados (la estatua gigante).
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class MiniatureSet : MonoBehaviour
    {
        static readonly Color Amber = new Color32(0xff, 0xaa, 0x33, 255);
        static readonly Color Gold = new Color32(0xff, 0xb0, 0x40, 255);
        static readonly Color Holo = new Color(0.35f, 0.78f, 1f);
        const int Sacks = 18;
        const float ConvoySpeed = 0.006f; // vueltas por segundo (unos 5 m/s)

        Vector3 convoyCenter;
        readonly List<Transform> sacks = new List<Transform>();
        readonly List<Vector3> sackOffsets = new List<Vector3>();
        readonly List<Transform> rotors = new List<Transform>();
        Transform condor;
        Material holoLines, holoFill;
        Light virginLight;
        float seed;

        public void Build(City city)
        {
            seed = Random.value * 100f;
            BuildConvoy(city.hqCenter + new Vector3(-40, 0, -30));
            BuildCondor(city.hqCenter + new Vector3(0, 95, 0));
            BuildVirgin(new Vector3(462, 210, -20), city);
        }

        // ---------- Convoy de costales dorados ----------
        // La cosecha de las terrazas viaja colgada de drones: de día son linternas ámbar, de noche brillan con el bloom.
        void BuildConvoy(Vector3 center)
        {
            convoyCenter = center;
            var city = City.I;
            var sackMat = FGMaterials.Emissive("costalDorado", Amber, 1.1f);
            if (city != null) city.RegisterGlow(sackMat, Amber, 1.1f, 1.6f);
            var dark = FGMaterials.Lit("dronCuerpo", FGMaterials.Hex("#23262b"), null, 0.55f, 0.8f);
            for (int i = 0; i < Sacks; i++)
            {
                var go = new GameObject("Costal " + (i + 1));
                go.transform.SetParent(transform, false);
                FGMesh.Bake(go.transform, "Costal", new List<FGMesh.Part>
                {
                    FGMesh.P(PrimitiveType.Capsule, Vector3.zero, Quaternion.Euler(0, 0, 90), new Vector3(2.2f, 1.7f, 2.2f), sackMat),
                    FGMesh.P(PrimitiveType.Cube, new Vector3(0, 2.3f, 0), new Vector3(0.08f, 2.2f, 0.08f), dark),
                    FGMesh.P(PrimitiveType.Cube, new Vector3(0, 3.5f, 0), new Vector3(2.2f, 0.4f, 2.2f), dark),
                }, "costal");
                foreach (float sx in new[] { -1f, 1f })
                    foreach (float sz in new[] { -1f, 1f })
                    {
                        var r = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Destroy(r.GetComponent<Collider>());
                        r.name = "Helice";
                        r.transform.SetParent(go.transform, false);
                        r.transform.localPosition = new Vector3(sx * 1.6f, 3.8f, sz * 1.6f);
                        r.transform.localScale = new Vector3(1.5f, 0.02f, 1.5f);
                        r.GetComponent<MeshRenderer>().sharedMaterial = dark;
                        rotors.Add(r.transform);
                    }
                sacks.Add(go.transform);
                sackOffsets.Add(new Vector3(Random.Range(-7f, 7f), Random.Range(-4f, 4f), Random.Range(-7f, 7f)));
            }
        }

        // Recorrido cerrado y ondulado sobre la ciudad (u de 0 a 1)
        Vector3 ConvoyPath(float u)
        {
            float a = u * Mathf.PI * 2f;
            return convoyCenter + new Vector3(Mathf.Cos(a) * 170f, 62f + Mathf.Sin(a * 2f) * 8f, Mathf.Sin(a) * 110f + Mathf.Sin(a * 3f) * 25f);
        }

        // ---------- Cóndor holográfico ----------
        // Malla de líneas y un relleno translúcido: alas abiertas, cuerpo y collar, proyectado sobre el HQ.
        void BuildCondor(Vector3 pos)
        {
            var verts = new List<Vector3>();
            var lines = new List<int>();
            var tris = new List<int>();
            const int NU = 30, NV = 10;
            foreach (float side in new[] { -1f, 1f })
            {
                int start = verts.Count;
                for (int i = 0; i <= NU; i++)
                    for (int j = 0; j <= NV; j++)
                        verts.Add(Wing(side, i / (float)NU, j / (float)NV));
                for (int i = 0; i <= NU; i++)
                    for (int j = 0; j <= NV; j++)
                    {
                        int k = start + i * (NV + 1) + j;
                        if (i < NU) { lines.Add(k); lines.Add(k + NV + 1); }
                        if (j < NV) { lines.Add(k); lines.Add(k + 1); }
                        if (i < NU && j < NV)
                        {
                            int b = k + NV + 1;
                            tris.Add(k); tris.Add(b); tris.Add(k + 1);
                            tris.Add(b); tris.Add(b + 1); tris.Add(k + 1);
                            tris.Add(k); tris.Add(k + 1); tris.Add(b);       // las dos caras
                            tris.Add(b); tris.Add(k + 1); tris.Add(b + 1);
                        }
                    }
            }
            // Cuerpo (anillos) y collar blanco del cóndor
            for (int i = 0; i <= 18; i++)
            {
                float t = i / 18f, r = 5.2f * Mathf.Sin(t * Mathf.PI) * (1 - t * 0.4f), z = 14f - t * 40f;
                int start = verts.Count;
                for (int j = 0; j < 14; j++)
                {
                    float a = j / 14f * Mathf.PI * 2f;
                    verts.Add(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r * 0.8f, z));
                    lines.Add(start + j); lines.Add(start + (j + 1) % 14);
                    if (i > 0) { lines.Add(start + j); lines.Add(start + j - 14); }
                }
            }
            var mesh = new Mesh { name = "CondorHolograma" };
            mesh.SetVertices(verts);
            mesh.subMeshCount = 2;
            mesh.SetIndices(lines, MeshTopology.Lines, 0);
            mesh.SetIndices(tris, MeshTopology.Triangles, 1);
            mesh.RecalculateBounds();

            holoLines = FGMaterials.Instance(FGMaterials.Transparent("holoCondorLineas", new Color(Holo.r, Holo.g, Holo.b, 0.7f)));
            holoFill = FGMaterials.Instance(FGMaterials.Transparent("holoCondorRelleno", new Color(0.16f, 0.55f, 1f, 0.1f)));
            var go = new GameObject("Condor holografico");
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new[] { holoLines, holoFill };
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            condor = go.transform;
        }

        static Vector3 Wing(float side, float u, float v)
        {
            const float Span = 68f, Chord = 20f;
            float chord = Chord * (1 - 0.55f * u) * (1 + 0.25f * Mathf.Sin(u * Mathf.PI));
            float finger = u > 0.82f ? Mathf.Sin(v * Mathf.PI * 7f) * 3f * (u - 0.82f) / 0.18f : 0f; // plumas de la punta
            float x = side * (4.5f + u * Span);
            float y = Mathf.Sin(u * Mathf.PI * 0.95f) * 10f - u * u * 4.5f + Mathf.Sin(v * Mathf.PI) * 1.5f;
            float z = -chord * v + chord * 0.35f + u * u * 12f + finger;
            return new Vector3(x, y, z);
        }

        // ---------- Virgen de Guadalupe colosal ----------
        void BuildVirgin(Vector3 basePos, City city)
        {
            var stone = FGMaterials.Lit("#9c8a80", 0.2f);
            var patina = FGMaterials.Lit("virgenManto", FGMaterials.Hex("#3f7a70"), null, 0.45f, 0.35f);
            var crownMat = FGMaterials.Lit("virgenCorona", FGMaterials.Hex("#c9a24a"), null, 0.65f, 0.8f);
            var rays = FGMaterials.Emissive("virgenRayos", Gold, 0.6f);
            if (city != null) city.RegisterGlow(rays, Gold, 0.6f, 2.4f);
            var stars = FGMaterials.Emissive("virgenEstrellas", Gold, 0.8f);
            if (city != null) city.RegisterGlow(stars, Gold, 0.8f, 2f);

            const float S = 1.6f; // unidades de la figura -> metros (la Virgen mide unos 75 m)
            var parts = new List<FGMesh.Part>
            {
                FGMesh.P(PrimitiveType.Cylinder, new Vector3(0, 2f * S, 0), new Vector3(26 * S, 2f * S, 26 * S), stone),
            };
            var robe = Lathe(new[] { V(0, 0), V(9, 0), V(8.2f, 8), V(6.8f, 18), V(5.9f, 27), V(6.3f, 33), V(5.8f, 36.5f), V(3.6f, 38.6f), V(1.6f, 39), V(0, 39) }, 32, 0, Mathf.PI * 2f, false);
            var mantle = Lathe(new[] { V(9.6f, 0), V(8.8f, 10), V(7.6f, 22), V(7, 32), V(6.6f, 37), V(5.2f, 42), V(3.4f, 46), V(1, 47.2f) }, 32, Mathf.PI * 0.2f, Mathf.PI * 1.6f, true);
            Vector3 feet = new Vector3(0, 4f * S, 0);
            parts.Add(FGMesh.P(robe, feet, Quaternion.identity, Vector3.one * S, stone));
            parts.Add(FGMesh.P(mantle, feet, Quaternion.identity, Vector3.one * S, patina));
            parts.Add(FGMesh.P(PrimitiveType.Sphere, feet + new Vector3(0, 41.8f, 1.4f) * S, Quaternion.Euler(14, 0, 0), new Vector3(7.4f, 9.2f, 8f) * S, stone));
            parts.Add(FGMesh.P(PrimitiveType.Cube, feet + new Vector3(0, 29f, 6.4f) * S, Quaternion.Euler(-17, 0, 0), new Vector3(2.2f, 5f, 2.6f) * S, stone));
            parts.Add(FGMesh.P(PrimitiveType.Cylinder, feet + new Vector3(0, 46f, 1f) * S, Quaternion.identity, new Vector3(4.6f, 1.1f, 4.6f) * S, crownMat));
            // Rayos dorados alrededor de la figura (la mandorla de la imagen de Guadalupe)
            for (int k = 0; k < 56; k++)
            {
                float a = k / 56f * Mathf.PI * 2f, len = 5f + (k * 7 % 5);
                Vector3 p = new Vector3(Mathf.Cos(a) * (12.5f + len * 0.5f), 22f + Mathf.Sin(a) * (27f + len * 0.5f), -4.5f);
                parts.Add(FGMesh.P(PrimitiveType.Cube, feet + p * S, Quaternion.Euler(0, 0, a * Mathf.Rad2Deg - 90f), new Vector3(0.8f, len, 0.4f) * S, rays));
            }
            // Estrellas del manto
            for (int k = 0; k < 40; k++)
            {
                float a = Mathf.PI * 0.22f + (k * 0.618f % 1f) * Mathf.PI * 1.56f, y = 4f + (k * 0.377f % 1f) * 36f, r = 9.6f - y * 0.085f;
                parts.Add(FGMesh.P(PrimitiveType.Cube, feet + new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r) * S, Quaternion.Euler(45, a * Mathf.Rad2Deg, 45), Vector3.one * 0.7f * S, stars));
            }
            var root = new GameObject("Virgen de Guadalupe colosal");
            root.transform.SetParent(transform, false);
            root.transform.SetPositionAndRotation(basePos, Quaternion.Euler(0, -90, 0)); // mira hacia la ciudad (occidente)
            FGMesh.Bake(root.transform, "Figura", parts);

            // De noche una luz cálida la ilumina desde abajo
            var lightGo = new GameObject("LuzVirgen");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0, 6f, 70f);
            lightGo.transform.LookAt(root.transform.TransformPoint(new Vector3(0, 40f * S, 0)));
            virginLight = lightGo.AddComponent<Light>();
            virginLight.type = LightType.Spot;
            virginLight.spotAngle = 40;
            virginLight.range = 220;
            virginLight.color = new Color32(0xff, 0xd9, 0xa0, 255);
            virginLight.shadows = LightShadows.None;
            virginLight.intensity = 0;
        }

        static Vector2 V(float r, float y) => new Vector2(r, y);

        /// <summary>Malla de revolución (perfil radio/altura girado en Y). Con twoSided, también se ve por dentro.</summary>
        static Mesh Lathe(Vector2[] profile, int segments, float phiStart, float phiLength, bool twoSided)
        {
            int n = profile.Length;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = phiStart + phiLength * i / segments, s = Mathf.Sin(a), c = Mathf.Cos(a);
                for (int j = 0; j < n; j++)
                {
                    verts.Add(new Vector3(s * profile[j].x, profile[j].y, c * profile[j].x));
                    uvs.Add(new Vector2(i / (float)segments, j / (float)(n - 1)));
                }
            }
            for (int i = 0; i < segments; i++)
                for (int j = 0; j < n - 1; j++)
                {
                    int a = i * n + j, b = a + n;
                    tris.Add(a); tris.Add(b); tris.Add(a + 1);
                    tris.Add(a + 1); tris.Add(b); tris.Add(b + 1);
                    if (twoSided)
                    {
                        tris.Add(a); tris.Add(a + 1); tris.Add(b);
                        tris.Add(a + 1); tris.Add(b + 1); tris.Add(b);
                    }
                }
            var mesh = new Mesh { name = "Torno" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        void Update()
        {
            float dt = Time.deltaTime, t = Time.time;
            float u0 = Mathf.Repeat(t * ConvoySpeed, 1f);
            for (int i = 0; i < sacks.Count; i++)
            {
                float u = Mathf.Repeat(u0 + i / (float)sacks.Count * 0.55f, 1f); // el convoy ocupa media vuelta
                Vector3 p = ConvoyPath(u), ahead = ConvoyPath(Mathf.Repeat(u + 0.004f, 1f));
                Vector3 off = sackOffsets[i];
                sacks[i].position = p + off + Vector3.up * Mathf.Sin(t * 2f + i * 1.7f) * 0.8f;
                Vector3 dir = ahead - p;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.001f) sacks[i].rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(4, 90, 0);
            }
            foreach (var r in rotors) r.Rotate(0, 1800 * dt, 0, Space.Self);

            float night = City.I != null ? City.I.Night : 0;
            if (condor != null)
            {
                // Planea en el sitio: gira despacio, se mece y la señal parpadea de vez en cuando
                condor.rotation = Quaternion.Euler(Mathf.Sin(t * 0.4f) * 3f, t * 6f, Mathf.Sin(t * 0.55f) * 6f);
                float k = t + seed, flicker = 0.88f + 0.12f * Mathf.Sin(k * 13f) * Mathf.Sin(k * 2.3f);
                if (Mathf.Repeat(k, 3.7f) < 0.07f) flicker *= 0.35f;
                float glow = Mathf.Lerp(1.6f, 2.6f, night) * flicker;
                holoLines.color = new Color(Holo.r * glow, Holo.g * glow, Holo.b * glow, 0.7f * flicker);
                holoFill.color = new Color(0.16f * glow, 0.55f * glow, glow, Mathf.Lerp(0.08f, 0.14f, night) * flicker);
            }
            if (virginLight != null) virginLight.intensity = Mathf.Lerp(0f, 30f, night);
        }
    }
}
