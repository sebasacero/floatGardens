// Personaje humanoide low-poly hecho de primitivas, con animación procedural.
// Si asignas un modelo animado (p. ej. de Mixamo) en FGBootstrap, se usa ese en su lugar.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class HumanoidStyle
    {
        public Color shirt = Color.white, pants = new Color(0.17f, 0.16f, 0.15f), skin = new Color(0.85f, 0.64f, 0.45f);
        public Color? hair, apron, cap, tie, sash;
        public string hat; // "fedora", "beret", "tophat"
        public bool badge, mustache;
        public float scale = 1f;
    }

    public class Humanoid : MonoBehaviour
    {
        public Transform body, legL, legR, armL, armR, head, rightHand;
        public Animator animator; // si hay modelo animado
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int AimHash = Animator.StringToHash("Aiming");
        static readonly int DeadHash = Animator.StringToHash("Dead");

        public static Humanoid Create(string name, HumanoidStyle s, Transform parent = null, GameObject modelPrefab = null)
        {
            var root = new GameObject(name);
            if (parent) root.transform.SetParent(parent, false);
            var h = root.AddComponent<Humanoid>();
            h.body = new GameObject("Cuerpo").transform;
            h.body.SetParent(root.transform, false);
            h.body.localScale = Vector3.one * s.scale;

            if (modelPrefab != null)
            {
                var model = Instantiate(modelPrefab, h.body);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                h.animator = model.GetComponentInChildren<Animator>();
                h.rightHand = h.animator != null && h.animator.isHuman ? h.animator.GetBoneTransform(HumanBodyBones.RightHand) : model.transform;
                foreach (var c in model.GetComponentsInChildren<Collider>()) Destroy(c);
                return h;
            }

            // Cada parte rígida (cuerpo, piernas, brazos) es UNA malla: 5 renderers por personaje en vez de ~22.
            // Todos los personajes comparten un solo material con una paleta de colores, y las mallas se reutilizan
            // entre personajes con la misma ropa.
            var leg = new[] { new Piece(0.22f, 0.85f, 0.26f, s.pants, new Vector3(0, -0.42f, 0)) };
            h.legL = Pivot(h.body, new Vector3(-0.13f, 0.85f, 0), leg);
            h.legR = Pivot(h.body, new Vector3(0.13f, 0.85f, 0), leg);
            var arm = new[]
            {
                new Piece(0.17f, 0.66f, 0.2f, s.shirt, new Vector3(0, -0.3f, 0)),
                new Piece(0.15f, 0.14f, 0.17f, s.skin, new Vector3(0, -0.66f, 0)),
            };
            h.armL = Pivot(h.body, new Vector3(-0.4f, 1.52f, 0), arm);
            h.armR = Pivot(h.body, new Vector3(0.4f, 1.52f, 0), arm);
            h.rightHand = new GameObject("Mano").transform; // ancla sin escala para el arma
            h.rightHand.SetParent(h.armR, false);
            h.rightHand.localPosition = new Vector3(0, -0.66f, 0);

            var b = new List<Piece>
            {
                new Piece(0.62f, 0.72f, 0.34f, s.shirt, new Vector3(0, 1.22f, 0)), // torso
                new Piece(0.36f, 0.38f, 0.36f, s.skin, new Vector3(0, 1.79f, 0)),  // cabeza
                new Piece(0.06f, 0.06f, 0.02f, Color.black, new Vector3(-0.08f, 1.82f, 0.185f)),
                new Piece(0.06f, 0.06f, 0.02f, Color.black, new Vector3(0.08f, 1.82f, 0.185f)),
            };
            if (s.hair.HasValue) b.Add(new Piece(0.38f, 0.1f, 0.38f, s.hair.Value, new Vector3(0, 2.0f, 0)));
            if (s.apron.HasValue) b.Add(new Piece(0.5f, 0.75f, 0.04f, s.apron.Value, new Vector3(0, 1.1f, 0.19f)));
            if (s.tie.HasValue) b.Add(new Piece(0.08f, 0.4f, 0.03f, s.tie.Value, new Vector3(0, 1.32f, 0.18f)));
            if (s.mustache) b.Add(new Piece(0.2f, 0.05f, 0.03f, FGMaterials.Hex("#2a1a10"), new Vector3(0, 1.7f, 0.185f)));
            if (s.cap.HasValue)
            {
                b.Add(new Piece(0.4f, 0.14f, 0.4f, s.cap.Value, new Vector3(0, 2.03f, 0)));
                b.Add(new Piece(0.36f, 0.04f, 0.22f, s.cap.Value, new Vector3(0, 1.97f, 0.26f)));
            }
            Color red = FGMaterials.Hex("#c0392b");
            if (s.hat == "fedora")
            {
                b.Add(new Piece(0.6f, 0.04f, 0.6f, FGMaterials.Hex("#222222"), new Vector3(0, 1.99f, 0)));
                b.Add(new Piece(0.38f, 0.2f, 0.38f, FGMaterials.Hex("#222222"), new Vector3(0, 2.1f, 0)));
                b.Add(new Piece(0.39f, 0.05f, 0.39f, red, new Vector3(0, 2.03f, 0)));
            }
            else if (s.hat == "beret") b.Add(new Piece(0.44f, 0.1f, 0.44f, FGMaterials.Hex("#8f1f1a"), new Vector3(0.03f, 2.02f, 0)));
            else if (s.hat == "tophat")
            {
                b.Add(new Piece(0.62f, 0.04f, 0.62f, FGMaterials.Hex("#111111"), new Vector3(0, 1.99f, 0)));
                b.Add(new Piece(0.38f, 0.5f, 0.38f, FGMaterials.Hex("#111111"), new Vector3(0, 2.25f, 0)));
                b.Add(new Piece(0.39f, 0.07f, 0.39f, red, new Vector3(0, 2.05f, 0)));
            }
            if (s.sash.HasValue) b.Add(new Piece(0.08f, 0.85f, 0.36f, s.sash.Value, new Vector3(0, 1.22f, 0), 34));
            if (s.badge) b.Add(new Piece(0.1f, 0.1f, 0.03f, FGMaterials.Hex("#e6c34a"), new Vector3(0.17f, 1.4f, 0.18f)));
            AddMesh(h.body.gameObject, b);
            h.head = h.body; // la cabeza va fusionada con el cuerpo
            return h;
        }

        /// <summary>Una caja de color (tamaño, color, posición y giro en z, en grados).</summary>
        readonly struct Piece
        {
            public readonly Vector3 size, pos;
            public readonly Color color;
            public readonly float rollZ;
            public Piece(float w, float h, float d, Color c, Vector3 p, float rz = 0) { size = new Vector3(w, h, d); color = c; pos = p; rollZ = rz; }
        }

        // ---------- Paleta compartida: un solo material para todos los personajes ----------
        const int PaletteSize = 32; // 32 x 32 = 1024 colores distintos
        static Texture2D palette;
        static Material paletteMat;
        static readonly Dictionary<Color32, Vector2> paletteSlots = new Dictionary<Color32, Vector2>();
        static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();

        static Material PaletteMaterial()
        {
            if (paletteMat != null) return paletteMat;
            // sRGB, sin filtro ni mipmaps: cada texel es un color exacto, igual que los materiales de un color que reemplaza
            palette = new Texture2D(PaletteSize, PaletteSize, TextureFormat.RGBA32, false, false)
            {
                name = "PaletaPersonajes", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp,
            };
            paletteSlots.Clear();
            meshCache.Clear();
            paletteMat = FGMaterials.Lit("humanoidePaleta", Color.white, palette, 0.25f);
            return paletteMat;
        }

        /// <summary>UV del centro del texel con ese color (lo agrega a la paleta si es nuevo).</summary>
        static Vector2 PaletteUV(Color c)
        {
            PaletteMaterial();
            Color32 key = c;
            if (paletteSlots.TryGetValue(key, out var uv)) return uv;
            int i = paletteSlots.Count;
            if (i >= PaletteSize * PaletteSize)
            {
                Debug.LogWarning("Float Gardens: la paleta de personajes está llena; se repite el último color.");
                i = PaletteSize * PaletteSize - 1;
            }
            int x = i % PaletteSize, y = i / PaletteSize;
            palette.SetPixel(x, y, c);
            palette.Apply(false);
            uv = new Vector2((x + 0.5f) / PaletteSize, (y + 0.5f) / PaletteSize);
            paletteSlots[key] = uv;
            return uv;
        }

        static void AddMesh(GameObject go, IList<Piece> pieces)
        {
            var key = new System.Text.StringBuilder();
            foreach (var p in pieces) key.Append(p.size).Append(p.pos).Append((Color32)p.color).Append(p.rollZ).Append(';');
            string k = key.ToString();
            var mat = PaletteMaterial();
            if (!meshCache.TryGetValue(k, out var mesh) || mesh == null)
            {
                var verts = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();
                var tris = new List<int>();
                Mesh cube = MeshBuilder.Primitive(PrimitiveType.Cube);
                foreach (var p in pieces)
                    FGMesh.Append(cube, Matrix4x4.TRS(p.pos, Quaternion.Euler(0, 0, p.rollZ), p.size), verts, normals, uvs, tris, PaletteUV(p.color));
                mesh = new Mesh { name = "Personaje" };
                mesh.SetVertices(verts);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateBounds();
                mesh.UploadMeshData(true);
                meshCache[k] = mesh;
            }
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        static Transform Pivot(Transform parent, Vector3 pos, IList<Piece> pieces)
        {
            var p = new GameObject("Pivote").transform;
            p.SetParent(parent, false);
            p.localPosition = pos;
            AddMesh(p.gameObject, pieces);
            return p;
        }

        /// <summary>phase: fase de paso; amount: 0 quieto .. 1 corriendo; aiming: brazos al frente.</summary>
        public void Animate(float phase, float amount, bool aiming)
        {
            if (animator != null)
            {
                animator.SetFloat(SpeedHash, amount);
                animator.SetBool(AimHash, aiming);
                return;
            }
            if (legL == null) return;
            float s = Mathf.Sin(phase) * 40f * amount;
            legL.localRotation = Quaternion.Euler(s, 0, 0);
            legR.localRotation = Quaternion.Euler(-s, 0, 0);
            if (aiming)
            {
                armR.localRotation = Quaternion.Euler(-83, 0, 0);
                armL.localRotation = Quaternion.Euler(-75, 0, -26);
            }
            else
            {
                armL.localRotation = Quaternion.Euler(-s, 0, 0);
                armR.localRotation = Quaternion.Euler(s, 0, 0);
            }
        }

        public void SetDead(float t)
        {
            if (animator != null) { animator.SetBool(DeadHash, true); return; }
            body.localRotation = Quaternion.Euler(Mathf.Min(90, t * 230), 0, 0);
        }

        public void ResetPose()
        {
            if (animator != null) animator.SetBool(DeadHash, false);
            body.localRotation = Quaternion.identity;
        }

        // ---------- AeroPress X-1 (el arma) ----------
        public static Transform MakeAeroPress(Transform hand)
        {
            var g = new GameObject("AeroPress").transform;
            g.SetParent(hand, false);
            g.localPosition = new Vector3(0, 0.06f, 0.12f);
            g.localRotation = Quaternion.Euler(90, 0, 0);
            Cyl(g, 0.18f, 0.42f, 0, FGMaterials.Transparent("aeropressCamara", new Color(0.74f, 0.84f, 0.9f, 0.6f), true));
            Cyl(g, 0.15f, 0.26f, 0.06f, FGMaterials.Lit("#4a2c1d", 0.6f));
            Cyl(g, 0.16f, 0.22f, -0.28f, FGMaterials.Lit("#333333", 0.5f));
            Cyl(g, 0.22f, 0.04f, 0.23f, FGMaterials.Lit("#2fbf71", 0.5f));
            var muzzle = new GameObject("Boca").transform;
            muzzle.SetParent(g, false);
            muzzle.localPosition = new Vector3(0, 0, 0.3f);
            return muzzle;
        }

        static void Cyl(Transform parent, float diameter, float length, float z, Material m)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(c.GetComponent<Collider>());
            c.transform.SetParent(parent, false);
            c.transform.localPosition = new Vector3(0, 0, z);
            c.transform.localRotation = Quaternion.Euler(90, 0, 0);
            c.transform.localScale = new Vector3(diameter, length / 2, diameter);
            c.GetComponent<MeshRenderer>().sharedMaterial = m;
        }
    }
}
