// Mapa 3D de Colombia: contorno real del país (Natural Earth), relieve de las tres cordilleras y la Sierra Nevada,
// ríos Magdalena y Cauca, regiones cafeteras con sus pines y una chiva que viaja de una región a otra.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public class ColombiaMap : MonoBehaviour
    {
        public SceneInfo info;
        public string origin = "bogota";   // de dónde sale la chiva (Bogotá o la última región visitada)

        // Contorno de Colombia (lon, lat), Natural Earth 1:110m
        static readonly float[] Outline =
        {
            -75.37f, -0.15f, -75.80f, 0.08f, -76.29f, 0.42f, -76.58f, 0.26f, -77.42f, 0.40f, -77.67f, 0.83f, -77.86f, 0.81f, -78.86f, 1.38f, -78.99f, 1.69f, -78.62f, 1.77f,
            -78.66f, 2.27f, -78.43f, 2.63f, -77.93f, 2.70f, -77.51f, 3.33f, -77.13f, 3.85f, -77.50f, 4.09f, -77.31f, 4.67f, -77.53f, 5.58f, -77.32f, 5.85f, -77.48f, 6.69f,
            -77.88f, 7.22f, -77.75f, 7.71f, -77.43f, 7.64f, -77.24f, 7.94f, -77.47f, 8.52f, -77.35f, 8.67f, -76.84f, 8.64f, -76.09f, 9.34f, -75.67f, 9.44f, -75.66f, 9.77f,
            -75.48f, 10.62f, -74.91f, 11.08f, -74.28f, 11.10f, -74.20f, 11.31f, -73.41f, 11.23f, -72.63f, 11.73f, -72.24f, 11.96f, -71.75f, 12.44f, -71.40f, 12.38f, -71.14f, 12.11f,
            -71.33f, 11.78f, -71.97f, 11.61f, -72.23f, 11.11f, -72.61f, 10.82f, -72.91f, 10.45f, -73.03f, 9.74f, -73.30f, 9.15f, -72.79f, 9.09f, -72.66f, 8.63f, -72.44f, 8.41f,
            -72.36f, 8.00f, -72.48f, 7.63f, -72.44f, 7.42f, -72.20f, 7.34f, -71.96f, 6.99f, -70.67f, 7.09f, -70.09f, 6.96f, -69.39f, 6.10f, -68.99f, 6.21f, -68.27f, 6.15f,
            -67.70f, 6.27f, -67.34f, 6.10f, -67.52f, 5.56f, -67.74f, 5.22f, -67.82f, 4.50f, -67.62f, 3.84f, -67.34f, 3.54f, -67.30f, 3.32f, -67.81f, 2.82f, -67.45f, 2.60f,
            -67.18f, 2.25f, -66.88f, 1.25f, -67.07f, 1.13f, -67.26f, 1.72f, -67.54f, 2.04f, -67.87f, 1.69f, -69.82f, 1.71f, -69.80f, 1.09f, -69.22f, 0.99f, -69.25f, 0.60f,
            -69.45f, 0.71f, -70.02f, 0.54f, -70.02f, -0.19f, -69.58f, -0.55f, -69.42f, -1.12f, -69.44f, -1.56f, -69.89f, -4.30f, -70.39f, -3.77f, -70.69f, -3.74f, -70.05f, -2.73f,
            -70.81f, -2.26f, -71.41f, -2.34f, -71.77f, -2.17f, -72.33f, -2.43f, -73.07f, -2.31f, -73.66f, -1.26f, -74.12f, -1.00f, -74.44f, -0.53f, -75.11f, -0.06f,
        };

        // Ejes aproximados de las cordilleras (lat, lon) desde el Macizo Colombiano hacia el norte
        static readonly float[][] Ranges =
        {
            new[] { 1.1f, -77.5f, 2.4f, -76.9f, 4.0f, -76.3f, 5.5f, -76.1f, 7.0f, -76.3f },                                 // Occidental
            new[] { 1.4f, -76.9f, 2.5f, -76.2f, 4.0f, -75.5f, 5.0f, -75.3f, 6.5f, -75.3f, 7.8f, -75.2f },                  // Central
            new[] { 1.8f, -76.3f, 3.0f, -75.2f, 4.5f, -74.0f, 5.5f, -73.3f, 6.8f, -72.7f, 7.8f, -72.6f, 9.0f, -73.0f, 10.5f, -72.9f }, // Oriental y Perijá
        };
        static readonly float[] Magdalena = { 1.9f, -76.6f, 2.9f, -75.3f, 4.2f, -74.8f, 5.2f, -74.7f, 6.5f, -74.4f, 8.0f, -74.0f, 9.2f, -74.6f, 10.4f, -74.8f, 11.05f, -74.85f };
        static readonly float[] Cauca = { 2.4f, -76.6f, 3.5f, -76.45f, 4.8f, -75.95f, 6.0f, -75.8f, 7.6f, -75.3f, 8.8f, -74.5f, 9.2f, -74.6f };

        const float Step = 0.2f;    // resolución del relieve en grados
        const float Base = 0.6f;    // grosor del país sobre el mar
        const float Relief = 1.0f;  // exageración del relieve

        HistoriaSave save;
        readonly Dictionary<string, Transform> pins = new Dictionary<string, Transform>();
        readonly List<Transform> labels = new List<Transform>();
        Vector3 look, lookTarget;
        string focused;
        float t;

        // viaje en chiva
        Transform bus;
        Vector3 from, to;
        float travelT = -1f;
        Action travelDone;
        readonly List<GameObject> trail = new List<GameObject>();

        static readonly Vector3 CamOffset = new Vector3(-28f, 150f, -105f);

        public static ColombiaMap Build(Transform root, HistoriaSave s)
        {
            var m = new GameObject("MapaColombia").AddComponent<ColombiaMap>();
            m.transform.SetParent(root, false);
            m.save = s;
            m.Make();
            m.look = m.lookTarget = new Vector3(0, 0, 10);
            m.info = new SceneInfo { walkable = false, cameraPos = m.look + CamOffset, cameraLook = m.look, altitude = HConfig.BogotaAltitude };
            return m;
        }

        // ============================ RELIEVE ============================

        static float DistToPolyline(float lat, float lon, float[] line)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 3 < line.Length; i += 2)
            {
                Vector2 a = new Vector2(line[i + 1], line[i]), b = new Vector2(line[i + 3], line[i + 2]), p = new Vector2(lon, lat);
                Vector2 ab = b - a;
                float k = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-5f, ab.sqrMagnitude));
                best = Mathf.Min(best, (p - (a + ab * k)).magnitude);
            }
            return best;
        }

        /// <summary>Altura del terreno (unidades del mapa) en una coordenada.</summary>
        public static float Height(float lat, float lon)
        {
            float h = 0f;
            float[] peaks = { 2.6f, 3.0f, 2.4f };
            float[] widths = { 0.35f, 0.4f, 0.55f };
            for (int i = 0; i < Ranges.Length; i++)
            {
                float d = DistToPolyline(lat, lon, Ranges[i]);
                h = Mathf.Max(h, peaks[i] * Mathf.Exp(-(d * d) / (widths[i] * widths[i])));
            }
            // Sierra Nevada de Santa Marta: la montaña costera más alta del mundo
            float ds = new Vector2(lon + 73.7f, lat - 10.85f).magnitude;
            h = Mathf.Max(h, 4.6f * Mathf.Exp(-(ds * ds) / (0.32f * 0.32f)));
            // ondulación suave
            h += (Mathf.PerlinNoise(lon * 1.7f + 100f, lat * 1.7f) - 0.5f) * 0.35f * Mathf.Clamp01(h);
            return Mathf.Max(0f, h) * Relief;
        }

        static bool Inside(float lon, float lat)
        {
            bool inside = false;
            int n = Outline.Length / 2;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                float xi = Outline[i * 2], yi = Outline[i * 2 + 1], xj = Outline[j * 2], yj = Outline[j * 2 + 1];
                if ((yi > lat) != (yj > lat) && lon < (xj - xi) * (lat - yi) / (yj - yi) + xi) inside = !inside;
            }
            return inside;
        }

        static int Biome(float lat, float lon, float h)
        {
            if (h > 3.6f) return 5;                        // nieve
            if (h > 0.9f) return 0;                        // Andes
            if (lat > 8.6f) return 1;                      // Caribe seco
            if (lon < -76.4f) return 2;                    // Pacífico selvático
            if (lat < 2.6f && lon > -74.8f) return 3;      // Amazonía
            if (lon > -73.4f) return 4;                    // Llanos
            return 0;
        }

        static Vector3 P(float lat, float lon) => ColombiaGeo.ToMap(lat, lon, Base + Height(lat, lon));

        void Make()
        {
            FGTextures.Init();
            var builders = new MeshBuilder[6];
            for (int i = 0; i < builders.Length; i++) builders[i] = new MeshBuilder();
            float minLon = -79.1f, maxLon = -66.8f, minLat = -4.3f, maxLat = 12.5f;
            for (float lat = minLat; lat < maxLat; lat += Step)
            {
                for (float lon = minLon; lon < maxLon; lon += Step)
                {
                    float cl = lat + Step / 2f, cn = lon + Step / 2f;
                    if (!Inside(cn, cl)) continue;
                    Vector3 a = P(lat, lon), b = P(lat, lon + Step), c = P(lat + Step, lon + Step), d = P(lat + Step, lon);
                    Vector3 n = Vector3.Cross(d - a, b - a).normalized;
                    if (n.y < 0) n = -n;
                    int biome = Biome(cl, cn, Height(cl, cn));
                    builders[biome].Quad(a, b, c, d, n, new Vector2(lon, lat), new Vector2(lon + Step, lat + Step));
                }
            }
            string[] colors = { "#4f8a3a", "#b8a868", "#2d6e3a", "#1f5a2a", "#a3b65a", "#f2f4f6" };
            for (int i = 0; i < builders.Length; i++)
                if (builders[i].VertexCount > 0) builders[i].Build("Terreno" + i, HWorld.M(colors[i], i == 5 ? 0.5f : 0.08f), transform, i == 0 || i == 5);

            // costados del país (bajan hasta el mar)
            var sides = new MeshBuilder();
            int count = Outline.Length / 2;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                Vector3 a = ColombiaGeo.ToMap(Outline[i * 2 + 1], Outline[i * 2], Base), b = ColombiaGeo.ToMap(Outline[j * 2 + 1], Outline[j * 2], Base);
                Vector3 a0 = a, b0 = b;
                a0.y = b0.y = -0.5f;
                Vector3 n = Vector3.Cross(b - a, Vector3.up).normalized;
                sides.Quad(a0, b0, b, a, n, Vector2.zero, Vector2.one);
            }
            sides.Build("Costados", HWorld.M("#6b5a3e", 0.1f), transform, false);

            // mar y países vecinos
            var sea = FGMaterials.Lit("mapaMar", new Color(0.16f, 0.42f, 0.62f), FGTextures.Water, 0.85f);
            HWorld.Box(transform, "Mar", new Vector3(0, -0.2f, 10), new Vector3(600, 0.2f, 600), sea, false, false);
            HWorld.Box(transform, "Vecinos", new Vector3(117, -0.05f, -12), new Vector3(165, 0.2f, 155), HWorld.M("#9a9583", 0.05f), false, false);   // Venezuela y Brasil
            HWorld.Box(transform, "Vecinos", new Vector3(-20, -0.05f, -110), new Vector3(120, 0.2f, 150), HWorld.M("#9a9583", 0.05f), false, false);  // Ecuador y Perú
            HWorld.Box(transform, "Vecinos", new Vector3(-71, -0.05f, 39), new Vector3(58, 0.2f, 24), HWorld.M("#9a9583", 0.05f), false, false);     // Panamá
            HWorld.Text3D(transform, "Mar Caribe", ColombiaGeo.ToMap(12.6f, -76.5f, 0.3f), Quaternion.Euler(90, 0, 0), 3.2f, new Color(0.8f, 0.92f, 1f, 0.8f));
            HWorld.Text3D(transform, "Océano Pacífico", ColombiaGeo.ToMap(3.5f, -80.5f, 0.3f), Quaternion.Euler(90, 0, 0), 3.2f, new Color(0.8f, 0.92f, 1f, 0.8f));
            HWorld.Text3D(transform, "LLANOS", ColombiaGeo.ToMap(5.0f, -70.3f, Base + 0.3f), Quaternion.Euler(90, 0, 0), 2.6f, new Color(0.25f, 0.3f, 0.12f, 0.7f));
            HWorld.Text3D(transform, "AMAZONÍA", ColombiaGeo.ToMap(0.3f, -72.0f, Base + 0.3f), Quaternion.Euler(90, 0, 0), 2.6f, new Color(0.7f, 0.9f, 0.7f, 0.7f));

            River(Magdalena, "Magdalena");
            River(Cauca, "Cauca");
            Pins();
        }

        void River(float[] line, string name)
        {
            var mb = new MeshBuilder();
            for (int i = 0; i + 3 < line.Length; i += 2)
            {
                Vector3 a = ColombiaGeo.ToMap(line[i], line[i + 1]), b = ColombiaGeo.ToMap(line[i + 2], line[i + 3]);
                int seg = 6;
                for (int k = 0; k < seg; k++)
                {
                    Vector3 p0 = Vector3.Lerp(a, b, k / (float)seg), p1 = Vector3.Lerp(a, b, (k + 1) / (float)seg);
                    float lat0 = p0.z / ColombiaGeo.Scale + ColombiaGeo.LatOrigin, lon0 = p0.x / ColombiaGeo.Scale + ColombiaGeo.LonOrigin;
                    float lat1 = p1.z / ColombiaGeo.Scale + ColombiaGeo.LatOrigin, lon1 = p1.x / ColombiaGeo.Scale + ColombiaGeo.LonOrigin;
                    p0.y = Base + Height(lat0, lon0) + 0.08f;
                    p1.y = Base + Height(lat1, lon1) + 0.08f;
                    Vector3 side = Vector3.Cross(p1 - p0, Vector3.up).normalized * 0.45f;
                    mb.Quad(p0 - side, p0 + side, p1 + side, p1 - side, Vector3.up, Vector2.zero, Vector2.one);
                }
            }
            mb.Build("Rio " + name, FGMaterials.Lit("rioMapa", new Color(0.25f, 0.55f, 0.85f), null, 0.8f), transform, false);
        }

        // ============================ PINES ============================

        static Vector3 RegionPos(Region r) => ColombiaGeo.ToMap(r.lat, r.lon, Base + Height(r.lat, r.lon));

        void Pins()
        {
            foreach (var r in RegionData.All)
            {
                var pin = HWorld.Group(transform, "Pin " + r.id, RegionPos(r));
                bool open = r.chapter <= save.chapter;
                bool stamp = save.stamps.Contains(r.id);
                bool visited = save.visited.Contains(r.id);
                var stick = HWorld.M("#2a1a10", 0.3f);
                HWorld.Cyl(pin, "Palo", new Vector3(0, 1.6f, 0), 0.18f, 3.2f, stick);
                Material head = !open ? HWorld.M("#8a8a8a", 0.3f) : stamp ? FGMaterials.Emissive("pinSello", HConfig.Gold, 1.2f) : FGMaterials.Lit("pinCereza", HConfig.Cherry, null, 0.8f);
                if (r.id == "bogota")
                {
                    // la capital: estrella y la sede de Float Gardens
                    var star = HWorld.Box(pin, "Estrella", new Vector3(0, 3.7f, 0), new Vector3(1.4f, 1.4f, 0.4f), FGMaterials.Emissive("pinBogota", HConfig.Gold, 1.6f), false, false);
                    star.transform.localRotation = Quaternion.Euler(0, 0, 45);
                    HWorld.Box(pin, "Estrella2", new Vector3(0, 3.7f, 0), new Vector3(1.4f, 1.4f, 0.4f), FGMaterials.Emissive("pinBogota", HConfig.Gold, 1.6f), false, false);
                    HWorld.Box(pin, "Bodega", new Vector3(1.2f, 0.5f, -0.6f), new Vector3(1.2f, 0.8f, 0.9f), HWorld.M("#9b4a35", 0.2f), false, false);
                }
                else HWorld.Ball(pin, "Cereza", new Vector3(0, 3.6f, 0), 1.3f, head);
                if (visited) HWorld.Cyl(pin, "Anillo", new Vector3(0, 0.1f, 0), 2.6f, 0.08f, FGMaterials.Emissive("pinVisitado", new Color(0.5f, 1f, 0.7f), 0.8f));
                var label = HWorld.Text3D(pin, r.name, new Vector3(0, 5.4f, 0), Quaternion.identity, 1.1f, open ? Color.white : new Color(0.75f, 0.75f, 0.75f));
                labels.Add(label.transform);
                pins[r.id] = pin;
            }
        }

        public void Focus(string regionId)
        {
            focused = regionId;
            var r = Catalog.Region(regionId);
            lookTarget = r != null ? Vector3.Lerp(new Vector3(0, 0, 10), RegionPos(r), 0.55f) : new Vector3(0, 0, 10);
            lookTarget.y = 0f;
        }

        // ============================ VIAJE ============================

        public void AnimateTravel(Region dest, Action onDone)
        {
            var start = Catalog.Region(origin) ?? Catalog.Region("bogota");
            if (dest == null) { onDone?.Invoke(); return; }
            from = start != null ? RegionPos(start) : Vector3.zero;
            to = RegionPos(dest);
            if (bus == null) bus = MakeChiva();
            bus.gameObject.SetActive(true);
            travelT = 0f;
            travelDone = onDone;
            foreach (var g in trail) if (g != null) Destroy(g);
            trail.Clear();
            Focus(dest.id);
            SFX.Play("jump", 0.6f);
        }

        Transform MakeChiva()
        {
            // la chiva: bus rural de madera pintado de colores, con parrilla en el techo
            var c = HWorld.Group(transform, "Chiva");
            HWorld.Box(c, "Cuerpo", new Vector3(0, 0.9f, 0), new Vector3(1.6f, 1.2f, 3.2f), HWorld.M("#f2c230", 0.4f));
            HWorld.Box(c, "Franja", new Vector3(0, 0.75f, 0), new Vector3(1.62f, 0.25f, 3.22f), HWorld.M("#c8102e", 0.4f));
            HWorld.Box(c, "Franja2", new Vector3(0, 1.05f, 0), new Vector3(1.62f, 0.15f, 3.22f), HWorld.M("#1f4ea8", 0.4f));
            HWorld.Box(c, "Techo", new Vector3(0, 1.6f, 0), new Vector3(1.7f, 0.12f, 3.3f), HWorld.M("#2fbf71", 0.4f));
            HWorld.Box(c, "Bultos", new Vector3(0, 1.85f, -0.4f), new Vector3(1.2f, 0.4f, 1.6f), HWorld.M("#b89a6a", 0.1f));
            HWorld.Box(c, "Capo", new Vector3(0, 0.6f, 1.9f), new Vector3(1.3f, 0.6f, 0.7f), HWorld.M("#c8102e", 0.4f));
            for (int i = 0; i < 4; i++)
            {
                var w = HWorld.Cyl(c, "Rueda", new Vector3(i < 2 ? -0.8f : 0.8f, 0.3f, i % 2 == 0 ? 1.3f : -1.1f), 0.6f, 0.25f, HWorld.M("#111111", 0.3f));
                w.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            c.localScale = Vector3.one * 1.2f;
            return c;
        }

        // ============================ BUCLE ============================

        public void Tick(float dt)
        {
            t += dt;
            var cam = StoryGame.I != null ? StoryGame.I.cam : Camera.main;
            if (travelT >= 0f)
            {
                travelT += dt / 3f;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(travelT));
                Vector3 p = Vector3.Lerp(from, to, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 6f + 0.6f);
                Vector3 dir = to - from;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f) bus.rotation = Quaternion.LookRotation(dir);
                bus.position = p;
                if (trail.Count < Mathf.FloorToInt(k * 24f))
                    trail.Add(HWorld.Ball(transform, "Huella", p - Vector3.up * 0.4f, 0.5f, FGMaterials.Emissive("huellaChiva", HConfig.Gold, 1f)));
                lookTarget = Vector3.Lerp(lookTarget, new Vector3(p.x, 0, p.z), Mathf.Min(1f, dt * 3f));
                if (travelT >= 1f)
                {
                    travelT = -1f;
                    var done = travelDone;
                    travelDone = null;
                    done?.Invoke();
                    return; // el mapa se destruye al llegar
                }
            }
            look = Vector3.Lerp(look, lookTarget, Mathf.Min(1f, dt * 2.5f));
            if (cam != null)
            {
                Vector3 drift = new Vector3(Mathf.Sin(t * 0.15f) * 3f, 0, Mathf.Cos(t * 0.12f) * 2f);
                float zoom = travelT >= 0f ? 0.75f : 1f;
                cam.transform.position = look + CamOffset * zoom + drift;
                cam.transform.LookAt(look);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 50f, Mathf.Min(1f, dt * 3f));
                foreach (var l in labels) if (l != null) l.rotation = Quaternion.LookRotation(l.position - cam.transform.position);
            }
            foreach (var kv in pins)
            {
                if (kv.Value == null) continue;
                float s = kv.Key == focused ? 1.35f + Mathf.Sin(t * 4f) * 0.08f : 1f;
                kv.Value.localScale = Vector3.Lerp(kv.Value.localScale, Vector3.one * s, Mathf.Min(1f, dt * 8f));
            }
        }
    }
}
