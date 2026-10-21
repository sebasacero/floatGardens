// Finca cafetera de cada región: ladera andina con surcos de café, sombrío de guamos y plátanos, casa campesina,
// beneficiadero, marquesina, Willys, mula, recolectores y el mentor. El estilo (FincaStyle) cambia el paisaje:
// palmas de cera, volcanes, picos nevados, río y pueblo colonial.
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class FincaBuilder
    {
        const float Size = 140f, Cell = 2f;
        static FincaStyle st;
        static float seedX, seedZ;
        static readonly Vector3 HousePos = new Vector3(0, 0, 8);
        const float PadRadius = 15f;
        static float padY;

        static float RawHeight(float x, float z)
        {
            float slope = st.slope;
            float h = slope * 0.3f * (z + 70f);                                  // la montaña sube hacia el norte
            h += Mathf.Sin(x * 0.045f + seedX) * 4f * slope;                       // lomas
            h += (Mathf.PerlinNoise(x * 0.03f + seedX, z * 0.03f + seedZ) - 0.5f) * 8f * slope;
            if (st.river) h -= 6f * Mathf.Exp(-((z + 62f) * (z + 62f)) / 60f);   // cañada del río
            return h;
        }

        /// <summary>Altura del terreno con la explanada de la casa y el camino aplanados.</summary>
        public static float Height(float x, float z)
        {
            float h = RawHeight(x, z);
            float d = new Vector2(x - HousePos.x, z - HousePos.z).magnitude;
            float pad = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(PadRadius * 0.6f, PadRadius, d));
            h = Mathf.Lerp(h, padY, pad);
            // camino desde el paradero: suaviza las lomas laterales
            if (z < HousePos.z && Mathf.Abs(x - PathX(z)) < 4f)
            {
                float k = 1f - Mathf.Abs(x - PathX(z)) / 4f;
                float along = RawHeight(PathX(z), z);
                h = Mathf.Lerp(h, Mathf.Lerp(along, padY, Mathf.InverseLerp(-55f, HousePos.z - PadRadius * 0.5f, z)), k * 0.8f);
            }
            return h;
        }

        static float PathX(float z) => Mathf.Sin(z * 0.08f) * 3f;

        public static SceneInfo Build(Transform root, Region r, HistoriaSave s)
        {
            st = r.style ?? new FincaStyle();
            seedX = (r.id.GetHashCode() & 0xff) * 0.37f;
            seedZ = ((r.id.GetHashCode() >> 8) & 0xff) * 0.23f;
            padY = 0f;
            padY = RawHeight(HousePos.x, HousePos.z);
            var rng = new System.Random(r.id.GetHashCode());

            BuildTerrain(root);
            BuildCoffee(root, rng, r);
            if (st.shadeTrees) BuildShade(root, rng);
            if (st.plantains) BuildPlantains(root, rng);
            if (st.waxPalms) BuildWaxPalms(root, rng);
            BuildBackdrop(root, rng);
            BuildHouse(root);
            BuildBeneficio(root);
            if (st.jeep) BuildJeep(root, new Vector3(-7f, 0, -14f));
            if (st.mule) BuildMule(root, new Vector3(9f, 0, -10f));
            if (st.colonialTown) BuildTown(root, rng);
            BuildPeople(root, r, rng);
            Vector3 spawn = BuildStop(root, r);

            RenderSettings.fogColor = st.fog;
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = st.fogEnd;
            RenderSettings.ambientSkyColor = Color.Lerp(st.sky, Color.white, 0.4f);
            return new SceneInfo { spawn = spawn, yaw = 0f, altitude = r.Altitude };
        }

        static Vector3 On(float x, float z, float up = 0f) => new Vector3(x, Height(x, z) + up, z);

        // ============================ TERRENO ============================

        static void BuildTerrain(Transform root)
        {
            int n = Mathf.RoundToInt(Size / Cell);
            var verts = new Vector3[(n + 1) * (n + 1)];
            var uvs = new Vector2[verts.Length];
            for (int i = 0; i <= n; i++)
                for (int j = 0; j <= n; j++)
                {
                    float x = -Size / 2 + j * Cell, z = -Size / 2 + i * Cell;
                    verts[i * (n + 1) + j] = new Vector3(x, Height(x, z), z);
                    uvs[i * (n + 1) + j] = new Vector2(x / 6f, z / 6f);
                }
            var tris = new int[n * n * 6];
            int t = 0;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    int a = i * (n + 1) + j, b = a + 1, c = a + n + 1, d = c + 1;
                    tris[t++] = a; tris[t++] = c; tris[t++] = b;
                    tris[t++] = b; tris[t++] = c; tris[t++] = d;
                }
            var mesh = new Mesh { name = "Ladera" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var go = new GameObject("Ladera");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = FGMaterials.Lit("finca" + ColorUtility.ToHtmlStringRGB(st.ground), st.ground, FGTextures.Grass, 0.05f);
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.isStatic = true;

            // camino de tierra
            var path = new MeshBuilder();
            for (float z = -56f; z < HousePos.z - 6f; z += 2f)
            {
                float x0 = PathX(z), x1 = PathX(z + 2f);
                Vector3 a = On(x0 - 1.2f, z, 0.06f), b = On(x0 + 1.2f, z, 0.06f), c = On(x1 + 1.2f, z + 2f, 0.06f), d = On(x1 - 1.2f, z + 2f, 0.06f);
                path.Quad(a, b, c, d, Vector3.up, new Vector2(0, z / 4f), new Vector2(1, (z + 2f) / 4f));
            }
            path.Build("Camino", FGMaterials.Lit("caminoTierra", new Color(0.55f, 0.4f, 0.28f), FGTextures.Concrete, 0.05f), root, false);

            if (st.river)
            {
                var water = FGMaterials.Lit("rioFinca", new Color(0.3f, 0.55f, 0.7f), FGTextures.Water, 0.9f);
                float y = RawHeight(0, -62f) - 3.5f;
                HWorld.Box(root, "Rio", new Vector3(0, y, -62f), new Vector3(Size, 0.2f, 7f), water, false, false);
            }
        }

        // ============================ CAFÉ Y SOMBRÍO ============================

        static bool Blocked(float x, float z)
        {
            if (new Vector2(x - HousePos.x, z - HousePos.z).magnitude < PadRadius + 2f) return true;
            if (z < HousePos.z && Mathf.Abs(x - PathX(z)) < 3.2f) return true;
            if (z < -50f) return true;
            return false;
        }

        static void BuildCoffee(Transform root, System.Random rng, Region r)
        {
            var leaves = new MeshBuilder();
            var cherries = new MeshBuilder();
            var ripe = new MeshBuilder();
            var cone = MeshBuilder.Cone(6, 0.9f, 1.4f);
            var berry = MeshBuilder.Cone(4, 0.07f, 0.13f);
            for (float z = -46f; z < 62f; z += 3.2f)
            {
                for (float x = -64f; x < 64f; x += 2.2f)
                {
                    float jx = x + (float)(rng.NextDouble() - 0.5) * 0.6f;
                    if (Blocked(jx, z) || rng.NextDouble() < 0.12) continue;
                    float hgt = 1.3f + (float)rng.NextDouble() * 0.6f;
                    Vector3 p = On(jx, z);
                    float yaw = (float)rng.NextDouble() * 360f;
                    leaves.Add(cone, Matrix4x4.TRS(p, Quaternion.Euler(0, yaw, 0), new Vector3(1f, hgt * 0.7f, 1f)));
                    leaves.Add(cone, Matrix4x4.TRS(p + Vector3.up * hgt * 0.45f, Quaternion.Euler(0, yaw + 30f, 0), new Vector3(0.75f, hgt * 0.6f, 0.75f)));
                    for (int k = 0; k < 4; k++)
                    {
                        float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                        Vector3 cp = p + new Vector3(Mathf.Cos(a) * 0.62f, 0.4f + (float)rng.NextDouble() * hgt * 0.6f, Mathf.Sin(a) * 0.62f);
                        (k < 3 ? ripe : cherries).Add(berry, Matrix4x4.TRS(cp, Quaternion.identity, Vector3.one));
                    }
                }
            }
            leaves.Build("Cafetales", FGMaterials.Lit("cafetal" + ColorUtility.ToHtmlStringRGB(st.foliage), st.foliage, null, 0.55f), root);
            var v = Catalog.Variety(r.giftVariety);
            Color cherry = v != null ? v.cherryColor : HConfig.Cherry;
            ripe.Build("CerezasMaduras", FGMaterials.Lit("cereza" + ColorUtility.ToHtmlStringRGB(cherry), cherry, null, 0.7f), root, false);
            cherries.Build("CerezasVerdes", HWorld.M("#7fb03c", 0.5f), root, false);
        }

        static void BuildShade(Transform root, System.Random rng)
        {
            var trunks = new MeshBuilder();
            var crowns = new MeshBuilder();
            var cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            var sph = MeshBuilder.Primitive(PrimitiveType.Sphere);
            for (int i = 0; i < 26; i++)
            {
                float x = -62f + (float)rng.NextDouble() * 124f, z = -44f + (float)rng.NextDouble() * 104f;
                if (Blocked(x, z)) continue;
                Vector3 p = On(x, z);
                float h = 7f + (float)rng.NextDouble() * 5f;
                trunks.Add(cyl, Matrix4x4.TRS(p + Vector3.up * h / 2f, Quaternion.identity, new Vector3(0.45f, h / 2f, 0.45f)));
                // el guamo: copa ancha y plana
                crowns.Add(sph, Matrix4x4.TRS(p + Vector3.up * h, Quaternion.Euler(0, (float)rng.NextDouble() * 90f, 0), new Vector3(8f + (float)rng.NextDouble() * 3f, 2.4f, 7f)));
            }
            trunks.Build("Guamos", HWorld.M("#5a4632", 0.1f), root);
            crowns.Build("CopasGuamos", FGMaterials.Lit("copaGuamo" + ColorUtility.ToHtmlStringRGB(st.foliage), Color.Lerp(st.foliage, new Color(0.45f, 0.6f, 0.25f), 0.35f), null, 0.15f), root);
        }

        static void BuildPlantains(Transform root, System.Random rng)
        {
            var stems = new MeshBuilder();
            var leaves = new MeshBuilder();
            var cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            var cube = MeshBuilder.Primitive(PrimitiveType.Cube);
            for (int i = 0; i < 20; i++)
            {
                float x = -60f + (float)rng.NextDouble() * 120f, z = -42f + (float)rng.NextDouble() * 60f;
                if (Blocked(x, z)) continue;
                Vector3 p = On(x, z);
                stems.Add(cyl, Matrix4x4.TRS(p + Vector3.up * 1.8f, Quaternion.identity, new Vector3(0.35f, 1.8f, 0.35f)));
                for (int k = 0; k < 6; k++)
                {
                    float a = k * 60f + (float)rng.NextDouble() * 20f;
                    var rot = Quaternion.Euler(0, a, 0) * Quaternion.Euler(-35f, 0, 0);
                    leaves.Add(cube, Matrix4x4.TRS(p + Vector3.up * 3.6f + rot * new Vector3(0, 0, 1.2f), rot, new Vector3(0.8f, 0.04f, 2.4f)));
                }
            }
            stems.Build("Platanos", HWorld.M("#6f8a3a", 0.2f), root);
            leaves.Build("HojasPlatano", HWorld.M("#5fa83a", 0.35f), root);
        }

        static void BuildWaxPalms(Transform root, System.Random rng)
        {
            // la palma de cera del Quindío, árbol nacional: altísima y delgada, en la parte alta, sobre la franja cafetera
            var trunks = new MeshBuilder();
            var tufts = new MeshBuilder();
            var cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            var tuft = MeshBuilder.Cone(7, 3.2f, 2.6f);
            for (int i = 0; i < 14; i++)
            {
                float x = -60f + (float)rng.NextDouble() * 120f, z = 48f + (float)rng.NextDouble() * 20f;
                Vector3 p = On(x, z);
                float h = 26f + (float)rng.NextDouble() * 12f;
                trunks.Add(cyl, Matrix4x4.TRS(p + Vector3.up * h / 2f, Quaternion.identity, new Vector3(0.5f, h / 2f, 0.5f)));
                tufts.Add(tuft, Matrix4x4.TRS(p + Vector3.up * (h + 1.6f), Quaternion.Euler(180, 0, 0), Vector3.one));
            }
            trunks.Build("PalmasDeCera", HWorld.M("#d8d2c0", 0.2f), root);
            tufts.Build("CopasPalmas", HWorld.M("#3d6b3a", 0.2f), root);
        }

        static void BuildBackdrop(Transform root, System.Random rng)
        {
            var hills = new MeshBuilder();
            for (int i = 0; i < 9; i++)
            {
                float x = -260f + i * 65f + (float)rng.NextDouble() * 20f;
                hills.Add(MeshBuilder.Cone(9, 70f + (float)rng.NextDouble() * 30f, 60f + (float)rng.NextDouble() * 50f), Matrix4x4.Translate(new Vector3(x, -10f, 170f + (float)rng.NextDouble() * 60f)));
                hills.Add(MeshBuilder.Cone(9, 60f, 40f + (float)rng.NextDouble() * 30f), Matrix4x4.Translate(new Vector3(x + 30f, -10f, -160f - (float)rng.NextDouble() * 40f)));
            }
            hills.Build("Montañas", FGMaterials.Lit("montanaLejana" + ColorUtility.ToHtmlStringRGB(st.foliage), Color.Lerp(st.foliage, st.fog, 0.35f), null, 0.05f), root, false);
            if (st.volcano)
            {
                var v = HWorld.Group(root, "Volcan", new Vector3(120f, -10f, 320f));
                HWorld.Cone(v, "Cono", Vector3.zero, 150f, 150f, HWorld.M("#5b5d58", 0.05f), 10);
                HWorld.Cone(v, "Nieve", new Vector3(0, 112f, 0), 38f, 38f, HWorld.M("#eef1f3", 0.4f), 10);
                for (int k = 0; k < 4; k++) HWorld.Ball(v, "Humo", new Vector3(k * 6f, 156f + k * 9f, k * 4f), 14f + k * 5f, FGMaterials.Transparent("humoVolcan", new Color(0.85f, 0.85f, 0.85f, 0.35f)));
            }
            if (st.snowPeaks)
            {
                var peaks = HWorld.Group(root, "Nevados", new Vector3(-80f, -10f, 360f));
                for (int k = 0; k < 4; k++)
                {
                    var p = new Vector3(k * 70f, 0, (k % 2) * 30f);
                    HWorld.Cone(peaks, "Pico", p, 80f, 160f + k * 15f, HWorld.M("#6b6e72", 0.05f), 8);
                    HWorld.Cone(peaks, "Nieve", p + Vector3.up * (110f + k * 10f), 26f, 52f + k * 5f, HWorld.M("#f4f6f8", 0.5f), 8);
                }
            }
        }

        // ============================ CONSTRUCCIONES ============================

        static void BuildHouse(Transform root)
        {
            var g = HWorld.Group(root, "CasaDeFinca", new Vector3(HousePos.x, padY, HousePos.z + 4f), 180f);
            var white = FGMaterials.Lit("casaFinca", new Color(0.96f, 0.95f, 0.9f), FGTextures.Stucco, 0.1f);
            var roof = FGMaterials.Lit("tejaFinca", new Color(0.72f, 0.33f, 0.22f), FGTextures.Roof, 0.15f);
            Color[] trims = { FGMaterials.Hex("#c8102e"), FGMaterials.Hex("#1f4ea8"), FGMaterials.Hex("#2f8f4e"), FGMaterials.Hex("#f2b705") };
            var trim = FGMaterials.Lit("marcoFinca" + st.temperatureC, trims[Mathf.Abs(Mathf.RoundToInt(st.temperatureC)) % trims.Length], null, 0.4f);
            HWorld.Block(g, "Muros", Vector3.zero, new Vector3(11f, 3.6f, 7f), white);
            HWorld.Block(g, "Zocalo", new Vector3(0, 0, 0), new Vector3(11.05f, 0.7f, 7.05f), trim, false);
            var r1 = HWorld.Box(g, "Tejado", new Vector3(0, 4.3f, 2.1f), new Vector3(13f, 0.25f, 5.6f), roof, false);
            r1.transform.localRotation = Quaternion.Euler(-22, 0, 0);
            var r2 = HWorld.Box(g, "Tejado", new Vector3(0, 4.3f, -2.1f), new Vector3(13f, 0.25f, 5.6f), roof, false);
            r2.transform.localRotation = Quaternion.Euler(22, 0, 0);
            // corredor con columnas, puerta y ventanas de colores, materas
            HWorld.Box(g, "Corredor", new Vector3(0, 0.1f, 4.5f), new Vector3(11f, 0.2f, 2.2f), HWorld.M("#a0522d", 0.2f), true);
            for (int i = -2; i <= 2; i++) HWorld.Cyl(g, "Columna", new Vector3(i * 2.6f, 1.8f, 5.4f), 0.22f, 3.4f, HWorld.M("#5b3a1e", 0.2f));
            HWorld.Box(g, "Puerta", new Vector3(0, 1.2f, 3.52f), new Vector3(1.3f, 2.4f, 0.06f), trim, false, false);
            HWorld.Box(g, "Ventana", new Vector3(-3.3f, 1.8f, 3.52f), new Vector3(1.4f, 1.2f, 0.06f), trim, false, false);
            HWorld.Box(g, "Ventana", new Vector3(3.3f, 1.8f, 3.52f), new Vector3(1.4f, 1.2f, 0.06f), trim, false, false);
            Color[] flowers = { FGMaterials.Hex("#e63973"), FGMaterials.Hex("#f2b705"), FGMaterials.Hex("#ff7a1a"), FGMaterials.Hex("#b04fd1") };
            for (int i = 0; i < 6; i++)
            {
                var pot = new Vector3(-5f + i * 2f, 2.9f, 5.4f);
                HWorld.Cyl(g, "Matera", pot, 0.4f, 0.3f, HWorld.M("#b5562b", 0.2f));
                HWorld.Ball(g, "Flores", pot + Vector3.up * 0.3f, 0.5f, FGMaterials.Lit("flor" + i % 4, flowers[i % 4], null, 0.3f));
            }
        }

        static void BuildBeneficio(Transform root)
        {
            // beneficiadero: despulpadora y tanques bajo techo
            var b = HWorld.Group(root, "Beneficiadero", new Vector3(-15f, padY, HousePos.z));
            var post = HWorld.M("#5b3a1e", 0.2f);
            for (int i = 0; i < 4; i++) HWorld.Cyl(b, "Poste", new Vector3(i < 2 ? -3f : 3f, 1.6f, i % 2 == 0 ? -2.5f : 2.5f), 0.25f, 3.2f, post, true);
            HWorld.Box(b, "Techo", new Vector3(0, 3.3f, 0), new Vector3(7.4f, 0.2f, 6.2f), HWorld.M("#8f9aa3", 0.5f), false);
            HWorld.Block(b, "Tanque", new Vector3(-1.5f, 0, 0), new Vector3(2.2f, 1.0f, 2.4f), HWorld.M("#9a8f80", 0.2f));
            HWorld.Block(b, "Tanque", new Vector3(1.5f, 0, 0), new Vector3(2.2f, 1.0f, 2.4f), HWorld.M("#9a8f80", 0.2f));
            HWorld.Block(b, "Despulpadora", new Vector3(0, 0, -2f), new Vector3(1.0f, 1.3f, 0.9f), HWorld.M("#c0392b", 0.4f));

            // marquesina: secador con techo plástico curvo y el pergamino en las camas
            var m = HWorld.Group(root, "Marquesina", new Vector3(15f, padY, HousePos.z - 1f));
            var plastic = FGMaterials.Transparent("plasticoMarquesina", new Color(0.92f, 0.95f, 0.97f, 0.35f), true);
            for (int i = 0; i < 3; i++)
            {
                var arc = HWorld.Box(m, "Techo", new Vector3((i - 1) * 2.4f, 2.6f - Mathf.Abs(i - 1) * 0.4f, 0), new Vector3(2.5f, 0.06f, 8f), plastic, false, false);
                arc.transform.localRotation = Quaternion.Euler(0, 0, (i - 1) * -18f);
            }
            for (int i = 0; i < 4; i++) HWorld.Cyl(m, "Poste", new Vector3(i < 2 ? -3.4f : 3.4f, 1.1f, i % 2 == 0 ? -3.8f : 3.8f), 0.18f, 2.2f, post, true);
            HWorld.Block(m, "Cama", new Vector3(0, 0, 0), new Vector3(6.2f, 0.8f, 7f), HWorld.M("#6b4f2a", 0.2f));
            HWorld.Box(m, "Pergamino", new Vector3(0, 0.83f, 0), new Vector3(6f, 0.06f, 6.8f), HWorld.M("#d8c08f", 0.1f), false, false);
        }

        static void BuildJeep(Transform root, Vector3 pos)
        {
            var j = HWorld.Group(root, "WillysYipao", On(pos.x, pos.z), 25f);
            var body = HWorld.M("#2f6b8f", 0.4f);
            HWorld.Box(j, "Chasis", new Vector3(0, 0.9f, 0), new Vector3(1.6f, 0.7f, 3.4f), body, true);
            HWorld.Box(j, "Capo", new Vector3(0, 1.4f, 1.15f), new Vector3(1.4f, 0.35f, 1.1f), body, false);
            HWorld.Box(j, "Parabrisas", new Vector3(0, 1.75f, 0.55f), new Vector3(1.45f, 0.6f, 0.05f), FGMaterials.Transparent("vidrioJeep", new Color(0.8f, 0.9f, 1f, 0.35f), true), false, false);
            for (int i = 0; i < 4; i++)
            {
                var w = HWorld.Cyl(j, "Rueda", new Vector3(i < 2 ? -0.85f : 0.85f, 0.45f, i % 2 == 0 ? 1.1f : -1.1f), 0.9f, 0.3f, HWorld.M("#151515", 0.3f));
                w.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            // el yipao: bultos de café y racimos de plátano hasta el tope
            var sack = FGMaterials.Lit("costalYipao", new Color(0.72f, 0.6f, 0.42f), FGTextures.Warehouse, 0.05f);
            for (int i = 0; i < 6; i++) HWorld.Box(j, "Bulto", new Vector3((i % 2) * 0.7f - 0.35f, 1.55f + (i / 4) * 0.5f, -0.6f - (i / 2 % 2) * 0.8f), new Vector3(0.65f, 0.45f, 0.75f), sack, false);
            HWorld.Box(j, "Platanos", new Vector3(0, 2.3f, -1.2f), new Vector3(0.9f, 0.4f, 0.6f), HWorld.M("#9ccc3a", 0.3f), false, false);
        }

        static void BuildMule(Transform root, Vector3 pos)
        {
            var m = HWorld.Group(root, "Mula", On(pos.x, pos.z), -40f);
            var coat = HWorld.M("#5a3d2b", 0.15f);
            HWorld.Box(m, "Cuerpo", new Vector3(0, 1.25f, 0), new Vector3(0.7f, 0.75f, 1.7f), coat, true);
            for (int i = 0; i < 4; i++) HWorld.Box(m, "Pata", new Vector3(i < 2 ? -0.24f : 0.24f, 0.45f, i % 2 == 0 ? 0.65f : -0.65f), new Vector3(0.16f, 0.9f, 0.16f), coat);
            var neck = HWorld.Box(m, "Cuello", new Vector3(0, 1.65f, 0.95f), new Vector3(0.35f, 0.8f, 0.35f), coat);
            neck.transform.localRotation = Quaternion.Euler(35, 0, 0);
            HWorld.Box(m, "Cabeza", new Vector3(0, 2.0f, 1.35f), new Vector3(0.3f, 0.3f, 0.6f), coat);
            HWorld.Box(m, "Oreja", new Vector3(-0.1f, 2.25f, 1.2f), new Vector3(0.06f, 0.3f, 0.08f), coat);
            HWorld.Box(m, "Oreja", new Vector3(0.1f, 2.25f, 1.2f), new Vector3(0.06f, 0.3f, 0.08f), coat);
            var sack = FGMaterials.Lit("costalYipao", new Color(0.72f, 0.6f, 0.42f), FGTextures.Warehouse, 0.05f);
            HWorld.Box(m, "Carga", new Vector3(-0.5f, 1.35f, 0), new Vector3(0.4f, 0.6f, 0.9f), sack);
            HWorld.Box(m, "Carga", new Vector3(0.5f, 1.35f, 0), new Vector3(0.4f, 0.6f, 0.9f), sack);
        }

        static void BuildTown(Transform root, System.Random rng)
        {
            // pueblo de balcones de colores al otro lado de la cañada
            string[] walls = { "#f3e6c8", "#7fb3d5", "#e76f51", "#f1faee", "#9ccc65", "#ffd166" };
            string[] trims = { "#c8102e", "#1f4ea8", "#2f8f4e", "#f2b705", "#7b2cbf" };
            var roof = FGMaterials.Lit("tejaPueblo", new Color(0.72f, 0.36f, 0.24f), FGTextures.Roof, 0.15f);
            float gy = RawHeight(0f, -70f) - 6f;
            HWorld.Box(root, "SueloPueblo", new Vector3(8f, gy - 0.5f, -98f), new Vector3(140f, 1f, 34f), FGMaterials.Lit("finca" + ColorUtility.ToHtmlStringRGB(st.ground), st.ground, FGTextures.Grass, 0.05f), false, false);
            for (int i = 0; i < 9; i++)
            {
                var p = new Vector3(-40f + i * 10f, gy, -92f - (i % 2) * 6f);
                var h = HWorld.Group(root, "CasaPueblo", p);
                HWorld.Block(h, "Muros", Vector3.zero, new Vector3(8.5f, 6f, 7f), FGMaterials.Lit("pueblo" + walls[i % walls.Length], FGMaterials.Hex(walls[i % walls.Length]), FGTextures.Stucco, 0.1f), false);
                HWorld.Box(h, "Tejado", new Vector3(0, 6.6f, 0), new Vector3(9f, 0.5f, 7.6f), roof, false, false);
                HWorld.Box(h, "Balcon", new Vector3(0, 3.6f, 3.9f), new Vector3(6f, 0.9f, 0.7f), HWorld.M(trims[i % trims.Length], 0.3f), false, false);
            }
            var church = HWorld.Group(root, "Iglesia", new Vector3(55f, gy, -102f));
            HWorld.Block(church, "Nave", Vector3.zero, new Vector3(10f, 11f, 16f), HWorld.M("#f2efe6", 0.15f), false);
            HWorld.Block(church, "Torre", new Vector3(0, 0, 8f), new Vector3(4f, 20f, 4f), HWorld.M("#f2efe6", 0.15f), false);
        }

        // ============================ GENTE ============================

        static HumanoidStyle MentorStyle(Region r)
        {
            var s = new HumanoidStyle { shirt = FGMaterials.Hex("#f5f1e8"), pants = FGMaterials.Hex("#3b3b3b"), skin = FGMaterials.Hex("#b98a62"), hair = FGMaterials.Hex("#2a1a10") };
            switch (r.id)
            {
                case "quindio": s.shirt = FGMaterials.Hex("#d8434a"); s.apron = FGMaterials.Hex("#3d6b3a"); s.hair = FGMaterials.Hex("#4a3a30"); break;
                case "caldas": s.shirt = FGMaterials.Hex("#ffffff"); s.apron = FGMaterials.Hex("#e8f1f5"); break;
                case "risaralda": s.shirt = FGMaterials.Hex("#2f8f4e"); s.cap = FGMaterials.Hex("#222222"); break;
                case "tolima": s.shirt = FGMaterials.Hex("#7b2cbf"); s.apron = FGMaterials.Hex("#4a2c1d"); break;
                case "huila": s.shirt = FGMaterials.Hex("#1f4ea8"); s.hair = FGMaterials.Hex("#bdb6aa"); break;
                case "cauca": s.shirt = FGMaterials.Hex("#f2efe6"); s.apron = FGMaterials.Hex("#4a2c1d"); break;
                case "sierranevada": s.shirt = FGMaterials.Hex("#2d6a4f"); s.pants = FGMaterials.Hex("#5b4a3a"); break;
                case "santander": s.shirt = FGMaterials.Hex("#e9c46a"); break;
            }
            return s;
        }

        static void Dress(Humanoid h, Region r)
        {
            switch (r.id)
            {
                case "quindio":
                case "risaralda":
                    HWorld.Hat(h, FGMaterials.Hex("#e8dcb5"), Color.black);   // sombrero aguadeño
                    break;
                case "antioquia":
                    HWorld.Hat(h, FGMaterials.Hex("#e8dcb5"), Color.black);
                    HWorld.Bag(h, FGMaterials.Hex("#6b3e1f"));   // el carriel
                    break;
                case "cundinamarca":
                case "narino":
                case "cauca":
                    HWorld.Ruana(h, FGMaterials.Hex(r.id == "narino" ? "#7a2e2e" : r.id == "cauca" ? "#3d4a6b" : "#6b5a44"));
                    HWorld.Hat(h, FGMaterials.Hex("#d9c9a0"), FGMaterials.Hex("#3a2a1a"), 0.62f);
                    break;
                case "santander":
                case "nortesantander":
                case "tolima":
                case "huila":
                    HWorld.Hat(h, FGMaterials.Hex("#e8dcb5"), FGMaterials.Hex("#3a2a1a"), 0.66f);
                    break;
                case "sierranevada":
                    HWorld.Bag(h, FGMaterials.Hex("#c9a46a"));   // mochila tejida
                    HWorld.Hat(h, FGMaterials.Hex("#f4f1e8"), FGMaterials.Hex("#f4f1e8"), 0.5f);
                    break;
            }
        }

        static void BuildPeople(Transform root, Region r, System.Random rng)
        {
            string name = string.IsNullOrEmpty(r.mentorName) ? "Mentor" : r.mentorName;
            var mentor = HWorld.Npc(root, name, MentorStyle(r), new Vector3(HousePos.x + 2.5f, padY, HousePos.z - 4f), 180f);
            Dress(mentor, r);
            Interactable.Add(mentor.gameObject, "mentor", "Hablar con " + (string.IsNullOrEmpty(r.mentorName) ? "el mentor" : r.mentorName), 2.6f);

            // recolectores con su coco a la cintura
            for (int i = 0; i < 3; i++)
            {
                float x = -24f + i * 22f + (float)rng.NextDouble() * 4f, z = -18f + (float)rng.NextDouble() * 20f;
                if (Blocked(x, z)) x += 8f;
                var w = HWorld.Npc(root, "Recolector", new HumanoidStyle
                {
                    shirt = FGMaterials.Hex(i == 0 ? "#c0392b" : i == 1 ? "#2f6b8f" : "#e9c46a"), pants = FGMaterials.Hex("#2c2a27"), skin = FGMaterials.Hex(i == 1 ? "#8d5a3b" : "#b98a62"),
                    cap = i == 2 ? FGMaterials.Hex("#f5f1e8") : (Color?)null, hair = FGMaterials.Hex("#1a120c"),
                }, On(x, z), (float)rng.NextDouble() * 360f);
                if (i != 2) HWorld.Hat(w, FGMaterials.Hex("#e8dcb5"), Color.black, 0.6f);
                if (w.body != null) HWorld.Cyl(w.body, "Coco", new Vector3(0, 0.95f, 0.3f), 0.38f, 0.32f, HWorld.M("#a0784a", 0.2f));
            }
        }

        static Vector3 BuildStop(Transform root, Region r)
        {
            // paradero del bus y letrero del pueblo
            float z = -53f;
            float x = PathX(z);
            var stop = HWorld.Group(root, "Paradero", On(x + 3.5f, z));
            var wood = HWorld.M("#6b4423", 0.2f);
            HWorld.Cyl(stop, "Poste", new Vector3(-1.4f, 1.3f, 0), 0.18f, 2.6f, wood, true);
            HWorld.Cyl(stop, "Poste", new Vector3(1.4f, 1.3f, 0), 0.18f, 2.6f, wood, true);
            HWorld.Box(stop, "Techo", new Vector3(0, 2.7f, 0), new Vector3(3.6f, 0.15f, 2f), HWorld.M("#c8102e", 0.3f), false);
            HWorld.Box(stop, "Banca", new Vector3(0, 0.45f, -0.4f), new Vector3(2.6f, 0.1f, 0.5f), wood, true);
            Interactable.Add(stop.gameObject, "volver", "Volver al mapa (tomar el bus)", 2.6f);

            var sign = HWorld.Group(root, "Letrero", On(x - 3.5f, z + 3f), 0f);
            HWorld.Cyl(sign, "Poste", new Vector3(0, 1.2f, 0), 0.15f, 2.4f, wood);
            HWorld.Box(sign, "Tabla", new Vector3(0, 2.3f, 0), new Vector3(3.4f, 1.1f, 0.1f), HWorld.M("#2d5a3a", 0.2f), false, false);
            string alt = Mathf.RoundToInt(r.altitudeMin).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " – " +
                         Mathf.RoundToInt(r.altitudeMax).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " m s. n. m.";
            HWorld.Text3D(sign, r.town + "\n" + alt, new Vector3(0, 2.3f, -0.07f), Quaternion.identity, 0.22f, Color.white);
            Interactable.Add(sign.gameObject, r.id + "_cartel", "Leer el letrero", 2f);
            return On(x, z + 1.5f, 0.3f);
        }
    }
}
