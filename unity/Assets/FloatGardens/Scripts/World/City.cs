// Generación de Bogotá: manzanas por barrio, Av. Caracas, cerros con Monserrate, Plaza de Bolívar y la torre de Coffee Land.
// De noche la ciudad se enciende sola: ventanas, neones de las agrocorporaciones, hologramas, LEDs de los
// invernaderos en las azoteas y postes (Atmosphere llama SetNight con la hora del día).
// Coordenadas: x = oriente, z = norte. La fila 0 del mapa es el norte.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class City : MonoBehaviour
    {
        public static City I;

        const float Cell = FGConfig.Cell;
        const float Half = FGConfig.Half;
        const float Block = FGConfig.Block;
        const float Lot = Block / 2f - 2f; // 2 m de andén

        System.Random rng;
        readonly Dictionary<string, MeshBuilder> builders = new Dictionary<string, MeshBuilder>();
        readonly Dictionary<string, Material> builderMats = new Dictionary<string, Material>();
        GameObject colliders;
        readonly List<Vector3> treeSpots = new List<Vector3>();
        readonly List<Vector2> parkCells = new List<Vector2>();

        // Información pública para el resto del juego
        public readonly Dictionary<string, List<Vector3>> planterSpots = new Dictionary<string, List<Vector3>>();
        public readonly Dictionary<string, Vector3> zoneCenters = new Dictionary<string, Vector3>();
        public Vector3 hqCenter, hqSpawn, hqShop, hqLab, bossArena;
        public readonly List<float> stationZ = new List<float>();
        public readonly List<Bus> buses = new List<Bus>();
        public Vector3 lakeCenter;
        public Vector2 lakeRadius;
        public bool hasLake;
        public Texture2D minimap;
        public const int MinimapPx = 1; // px por metro
        public Material waterMat, asphaltMat, crownMat;
        Transform cabin;

        // ---------- Noche ----------
        public class Glow { public Material mat; public Color color; public float day, night; }
        public struct NeonSpot { public Vector3 pos; public Color color; }
        public readonly List<Glow> glows = new List<Glow>();
        readonly HashSet<Material> glowSet = new HashSet<Material>();
        public readonly List<Vector3> lampHeads = new List<Vector3>();   // para las luces reales de los postes
        public readonly List<NeonSpot> neonSpots = new List<NeonSpot>(); // luces de color de neones e invernaderos
        public float Night { get; private set; }
        readonly HashSet<string> noShadow = new HashSet<string>();
        static readonly Vector3[] Faces = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        // Marcas del agro-punk bogotano: las corporaciones que controlan semillas, agua y tierra
        static readonly string[] Brands =
        {
            "AGROCORP", "SEMILLAS Ω", "HIDRO·PONIA", "PANELA+", "CACAO.NET", "AGUA S.A.", "FINCA 2.0", "DRON·FUMIGA",
            "TINTO EXPRESS", "FEDERACIÓN", "BIO·LAB", "ARROZ NORTE", "CAFÉ 24H", "MOTO·TAXI", "INVERNADERO", "ABONO MAX",
        };
        static readonly string[] NeonHex = { "#ff2bd6", "#22e8ff", "#7dff3a", "#ff8a1f", "#a95bff", "#ffe600" };

        /// <summary>Registra un material emisivo con su intensidad de día y de noche.</summary>
        public void RegisterGlow(Material m, Color color, float day, float night)
        {
            if (m == null || !glowSet.Add(m)) return;
            glows.Add(new Glow { mat = m, color = color, day = day, night = night });
            FGMaterials.SetEmission(m, color, Mathf.Lerp(day, night, Night));
        }

        /// <summary>0 = pleno día, 1 = noche cerrada. Enciende ventanas, neones y LEDs.</summary>
        public void SetNight(float n)
        {
            Night = n;
            foreach (var g in glows) FGMaterials.SetEmission(g.mat, g.color, Mathf.Lerp(g.day, g.night, n));
        }
        Vector3 cableStart, cableEnd;

        public static Vector3 CellCenter(int row, int col) => new Vector3(-Half + col * Cell + Cell / 2f, 0, Half - row * Cell - Cell / 2f);
        public static float LineX(int k) => -Half + k * Cell;
        public static float LineZ(int k) => Half - k * Cell;
        public static Vector2Int CellOf(Vector3 p)
        {
            int col = Mathf.Clamp(Mathf.FloorToInt((p.x + Half) / Cell), 0, FGConfig.Grid - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt((Half - p.z) / Cell), 0, FGConfig.Grid - 1);
            return new Vector2Int(col, row);
        }
        public static char DistrictCode(int row, int col) => FGConfig.DistrictMap[row][col];
        public static char DistrictAt(Vector3 p) { var c = CellOf(p); return DistrictCode(c.y, c.x); }

        public static string StreetName(Vector3 p)
        {
            int k = Mathf.Clamp(Mathf.RoundToInt((p.x + Half) / Cell), 0, FGConfig.Grid);
            int j = Mathf.Clamp(Mathf.RoundToInt((Half - p.z) / Cell), 0, FGConfig.Grid);
            return FGConfig.Calles[j] + " con " + FGConfig.Carreras[k];
        }

        /// <summary>Altura del piso para personajes sin física: andenes de 0.25 m.</summary>
        public static float BaseY(Vector3 p)
        {
            var c = CellOf(p);
            if (DistrictCode(c.y, c.x) == 'P') return 0.25f;
            var cc = CellCenter(c.y, c.x);
            return Mathf.Abs(p.x - cc.x) <= Block / 2 && Mathf.Abs(p.z - cc.z) <= Block / 2 ? 0.25f : 0f;
        }

        float R() => (float)rng.NextDouble();
        float RR(float a, float b) => a + R() * (b - a);
        T Pick<T>(T[] a) => a[rng.Next(a.Length)];

        MeshBuilder B(string key, Material m)
        {
            if (!builders.TryGetValue(key, out var b))
            {
                b = new MeshBuilder();
                builders[key] = b;
                builderMats[key] = m;
            }
            return b;
        }

        void Collider(Vector3 center, Vector3 size)
        {
            var bc = colliders.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }

        // ---------- Construcción ----------
        public void Build()
        {
            I = this;
            rng = new System.Random(20260);
            FGTextures.Init();
            colliders = new GameObject("Colisiones");
            colliders.transform.SetParent(transform, false);
            colliders.isStatic = true;

            // sin máscara: la lluvia sube el brillo de todo el asfalto
            asphaltMat = FGMaterials.Lit("asfalto", Color.white, FGTextures.Asphalt, 0.12f, 0f, false);
            var ground = new MeshBuilder();
            float g = Half + 260;
            ground.Quad(new Vector3(-g, 0, -g), new Vector3(g, 0, -g), new Vector3(g, 0, g), new Vector3(-g, 0, g), Vector3.up, new Vector2(-g / 6, -g / 6), new Vector2(g / 6, g / 6));
            ground.Build("Suelo", asphaltMat, transform, false);
            Collider(new Vector3(0, -0.5f, 0), new Vector3(g * 2, 1, g * 2));

            for (int row = 0; row < FGConfig.Grid; row++)
                for (int col = 0; col < FGConfig.Grid; col++)
                    BuildCell(row, col);

            BuildStreets();
            BuildCaracas();
            BuildMountains();
            BuildTrees();
            BuildBounds();

            BuildDrones();
            foreach (var kv in builders) kv.Value.Build(kv.Key, builderMats[kv.Key], transform, !noShadow.Contains(kv.Key));
            BuildMinimap();
        }

        void BuildCell(int row, int col)
        {
            char code = DistrictCode(row, col);
            Vector3 c = CellCenter(row, col);
            var zone = ZoneAt(row, col);

            if (code == 'P') { ParkSlab(row, col, c); if (zone.HasValue) ZonePlaza(zone.Value, c, true); return; }
            Dressing(row, col, code, c);
            if (code == 'B') { Slab(c, "empedrado"); PlazaBolivar(c); return; }
            if (code == 'F') { Slab(c, "anden"); Sede(c); return; }
            Slab(c, code == 'L' ? "empedrado" : "anden");
            if (row == FGConfig.HQ.y && col == FGConfig.HQ.x) { HQ(c); return; }
            if (zone.HasValue) { ZonePlaza(zone.Value, c, false); return; }
            if (row == 7 && col == 10) { Torre(c); return; }
            Buildings(code, c);
        }

        FGConfig.ZoneDef? ZoneAt(int row, int col)
        {
            foreach (var z in FGConfig.Zones) if (z.row == row && z.col == col) return z;
            return null;
        }

        void Slab(Vector3 c, string kind)
        {
            Material m = kind == "empedrado" ? FGMaterials.Lit("empedrado", Color.white, FGTextures.Cobble, 0.2f)
                                             : FGMaterials.Lit("anden", Color.white, FGTextures.Sidewalk, 0.1f);
            B(kind, m).Box(c.x, c.z, 0, Block, 0.25f, Block, 8, 8);
            Collider(new Vector3(c.x, 0.125f, c.z), new Vector3(Block, 0.25f, Block));
        }

        void Building(string mat, int variant, float x, float z, float w, float h, float d)
        {
            Material m = FacadeMaterial(mat, variant);
            B(mat + variant, m).Walls(x, z, 0.25f, w, h, d);
            B("techo", FGMaterials.Lit("techo", Color.white, FGTextures.Roof, 0.05f)).Top(x, z, h + 0.25f, w, d);
            Collider(new Vector3(x, (h + 0.25f) / 2f, z), new Vector3(w, h + 0.25f, d));
            Details(mat, x, z, w, h, d);
            Signage(mat, x, z, w, h, d);

            // Toldos de colores en algunos locales del primer piso
            if (h < 40 && R() < 0.35f)
            {
                string col = Pick(new[] { "#d6423a", "#2fbf71", "#f2a541", "#47b5e0", "#e85d75" });
                float aw = Mathf.Min(w, d) * 0.6f;
                int side = rng.Next(4);
                float[][] off = { new[] { w / 2 + 0.6f, 0, 1.2f, aw }, new[] { -w / 2 - 0.6f, 0, 1.2f, aw }, new[] { 0, d / 2 + 0.6f, aw, 1.2f }, new[] { 0, -d / 2 - 0.6f, aw, 1.2f } };
                var o = off[side];
                B("toldo" + col, FGMaterials.Lit(col, 0.3f)).Box(x + o[0], z + o[1], 3f, o[2], 0.25f, o[3]);
            }
            // Murales
            if (h >= 6 && R() < 0.08f && (mat == "ladrillo" || mat == "bodega" || mat == "concreto"))
            {
                int k = rng.Next(FGTextures.Murals.Length);
                float mw = Mathf.Min(10, Mathf.Max(w, d) * 0.7f);
                int side = rng.Next(4);
                Vector3 n = side == 0 ? Vector3.right : side == 1 ? Vector3.left : side == 2 ? Vector3.forward : Vector3.back;
                Vector3 center = new Vector3(x, 1.2f + mw / 4, z) + Vector3.Scale(n, new Vector3(w / 2 + 0.05f, 0, d / 2 + 0.05f));
                Vector3 right = Vector3.Cross(Vector3.up, -n);
                var mb = B("mural" + k, FGMaterials.Lit("mural" + k, Color.white, FGTextures.Murals[k], 0.1f));
                Vector3 hx = right * mw / 2, hy = Vector3.up * mw / 4;
                mb.Quad(center - hx - hy, center + hx - hy, center + hx + hy, center - hx + hy, n, Vector2.zero, Vector2.one);
            }
        }

        // ---------- Detalle arquitectónico ----------
        // Cornisas, antepechos, franjas de entrepiso, balcones y cosas en la terraza: le dan sombra y silueta a cada edificio.
        bool plainRoof;

        void Details(string mat, float x, float z, float w, float h, float d)
        {
            if (mat == "colonial") return; // ya tiene su alero de teja
            float roof = h + 0.25f;
            var trim = B("cornisa", FGMaterials.Lit("cornisa", FGMaterials.Hex("#cfc8bb"), FGTextures.ConcreteBare, 0.1f));

            // Cornisa que sobresale y antepecho alrededor de la terraza
            trim.Solid(x, z, h - 0.4f, w + 0.5f, 0.4f, d + 0.5f);
            trim.Solid(x, z + d / 2 - 0.15f, roof, w, 1f, 0.3f);
            trim.Solid(x, z - d / 2 + 0.15f, roof, w, 1f, 0.3f);
            trim.Solid(x + w / 2 - 0.15f, z, roof, 0.3f, 1f, d - 0.6f);
            trim.Solid(x - w / 2 + 0.15f, z, roof, 0.3f, 1f, d - 0.6f);

            // Franjas de entrepiso: aluminio en las torres de vidrio, concreto en los edificios de oficinas
            if (mat == "vidrio" || mat == "concreto")
            {
                var band = mat == "vidrio"
                    ? B("franjaMetal", FGMaterials.Lit("franjaMetal", FGMaterials.Hex("#a3adb4"), null, 0.65f, 0.85f))
                    : B("franjaConcreto", FGMaterials.Lit("franjaConcreto", FGMaterials.Hex("#c2bcb2"), FGTextures.ConcreteBare, 0.1f));
                for (float y = 3; y < h - 1; y += 3) band.Solid(x, z, y - 0.12f, w + 0.18f, 0.24f, d + 0.18f);
            }

            // Balcones de ladrillo en los edificios de vivienda (alineados con las ventanas del módulo de 4 m)
            if (mat == "ladrillo" && h <= 30 && !plainRoof && R() < 0.55f)
            {
                var slab = B("balcon", FGMaterials.Lit("balcon", FGMaterials.Hex("#b9b2a6"), FGTextures.ConcreteBare, 0.1f));
                var rail = B("baranda", FGMaterials.Lit("baranda", FGMaterials.Hex("#26282b"), null, 0.45f, 0.7f));
                int side = rng.Next(4);
                Vector3 n = side == 0 ? Vector3.right : side == 1 ? Vector3.left : side == 2 ? Vector3.forward : Vector3.back;
                Vector3 right = Vector3.Cross(Vector3.up, -n);
                float width = side < 2 ? d : w, depth = side < 2 ? w : d;
                Vector3 left = new Vector3(x, 0, z) + n * depth / 2 - right * width / 2;
                bool alongX = side >= 2;
                for (float y = 3; y < h - 1; y += 3)
                    for (int k = 0; 4 * k + 4 <= width; k += 2)
                    {
                        Vector3 p = left + right * (4 * k + 2);
                        Vector3 sp = p + n * 0.55f, rp = p + n * 1.07f;
                        float bw = alongX ? 2.6f : 1.1f, bd = alongX ? 1.1f : 2.6f;
                        slab.Solid(sp.x, sp.z, y + 0.5f, bw, 0.15f, bd);
                        rail.Solid(rp.x, rp.z, y + 0.65f, alongX ? 2.6f : 0.05f, 0.95f, alongX ? 0.05f : 2.6f);
                    }
            }

            // Terraza: tanques de agua negros, aire acondicionado y antenas; las torres llevan cuarto de máquinas
            if (plainRoof) return;
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            // Invernaderos hidropónicos en las azoteas: la agricultura vertical de esta Bogotá
            if ((mat == "ladrillo" || mat == "concreto") && h <= 30 && w >= 12 && d >= 12 && R() < 0.3f)
            {
                Greenhouse(x, z, roof, w, d);
                return;
            }
            if (h >= 45)
            {
                trim.Solid(x + RR(-2, 2), z + RR(-2, 2), roof, w * 0.4f, 3.5f, d * 0.4f);
                B("antena", FGMaterials.Lit("antena", FGMaterials.Hex("#8d9399"), null, 0.5f, 0.9f))
                    .Add(cyl, Matrix4x4.TRS(new Vector3(x, roof + 3.5f + 4f, z), Quaternion.identity, new Vector3(0.12f, 4f, 0.12f)));
                return;
            }
            var tank = B("tanque", FGMaterials.Lit("tanque", FGMaterials.Hex("#1c1e21"), null, 0.35f));
            int tanks = R() < 0.75f ? 1 + rng.Next(2) : 0;
            for (int i = 0; i < tanks; i++)
            {
                float tx = x + RR(-w / 2 + 2, w / 2 - 2), tz = z + RR(-d / 2 + 2, d / 2 - 2);
                trim.Solid(tx, tz, roof, 1.9f, 0.6f, 1.9f);
                tank.Add(cyl, Matrix4x4.TRS(new Vector3(tx, roof + 0.6f + 0.85f, tz), Quaternion.identity, new Vector3(1.7f, 0.85f, 1.7f)));
            }
            if (R() < 0.6f)
            {
                var ac = B("aire", FGMaterials.Lit("aire", FGMaterials.Hex("#b4b8bb"), FGTextures.Warehouse, 0.4f, 0.6f));
                ac.Solid(x + RR(-w / 2 + 2, w / 2 - 2), z + RR(-d / 2 + 2, d / 2 - 2), roof, 1.3f, 0.9f, 0.9f);
            }
            if (R() < 0.35f)
                B("antena", FGMaterials.Lit("antena", FGMaterials.Hex("#8d9399"), null, 0.5f, 0.9f))
                    .Add(cyl, Matrix4x4.TRS(new Vector3(x + RR(-w / 3, w / 3), roof + 2f, z + RR(-d / 3, d / 3)), Quaternion.identity, new Vector3(0.07f, 2f, 0.07f)));
        }

        // ---------- Mobiliario urbano ----------
        // Postes de luz, postes de concreto con el enredo de cables tan bogotano y carros parqueados (muchos taxis amarillos).
        void Dressing(int row, int col, char code, Vector3 c)
        {
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            Mesh cube = MeshBuilder.Primitive(PrimitiveType.Cube);
            const float edge = Block / 2 - 0.6f;
            var faces = new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

            // Postes de luz en la mitad de cada cuadra, con la lámpara hacia la calle
            var pole = B("posteLuz", FGMaterials.Lit("posteLuz", FGMaterials.Hex("#3a3f44"), null, 0.5f, 0.8f));
            var lampMat = FGMaterials.Emissive("lampara", FGMaterials.Hex("#ffd59a"), 3f);
            RegisterGlow(lampMat, FGMaterials.Hex("#ffd59a"), 0.2f, 5f);
            var lamp = B("lampara", lampMat);
            foreach (var n in faces)
            {
                Vector3 b = c + n * edge;
                pole.Add(cyl, Matrix4x4.TRS(new Vector3(b.x, 0.25f + 3.75f, b.z), Quaternion.identity, new Vector3(0.18f, 3.75f, 0.18f)));
                Vector3 arm = b + n * 0.9f;
                pole.Solid(arm.x, arm.z, 7.4f, Mathf.Abs(n.x) > 0 ? 1.8f : 0.1f, 0.1f, Mathf.Abs(n.z) > 0 ? 1.8f : 0.1f);
                Vector3 head = b + n * 1.7f;
                lamp.Solid(head.x, head.z, 7.2f, Mathf.Abs(n.x) > 0 ? 0.7f : 0.35f, 0.2f, Mathf.Abs(n.z) > 0 ? 0.7f : 0.35f);
                lampHeads.Add(new Vector3(head.x, 7.1f, head.z));
                Collider(new Vector3(b.x, 3.5f, b.z), new Vector3(0.35f, 7f, 0.35f));
            }

            // Postes de concreto en las esquinas y cables colgando entre ellos
            if (code != 'T' && code != 'I' && code != 'B' && code != 'F')
            {
                var concrete = B("posteConcreto", FGMaterials.Lit("posteConcreto", FGMaterials.Hex("#a7a39b"), FGTextures.ConcreteBare, 0.1f));
                var wire = B("cable", FGMaterials.Lit("cable", FGMaterials.Hex("#111214"), null, 0.3f));
                var corners = new[] { new Vector3(edge, 0, edge), new Vector3(-edge, 0, edge), new Vector3(-edge, 0, -edge), new Vector3(edge, 0, -edge) };
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = c + corners[i];
                    concrete.Add(cyl, Matrix4x4.TRS(new Vector3(p.x, 0.25f + 4.5f, p.z), Quaternion.identity, new Vector3(0.28f, 4.5f, 0.28f)));
                    concrete.Solid(p.x, p.z, 8.4f, 1.6f, 0.15f, 0.15f);
                    Collider(new Vector3(p.x, 4.5f, p.z), new Vector3(0.4f, 9f, 0.4f));
                    Vector3 q = c + corners[(i + 1) % 4];
                    int count = 2 + rng.Next(2);
                    for (int k = 0; k < count; k++)
                        Cable(wire, cube, new Vector3(p.x, 8.5f - k * 0.35f, p.z), new Vector3(q.x, 8.5f - k * 0.35f, q.z), RR(0.8f, 1.6f));
                }
                // Cruces de calle hacia la manzana vecina (no sobre la Caracas)
                if (col + 1 < FGConfig.Grid && col != 5 && R() < 0.5f)
                {
                    Vector3 a = c + corners[0], b = CellCenter(row, col + 1) + corners[1];
                    Cable(wire, cube, a + Vector3.up * 8.6f, b + Vector3.up * 8.6f, RR(0.6f, 1.2f));
                }
                if (row + 1 < FGConfig.Grid && R() < 0.5f)
                {
                    Vector3 a = c + corners[2], b = CellCenter(row + 1, col) + corners[1];
                    Cable(wire, cube, a + Vector3.up * 8.6f, b + Vector3.up * 8.6f, RR(0.6f, 1.2f));
                }
            }

            // Carros parqueados junto al andén (no en la Caracas ni frente a la Sede)
            if (row == FGConfig.HQ.y && col == FGConfig.HQ.x) return;
            for (int f = 0; f < 4; f++)
            {
                if ((f == 0 && col == 5) || (f == 1 && col == 6)) continue;
                Vector3 n = faces[f];
                Vector3 along = Vector3.Cross(Vector3.up, n);
                for (float t = -16; t <= 16; t += 5.5f)
                    if (R() < 0.2f) Car(c + n * (Block / 2 + 1.2f) + along * t, along);
            }
        }

        /// <summary>Cable colgante: tramos delgados que siguen una parábola.</summary>
        void Cable(MeshBuilder mb, Mesh cube, Vector3 a, Vector3 b, float sag)
        {
            const int segs = 6;
            Vector3 prev = a;
            for (int i = 1; i <= segs; i++)
            {
                float t = (float)i / segs;
                Vector3 p = Vector3.Lerp(a, b, t) + Vector3.down * sag * 4 * t * (1 - t);
                Vector3 dir = p - prev;
                mb.Add(cube, Matrix4x4.TRS((p + prev) / 2, Quaternion.LookRotation(dir), new Vector3(0.035f, 0.035f, dir.magnitude)));
                prev = p;
            }
        }

        static readonly string[] CarColors = { "#f2c014", "#f2c014", "#f2c014", "#e9eaec", "#b6bcc2", "#8e1a1a", "#17191c", "#1f3d70", "#5b6168" };

        /// <summary>Carro bajo-poligonal con pintura brillante (refleja la ciudad), vidrios oscuros y llantas.</summary>
        void Car(Vector3 p, Vector3 dir)
        {
            string hex = Pick(CarColors);
            var paint = B("auto" + hex, FGMaterials.Lit("auto" + hex, FGMaterials.Hex(hex), null, 0.82f, 0.45f));
            var glass = B("autoVidrio", FGMaterials.Lit("autoVidrio", FGMaterials.Hex("#141a20"), null, 0.95f, 0.3f));
            var tire = B("llanta", FGMaterials.Lit("llanta", FGMaterials.Hex("#121212"), null, 0.2f));
            var lightsMat = FGMaterials.Emissive("farolas", FGMaterials.Hex("#ffe9c4"), 1.5f);
            RegisterGlow(lightsMat, FGMaterials.Hex("#ffe9c4"), 0.3f, 2.5f);
            var lights = B("farolas", lightsMat);
            bool alongZ = Mathf.Abs(dir.z) > 0.5f;
            float L = 4.3f, W = 1.8f;
            float sx = alongZ ? W : L, sz = alongZ ? L : W;
            paint.Solid(p.x, p.z, 0.32f, sx, 0.72f, sz);
            Vector3 cab = p - dir * 0.25f;
            glass.Solid(cab.x, cab.z, 1.04f, alongZ ? 1.6f : 2.2f, 0.6f, alongZ ? 2.2f : 1.6f);
            paint.Solid(cab.x, cab.z, 1.62f, alongZ ? 1.55f : 2.0f, 0.06f, alongZ ? 2.0f : 1.55f);
            Vector3 front = p + dir * (L / 2 + 0.01f), side = Vector3.Cross(Vector3.up, dir);
            foreach (float s2 in new[] { -0.6f, 0.6f })
            {
                Vector3 hl = front + side * s2;
                lights.Solid(hl.x, hl.z, 0.75f, alongZ ? 0.35f : 0.04f, 0.14f, alongZ ? 0.04f : 0.35f);
            }
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            Quaternion wheelRot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 0, 90);
            foreach (float a in new[] { -1.35f, 1.35f })
                foreach (float s2 in new[] { -0.82f, 0.82f })
                {
                    Vector3 wp = p + dir * a + side * s2;
                    tire.Add(cyl, Matrix4x4.TRS(new Vector3(wp.x, 0.34f, wp.z), wheelRot, new Vector3(0.66f, 0.13f, 0.66f)));
                }
            Collider(new Vector3(p.x, 0.85f, p.z), new Vector3(sx + 0.1f, 1.7f, sz + 0.1f));
        }

        Material FacadeMaterial(string mat, int variant)
        {
            var m = FacadeBase(mat, variant);
            // ventanas encendidas de noche (el mapa de emisión trae el color; aquí solo la intensidad)
            if (FGMaterials.HasWindows(m)) RegisterGlow(m, Color.white, 0f, mat == "vidrio" ? 1.7f : 1.4f);
            return m;
        }

        Material FacadeBase(string mat, int variant)
        {
            switch (mat)
            {
                case "ladrillo":
                    return FGMaterials.Lit("ladrillo" + variant, Color.Lerp(Color.white, FGMaterials.Hex("#d8b8a8"), variant * 0.35f), FGTextures.Brick, 0.12f);
                case "vidrio":
                    return FGMaterials.Lit("vidrio" + variant, variant == 0 ? Color.white : FGMaterials.Hex("#dfe9f5"), FGTextures.Glass, 0.85f, 0.25f);
                case "concreto":
                    return FGMaterials.Lit("concreto" + variant, Color.white, FGTextures.Concrete, 0.1f);
                case "bodega":
                    return FGMaterials.Lit("bodega" + variant, variant == 0 ? Color.white : FGMaterials.Hex("#e6ecff"), FGTextures.Warehouse, 0.3f, 0.3f);
                default: // colonial: un color por variante
                    string[] colors = { "#ffffff", "#f6e27a", "#9fd3f0", "#f3b27a", "#d6f0c2", "#f7a6a6", "#ffd9a0" };
                    return FGMaterials.Lit("colonial" + variant, FGMaterials.Hex(colors[variant % colors.Length]), FGTextures.Stucco, 0.1f);
            }
        }

        float Floors(float a, float b) => Mathf.Round(RR(a, b) / 3f) * 3f;

        void Grid(Vector3 c, int n, System.Action<float, float, float> fn)
        {
            float s = Lot * 2 / n;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    fn(c.x - Lot + s * (i + 0.5f), c.z - Lot + s * (j + 0.5f), s - 0.02f);
        }

        void Buildings(char code, Vector3 c)
        {
            switch (code)
            {
                case 'T':
                    if (R() < 0.5f) Building("vidrio", rng.Next(2), c.x, c.z, 30, Floors(33, 75), 30);
                    else
                    {
                        Building("vidrio", 1, c.x - 9, c.z, 16, Floors(24, 60), 34);
                        Building("vidrio", 0, c.x + 9, c.z, 16, Floors(21, 54), 34);
                    }
                    break;
                case 'I':
                    Building("concreto", 0, c.x, c.z, 36, 9, 36);
                    Building("vidrio", 1, c.x, c.z, 24, Floors(45, 105), 24);
                    break;
                case 'C':
                    Grid(c, 2, (x, z, s) => Building(R() < 0.85f ? "ladrillo" : "concreto", rng.Next(3), x, z, s, Floors(12, 27), s));
                    break;
                case 'U':
                    Grid(c, 3, (x, z, s) => Building("ladrillo", rng.Next(3), x, z, s, Floors(6, 10), s));
                    break;
                case 'L':
                    Grid(c, 3, (x, z, s) =>
                    {
                        float h = Floors(5, 9);
                        Building("colonial", rng.Next(7), x, z, s, h, s);
                        B("teja", FGMaterials.Lit("#a8492f", 0.2f)).Box(x, z, h + 0.25f, s + 0.8f, 0.5f, s + 0.8f);
                    });
                    break;
                case 'A':
                    if (R() < 0.5f) Building("bodega", rng.Next(2), c.x, c.z, 36, Floors(8, 13), 36);
                    else
                    {
                        Building("bodega", 0, c.x, c.z - 9, 36, Floors(8, 12), 17);
                        Building("bodega", 1, c.x, c.z + 9, 36, Floors(6, 10), 17);
                    }
                    break;
                default:
                    Grid(c, 2, (x, z, s) => Building(R() < 0.6f ? "ladrillo" : "concreto", rng.Next(3), x, z, s, Floors(9, 22), s));
                    break;
            }
        }

        // Parque Simón Bolívar: el pasto cubre también las calles internas; lago con balsas flotantes
        void ParkSlab(int row, int col, Vector3 c)
        {
            bool IsP(int r, int k) => r >= 0 && k >= 0 && r < FGConfig.Grid && k < FGConfig.Grid && DistrictCode(r, k) == 'P';
            float half = Block / 2, full = Cell / 2;
            float x0 = c.x - (IsP(row, col - 1) ? full : half), x1 = c.x + (IsP(row, col + 1) ? full : half);
            float z1 = c.z + (IsP(row - 1, col) ? full : half), z0 = c.z - (IsP(row + 1, col) ? full : half);
            var grass = B("pasto", FGMaterials.Lit("pasto", Color.white, FGTextures.Grass, 0.05f));
            grass.Box((x0 + x1) / 2, (z0 + z1) / 2, 0, x1 - x0, 0.25f, z1 - z0, 8, 8);
            Collider(new Vector3((x0 + x1) / 2, 0.125f, (z0 + z1) / 2), new Vector3(x1 - x0, 0.25f, z1 - z0));
            if (IsP(row, col + 1) && IsP(row + 1, col) && IsP(row + 1, col + 1))
                grass.Box(c.x + full, c.z - full, 0, FGConfig.Street, 0.25f, FGConfig.Street, 8, 8);
            parkCells.Add(new Vector2(c.x, c.z));

            if (row == 1 && col == 2 && !hasLake)
            {
                hasLake = true;
                lakeCenter = new Vector3(c.x + Cell / 2, 0.3f, c.z - 6);
                lakeRadius = new Vector2(38, 24);
                var lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(lake.GetComponent<UnityEngine.Collider>());
                lake.name = "Lago";
                lake.transform.SetParent(transform, false);
                lake.transform.position = lakeCenter;
                lake.transform.localScale = new Vector3(lakeRadius.x * 2, 0.02f, lakeRadius.y * 2);
                waterMat = FGMaterials.Lit("agua", Color.white, FGTextures.Water, 0.92f);
                waterMat.mainTextureScale = new Vector2(6, 6);
                lake.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
                // muro invisible alrededor del lago
                int n = 28;
                for (int i = 0; i < n; i++)
                {
                    float a = i * Mathf.PI * 2 / n;
                    var wall = new GameObject("Orilla");
                    wall.transform.SetParent(colliders.transform, false);
                    wall.transform.position = lakeCenter + new Vector3(Mathf.Cos(a) * lakeRadius.x, 1, Mathf.Sin(a) * lakeRadius.y);
                    Vector3 tangent = new Vector3(-Mathf.Sin(a) * lakeRadius.x, 0, Mathf.Cos(a) * lakeRadius.y);
                    wall.transform.rotation = Quaternion.LookRotation(tangent);
                    var bc = wall.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.5f, 2.5f, tangent.magnitude * Mathf.PI * 2 / n + 0.5f);
                }
            }
        }

        public bool InLake(Vector3 p, float pad = 0)
        {
            if (!hasLake) return false;
            float dx = (p.x - lakeCenter.x) / (lakeRadius.x + pad), dz = (p.z - lakeCenter.z) / (lakeRadius.y + pad);
            return dx * dx + dz * dz < 1;
        }

        void ZonePlaza(FGConfig.ZoneDef zone, Vector3 c, bool park)
        {
            if (!park) B("empedrado", FGMaterials.Lit("empedrado", Color.white, FGTextures.Cobble, 0.2f)).Top(c.x, c.z, 0.27f, Block - 4, Block - 4);
            var wood = B("madera", FGMaterials.Lit("#6b4a30", 0.2f));
            wood.Box(c.x - 14, c.z, 0.3f, 1, 0.5f, 5); wood.Box(c.x + 14, c.z, 0.3f, 1, 0.5f, 5);
            wood.Box(c.x, c.z - 14, 0.3f, 5, 0.5f, 1); wood.Box(c.x, c.z + 14, 0.3f, 5, 0.5f, 1);
            Vector3 center = new Vector3(c.x, 0.27f, c.z);
            var spots = new List<Vector3>();
            if (zone.id == "simon" && hasLake)
            {
                center = new Vector3(lakeCenter.x - 8, 0.25f, lakeCenter.z - lakeRadius.y - 10);
                foreach (float a in new[] { -0.75f, -0.55f, -0.35f, -0.15f })
                {
                    float ang = -Mathf.PI / 2 + a;
                    spots.Add(new Vector3(lakeCenter.x + Mathf.Cos(ang) * lakeRadius.x * 0.9f - 6, 0.32f, lakeCenter.z + Mathf.Sin(ang) * lakeRadius.y * 0.9f));
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI / 2 + Mathf.PI / 4;
                    spots.Add(new Vector3(c.x + Mathf.Cos(a) * 8, 0.3f, c.z + Mathf.Sin(a) * 8));
                }
            }
            zoneCenters[zone.id] = center;
            planterSpots[zone.id] = spots;
            foreach (var t in new[] { new Vector2(-16, -16), new Vector2(16, -16), new Vector2(-16, 16), new Vector2(16, 16) })
                treeSpots.Add(new Vector3(c.x + t.x, 0.25f, c.z + t.y));
        }

        void HQ(Vector3 c)
        {
            B("empedrado", FGMaterials.Lit("empedrado", Color.white, FGTextures.Cobble, 0.2f)).Top(c.x, c.z, 0.27f, Block - 4, Block - 4);
            // edificio de ladrillo con huerta en la azotea (lado norte)
            plainRoof = true; // la azotea es la huerta y el letrero va en la fachada sur
            Building("ladrillo", 0, c.x, c.z + 12, 34, 15, 14);
            plainRoof = false;
            var leaf = B("hojaHQ", FGMaterials.Lit("#2f8f45", 0.1f));
            for (int i = -3; i <= 3; i++) leaf.Box(c.x + i * 4.5f, c.z + 12, 15.25f, 3, 1.4f, 10);
            var ledHQ = GrowLight();
            for (int i = -3; i <= 3; i++) B("ledCultivo", ledHQ).Solid(c.x + i * 4.5f, c.z + 12, 17.3f, 0.25f, 0.06f, 10);
            neonSpots.Add(new NeonSpot { pos = new Vector3(c.x, 17.5f, c.z + 12), color = FGMaterials.Hex("#ff4fd8") });
            Sign("FLOAT GARDENS", new Vector3(c.x, 11, c.z + 4.9f), 0.55f, FGMaterials.Hex("#7dffb0"), FGMaterials.Hex("#0f3d2e"), new Vector2(20, 4));
            // piscina hidropónica (transitable, agua poco profunda)
            B("borde", FGMaterials.Lit("#9fb7a8", 0.2f)).Walls(c.x - 4, c.z - 7, 0.25f, 22, 0.6f, 11);
            var pool = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(pool.GetComponent<UnityEngine.Collider>());
            pool.name = "PiscinaHidroponica";
            pool.transform.SetParent(transform, false);
            pool.transform.position = new Vector3(c.x - 4, 0.75f, c.z - 7);
            pool.transform.rotation = Quaternion.Euler(90, 0, 0);
            pool.transform.localScale = new Vector3(21.5f, 10.5f, 1);
            if (waterMat == null) { waterMat = FGMaterials.Lit("agua", Color.white, FGTextures.Water, 0.92f); waterMat.mainTextureScale = new Vector2(6, 6); }
            pool.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
            var spots = new List<Vector3>();
            for (int i = 0; i < 6; i++) spots.Add(new Vector3(c.x - 12 + (i % 3) * 8, 0.8f, c.z - 4.5f - (i / 3) * 5));
            planterSpots["hq"] = spots;
            // kiosco de la tienda
            var wood = B("madera", FGMaterials.Lit("#6b4a30", 0.2f));
            wood.Box(c.x + 13, c.z - 7, 0.3f, 6, 1.2f, 3);
            B("toldoHQ", FGMaterials.Lit("#2fbf71", 0.3f)).Box(c.x + 13, c.z - 7, 3.4f, 7, 0.3f, 4);
            var steel = B("acero", FGMaterials.Lit("#dddddd", 0.6f));
            foreach (var p in new[] { new Vector2(-3.2f, -1.8f), new Vector2(3.2f, -1.8f), new Vector2(-3.2f, 1.8f), new Vector2(3.2f, 1.8f) })
                steel.Box(c.x + 13 + p.x, c.z - 7 + p.y, 0.3f, 0.2f, 3.1f, 0.2f);
            Collider(new Vector3(c.x + 13, 0.9f, c.z - 7), new Vector3(6, 1.2f, 3));
            hqCenter = c;
            hqShop = new Vector3(c.x + 13, 0.27f, c.z - 10);
            // El PC del barista: escritorio con monitor encendido, donde se afinan las recetas (Laboratorio AeroPress)
            var desk = wood;
            desk.Solid(c.x + 14, c.z - 15.5f, 0.3f, 2.6f, 0.08f, 1.2f);
            desk.Solid(c.x + 12.9f, c.z - 15.5f, 0.3f, 0.1f, 0.78f, 1.1f);
            desk.Solid(c.x + 15.1f, c.z - 15.5f, 0.3f, 0.1f, 0.78f, 1.1f);
            desk.Solid(c.x + 14, c.z - 15.5f, 1.08f, 2.6f, 0.08f, 1.2f);
            var screen = FGMaterials.Emissive("pantallaPC", FGMaterials.Hex("#7dffb0"), 1.4f);
            RegisterGlow(screen, FGMaterials.Hex("#7dffb0"), 1.2f, 2.6f);
            B("pantallaPC", screen).Solid(c.x + 14, c.z - 15.1f, 1.3f, 1.1f, 0.65f, 0.04f);
            B("plasticoPC", FGMaterials.Lit("plasticoPC", FGMaterials.Hex("#1c1e22"), null, 0.4f)).Solid(c.x + 14, c.z - 15.05f, 1.16f, 0.3f, 0.14f, 0.2f);
            Sign("FLOAT GARDENS · LAB", new Vector3(c.x + 14, 1.62f, c.z - 15.13f), 0.5f, FGMaterials.Hex("#0b1a14"), new Color(0, 0, 0, 0), new Vector2(1.1f, 0.3f));
            Collider(new Vector3(c.x + 14, 0.6f, c.z - 15.5f), new Vector3(2.6f, 1.2f, 1.2f));
            hqLab = new Vector3(c.x + 14, 0.27f, c.z - 17.3f);
            hqSpawn = new Vector3(c.x - 2, 0.3f, c.z - 16);
            treeSpots.Add(new Vector3(c.x - 17, 0.25f, c.z - 17));
        }

        void Torre(Vector3 c)
        {
            Building("concreto", 0, c.x, c.z, 36, 9, 36);
            Building("vidrio", 1, c.x, c.z, 22, 140, 22);
            crownMat = FGMaterials.Instance(FGMaterials.Emissive("corona", FGMaterials.Hex("#ff3366"), 3f));
            var crown = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(crown.GetComponent<UnityEngine.Collider>());
            crown.name = "CoronaTorre";
            crown.transform.SetParent(transform, false);
            crown.transform.position = new Vector3(c.x, 136, c.z);
            crown.transform.localScale = new Vector3(23, 10, 23);
            crown.GetComponent<MeshRenderer>().sharedMaterial = crownMat;
        }

        void PlazaBolivar(Vector3 c)
        {
            var stone = B("piedra", FGMaterials.Lit("#cfc8bb", 0.2f));
            stone.Box(c.x, c.z, 0.25f, 4, 2.5f, 4);
            B("bronce", FGMaterials.Lit("statue", FGMaterials.Hex("#5d6b5a"), null, 0.6f, 0.8f)).Box(c.x, c.z, 2.75f, 1.4f, 3, 1.4f);
            Collider(new Vector3(c.x, 1.4f, c.z), new Vector3(4, 2.8f, 4));
            bossArena = new Vector3(c.x, 0.25f, c.z);
        }

        void Sede(Vector3 c)
        {
            var gray = B("sede", FGMaterials.Lit("sede", FGMaterials.Hex("#d6d1c6"), FGTextures.Concrete, 0.1f));
            gray.Walls(c.x, c.z + 4, 0.25f, 36, 18, 26);
            B("techo", FGMaterials.Lit("techo", Color.white, FGTextures.Roof, 0.05f)).Top(c.x, c.z + 4, 18.25f, 36, 26);
            Collider(new Vector3(c.x, 9.25f, c.z + 4), new Vector3(36, 18, 26));
            var marble = FGMaterials.Lit("marmol", FGMaterials.Hex("#ece8de"), null, 0.55f);
            var cols = B("columnas", marble);
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            for (int i = 0; i < 8; i++)
            {
                float x = c.x - 14 + i * 4;
                cols.Add(cyl, Matrix4x4.TRS(new Vector3(x, 7.25f, c.z - 12), Quaternion.identity, new Vector3(1.9f, 7f, 1.9f)));
                Collider(new Vector3(x, 7.25f, c.z - 12), new Vector3(2, 14, 2));
            }
            B("friso", marble).Box(c.x, c.z - 11, 14.25f, 34, 2, 5);
            // frontón triangular
            var ped = B("fronton", marble);
            Vector3 a = new Vector3(c.x - 17, 16.25f, c.z - 13.5f), bpt = new Vector3(c.x + 17, 16.25f, c.z - 13.5f), top = new Vector3(c.x, 21.25f, c.z - 13.5f);
            ped.Quad(a, bpt, top, top, Vector3.back, Vector2.zero, Vector2.one);
            Vector3 a2 = a + Vector3.forward * 5, b2 = bpt + Vector3.forward * 5, t2 = top + Vector3.forward * 5;
            ped.Quad(a, top, t2, a2, new Vector3(-0.3f, 1, 0).normalized, Vector2.zero, Vector2.one);
            ped.Quad(top, bpt, b2, t2, new Vector3(0.3f, 1, 0).normalized, Vector2.zero, Vector2.one);
            Sign("COFFEE LAND S.A.", new Vector3(c.x, 15.25f, c.z - 13.6f), 0.45f, FGMaterials.Hex("#fff3dc"), FGMaterials.Hex("#8f1f1a"), new Vector2(26, 1.8f));
            Neon("CAFÉ CERTIFICADO · SELLO OFICIAL", new Vector3(c.x, 19.6f, c.z - 9f), Vector3.back, 22, 1.4f, "#ff2bd6");
            var banner = FGMaterials.Lit("#c0392b", 0.3f);
            foreach (float bx in new[] { -16f, 16f }) B("banderas", banner).Box(c.x + bx, c.z - 9.2f, 5, 3, 8, 0.1f);
        }

        /// <summary>Letrero en un Canvas de espacio mundo (respeta la profundidad, a diferencia de TextMesh). Mira hacia el sur.</summary>
        void Sign(string text, Vector3 pos, float charSize, Color fg, Color bg, Vector2 size, Vector3? facing = null, Transform parent = null)
        {
            var go = new GameObject("Letrero " + text, typeof(RectTransform));
            go.transform.SetParent(parent != null ? parent : transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 4;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = size * 100f;
            // el frente del canvas mira hacia -z; facing es la normal de la pared donde va
            Vector3 n = facing ?? Vector3.back;
            rt.position = pos + n * 0.03f;
            rt.rotation = Quaternion.LookRotation(-n);
            rt.localScale = Vector3.one * 0.01f;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.sprite = UIKit.White;
            img.color = bg;
            var label = UIKit.Label(go.transform, text, Mathf.RoundToInt(size.y * 100 * charSize * 1.2f), fg, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, size * 100f);
            label.resizeTextForBestFit = true;
            label.resizeTextMaxSize = label.fontSize;
            label.resizeTextMinSize = 10;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        // ---------- Agro-punk: neones, hologramas, invernaderos y drones ----------
        Material GrowLight()
        {
            var led = FGMaterials.Emissive("ledCultivo", FGMaterials.Hex("#ff4fd8"), 1f);
            RegisterGlow(led, FGMaterials.Hex("#ff4fd8"), 0.9f, 3.8f);
            return led;
        }

        /// <summary>Invernadero de vidrio con camas hidropónicas y LEDs rosados de cultivo sobre una azotea.</summary>
        void Greenhouse(float x, float z, float y, float w, float d)
        {
            float gw = w * 0.62f, gd = d * 0.62f, gh = 2.8f;
            var glass = B("invernaderoVidrio", FGMaterials.Transparent("invernaderoVidrio", new Color(0.78f, 0.95f, 0.88f, 0.26f), true));
            noShadow.Add("invernaderoVidrio");
            glass.Box(x, z, y, gw, gh, gd);
            var frame = B("invernaderoMarco", FGMaterials.Lit("invernaderoMarco", FGMaterials.Hex("#e8ecef"), null, 0.5f, 0.6f));
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    frame.Solid(x + sx * gw / 2, z + sz * gd / 2, y, 0.12f, gh, 0.12f);
            frame.Solid(x, z + gd / 2, y + gh - 0.1f, gw, 0.1f, 0.12f);
            frame.Solid(x, z - gd / 2, y + gh - 0.1f, gw, 0.1f, 0.12f);
            frame.Solid(x + gw / 2, z, y + gh - 0.1f, 0.12f, 0.1f, gd);
            frame.Solid(x - gw / 2, z, y + gh - 0.1f, 0.12f, 0.1f, gd);

            var bed = B("camaCultivo", FGMaterials.Lit("camaCultivo", FGMaterials.Hex("#2f8f45"), null, 0.1f));
            var tray = B("bandejaCultivo", FGMaterials.Lit("bandejaCultivo", FGMaterials.Hex("#f2f4f5"), null, 0.35f));
            var led = B("ledCultivo", GrowLight());
            int rows = Mathf.Max(2, Mathf.RoundToInt(gd / 2.4f));
            for (int r = 0; r < rows; r++)
            {
                float rz = z - gd / 2 + 1.2f + r * (gd - 2.4f) / (rows - 1);
                tray.Solid(x, rz, y, gw * 0.8f, 0.5f, 1f);
                bed.Solid(x, rz, y + 0.5f, gw * 0.78f, 0.45f, 0.9f);
                led.Solid(x, rz, y + gh - 0.3f, gw * 0.8f, 0.06f, 0.25f);
            }
            neonSpots.Add(new NeonSpot { pos = new Vector3(x, y + 1.6f, z), color = FGMaterials.Hex("#ff4fd8") });
        }

        /// <summary>Caja de luz de neón con el nombre de una marca, pegada a una fachada de normal n.</summary>
        void Neon(string text, Vector3 center, Vector3 n, float width, float height, string hex)
        {
            var color = FGMaterials.Hex(hex);
            var mat = FGMaterials.Emissive("neon" + hex, color, 1.5f);
            RegisterGlow(mat, color, 1.4f, 4.5f);
            bool alongX = Mathf.Abs(n.z) > 0.5f;
            B("neon" + hex, mat).Solid(center.x, center.z, center.y - height / 2, alongX ? width : 0.18f, height, alongX ? 0.18f : width);
            Sign(text, center + n * 0.09f, 0.62f, FGMaterials.Hex("#0a0a14"), new Color(0, 0, 0, 0), new Vector2(width, height), n);
            neonSpots.Add(new NeonSpot { pos = center + n * 1.6f, color = color });
        }

        /// <summary>Letreros y hologramas de cada edificio según su tipo y altura.</summary>
        void Signage(string mat, float x, float z, float w, float h, float d)
        {
            if (plainRoof) return;
            // Local en el primer piso
            if (h >= 6 && mat != "bodega" && R() < 0.5f)
            {
                int side = rng.Next(4);
                Vector3 n = Faces[side];
                float faceW = side < 2 ? d : w;
                float width = Mathf.Min(faceW * 0.7f, RR(4, 9));
                Vector3 right = Vector3.Cross(Vector3.up, -n);
                Vector3 center = new Vector3(x, 3.7f, z) + n * ((side < 2 ? w : d) / 2 + 0.1f) + right * RR(-(faceW - width) / 2, (faceW - width) / 2);
                Neon(Pick(Brands), center, n, width, 1.1f, Pick(NeonHex));
            }
            // Marca grande en lo alto de torres y oficinas
            if (h >= 24 && (mat == "vidrio" || mat == "concreto") && R() < 0.5f)
            {
                int side = rng.Next(4);
                Vector3 n = Faces[side];
                float faceW = side < 2 ? d : w;
                float width = Mathf.Min(faceW * 0.65f, 14f);
                Vector3 center = new Vector3(x, h - RR(4, 10), z) + n * ((side < 2 ? w : d) / 2 + 0.1f);
                Neon(Pick(Brands), center, n, width, 2.4f, Pick(NeonHex));
            }
            // Tiras verticales de neón en dos aristas de las torres
            if (h >= 45 && R() < 0.7f)
            {
                string hex = Pick(NeonHex);
                var color = FGMaterials.Hex(hex);
                var m = FGMaterials.Emissive("neon" + hex, color, 1.5f);
                RegisterGlow(m, color, 1.4f, 4.5f);
                var mb = B("neon" + hex, m);
                float sx = R() < 0.5f ? 1 : -1;
                mb.Solid(x + sx * (w / 2 + 0.12f), z + d / 2 + 0.12f, 8, 0.22f, h - 14, 0.22f);
                mb.Solid(x - sx * (w / 2 + 0.12f), z - d / 2 - 0.12f, 8, 0.22f, h - 14, 0.22f);
                neonSpots.Add(new NeonSpot { pos = new Vector3(x + sx * (w / 2 + 1.5f), 12, z + d / 2 + 1.5f), color = color });
            }
            // Holograma publicitario girando sobre la azotea
            if (h >= 40 && R() < 0.35f) Hologram(new Vector3(x, h + 9, z), Pick(Brands), Pick(NeonHex), 6f);
        }

        /// <summary>Panel holográfico translúcido que gira y parpadea sobre una azotea.</summary>
        void Hologram(Vector3 pos, string text, string hex, float size)
        {
            var color = FGMaterials.Hex(hex);
            var go = new GameObject("Holograma " + text);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, R() * 360, 0);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<UnityEngine.Collider>());
            quad.name = "Panel";
            quad.transform.SetParent(go.transform, false);
            quad.transform.localScale = new Vector3(size * 2.2f, size, 1);
            var mat = FGMaterials.Instance(FGMaterials.Transparent("holo" + hex, new Color(color.r, color.g, color.b, 0.3f)));
            var mr = quad.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Sign(text, Vector3.zero, 0.5f, color, new Color(0, 0, 0, 0), new Vector2(size * 2.2f, size), Vector3.back, go.transform);
            go.AddComponent<Hologram>().Init(mat, color);
            neonSpots.Add(new NeonSpot { pos = pos, color = color });
        }

        // Drones fumigadores de AgroCorp patrullando con reflectores sobre los barrios
        void BuildDrones()
        {
            int[][] routes = { new[] { 1, 2 }, new[] { 2, 8 }, new[] { 4, 4 }, new[] { 7, 9 }, new[] { 9, 2 }, new[] { 10, 7 } };
            for (int i = 0; i < routes.Length; i++)
            {
                Vector3 c = CellCenter(routes[i][0], routes[i][1]);
                Drone.Create(transform, c, RR(45, 80), RR(38, 60), RR(6, 10), R() * Mathf.PI * 2);
            }
        }

        // ---------- Calles ----------
        void BuildStreets()
        {
            var white = B("lineaBlanca", FGMaterials.Lit("#e8e8e0", 0.3f));
            var yellow = B("lineaAmarilla", FGMaterials.Lit("#e8c547", 0.3f));
            for (int k = 0; k <= FGConfig.Grid; k++)
            {
                float px = LineX(k), pz = LineZ(k);
                var mb = k % 3 == 0 ? yellow : white;
                for (int s = 0; s < FGConfig.Grid; s++)
                {
                    float a = -Half + s * Cell + FGConfig.Street / 2 + 1;
                    for (float t = a; t < a + Block - 2; t += 6)
                    {
                        if (k != 6) mb.Top(px, t + 1.5f, 0.03f, 0.25f, 3);
                        mb.Top(t + 1.5f, pz, 0.03f, 3, 0.25f);
                    }
                }
            }
            // cebras frente al HQ
            Vector3 hq = CellCenter(FGConfig.HQ.y, FGConfig.HQ.x);
            for (int i = -5; i <= 5; i++)
            {
                white.Top(hq.x + i * 1.2f, hq.z - Cell / 2 + FGConfig.Street / 2 - 1.5f, 0.04f, 0.6f, 2.5f);
                white.Top(hq.x + i * 1.2f, hq.z + Cell / 2 - FGConfig.Street / 2 + 1.5f, 0.04f, 0.6f, 2.5f);
            }
        }

        // Av. Caracas: carril exclusivo, estaciones y buses rojos articulados
        void BuildCaracas()
        {
            float x = LineX(6);
            B("carrilRojo", FGMaterials.Lit("#8a3b34", 0.15f)).Top(x, 0, 0.025f, 9, Half * 2);
            var white = B("lineaBlanca", FGMaterials.Lit("#e8e8e0", 0.3f));
            white.Top(x - 4.6f, 0, 0.04f, 0.3f, Half * 2);
            white.Top(x + 4.6f, 0, 0.04f, 0.3f, Half * 2);
            var glass = FGMaterials.Transparent("vidrioEstacion", new Color(0.62f, 0.82f, 0.91f, 0.45f), true);
            foreach (int row in new[] { 1, 4, 7, 10 })
            {
                float z = CellCenter(row, 6).z;
                B("concretoGris", FGMaterials.Lit("#9a9a9a", 0.1f)).Box(x, z, 0, 2.4f, 0.4f, 26);
                B("techoEstacion", FGMaterials.Lit("#c0392b", 0.4f)).Box(x, z, 3.2f, 2.8f, 0.3f, 26);
                var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(g.GetComponent<UnityEngine.Collider>());
                g.name = "Estacion";
                g.transform.SetParent(transform, false);
                g.transform.position = new Vector3(x, 1.8f, z);
                g.transform.localScale = new Vector3(2.2f, 2.8f, 25);
                g.GetComponent<MeshRenderer>().sharedMaterial = glass;
                Collider(new Vector3(x, 1.75f, z), new Vector3(2.4f, 3.5f, 26));
                stationZ.Add(z);
            }
            for (int i = 0; i < 4; i++)
            {
                int dir = i % 2 == 1 ? 1 : -1;
                float lane = x + (dir > 0 ? 2.6f : -2.6f);
                var bus = Bus.Create(transform, lane, -Half + ((i * 0.27f) % 1f) * Half * 2, dir);
                buses.Add(bus);
            }
        }

        // Cerros orientales con Monserrate, su santuario blanco y el teleférico
        void BuildMountains()
        {
            var green = B("cerro1", FGMaterials.Lit("#3d6b3a", 0.05f));
            var green2 = B("cerro2", FGMaterials.Lit("#2f5a33", 0.05f));
            float[][] peaks =
            {
                new[] { 430f, 380, 130, 150 }, new[] { 470f, 260, 150, 190 }, new[] { 440f, 120, 120, 160 }, new[] { 455f, -20, 140, 185 },
                new[] { 440f, -150, 130, 210 }, new[] { 470f, -300, 150, 180 }, new[] { 430f, -430, 120, 140 }, new[] { 560f, 200, 160, 230 }, new[] { 580f, -150, 170, 240 },
            };
            Mesh cone = MeshBuilder.Cone(7, 1, 1);
            for (int i = 0; i < peaks.Length; i++)
            {
                var p = peaks[i];
                (i % 2 == 0 ? green2 : green).Add(cone, Matrix4x4.TRS(new Vector3(p[0], -2, p[1]), Quaternion.Euler(0, i * 57, 0), new Vector3(p[2], p[3], p[2])));
            }
            // Monserrate: pico de 210 m al oriente de La Candelaria
            var white = B("santuario", FGMaterials.Lit("#f5f5f0", 0.3f));
            white.Box(440, -150, 198, 14, 8, 8);
            white.Box(434, -150, 198, 4, 16, 4);
            cableStart = new Vector3(352, 2, -170);
            cableEnd = new Vector3(432, 196, -152);
            var lineGo = new GameObject("CableTeleferico");
            lineGo.transform.SetParent(transform, false);
            var lr = lineGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[] { cableStart, cableEnd });
            lr.widthMultiplier = 0.15f;
            lr.sharedMaterial = FGMaterials.Lit("#222222", 0.2f);
            var cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(cab.GetComponent<UnityEngine.Collider>());
            cab.name = "Teleferico";
            cab.transform.SetParent(transform, false);
            cab.transform.localScale = new Vector3(3, 2.4f, 3);
            cab.GetComponent<MeshRenderer>().sharedMaterial = FGMaterials.Lit("#d6423a", 0.4f);
            cabin = cab.transform;
        }

        static Mesh icosa;
        static Mesh Icosahedron()
        {
            if (icosa != null) return icosa;
            float t = (1 + Mathf.Sqrt(5)) / 2;
            Vector3[] v =
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            int[] f = { 0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8, 3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1 };
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int i = 0; i < f.Length; i += 3)
            {
                Vector3 a = v[f[i]].normalized, b = v[f[i + 1]].normalized, c = v[f[i + 2]].normalized;
                // orden para que la cara mire hacia afuera
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), a + b + c) < 0) { var tmp = b; b = c; c = tmp; }
                int k = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c);
                tris.Add(k); tris.Add(k + 1); tris.Add(k + 2);
            }
            icosa = new Mesh { name = "Icosaedro" };
            icosa.SetVertices(verts);
            icosa.SetTriangles(tris, 0);
            icosa.SetUVs(0, new List<Vector2>(new Vector2[verts.Count]));
            icosa.RecalculateNormals();
            return icosa;
        }

        void BuildTrees()
        {
            foreach (var p in parkCells)
                for (int i = 0; i < 14; i++)
                {
                    var t = new Vector3(p.x + RR(-26, 26), 0.25f, p.y + RR(-26, 26));
                    if (InLake(t, 4)) continue;
                    treeSpots.Add(t);
                }
            for (int row = 0; row < FGConfig.Grid; row++)
                for (int col = 0; col < FGConfig.Grid; col++)
                {
                    char code = DistrictCode(row, col);
                    if (code != 'U' && code != 'C' && code != 'T') continue;
                    Vector3 c = CellCenter(row, col);
                    foreach (var d in new[] { new Vector2(-19, -10), new Vector2(-19, 10), new Vector2(19, -10), new Vector2(19, 10), new Vector2(-10, -19), new Vector2(10, -19) })
                        if (R() < 0.5f) treeSpots.Add(new Vector3(c.x + d.x, 0.25f, c.z + d.y) + Vector3.up * 0.0001f);
                }
            var trunks = B("troncos", FGMaterials.Lit("#6b4a30", 0.1f));
            var leaves = new[]
            {
                B("hojas0", FGMaterials.Lit("#3f8f3f", 0.05f)), B("hojas1", FGMaterials.Lit("#2f7a3a", 0.05f)),
                B("hojas2", FGMaterials.Lit("#4f9f45", 0.05f)), B("hojas3", FGMaterials.Lit("#5a8f3a", 0.05f)),
            };
            Mesh cyl = MeshBuilder.Primitive(PrimitiveType.Cylinder);
            Mesh ico = Icosahedron();
            foreach (var s in treeSpots)
            {
                bool small = s.y > 0.25f;
                float k = small ? 0.7f : RR(0.9f, 1.5f);
                trunks.Add(cyl, Matrix4x4.TRS(new Vector3(s.x, 1.5f * k + 0.25f, s.z), Quaternion.identity, new Vector3(0.6f * k, 1.5f * k, 0.6f * k)));
                leaves[rng.Next(leaves.Length)].Add(ico, Matrix4x4.TRS(new Vector3(s.x, 4f * k + 0.25f, s.z), Quaternion.Euler(0, R() * 360, 0), new Vector3(2.2f * k, 2.5f * k, 2.2f * k)));
                Collider(new Vector3(s.x, 1.5f, s.z), new Vector3(0.6f, 3, 0.6f));
            }
        }

        void BuildBounds()
        {
            float e = Half + 10;
            Collider(new Vector3(0, 20, e + 1), new Vector3(e * 2 + 4, 40, 2));
            Collider(new Vector3(0, 20, -e - 1), new Vector3(e * 2 + 4, 40, 2));
            Collider(new Vector3(e + 1, 20, 0), new Vector3(2, 40, e * 2 + 4));
            Collider(new Vector3(-e - 1, 20, 0), new Vector3(2, 40, e * 2 + 4));
        }

        // ---------- Minimapa (1 px por metro; norte arriba) ----------
        void BuildMinimap()
        {
            int size = Mathf.CeilToInt(Half * 2 + 40);
            float o = Half + 20;
            var px = new Color32[size * size];
            Color32 road = new Color32(0x2b, 0x30, 0x36, 255);
            for (int i = 0; i < px.Length; i++) px[i] = road;
            void Fill(float x0, float z0, float x1, float z1, Color32 c)
            {
                int ix0 = Mathf.Clamp((int)(x0 + o), 0, size), ix1 = Mathf.Clamp((int)(x1 + o), 0, size);
                int iz0 = Mathf.Clamp((int)(z0 + o), 0, size), iz1 = Mathf.Clamp((int)(z1 + o), 0, size);
                for (int z = iz0; z < iz1; z++)
                    for (int x = ix0; x < ix1; x++) px[z * size + x] = c;
            }
            for (int row = 0; row < FGConfig.Grid; row++)
                for (int col = 0; col < FGConfig.Grid; col++)
                {
                    var c = CellCenter(row, col);
                    char code = DistrictCode(row, col);
                    Color32 color;
                    switch (code)
                    {
                        case 'T': color = new Color32(0x5d, 0x7f, 0x99, 255); break;
                        case 'I': color = new Color32(0x6d, 0x8a, 0xa3, 255); break;
                        case 'C': color = new Color32(0x8f, 0x5a, 0x46, 255); break;
                        case 'U': color = new Color32(0x8a, 0x61, 0x50, 255); break;
                        case 'L': color = new Color32(0xc9, 0xa4, 0x6a, 255); break;
                        case 'A': color = new Color32(0x7c, 0x85, 0x8c, 255); break;
                        case 'P': color = new Color32(0x3f, 0x7a, 0x3a, 255); break;
                        case 'B': color = new Color32(0xa8, 0x9f, 0x90, 255); break;
                        case 'F': color = new Color32(0xa3, 0x3a, 0x30, 255); break;
                        default: color = new Color32(0x7e, 0x6f, 0x66, 255); break;
                    }
                    float s = code == 'P' ? Cell : Block;
                    Fill(c.x - s / 2, c.z - s / 2, c.x + s / 2, c.z + s / 2, color);
                }
            if (hasLake)
                for (int z = 0; z < size; z++)
                    for (int x = 0; x < size; x++)
                        if (InLake(new Vector3(x - o, 0, z - o))) px[z * size + x] = new Color32(0x3f, 0x8f, 0xb3, 255);
            Fill(LineX(6) - 2, -o, LineX(6) + 2, o, new Color32(0xb8, 0x44, 0x3a, 255));
            minimap = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Minimapa", wrapMode = TextureWrapMode.Clamp };
            minimap.SetPixels32(px);
            minimap.Apply();
        }

        /// <summary>Coordenadas UV (0..1) del minimapa para un punto del mundo.</summary>
        public Vector2 MinimapUV(Vector3 p)
        {
            float size = minimap.width;
            float o = Half + 20;
            return new Vector2((p.x + o) / size, (p.z + o) / size);
        }

        // ---------- Consultas ----------
        /// <summary>Punto aleatorio en un andén (peatones, botín y enemigos).</summary>
        public Vector3 SidewalkPoint(System.Func<char, bool> filter = null)
        {
            for (int tries = 0; tries < 40; tries++)
            {
                int row = Random.Range(0, FGConfig.Grid), col = Random.Range(0, FGConfig.Grid);
                char code = DistrictCode(row, col);
                if (filter != null && !filter(code)) continue;
                Vector3 c = CellCenter(row, col);
                float s = Block / 2 - 0.9f;
                float t = Random.Range(-s, s);
                Vector3 p;
                switch (Random.Range(0, 4))
                {
                    case 0: p = new Vector3(c.x + t, 0.25f, c.z - s); break;
                    case 1: p = new Vector3(c.x + t, 0.25f, c.z + s); break;
                    case 2: p = new Vector3(c.x - s, 0.25f, c.z + t); break;
                    default: p = new Vector3(c.x + s, 0.25f, c.z + t); break;
                }
                if (code == 'P' && InLake(p, 3)) continue;
                if (!Physics.CheckSphere(p + Vector3.up * 1f, 0.4f)) return p;
            }
            return new Vector3(0, 0, 0);
        }

        void Update()
        {
            float t = Time.time;
            if (crownMat != null) FGMaterials.SetEmission(crownMat, Color.HSVToRGB((t * 0.05f) % 1f, 0.8f, 1f), 3f);
            if (cabin != null)
            {
                float k = (Mathf.Sin(t * 0.08f) + 1) / 2;
                cabin.position = Vector3.Lerp(cableStart, cableEnd, k) + Vector3.down * 1.8f;
            }
            if (waterMat != null) waterMat.mainTextureOffset = new Vector2(t * 0.01f, t * 0.006f);
        }
    }
}
