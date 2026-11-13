// Peatones bogotanos: caminan por los andenes, sacan sombrilla cuando llueve y a veces quieren café.
using UnityEngine;

namespace FloatGardens
{
    public class Pedestrian : MonoBehaviour
    {
        static readonly string[][] Styles =
        {
            new[] { "#5b8def", "#2d3a5a", "#2b1b12" }, new[] { "#e8e8e8", "#3b3b46", "#4a3020" }, new[] { "#f4c542", "#6b8e4e", "#1a1a1a" },
            new[] { "#7a5c99", "#222222", "#111111" }, new[] { "#e85d75", "#333333", "#5a3a20" }, new[] { "#3f9e8a", "#2d2d2d", "#2b1b12" },
            new[] { "#8a3b2a", "#3a3020", "#1a1a1a" }, // ruana
        };
        static readonly string[] Skins = { "#d9a273", "#a8714a", "#e8c19a", "#8a5a3a" };

        Humanoid rig;
        Vector2Int cell;
        Vector3 goal;
        float phase, speed, wantTimer;
        public float stop;
        public bool wants;
        Transform icon, umbrella;
        public bool IconVisible => icon.gameObject.activeSelf;

        public static Pedestrian Spawn(Vector3 pos, Transform parent)
        {
            var go = new GameObject("Peaton");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var p = go.AddComponent<Pedestrian>();
            var st = Styles[Random.Range(0, Styles.Length)];
            p.rig = Humanoid.Create("Modelo", new HumanoidStyle
            {
                shirt = FGMaterials.Hex(st[0]), pants = FGMaterials.Hex(st[1]), hair = FGMaterials.Hex(st[2]),
                skin = FGMaterials.Hex(Skins[Random.Range(0, Skins.Length)]),
            }, go.transform, GameManager.I != null ? GameManager.I.pedestrianModel : null);
            p.cell = City.CellOf(pos);
            p.phase = Random.value * 6;
            p.speed = Random.Range(1.3f, 1.9f);
            p.wantTimer = Random.Range(5f, 25f);

            // Ícono de café (billboard)
            var ic = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(ic.GetComponent<Collider>());
            ic.name = "QuiereCafe";
            ic.transform.SetParent(go.transform, false);
            ic.transform.localPosition = Vector3.up * 2.4f;
            ic.transform.localScale = Vector3.one * 0.6f;
            ic.GetComponent<MeshRenderer>().sharedMaterial = CupMaterial();
            ic.SetActive(false);
            p.icon = ic.transform;

            // Sombrilla
            string umbHex = new[] { "#111111", "#2c5aa0", "#c0392b", "#2fbf71", "#f2a541" }[Random.Range(0, 5)];
            var umb = FGMesh.Bake(go.transform, "Sombrilla", new[]
            {
                FGMesh.P(UmbrellaMesh(), Vector3.up * 2.2f, Quaternion.identity, new Vector3(0.75f, 0.3f, 0.75f), FGMaterials.Lit(umbHex, 0.5f)),
                FGMesh.P(PrimitiveType.Cube, Vector3.up * 1.9f, new Vector3(0.03f, 0.9f, 0.03f), FGMaterials.Lit("#333333", 0.3f)),
            }, "sombrilla" + umbHex).transform;
            umb.localPosition = new Vector3(0.25f, 0, 0.1f);
            umb.gameObject.SetActive(false);
            p.umbrella = umb;
            p.PickGoal();
            return p;
        }

        static Mesh umbrellaMesh;
        static Mesh UmbrellaMesh() => umbrellaMesh != null ? umbrellaMesh : umbrellaMesh = MeshBuilder.Cone(8, 1, 1);

        static Material cupMat;
        static Material CupMaterial()
        {
            if (cupMat != null) return cupMat;
            // Ícono de taza dibujado por código
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[s * s];
            Color32 white = new Color32(255, 255, 255, 240), brown = new Color32(0x4a, 0x2c, 0x1d, 255), green = new Color32(0x2f, 0xbf, 0x71, 255), clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float dx = x - 31.5f, dy = y - 31.5f;
                    Color32 c = dx * dx + dy * dy < 30 * 30 ? white : clear;
                    if (x > 18 && x < 42 && y > 16 && y < 40) c = brown;
                    if (x > 18 && x < 42 && y > 24 && y < 29) c = green;
                    if (x >= 42 && x < 48 && y > 22 && y < 34 && !(x > 43 && x < 46 && y > 25 && y < 31)) c = brown;
                    px[y * s + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            cupMat = FGMaterials.Instance(FGMaterials.Transparent("iconoCafe", Color.white));
            cupMat.mainTexture = tex;
            return cupMat;
        }

        void PickGoal()
        {
            Vector3 c = City.CellCenter(cell.y, cell.x);
            // a veces cruza la calle a la manzana vecina
            if (Random.value < 0.2f)
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                var n = cell + dirs[Random.Range(0, 4)];
                if (n.x >= 0 && n.y >= 0 && n.x < FGConfig.Grid && n.y < FGConfig.Grid && City.DistrictCode(n.y, n.x) != 'P') cell = n;
            }
            Vector3 c2 = City.CellCenter(cell.y, cell.x);
            float s = FGConfig.Block / 2 - 0.9f;
            Vector3[] corners = { new Vector3(-s, 0, -s), new Vector3(s, 0, -s), new Vector3(s, 0, s), new Vector3(-s, 0, s) };
            goal = c2 + corners[Random.Range(0, 4)];
        }

        public void Tick(float dt, GameManager g)
        {
            Vector3 pos = transform.position;
            bool market = g.IsMarket(pos);
            // SetActive solo cuando cambia: activar/desactivar en cada cuadro dispara trabajo interno de Unity
            bool showIcon = wants && market, raining = g.atmosphere.Raining;
            if (icon.gameObject.activeSelf != showIcon) icon.gameObject.SetActive(showIcon);
            if (umbrella.gameObject.activeSelf != raining) umbrella.gameObject.SetActive(raining);
            if (icon.gameObject.activeSelf)
            {
                icon.rotation = g.cam.transform.rotation;
                icon.localPosition = Vector3.up * (2.4f + Mathf.Sin(Time.time * 3 + phase) * 0.08f);
            }
            wantTimer -= dt;
            if (wantTimer <= 0)
            {
                wants = !wants;
                wantTimer = wants ? Random.Range(25f, 45f) : Random.Range(10f, 35f);
            }
            if (stop > 0) { stop -= dt; rig.Animate(0, 0, false); return; }
            Vector3 to = goal - pos;
            to.y = 0;
            if (to.magnitude < 0.5f) PickGoal();
            else
            {
                pos += to.normalized * speed * dt;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), Mathf.Min(1, dt * 6));
                phase += dt * 7;
            }
            pos.y = City.BaseY(pos);
            transform.position = pos;
            rig.Animate(phase, 0.6f, false);
        }

        public void Served()
        {
            wants = false;
            icon.gameObject.SetActive(false);
            wantTimer = Random.Range(30f, 60f);
            stop = 1.5f;
        }
    }
}
