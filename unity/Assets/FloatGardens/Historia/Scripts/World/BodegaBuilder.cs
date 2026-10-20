// La bodega de Float Gardens en Puente Aranda (Bogotá): huerta de balsas flotantes bajo luces LED, beneficio, tostadora,
// barra de AeroPress, PC, cama y el portón. Por las ventanas se ven los cerros orientales con Monserrate.
// También construye el centro histórico (La Candelaria) para la región "bogota".
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class BodegaBuilder
    {
        const float W = 26f, D = 18f, H = 7f;           // interior: x ∈ [-13, 13], z ∈ [-9, 9]
        static Transform plantsRoot;
        static readonly Light[] growLights = new Light[3];
        static readonly Material[] growMats = new Material[3];

        /// <summary>Posición de la balsa i (3 tanques × 4 balsas).</summary>
        static Vector3 RaftPos(int i) => new Vector3(-5.4f + (i % 4) * 2.3f, 0.86f, 2.6f + (i / 4) * 2.1f);

        public static SceneInfo Build(Transform root, HistoriaSave s)
        {
            FGTextures.Init();
            var brick = FGMaterials.Lit("bodegaLadrillo", Color.white, FGTextures.Brick, 0.08f);
            var concrete = FGMaterials.Lit("bodegaPiso", new Color(0.78f, 0.77f, 0.74f), FGTextures.Concrete, 0.35f);
            var steel = HWorld.M("#2b2f33", 0.5f);
            var wood = HWorld.M("#8a5a35", 0.25f);
            var darkWood = HWorld.M("#4e3220", 0.25f);
            var white = HWorld.M("#efeee8", 0.3f);

            // Piso, paredes y techo
            var floor = new MeshBuilder();
            floor.Top(0, 0, 0, W, D, 4f);
            floor.Build("Piso", concrete, root, false);
            HWorld.Box(root, "PisoColision", new Vector3(0, -0.25f, 0), new Vector3(W + 2, 0.5f, D + 2), concrete, true, false).GetComponent<MeshRenderer>().enabled = false;

            var walls = new MeshBuilder();
            // norte y occidente completas
            WallBox(walls, root, new Vector3(0, H / 2, D / 2 + 0.15f), new Vector3(W + 0.6f, H, 0.3f));
            WallBox(walls, root, new Vector3(-W / 2 - 0.15f, H / 2, 0), new Vector3(0.3f, H, D));
            // sur con el hueco del portón (5 m)
            WallBox(walls, root, new Vector3(-7.75f, H / 2, -D / 2 - 0.15f), new Vector3(10.5f, H, 0.3f));
            WallBox(walls, root, new Vector3(7.75f, H / 2, -D / 2 - 0.15f), new Vector3(10.5f, H, 0.3f));
            WallBox(walls, root, new Vector3(0, H - 1.25f, -D / 2 - 0.15f), new Vector3(5f, 2.5f, 0.3f));
            // oriente: muro bajo, dintel y columnas; entre columnas, ventanales hacia los cerros
            WallBox(walls, root, new Vector3(W / 2 + 0.15f, 0.6f, 0), new Vector3(0.3f, 1.2f, D));
            WallBox(walls, root, new Vector3(W / 2 + 0.15f, H - 0.9f, 0), new Vector3(0.3f, 1.8f, D));
            for (int i = 0; i < 5; i++) WallBox(walls, root, new Vector3(W / 2 + 0.15f, H / 2, -D / 2 + i * (D / 4f)), new Vector3(0.32f, H, 0.6f));
            walls.Build("Paredes", brick, root);
            var glass = FGMaterials.Transparent("bodegaVidrio", new Color(0.75f, 0.85f, 0.92f, 0.18f), true);
            HWorld.Box(root, "Ventanal", new Vector3(W / 2 + 0.1f, 3.5f, 0), new Vector3(0.05f, 4.0f, D), glass, true, false);
            for (int i = 0; i < 4; i++)
                HWorld.Box(root, "Marco", new Vector3(W / 2, 3.5f, -D / 2 + D / 8f + i * D / 4f), new Vector3(0.08f, 4f, 0.08f), steel);

            // Techo con cerchas y claraboyas
            var roof = new MeshBuilder();
            roof.Box(0, 0, H, W + 0.6f, 0.25f, D + 0.6f, 4f, 3f);
            roof.Build("Techo", HWorld.M("#3b3f44", 0.2f), root);
            var sky = FGMaterials.Emissive("bodegaClaraboya", new Color(0.82f, 0.9f, 1f), 1.4f);
            for (int i = -1; i <= 1; i++) HWorld.Box(root, "Claraboya", new Vector3(i * 7f, H - 0.02f, 0), new Vector3(2.4f, 0.05f, D - 2f), sky, false, false);
            for (int i = 0; i < 6; i++)
            {
                float x = -W / 2 + 2.2f + i * 4.3f;
                HWorld.Box(root, "Cercha", new Vector3(x, H - 0.45f, 0), new Vector3(0.18f, 0.25f, D), steel, false, false);
                HWorld.Box(root, "Tensor", new Vector3(x, H - 0.95f, 0), new Vector3(0.06f, 0.06f, D), steel, false, false);
            }

            BuildGarden(root, s);
            BuildBeneficio(root, wood, steel);
            BuildRoaster(root, steel, darkWood);
            BuildBar(root, wood, darkWood, steel, white);
            BuildDesk(root, wood, steel);
            BuildBed(root, darkWood);
            BuildDoor(root, steel);
            BuildDecor(root, wood, darkWood);
            BuildOutside(root);

            // Luces cálidas
            HWorld.Lamp(root, new Vector3(8, 4.5f, -5), new Color(1f, 0.82f, 0.6f), 11, 1.6f);
            HWorld.Lamp(root, new Vector3(-9, 4.5f, -4), new Color(1f, 0.82f, 0.6f), 11, 1.3f);
            HWorld.Lamp(root, new Vector3(-9, 4.5f, 5), new Color(1f, 0.85f, 0.65f), 10, 1.1f);
            HWorld.Lamp(root, new Vector3(9, 4f, 5), new Color(0.8f, 0.9f, 1f), 9, 1.0f);
            for (int i = 0; i < 3; i++)
            {
                var b = HWorld.Box(root, "Bombillo", new Vector3(i == 0 ? 8 : -9, 5.2f, i == 2 ? 5 : -4.5f), new Vector3(0.25f, 0.3f, 0.25f), FGMaterials.Emissive("bombillo", new Color(1f, 0.8f, 0.5f), 3f), false, false);
                HWorld.Box(root, "Cable", b.transform.localPosition + Vector3.up * 0.9f, new Vector3(0.02f, 1.5f, 0.02f), steel, false, false);
            }

            RenderSettings.fogColor = new Color32(0xb9, 0xc4, 0xcc, 255);
            RenderSettings.fogStartDistance = 30f;
            RenderSettings.fogEndDistance = 420f;
            return new SceneInfo { spawn = new Vector3(0, 0.05f, -6.8f), yaw = 0f, altitude = HConfig.BogotaAltitude };
        }

        static void WallBox(MeshBuilder mb, Transform root, Vector3 c, Vector3 size)
        {
            mb.Box(c.x, c.z, c.y - size.y / 2, size.x, size.y, size.z, 3f, 2.2f);
            var go = new GameObject("Muro");
            go.transform.SetParent(root, false);
            go.transform.localPosition = c;
            go.AddComponent<BoxCollider>().size = size;
        }

        // ============================ HUERTA FLOTANTE ============================

        static void BuildGarden(Transform root, HistoriaSave s)
        {
            var tankMat = HWorld.M("#1f2a33", 0.4f);
            var water = FGMaterials.Lit("bodegaAgua", new Color(0.3f, 0.55f, 0.75f), FGTextures.Water, 0.92f);
            var steel = HWorld.M("#2b2f33", 0.5f);
            for (int t = 0; t < 3; t++)
            {
                float z = 2.6f + t * 2.1f;
                HWorld.Block(root, "Tanque", new Vector3(-2, 0, z), new Vector3(9.6f, 0.75f, 1.4f), tankMat);
                HWorld.Box(root, "Agua", new Vector3(-2, 0.76f, z), new Vector3(9.3f, 0.02f, 1.15f), water, false, false);
                // barra LED encima
                var led = HWorld.Box(root, "LED", new Vector3(-2, 3.0f, z), new Vector3(9f, 0.08f, 0.25f), Mat(t), false, false);
                growMats[t] = led.GetComponent<MeshRenderer>().sharedMaterial;
                HWorld.Box(root, "Colgante", new Vector3(-6, 5f, z), new Vector3(0.03f, 4f, 0.03f), steel, false, false);
                HWorld.Box(root, "Colgante", new Vector3(2, 5f, z), new Vector3(0.03f, 4f, 0.03f), steel, false, false);
                growLights[t] = HWorld.Lamp(root, new Vector3(-2, 2.6f, z), new Color(1f, 0.55f, 0.85f), 4.5f, 1.2f);
            }
            // bomba de aire y tubería: las raíces del café necesitan oxígeno
            HWorld.Block(root, "BombaAire", new Vector3(3.6f, 0, 2.6f), new Vector3(0.5f, 0.45f, 0.4f), HWorld.M("#3d6b5a", 0.4f));
            HWorld.Box(root, "Manguera", new Vector3(3.2f, 0.5f, 4.7f), new Vector3(0.05f, 0.05f, 4.4f), HWorld.M("#9ccfe0", 0.3f), false, false);
            HWorld.Text3D(root, "HUERTA FLOTANTE\nbalsas hidropónicas · 2.600 m", new Vector3(-2, 3.6f, 1.6f), Quaternion.identity, 0.28f, HConfig.Gold);

            var station = HWorld.Group(root, "EstacionCultivo", new Vector3(-2, 0, 1.2f));
            Interactable.Add(station.gameObject, "cultivo", "Cuidar la huerta flotante", 2.6f);

            plantsRoot = HWorld.Group(root, "Plantas");
            RefreshPlants(s);
        }

        static Material Mat(int t) => FGMaterials.Instance(FGMaterials.Emissive("ledCultivo", new Color(1f, 0.45f, 0.85f), 2.6f));

        /// <summary>Redibuja las matas según el guardado (lo llaman el panel de la huerta y la tienda).</summary>
        public static void RefreshPlants(HistoriaSave s)
        {
            if (plantsRoot == null || s == null) return;
            for (int i = plantsRoot.childCount - 1; i >= 0; i--) Object.Destroy(plantsRoot.GetChild(i).gameObject);
            float light = Mathf.Clamp01(s.garden.lightHours / 12f);
            for (int t = 0; t < 3; t++)
            {
                if (growLights[t] != null) growLights[t].intensity = 1.2f * light;
                if (growMats[t] != null) FGMaterials.SetEmission(growMats[t], new Color(1f, 0.45f, 0.85f), 2.6f * light);
            }
            var raft = HWorld.M("#f4f4f0", 0.2f);
            var pot = HWorld.M("#202020", 0.3f);
            for (int i = 0; i < s.garden.rafts.Count && i < HConfig.MaxRafts; i++)
            {
                var p = s.garden.rafts[i];
                var slot = HWorld.Group(plantsRoot, "Balsa " + (i + 1), RaftPos(i));
                HWorld.Box(slot, "Icopor", new Vector3(0, 0.03f, 0), new Vector3(1.9f, 0.08f, 1.0f), raft, false, false);
                HWorld.Cyl(slot, "Canastilla", new Vector3(0, 0.1f, 0), 0.22f, 0.1f, pot);
                Plant(slot, p);
            }
        }

        static void Plant(Transform slot, Plant p)
        {
            if (p.Empty) return;
            var v = Catalog.Variety(p.variety);
            var stem = HWorld.M("#6b4f2a", 0.2f);
            var leaf = HWorld.M(p.roya ? "#5f7a2e" : "#1f6b2c", 0.65f);
            var bean = HWorld.M("#8a6a3d", 0.3f);
            switch (p.stage)
            {
                case PlantStage.Semilla:
                    HWorld.Ball(slot, "Semilla", new Vector3(0, 0.17f, 0), 0.07f, bean);
                    return;
                case PlantStage.Fosforito:
                    HWorld.Box(slot, "Tallo", new Vector3(0, 0.3f, 0), new Vector3(0.02f, 0.28f, 0.02f), stem, false, false);
                    HWorld.Ball(slot, "Cabeza", new Vector3(0, 0.46f, 0), 0.07f, bean);
                    return;
                case PlantStage.Chapola:
                    HWorld.Box(slot, "Tallo", new Vector3(0, 0.32f, 0), new Vector3(0.025f, 0.32f, 0.025f), stem, false, false);
                    HWorld.Box(slot, "Hoja", new Vector3(-0.08f, 0.48f, 0), new Vector3(0.14f, 0.015f, 0.1f), leaf, false, false);
                    HWorld.Box(slot, "Hoja", new Vector3(0.08f, 0.48f, 0), new Vector3(0.14f, 0.015f, 0.1f), leaf, false, false);
                    return;
            }
            float size = p.stage == PlantStage.Almacigo ? 0.45f : 1f;
            float hgt = 1.4f * size;
            HWorld.Box(slot, "Tronco", new Vector3(0, 0.15f + hgt * 0.35f, 0), new Vector3(0.05f * size + 0.02f, hgt * 0.7f, 0.05f * size + 0.02f), stem, false, false);
            for (int k = 0; k < 3; k++)
            {
                float y = 0.3f + hgt * (0.25f + k * 0.25f);
                float r = (0.55f - k * 0.12f) * size;
                HWorld.Ball(slot, "Follaje", new Vector3(0, y, 0), r, leaf).transform.localScale = new Vector3(r * 1.5f, r * 0.8f, r * 1.5f);
            }
            if (p.roya)
                for (int k = 0; k < 6; k++)
                    HWorld.Box(slot, "Roya", new Vector3(Mathf.Cos(k) * 0.35f * size, 0.4f + hgt * (0.2f + k * 0.1f), Mathf.Sin(k) * 0.35f * size), Vector3.one * 0.06f, HWorld.M("#e07a1f", 0.1f), false, false);
            if (p.stage < PlantStage.Floracion) return;

            Material dots;
            switch (p.stage)
            {
                case PlantStage.Floracion: dots = FGMaterials.Emissive("florCafe", Color.white, 0.4f); break;
                case PlantStage.CerezaVerde: dots = HWorld.M("#6fae3a", 0.5f); break;
                case PlantStage.CerezaMadura:
                    var c = v != null ? v.cherryColor : HConfig.Cherry;
                    dots = FGMaterials.Lit("cereza" + ColorUtility.ToHtmlStringRGB(c), c, null, 0.7f);
                    break;
                default: dots = HWorld.M("#4a1420", 0.6f); break;
            }
            var rng = new System.Random(slot.GetSiblingIndex() * 31 + (int)p.stage);
            for (int k = 0; k < 16; k++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float y = 0.5f + (float)rng.NextDouble() * hgt * 0.8f;
                float rr = 0.32f + (float)rng.NextDouble() * 0.2f;
                HWorld.Box(slot, "Fruto", new Vector3(Mathf.Cos(a) * rr, y, Mathf.Sin(a) * rr), Vector3.one * (p.stage == PlantStage.Floracion ? 0.07f : 0.09f), dots, false, false);
            }
            if (p.broca)
                for (int k = 0; k < 4; k++)
                    HWorld.Box(slot, "Broca", new Vector3(Mathf.Sin(k * 2f) * 0.4f, 0.7f + k * 0.18f, Mathf.Cos(k * 2f) * 0.4f), Vector3.one * 0.04f, HWorld.M("#111111", 0.3f), false, false);
        }

        // ============================ ESTACIONES ============================

        static void BuildBeneficio(Transform root, Material wood, Material steel)
        {
            var g = HWorld.Group(root, "Beneficio", new Vector3(-10, 0, 5));
            // despulpadora manual
            HWorld.Block(g, "Base", new Vector3(-1.6f, 0, 1.6f), new Vector3(0.8f, 0.9f, 0.7f), wood);
            HWorld.Box(g, "Tolva", new Vector3(-1.6f, 1.15f, 1.6f), new Vector3(0.7f, 0.4f, 0.6f), HWorld.M("#a8b0b5", 0.6f));
            HWorld.Cyl(g, "Manivela", new Vector3(-1.15f, 0.95f, 1.6f), 0.35f, 0.04f, steel).transform.localRotation = Quaternion.Euler(0, 0, 90);
            // tanques de fermentación
            HWorld.Cyl(g, "TanqueFermento", new Vector3(0.2f, 0.55f, 1.8f), 0.9f, 1.1f, HWorld.M("#3c6e8f", 0.4f), true);
            HWorld.Cyl(g, "TanqueFermento", new Vector3(1.3f, 0.55f, 1.8f), 0.9f, 1.1f, HWorld.M("#3c6e8f", 0.4f), true);
            // camas africanas con pergamino
            var parch = HWorld.M("#d8c08f", 0.1f);
            for (int i = 0; i < 2; i++)
            {
                float z = -0.4f - i * 1.3f;
                HWorld.Block(g, "Cama", new Vector3(0, 0, z), new Vector3(3.6f, 0.9f, 1.0f), wood, true);
                HWorld.Box(g, "Pergamino", new Vector3(0, 0.92f, z), new Vector3(3.4f, 0.05f, 0.85f), parch, false, false);
            }
            HWorld.Text3D(g, "BENEFICIO", new Vector3(0, 2.6f, 2.6f), Quaternion.identity, 0.32f, HConfig.Cream);
            var st = HWorld.Group(root, "EstacionBeneficio", new Vector3(-8.2f, 0, 3.2f));
            Interactable.Add(st.gameObject, "beneficio", "Beneficio: despulpar, fermentar y secar", 2.2f);
        }

        static void BuildRoaster(Transform root, Material steel, Material darkWood)
        {
            var g = HWorld.Group(root, "Tostadora", new Vector3(-10.2f, 0, -4.2f));
            HWorld.Block(g, "Cuerpo", Vector3.zero, new Vector3(1.0f, 1.0f, 0.9f), HWorld.M("#1b1b1b", 0.5f));
            var drum = HWorld.Cyl(g, "Tambor", new Vector3(0, 1.35f, 0), 0.75f, 0.9f, HWorld.M("#262626", 0.7f));
            drum.transform.localRotation = Quaternion.Euler(90, 0, 0);
            HWorld.Box(g, "Frente", new Vector3(0, 1.35f, -0.48f), new Vector3(0.5f, 0.5f, 0.05f), HWorld.M("#c9a24a", 0.8f));
            HWorld.Cone(g, "Tolva", new Vector3(0, 1.8f, 0), 0.3f, -0.45f, HWorld.M("#9aa2a8", 0.7f), 8).transform.localPosition = new Vector3(0, 2.3f, 0);
            HWorld.Cyl(g, "Chimenea", new Vector3(0.35f, 3.4f, 0.3f), 0.18f, 3.2f, steel);
            HWorld.Cyl(g, "Enfriador", new Vector3(0, 0.75f, -1.1f), 1.1f, 0.15f, HWorld.M("#8d969c", 0.6f), true);
            HWorld.Ball(g, "Granos", new Vector3(0, 0.83f, -1.1f), 0.8f, HWorld.M("#4a2c1d", 0.4f)).transform.localScale = new Vector3(0.9f, 0.08f, 0.9f);
            HWorld.Block(g, "Costal", new Vector3(1.1f, 0, 0.3f), new Vector3(0.6f, 0.8f, 0.45f), FGMaterials.Lit("costal", new Color(0.72f, 0.6f, 0.42f), FGTextures.Warehouse, 0.05f));
            var st = HWorld.Group(root, "EstacionTostadora", new Vector3(-8.6f, 0, -4.2f));
            Interactable.Add(st.gameObject, "tostadora", "Tostar café", 2f);
        }

        static void BuildBar(Transform root, Material wood, Material darkWood, Material steel, Material white)
        {
            var g = HWorld.Group(root, "Barra", new Vector3(8, 0, -6.2f));
            HWorld.Block(g, "Mostrador", Vector3.zero, new Vector3(5.2f, 1.0f, 0.9f), darkWood);
            HWorld.Box(g, "Encimera", new Vector3(0, 1.03f, 0), new Vector3(5.4f, 0.06f, 1.0f), wood, false);
            // AeroPress, molino, báscula, hervidor y tazas
            var ap = HWorld.Group(g, "AeroPress", new Vector3(-0.8f, 1.06f, 0));
            HWorld.Cyl(ap, "Camara", new Vector3(0, 0.16f, 0), 0.11f, 0.3f, FGMaterials.Transparent("aeropressCamara", new Color(0.74f, 0.84f, 0.9f, 0.6f), true));
            HWorld.Cyl(ap, "Embolo", new Vector3(0, 0.38f, 0), 0.09f, 0.18f, HWorld.M("#333333", 0.5f));
            HWorld.Cyl(ap, "Taza", new Vector3(0, -0.02f, 0), 0.12f, 0.1f, white);
            HWorld.Cyl(g, "Molino", new Vector3(0.1f, 1.3f, 0.1f), 0.18f, 0.45f, HWorld.M("#b0b5b9", 0.8f));
            HWorld.Box(g, "Bascula", new Vector3(-1.6f, 1.08f, 0), new Vector3(0.3f, 0.03f, 0.25f), HWorld.M("#111111", 0.6f));
            HWorld.Cyl(g, "Hervidor", new Vector3(1.0f, 1.2f, 0), 0.22f, 0.24f, steel);
            HWorld.Box(g, "Cuello", new Vector3(1.18f, 1.28f, 0), new Vector3(0.24f, 0.03f, 0.03f), steel, false, false).transform.localRotation = Quaternion.Euler(0, 0, 35);
            for (int i = 0; i < 4; i++) HWorld.Cyl(g, "Taza", new Vector3(1.6f + i * 0.2f, 1.1f, 0.25f), 0.09f, 0.09f, white);
            // tablero de menú
            HWorld.Box(g, "Tablero", new Vector3(-1.5f, 3f, -2.6f), new Vector3(3f, 1.6f, 0.05f), HWorld.M("#1e2622", 0.1f), false, false);
            HWorld.Text3D(g, "MÉTODO DEL DÍA\nAeroPress · 15 g · 220 g\n88 °C · 2 min", new Vector3(-1.5f, 3f, -2.56f), Quaternion.Euler(0, 180, 0), 0.18f, Color.white);
            var st = HWorld.Group(root, "EstacionAeroPress", new Vector3(7.2f, 0, -5.0f));
            Interactable.Add(st.gameObject, "aeropress", "Preparar una AeroPress", 2.2f);
        }

        static void BuildDesk(Transform root, Material wood, Material steel)
        {
            var g = HWorld.Group(root, "Escritorio", new Vector3(10.6f, 0, 5.4f));
            HWorld.Block(g, "Mesa", Vector3.zero, new Vector3(1.0f, 0.76f, 2.2f), wood);
            HWorld.Box(g, "Monitor", new Vector3(0.3f, 1.15f, 0), new Vector3(0.06f, 0.55f, 0.95f), HWorld.M("#111111", 0.6f));
            HWorld.Box(g, "Pantalla", new Vector3(0.26f, 1.15f, 0), new Vector3(0.02f, 0.48f, 0.88f), FGMaterials.Emissive("pantallaPC", new Color(0.22f, 0.5f, 0.38f), 1.6f), false, false);
            HWorld.Text3D(g, HConfig.Channel + "\nmeta: 50.000 tazas", new Vector3(0.24f, 1.17f, 0), Quaternion.Euler(0, 90, 0), 0.07f, Color.white);
            HWorld.Box(g, "Teclado", new Vector3(-0.1f, 0.78f, 0), new Vector3(0.18f, 0.02f, 0.5f), HWorld.M("#222222", 0.4f), false, false);
            HWorld.Block(g, "Silla", new Vector3(-0.9f, 0, 0), new Vector3(0.55f, 0.48f, 0.55f), HWorld.M("#2fbf71", 0.3f));
            HWorld.Box(g, "Espaldar", new Vector3(-1.15f, 0.85f, 0), new Vector3(0.08f, 0.7f, 0.5f), HWorld.M("#2fbf71", 0.3f), false);
            HWorld.Box(g, "Afiche", new Vector3(0.0f, 2.3f, 1.6f), new Vector3(0.8f, 1.1f, 0.03f), FGMaterials.Lit("afiche", Color.white, FGTextures.Murals != null && FGTextures.Murals.Length > 0 ? FGTextures.Murals[0] : null, 0.1f), false, false);
            var st = HWorld.Group(root, "EstacionPC", new Vector3(9.2f, 0, 5.4f));
            Interactable.Add(st.gameObject, "pc", "Usar el computador", 1.9f);
        }

        static void BuildBed(Transform root, Material darkWood)
        {
            var g = HWorld.Group(root, "Rincon", new Vector3(5.6f, 0, 7.6f));
            HWorld.Block(g, "Cama", Vector3.zero, new Vector3(2.2f, 0.45f, 1.2f), darkWood);
            HWorld.Box(g, "Colchon", new Vector3(0, 0.55f, 0), new Vector3(2.1f, 0.2f, 1.1f), HWorld.M("#e8e1d3", 0.1f), false);
            HWorld.Box(g, "Ruana", new Vector3(0.3f, 0.67f, 0), new Vector3(1.2f, 0.05f, 1.12f), HWorld.M("#7a3b2e", 0.1f), false, false);
            HWorld.Box(g, "Almohada", new Vector3(-0.8f, 0.72f, 0), new Vector3(0.4f, 0.14f, 0.7f), HWorld.M("#f5f1e8", 0.1f), false, false);
            HWorld.Block(g, "Mesita", new Vector3(1.5f, 0, 0.1f), new Vector3(0.5f, 0.55f, 0.5f), darkWood);
            HWorld.Lamp(g, new Vector3(1.5f, 1.0f, 0.1f), new Color(1f, 0.75f, 0.45f), 3f, 0.8f);
            HWorld.Ball(g, "Lampara", new Vector3(1.5f, 0.75f, 0.1f), 0.22f, FGMaterials.Emissive("lamparita", new Color(1f, 0.75f, 0.45f), 2f));
            var st = HWorld.Group(root, "EstacionCama", new Vector3(5.6f, 0, 6.4f));
            Interactable.Add(st.gameObject, "cama", "Dormir hasta mañana", 1.8f);
        }

        static void BuildDoor(Transform root, Material steel)
        {
            var door = FGMaterials.Lit("porton", new Color(0.55f, 0.6f, 0.62f), FGTextures.Warehouse, 0.3f);
            // portón enrollado arriba (abierto): se ve la calle
            HWorld.Box(root, "Porton", new Vector3(0, 4.6f, -D / 2 - 0.1f), new Vector3(5f, 0.6f, 0.4f), door, false);
            HWorld.Box(root, "Riel", new Vector3(-2.55f, 2.3f, -D / 2 - 0.1f), new Vector3(0.1f, 4.6f, 0.2f), steel, false, false);
            HWorld.Box(root, "Riel", new Vector3(2.55f, 2.3f, -D / 2 - 0.1f), new Vector3(0.1f, 4.6f, 0.2f), steel, false, false);
            HWorld.Box(root, "Aviso", new Vector3(0, 5.6f, -D / 2 - 0.32f), new Vector3(4.4f, 0.8f, 0.06f), HWorld.M("#0f3d2e", 0.3f), false, false);
            HWorld.Text3D(root, "FLOAT GARDENS · café cultivado en Bogotá", new Vector3(0, 5.6f, -D / 2 - 0.36f), Quaternion.identity, 0.24f, HConfig.Cream);
            // la calle de Puente Aranda afuera
            HWorld.Box(root, "Anden", new Vector3(0, -0.05f, -D / 2 - 3f), new Vector3(30f, 0.1f, 6f), FGMaterials.Lit("anden", Color.white, FGTextures.Sidewalk, 0.2f), true, false);
            HWorld.Box(root, "Calle", new Vector3(0, -0.08f, -D / 2 - 11f), new Vector3(60f, 0.1f, 10f), FGMaterials.Lit("asfalto", Color.white, FGTextures.Asphalt, 0.2f), true, false);
            HWorld.Block(root, "BodegaVecina", new Vector3(-8, 0, -D / 2 - 22f), new Vector3(20f, 8f, 8f), FGMaterials.Lit("bodegaVecina", Color.white, FGTextures.Warehouse, 0.1f));
            HWorld.Block(root, "BodegaVecina", new Vector3(14, 0, -D / 2 - 22f), new Vector3(14f, 6f, 8f), FGMaterials.Lit("bodegaVecina2", new Color(0.85f, 0.75f, 0.7f), FGTextures.Warehouse, 0.1f));
            var st = HWorld.Group(root, "EstacionPuerta", new Vector3(0, 0, -D / 2 + 0.6f));
            Interactable.Add(st.gameObject, "puerta", "Salir: mapa de Colombia", 2.2f);
        }

        static void BuildDecor(Transform root, Material wood, Material darkWood)
        {
            // Mural en la pared occidental
            var mural = HWorld.Box(root, "Mural", new Vector3(-W / 2 + 0.02f, 3.4f, -0.5f), new Vector3(0.04f, 3.6f, 7.5f), FGMaterials.Lit("mural", Color.white, FGTextures.Murals != null && FGTextures.Murals.Length > 1 ? FGTextures.Murals[1] : null, 0.05f), false, false);
            mural.name = "Mural";
            HWorld.Text3D(root, "FLOAT GARDENS", new Vector3(-W / 2 + 0.08f, 4.5f, -0.5f), Quaternion.Euler(0, -90, 0), 0.8f, HConfig.Gold);
            HWorld.Text3D(root, "BOGOTÁ · 2.600 m más cerca de las estrellas", new Vector3(-W / 2 + 0.08f, 3.4f, -0.5f), Quaternion.Euler(0, -90, 0), 0.3f, Color.white);
            Interactable.Add(HWorld.Group(root, "VerMural", new Vector3(-W / 2 + 1.6f, 0, -0.5f)).gameObject, "mural", "Mirar el mural", 1.8f);

            // Ventana hacia los cerros
            Interactable.Add(HWorld.Group(root, "VerVentana", new Vector3(W / 2 - 1.2f, 0, -0.6f)).gameObject, "ventana", "Mirar los cerros", 1.8f);

            // Tablero de metas junto a la barra (pared sur)
            HWorld.Box(root, "TableroMetas", new Vector3(4.4f, 2.4f, -D / 2 + 0.05f), new Vector3(2.4f, 1.5f, 0.05f), HWorld.M("#1e2622", 0.1f), false, false);
            HWorld.Text3D(root, "METAS\n- 50.000 tazas\n- café cultivado en Bogotá\n- el Mundial", new Vector3(4.4f, 2.4f, -D / 2 + 0.09f), Quaternion.Euler(0, 180, 0), 0.16f, Color.white);
            Interactable.Add(HWorld.Group(root, "VerTablero", new Vector3(4.4f, 0, -D / 2 + 1.3f)).gameObject, "tablero", "Leer el tablero de metas", 1.5f);

            // Estantería con costales, libros y frascos
            var sh = HWorld.Group(root, "Estanteria", new Vector3(-6.2f, 0, D / 2 - 0.5f));
            for (int i = 0; i < 4; i++) HWorld.Box(sh, "Repisa", new Vector3(0, 0.4f + i * 0.6f, 0), new Vector3(2.6f, 0.05f, 0.5f), wood, false);
            HWorld.Box(sh, "Lado", new Vector3(-1.3f, 1.1f, 0), new Vector3(0.05f, 2.2f, 0.5f), darkWood, true);
            HWorld.Box(sh, "Lado", new Vector3(1.3f, 1.1f, 0), new Vector3(0.05f, 2.2f, 0.5f), darkWood, true);
            var rng = new System.Random(5);
            string[] colors = { "#c0392b", "#2f6b8f", "#e0b13a", "#3d7a4a", "#7b4a8a", "#d9d2c3" };
            for (int i = 0; i < 3; i++)
                for (int k = 0; k < 7; k++)
                    HWorld.Box(sh, "Libro", new Vector3(-1.05f + k * 0.33f, 0.62f + i * 0.6f, 0), new Vector3(0.12f + (float)rng.NextDouble() * 0.12f, 0.34f, 0.3f), HWorld.M(colors[rng.Next(colors.Length)], 0.2f), false, false);
            Interactable.Add(HWorld.Group(root, "VerEstanteria", new Vector3(-6.2f, 0, D / 2 - 1.6f)).gameObject, "estanteria", "Revisar la estantería", 1.5f);

            // Costales de fique apilados
            var jute = FGMaterials.Lit("costalFique", new Color(0.72f, 0.6f, 0.42f), FGTextures.Warehouse, 0.05f);
            for (int i = 0; i < 5; i++)
                HWorld.Block(root, "Costal", new Vector3(-11.6f + (i % 3) * 0.75f, (i / 3) * 0.62f, -7.6f), new Vector3(0.7f, 0.6f, 0.5f), jute, i < 3);
            HWorld.Text3D(root, "Float Gardens\nBogotá", new Vector3(-10.85f, 0.4f, -7.84f), Quaternion.identity, 0.09f, new Color(0.2f, 0.12f, 0.08f));

            // Banderines amarillo, azul y rojo
            for (int i = 0; i < 18; i++)
            {
                Color c = i % 3 == 0 ? HConfig.Gold : i % 3 == 1 ? HConfig.Blue : HConfig.Red;
                HWorld.Box(root, "Banderin", new Vector3(-8 + i * 0.9f, 5.6f - Mathf.Sin(i / 17f * Mathf.PI) * 0.6f, -8.4f), new Vector3(0.4f, 0.4f, 0.02f),
                    FGMaterials.Lit("banderin" + (i % 3), c, null, 0.2f), false, false).transform.localRotation = Quaternion.Euler(0, 0, 45);
            }
            // Plantas en matera y una mata madre en un barril
            HWorld.Cyl(root, "Barril", new Vector3(11.2f, 0.45f, -1.8f), 0.8f, 0.9f, HWorld.M("#3d6b5a", 0.3f), true);
            var leaves = new MeshBuilder();
            var fruits = new MeshBuilder();
            HWorld.CoffeeShrub(leaves, fruits, new Vector3(11.2f, 0.9f, -1.8f), 1.5f, new System.Random(2));
            leaves.Build("MataMadre", HWorld.M("#1f6b2c", 0.65f), root);
            fruits.Build("MataMadreFrutos", FGMaterials.Lit("cerezaRoja", HConfig.Cherry, null, 0.7f), root, false);
        }

        static void BuildOutside(Transform root)
        {
            var ground = FGMaterials.Lit("sabana", new Color(0.42f, 0.5f, 0.36f), FGTextures.Grass, 0.05f);
            HWorld.Box(root, "Sabana", new Vector3(260, -0.15f, 0), new Vector3(500, 0.1f, 600), ground, false, false);
            // barrios bajos entre la bodega y los cerros
            var rng = new System.Random(11);
            var city = new MeshBuilder();
            for (int i = 0; i < 70; i++)
            {
                float x = 25 + (float)rng.NextDouble() * 120f, z = -120 + (float)rng.NextDouble() * 240f;
                float h = 4 + (float)rng.NextDouble() * (x > 90 ? 40f : 12f);
                city.Box(x, z, 0, 6 + (float)rng.NextDouble() * 10f, h, 6 + (float)rng.NextDouble() * 10f, 4f, 3f);
            }
            city.Build("Ciudad", FGMaterials.Lit("ciudadLadrillo", new Color(0.85f, 0.65f, 0.55f), FGTextures.Brick, 0.05f), root);
            // cerros orientales
            var hills = new MeshBuilder();
            for (int i = 0; i < 9; i++)
            {
                float z = -260 + i * 65f;
                hills.Add(MeshBuilder.Cone(9, 75 + (i % 3) * 18f, 120 + (i % 4) * 25f), Matrix4x4.Translate(new Vector3(200 + (i % 2) * 25f, 0, z)));
            }
            hills.Build("Cerros", HWorld.M("#2e4a33", 0.1f), root, false);
            // Monserrate con el santuario blanco en la cima
            var mons = HWorld.Group(root, "Monserrate", new Vector3(190, 0, 10));
            HWorld.Cone(mons, "Cerro", Vector3.zero, 70, 175, HWorld.M("#2c4731", 0.1f), 10);
            var church = HWorld.M("#f2f0ea", 0.2f);
            HWorld.Box(mons, "Santuario", new Vector3(0, 174, 0), new Vector3(12, 8, 7), church, false, false);
            HWorld.Box(mons, "Torre", new Vector3(0, 182, -2), new Vector3(3.5f, 10, 3.5f), church, false, false);
            HWorld.Box(mons, "Cupula", new Vector3(0, 188, -2), new Vector3(2.6f, 2.6f, 2.6f), HWorld.M("#c8c2b5", 0.2f), false, false);
        }

        // ============================ BOGOTÁ: CENTRO HISTÓRICO ============================

        public static SceneInfo BuildCity(Transform root, HistoriaSave s)
        {
            FGTextures.Init();
            var cobble = FGMaterials.Lit("candelariaPiedra", Color.white, FGTextures.Cobble, 0.25f);
            var plaza = new MeshBuilder();
            plaza.Top(0, 0, 0, 70, 70, 3f);
            plaza.Build("Plaza", cobble, root, false);
            HWorld.Box(root, "PisoColision", new Vector3(0, -0.25f, 0), new Vector3(72, 0.5f, 72), cobble, true, false).GetComponent<MeshRenderer>().enabled = false;

            // casas coloniales de colores alrededor
            string[] walls = { "#f3e6c8", "#e9c46a", "#7fb3d5", "#e76f51", "#f1faee", "#9ccc65", "#ffb4a2" };
            var roof = FGMaterials.Lit("teja", new Color(0.72f, 0.36f, 0.24f), FGTextures.Roof, 0.15f);
            var wood = HWorld.M("#5b3a1e", 0.25f);
            int idx = 0;
            for (int side = 0; side < 3; side++)
            {
                for (int k = 0; k < 6; k++)
                {
                    Vector3 c; float yaw;
                    if (side == 0) { c = new Vector3(-25 + k * 10f, 0, 30); yaw = 180; }
                    else if (side == 1) { c = new Vector3(-30, 0, -22 + k * 9f); yaw = 90; }
                    else { c = new Vector3(30, 0, -22 + k * 9f); yaw = -90; }
                    if (side == 0 && (k == 2 || k == 3)) continue; // espacio para la catedral
                    var hg = HWorld.Group(root, "Casa", c, yaw);
                    var wm = FGMaterials.Lit("candelaria" + walls[idx % walls.Length], FGMaterials.Hex(walls[idx % walls.Length]), FGTextures.Stucco, 0.1f);
                    float h = 6.5f + (idx % 3) * 1.2f;
                    HWorld.Block(hg, "Muros", Vector3.zero, new Vector3(9.4f, h, 7f), wm);
                    var r1 = HWorld.Box(hg, "Tejado", new Vector3(0, h + 0.9f, 1.6f), new Vector3(9.8f, 0.25f, 4.4f), roof, false);
                    r1.transform.localRotation = Quaternion.Euler(-24, 0, 0);
                    var r2 = HWorld.Box(hg, "Tejado", new Vector3(0, h + 0.9f, -1.6f), new Vector3(9.8f, 0.25f, 4.4f), roof, false);
                    r2.transform.localRotation = Quaternion.Euler(24, 0, 0);
                    // puerta, ventanas y balcón de madera
                    HWorld.Box(hg, "Puerta", new Vector3(-2.5f, 1.3f, 3.52f), new Vector3(1.4f, 2.6f, 0.06f), wood, false, false);
                    HWorld.Box(hg, "Ventana", new Vector3(1.8f, 1.6f, 3.52f), new Vector3(1.4f, 1.4f, 0.06f), HWorld.M("#2d6a4f", 0.3f), false, false);
                    HWorld.Box(hg, "Balcon", new Vector3(0, h - 2.2f, 3.9f), new Vector3(6f, 0.15f, 0.8f), wood, false);
                    HWorld.Box(hg, "Baranda", new Vector3(0, h - 1.7f, 4.25f), new Vector3(6f, 0.9f, 0.06f), wood, false, false);
                    idx++;
                }
            }

            // Catedral (fachada colonial genérica)
            var cathedral = HWorld.Group(root, "Catedral", new Vector3(0, 0, 33));
            var stone = HWorld.M("#d8d2c4", 0.15f);
            HWorld.Block(cathedral, "Fachada", Vector3.zero, new Vector3(20, 18, 4), stone);
            HWorld.Block(cathedral, "TorreIzq", new Vector3(-8, 0, 0), new Vector3(4.5f, 26, 4.5f), stone);
            HWorld.Block(cathedral, "TorreDer", new Vector3(8, 0, 0), new Vector3(4.5f, 26, 4.5f), stone);
            HWorld.Box(cathedral, "Portal", new Vector3(0, 4, -2.05f), new Vector3(4, 8, 0.1f), HWorld.M("#3d2a1a", 0.2f), false, false);
            for (int i = -1; i <= 1; i++) HWorld.Box(cathedral, "Columna", new Vector3(i * 4.5f, 6, -2.3f), new Vector3(0.8f, 12, 0.6f), stone, false);

            // Café histórico ficticio con la mentora en la puerta
            var cafe = HWorld.Group(root, "CafeLaTertulia", new Vector3(-30f, 0, 0.5f), 90);
            HWorld.Box(cafe, "Toldo", new Vector3(0, 3.2f, 4.4f), new Vector3(7f, 0.15f, 1.6f), HWorld.M("#7a1f1f", 0.2f), false);
            HWorld.Box(cafe, "Letrero", new Vector3(0, 4.6f, 3.6f), new Vector3(6f, 0.9f, 0.1f), HWorld.M("#1e2622", 0.2f), false, false);
            HWorld.Text3D(cafe, "CAFÉ LA TERTULIA · desde 1936", new Vector3(0, 4.6f, 3.68f), Quaternion.Euler(0, 180, 0), 0.32f, HConfig.Gold);
            HWorld.Box(cafe, "Placa", new Vector3(3.2f, 1.7f, 3.56f), new Vector3(0.8f, 0.55f, 0.04f), HWorld.M("#b08d57", 0.7f), false, false);
            Interactable.Add(HWorld.Group(cafe, "LeerPlaca", new Vector3(3.2f, 0, 5f)).gameObject, "placa_cafe", "Leer la placa", 1.6f);
            for (int i = 0; i < 3; i++)
            {
                HWorld.Cyl(cafe, "Mesa", new Vector3(-2.5f + i * 2.5f, 0.75f, 6.4f), 0.8f, 0.05f, HWorld.M("#f5f1e8", 0.3f), true);
                HWorld.Cyl(cafe, "Pata", new Vector3(-2.5f + i * 2.5f, 0.37f, 6.4f), 0.08f, 0.74f, HWorld.M("#222222", 0.4f));
            }
            var region = Catalog.Region("bogota");
            string mentorName = region != null && !string.IsNullOrEmpty(region.mentorName) ? region.mentorName : "la mentora";
            var mentor = HWorld.Npc(root, mentorName, new HumanoidStyle
            {
                shirt = FGMaterials.Hex("#3a2a45"), pants = FGMaterials.Hex("#2a2422"), skin = FGMaterials.Hex("#c99a76"),
                hair = FGMaterials.Hex("#cfcac2"), apron = FGMaterials.Hex("#5b2e1f"),
            }, new Vector3(-23.2f, 0, 2.5f), 90);
            HWorld.Ruana(mentor, FGMaterials.Hex("#7d5a44"));
            Interactable.Add(mentor.gameObject, "mentor", "Hablar con " + mentorName, 2.4f);

            // Vendedor de tinto con su carrito de termos
            var cart = HWorld.Group(root, "CarritoTinto", new Vector3(6, 0, 4));
            HWorld.Block(cart, "Carrito", Vector3.zero, new Vector3(1.4f, 1.0f, 0.8f), HWorld.M("#e0b13a", 0.3f));
            for (int i = 0; i < 3; i++) HWorld.Cyl(cart, "Termo", new Vector3(-0.4f + i * 0.4f, 1.25f, 0), 0.22f, 0.5f, HWorld.M(i == 1 ? "#c0392b" : "#2f6b8f", 0.5f));
            HWorld.Cyl(cart, "Sombrilla", new Vector3(0, 2.4f, 0), 0.05f, 2.8f, HWorld.M("#444444", 0.3f));
            HWorld.Cone(cart, "Lona", new Vector3(0, 3.4f, 0), 1.4f, 0.6f, HWorld.M("#c0392b", 0.2f), 8);
            var vendor = HWorld.Npc(root, "Don Chucho", new HumanoidStyle { shirt = FGMaterials.Hex("#2f6b8f"), pants = FGMaterials.Hex("#3a3a3a"), cap = FGMaterials.Hex("#e0b13a"), skin = FGMaterials.Hex("#b07850") }, new Vector3(7.4f, 0, 4), -90);
            Interactable.Add(vendor.gameObject, "tintero", "Comprar un tinto", 2f);

            // Palomas de la plaza
            var pigeon = HWorld.M("#8e9297", 0.2f);
            var prng = new System.Random(3);
            for (int i = 0; i < 22; i++)
                HWorld.Box(root, "Paloma", new Vector3(-8 + (float)prng.NextDouble() * 16f, 0.12f, 8 + (float)prng.NextDouble() * 10f), new Vector3(0.18f, 0.18f, 0.3f), pigeon, false, false)
                    .transform.localRotation = Quaternion.Euler(0, prng.Next(360), 0);

            // Faroles
            for (int i = 0; i < 6; i++)
            {
                var pos = new Vector3(-15 + i * 6f, 0, -6);
                HWorld.Cyl(root, "Poste", pos + Vector3.up * 1.8f, 0.12f, 3.6f, HWorld.M("#1b1b1b", 0.4f), true);
                HWorld.Box(root, "Farol", pos + Vector3.up * 3.75f, new Vector3(0.35f, 0.45f, 0.35f), FGMaterials.Emissive("farol", new Color(1f, 0.8f, 0.5f), 1.5f), false, false);
            }

            // Estación tipo TransMilenio (vuelta a la bodega) y paradero de buses al mapa
            var station = HWorld.Group(root, "Estacion", new Vector3(0, 0, -28));
            var red = HWorld.M("#c8102e", 0.4f);
            HWorld.Block(station, "Plataforma", Vector3.zero, new Vector3(16, 0.4f, 4), HWorld.M("#9aa0a4", 0.3f));
            HWorld.Box(station, "Techo", new Vector3(0, 3.4f, 0), new Vector3(16.4f, 0.2f, 4.4f), red, false);
            for (int i = -1; i <= 1; i++) HWorld.Cyl(station, "Columna", new Vector3(i * 7f, 1.9f, 1.6f), 0.2f, 3f, red, true);
            HWorld.Text3D(station, "ESTACIÓN · Puente Aranda", new Vector3(0, 3.0f, 2.25f), Quaternion.Euler(0, 180, 0), 0.32f, Color.white);
            Interactable.Add(HWorld.Group(station, "VolverBodega", new Vector3(-4, 0.4f, 0)).gameObject, "bodega", "Volver a la bodega (TransMilenio)", 2.4f);
            Interactable.Add(HWorld.Group(station, "AbrirMapa", new Vector3(4, 0.4f, 0)).gameObject, "volver", "Abrir el mapa de Colombia", 2.4f);

            // Monserrate al oriente
            var mons = HWorld.Group(root, "Monserrate", new Vector3(180, 0, 20));
            HWorld.Cone(mons, "Cerro", Vector3.zero, 75, 170, HWorld.M("#2c4731", 0.1f), 10);
            HWorld.Box(mons, "Santuario", new Vector3(0, 169, 0), new Vector3(12, 8, 7), HWorld.M("#f2f0ea", 0.2f), false, false);
            var hills = new MeshBuilder();
            for (int i = 0; i < 7; i++) hills.Add(MeshBuilder.Cone(9, 70, 110 + (i % 3) * 30f), Matrix4x4.Translate(new Vector3(185 + (i % 2) * 30f, 0, -200 + i * 70f)));
            hills.Build("Cerros", HWorld.M("#2e4a33", 0.1f), root, false);

            RenderSettings.fogColor = new Color32(0xb9, 0xc4, 0xcc, 255);
            RenderSettings.fogStartDistance = 30f;
            RenderSettings.fogEndDistance = 380f;
            return new SceneInfo { spawn = new Vector3(0, 0.45f, -26f), yaw = 0f, altitude = HConfig.BogotaAltitude };
        }
    }
}
