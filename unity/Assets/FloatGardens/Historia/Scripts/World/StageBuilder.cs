// Escenario de las competencias: tarima, mesa de tres jueces, estaciones de AeroPress, público, reflectores y pancarta.
// Copa Bogotá: bodega de ladrillo con bombillos; Nacional: recinto ferial con banderas de Colombia; Mundial: arena internacional.
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class StageBuilder
    {
        public static SceneInfo Build(Transform root, Competition c, HistoriaSave s)
        {
            FGTextures.Init();
            string id = c != null ? c.id : "copa_bogota";
            bool bogota = id == "copa_bogota", world = id == "mundial";
            const float W = 40f, D = 30f, H = 11f;

            // Recinto
            var floorMat = FGMaterials.Lit("pisoCompetencia" + id, bogota ? new Color(0.62f, 0.6f, 0.58f) : new Color(0.3f, 0.32f, 0.36f), FGTextures.Concrete, bogota ? 0.3f : 0.6f);
            var floor = new MeshBuilder();
            floor.Top(0, 0, 0, W, D, 4f);
            floor.Build("Piso", floorMat, root, false);
            HWorld.Box(root, "PisoColision", new Vector3(0, -0.25f, 0), new Vector3(W, 0.5f, D), floorMat, true, false).GetComponent<MeshRenderer>().enabled = false;
            var wallMat = bogota ? FGMaterials.Lit("bodegaLadrillo", Color.white, FGTextures.Brick, 0.08f) : HWorld.M(world ? "#1d2230" : "#e8e4da", 0.2f);
            HWorld.Box(root, "MuroNorte", new Vector3(0, H / 2, D / 2), new Vector3(W, H, 0.4f), wallMat, true);
            HWorld.Box(root, "MuroSur", new Vector3(0, H / 2, -D / 2), new Vector3(W, H, 0.4f), wallMat, true);
            HWorld.Box(root, "MuroOeste", new Vector3(-W / 2, H / 2, 0), new Vector3(0.4f, H, D), wallMat, true);
            HWorld.Box(root, "MuroEste", new Vector3(W / 2, H / 2, 0), new Vector3(0.4f, H, D), wallMat, true);
            HWorld.Box(root, "Techo", new Vector3(0, H, 0), new Vector3(W, 0.3f, D), HWorld.M("#15171c", 0.2f), false);

            // Tarima con las estaciones de los competidores
            var stage = HWorld.Group(root, "Tarima", new Vector3(0, 0, 9f));
            HWorld.Block(stage, "Plataforma", Vector3.zero, new Vector3(20f, 1f, 8f), HWorld.M(world ? "#2a2f3d" : "#4e3220", 0.3f));
            HWorld.Box(stage, "Escalon", new Vector3(0, 0.25f, -4.6f), new Vector3(6f, 0.5f, 1.2f), HWorld.M("#3b2a1c", 0.3f), true);
            for (int i = -1; i <= 1; i++)
            {
                var st = HWorld.Group(stage, "Estacion", new Vector3(i * 6f, 1f, 1f));
                HWorld.Block(st, "Mesa", Vector3.zero, new Vector3(3.2f, 1.0f, 1.2f), HWorld.M("#f5f1e8", 0.3f));
                HWorld.Cyl(st, "AeroPress", new Vector3(-0.6f, 1.25f, 0), 0.12f, 0.45f, FGMaterials.Transparent("aeropressCamara", new Color(0.74f, 0.84f, 0.9f, 0.6f), true));
                HWorld.Cyl(st, "Hervidor", new Vector3(0.4f, 1.15f, 0), 0.22f, 0.25f, HWorld.M("#2b2f33", 0.6f));
                HWorld.Cyl(st, "Molino", new Vector3(1.1f, 1.2f, 0), 0.16f, 0.4f, HWorld.M("#b0b5b9", 0.8f));
            }

            // Mesa de los jueces frente a la tarima, con la bandeja de dos tazas
            var judges = HWorld.Group(root, "Jueces", new Vector3(0, 0, 2.5f));
            HWorld.Block(judges, "Mesa", Vector3.zero, new Vector3(7f, 0.95f, 1.4f), HWorld.M("#1e2622", 0.3f));
            HWorld.Box(judges, "Bandeja", new Vector3(0, 0.98f, -0.2f), new Vector3(1.2f, 0.04f, 0.5f), HWorld.M("#b08d57", 0.6f), false, false);
            HWorld.Cyl(judges, "Taza", new Vector3(-0.3f, 1.05f, -0.2f), 0.14f, 0.12f, HWorld.M("#f5f1e8", 0.3f));
            HWorld.Cyl(judges, "Taza", new Vector3(0.3f, 1.05f, -0.2f), 0.14f, 0.12f, HWorld.M("#f5f1e8", 0.3f));
            string[] shirts = { "#1f2a44", "#5b2e1f", "#2d6a4f" };
            for (int i = -1; i <= 1; i++)
            {
                var j = HWorld.Npc(judges, "Juez", new HumanoidStyle
                {
                    shirt = FGMaterials.Hex(shirts[i + 1]), pants = FGMaterials.Hex("#2c2a27"), skin = FGMaterials.Hex(i == 0 ? "#8d5a3b" : i < 0 ? "#e0b896" : "#b98a62"),
                    hair = FGMaterials.Hex(i == 1 ? "#bdb6aa" : "#1a120c"), tie = i == 0 ? FGMaterials.Hex("#c8102e") : (Color?)null,
                }, new Vector3(i * 2.2f, 0, 1.2f), 180f);
                j.transform.localPosition = new Vector3(i * 2.2f, 0, 1.2f);
            }
            Interactable.Add(HWorld.Group(root, "HablarJueces", new Vector3(0, 0, 0.6f)).gameObject, "jueces", "Hablar con los jueces", 2.6f);

            // Pancarta
            var bannerColor = world ? "#0f3d2e" : bogota ? "#4a2c1d" : "#1f4ea8";
            HWorld.Box(root, "Pancarta", new Vector3(0, 7.2f, D / 2 - 0.3f), new Vector3(22f, 3.2f, 0.1f), HWorld.M(bannerColor, 0.3f), false, false);
            string title = c != null ? c.name.ToUpperInvariant() : "COMPETENCIA";
            string city = c != null ? c.city + " · " + Mathf.RoundToInt(c.altitude).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " m s. n. m." : "";
            HWorld.Text3D(root, title, new Vector3(0, 7.8f, D / 2 - 0.4f), Quaternion.identity, 0.9f, HConfig.Gold);
            HWorld.Text3D(root, city, new Vector3(0, 6.5f, D / 2 - 0.4f), Quaternion.identity, 0.45f, Color.white);

            // Decoración según la sede
            if (bogota)
            {
                var bulb = FGMaterials.Emissive("bombilloFiesta", new Color(1f, 0.8f, 0.45f), 3f);
                for (int i = 0; i < 26; i++)
                {
                    float x = -18f + i * 1.44f;
                    HWorld.Ball(root, "Bombillo", new Vector3(x, 8.6f - Mathf.Sin(i / 25f * Mathf.PI) * 1.4f, 5f), 0.22f, bulb);
                    HWorld.Ball(root, "Bombillo", new Vector3(x, 8.6f - Mathf.Sin(i / 25f * Mathf.PI) * 1.4f, -5f), 0.22f, bulb);
                }
            }
            else
            {
                // banderas: Colombia en el Nacional; países cafeteros en el Mundial
                string[][] flags = world
                    ? new[] { new[] { "#f2b705", "#1f4ea8", "#ce1126" }, new[] { "#078930", "#fcdd09", "#da121a" }, new[] { "#009b3a", "#fedf00", "#002776" },
                              new[] { "#006600", "#bb0000", "#000000" }, new[] { "#4997d0", "#ffffff", "#4997d0" }, new[] { "#002b7f", "#ffffff", "#ce1126" },
                              new[] { "#ffffff", "#bc002d", "#ffffff" }, new[] { "#ba0c2f", "#ffffff", "#00205b" } }
                    : new[] { new[] { "#f2b705", "#1f4ea8", "#ce1126" } };
                for (int i = 0; i < 12; i++)
                {
                    var f = flags[i % flags.Length];
                    float x = -18f + i * 3.27f;
                    var g = HWorld.Group(root, "Bandera", new Vector3(x, 9.2f, D / 2 - 1.2f));
                    HWorld.Box(g, "Franja", new Vector3(0, 0.6f, 0), new Vector3(1.8f, 0.4f, 0.05f), HWorld.M(f[0], 0.2f), false, false);
                    HWorld.Box(g, "Franja", new Vector3(0, 0.2f, 0), new Vector3(1.8f, 0.4f, 0.05f), HWorld.M(f[1], 0.2f), false, false);
                    HWorld.Box(g, "Franja", new Vector3(0, -0.2f, 0), new Vector3(1.8f, 0.4f, 0.05f), HWorld.M(f[2], 0.2f), false, false);
                }
            }
            if (world)
            {
                HWorld.Box(root, "Pantalla", new Vector3(-14f, 5f, D / 2 - 0.5f), new Vector3(8f, 4.5f, 0.2f), FGMaterials.Emissive("pantallaMundial", new Color(0.25f, 0.45f, 0.8f), 1.4f), false, false);
                HWorld.Box(root, "Pantalla", new Vector3(14f, 5f, D / 2 - 0.5f), new Vector3(8f, 4.5f, 0.2f), FGMaterials.Emissive("pantallaMundial", new Color(0.25f, 0.45f, 0.8f), 1.4f), false, false);
            }

            // Público (figuras low-poly combinadas en pocas mallas)
            var bodies = new MeshBuilder();
            var heads = new MeshBuilder();
            var cube = MeshBuilder.Primitive(PrimitiveType.Cube);
            var rng = new System.Random(id.GetHashCode());
            for (int row = 0; row < 4; row++)
                for (int k = 0; k < 16; k++)
                {
                    if (rng.NextDouble() < 0.18) continue;
                    float x = -15f + k * 2f + (float)(rng.NextDouble() - 0.5) * 0.6f, z = -6f - row * 2.2f;
                    float h = 1.5f + (float)rng.NextDouble() * 0.35f;
                    bodies.Add(cube, Matrix4x4.TRS(new Vector3(x, h * 0.4f, z), Quaternion.identity, new Vector3(0.55f, h * 0.8f, 0.35f)));
                    heads.Add(cube, Matrix4x4.TRS(new Vector3(x, h * 0.8f + 0.2f, z), Quaternion.identity, Vector3.one * 0.32f));
                }
            bodies.Build("Publico", HWorld.M(world ? "#3a4560" : "#5b4a6b", 0.2f), root);
            heads.Build("Cabezas", HWorld.M("#c99a76", 0.2f), root);

            // Reflectores
            var beam = FGMaterials.Transparent("haz", new Color(1f, 0.95f, 0.8f, 0.07f));
            for (int i = -1; i <= 1; i += 2)
            {
                HWorld.Cone(root, "Haz", new Vector3(i * 6f, 0.6f, 9f), 3.2f, 9.8f, beam, 10).transform.localRotation = Quaternion.Euler(0, 0, 0);
                HWorld.Box(root, "Reflector", new Vector3(i * 6f, H - 0.6f, 9f), new Vector3(0.8f, 0.6f, 0.8f), HWorld.M("#111111", 0.5f), false, false);
            }
            HWorld.Lamp(root, new Vector3(0, 8f, 8f), new Color(1f, 0.93f, 0.8f), 18f, 2.2f);
            HWorld.Lamp(root, new Vector3(0, 6f, -6f), new Color(1f, 0.85f, 0.65f), 16f, 1.0f);
            HWorld.Lamp(root, new Vector3(-12f, 6f, 0), new Color(0.8f, 0.85f, 1f), 14f, 0.7f);
            HWorld.Lamp(root, new Vector3(12f, 6f, 0), new Color(0.8f, 0.85f, 1f), 14f, 0.7f);

            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 400f;
            float alt = c != null ? c.altitude : HConfig.BogotaAltitude;
            return new SceneInfo { spawn = new Vector3(0, 0.05f, -3.5f), yaw = 0f, altitude = alt };
        }
    }
}
