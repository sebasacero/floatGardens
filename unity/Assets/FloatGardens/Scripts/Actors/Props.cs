// Objetos del mundo: cultivos hidropónicos flotantes, cajas de café, zonas de captura y sellos de la Federación.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class Planter : MonoBehaviour
    {
        public float growth;
        public bool onWater;
        Transform plant, cherries;
        float bob, baseY;
        public bool Ripe => growth >= 1;

        public static Planter Spawn(Vector3 pos, bool water, Transform parent)
        {
            var go = new GameObject("CultivoFlotante");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var p = go.AddComponent<Planter>();
            p.onWater = water;
            p.baseY = pos.y;
            p.bob = Random.value * 6;
            p.growth = Random.value * 0.6f;
            // Base, planta y cerezas: tres mallas compartidas por todos los cultivos (antes eran 16 objetos sueltos)
            FGMesh.Bake(go.transform, "Balsa", new[]
            {
                FGMesh.P(PrimitiveType.Cylinder, new Vector3(0, 0.12f, 0), new Vector3(2.2f, 0.12f, 2.2f), FGMaterials.Lit("#f2f2ee", 0.3f)),
                FGMesh.P(PrimitiveType.Cylinder, new Vector3(0, 0.3f, 0), new Vector3(0.7f, 0.15f, 0.7f), FGMaterials.Lit("#2d2d2d", 0.4f)),
            }, "cultivoBalsa");
            var plant = new List<FGMesh.Part> { FGMesh.P(PrimitiveType.Cube, new Vector3(0, 0.5f, 0), new Vector3(0.08f, 1, 0.08f), FGMaterials.Lit("#2a7a41", 0.1f)) };
            for (int i = 0; i < 4; i++)
                plant.Add(FGMesh.P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(i * 1.6f) * 0.3f, 0.6f + i * 0.15f, Mathf.Sin(i * 1.6f) * 0.3f),
                    new Vector3(0.9f, 0.6f, 0.9f), FGMaterials.Lit(i % 2 == 1 ? "#3fa35b" : "#2a7a41", 0.1f)));
            p.plant = FGMesh.Bake(go.transform, "Planta", plant, "cultivoPlanta").transform;
            p.plant.localPosition = Vector3.up * 0.4f;
            var red = FGMaterials.Lit("cereza", FGMaterials.Hex("#d6423a"), null, 0.7f);
            var cherries = new List<FGMesh.Part>();
            for (int i = 0; i < 9; i++)
                cherries.Add(FGMesh.P(PrimitiveType.Sphere, new Vector3(Mathf.Cos(i * 2.1f) * 0.45f, 0.55f + (i % 3) * 0.22f, Mathf.Sin(i * 2.1f) * 0.45f), Vector3.one * 0.18f, red));
            p.cherries = FGMesh.Bake(p.plant, "Cerezas", cherries, "cultivoCerezas").transform;
            return p;
        }

        public static void Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material m)
        {
            var g = GameObject.CreatePrimitive(type);
            Object.Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.GetComponent<MeshRenderer>().sharedMaterial = m;
        }

        public void Tick(float dt, float rate)
        {
            if (growth < 1) growth = Mathf.Min(1, growth + dt / FGConfig.RegrowTime * rate);
            plant.localScale = Vector3.one * (0.35f + growth * 0.65f);
            if (cherries.gameObject.activeSelf != Ripe) cherries.gameObject.SetActive(Ripe);
            if (onWater)
            {
                var p = transform.position;
                p.y = baseY + Mathf.Sin(Time.time * 1.5f + bob) * 0.05f;
                transform.position = p;
            }
        }

        public void Harvest() { growth = 0; }
    }

    public class Loot : MonoBehaviour
    {
        public int amount;
        public bool crate;
        Transform body;
        Material ringMat;
        float t;

        public static Loot Spawn(Vector3 pos, int amount, bool crate, Transform parent)
        {
            var go = new GameObject(crate ? "CajaDeCafe" : "Cerezas");
            go.transform.SetParent(parent, false);
            go.transform.position = pos + Vector3.up * 0.05f;
            var l = go.AddComponent<Loot>();
            l.amount = amount;
            l.crate = crate;
            l.t = Random.value * 6;
            l.body = (crate
                ? FGMesh.Bake(go.transform, "Cuerpo", new[]
                {
                    FGMesh.P(PrimitiveType.Cube, Vector3.zero, new Vector3(0.8f, 0.6f, 0.8f), FGMaterials.Lit("#8b5a2b", 0.2f)),
                    FGMesh.P(PrimitiveType.Cube, new Vector3(0, 0.15f, 0), new Vector3(0.82f, 0.12f, 0.82f), FGMaterials.Lit("#2fbf71", 0.4f)),
                }, "cajaCafe")
                : FGMesh.Bake(go.transform, "Cuerpo", new[]
                {
                    FGMesh.P(PrimitiveType.Sphere, Vector3.zero, Vector3.one * 0.44f, FGMaterials.Emissive("orbeCereza", FGMaterials.Hex("#e0413a"), 1.5f)),
                }, "orbeCereza")).transform;
            l.ringMat = FGMaterials.Instance(FGMaterials.Transparent(crate ? "anilloCaja" : "anilloCereza", crate ? new Color(1, 0.83f, 0.3f, 0.7f) : new Color(1, 0.42f, 0.36f, 0.7f)));
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(ring.GetComponent<Collider>());
            ring.transform.SetParent(go.transform, false);
            ring.transform.localScale = new Vector3(1.8f, 0.01f, 1.8f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = l.ringMat;
            ring.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return l;
        }

        void Update()
        {
            t += Time.deltaTime;
            body.localRotation = Quaternion.Euler(0, t * 60, 0);
            body.localPosition = Vector3.up * ((crate ? 0.35f : 0.45f) + Mathf.Sin(t * 3) * 0.1f);
            var c = ringMat.color;
            c.a = 0.3f + Mathf.Sin(t * 4) * 0.25f;
            ringMat.color = c;
        }
    }

    public class CaptureZone : MonoBehaviour
    {
        public FGConfig.ZoneDef def;
        public float progress;
        public bool ours;
        public int waves;
        Material canopyMat, flagMat, ringMat, beamMat;
        Transform flag;
        static readonly Color Fed = new Color32(0xe0, 0x41, 0x3a, 255);
        static readonly Color Ours = new Color32(0x2f, 0xbf, 0x71, 255);
        static readonly Color Target = new Color32(0xff, 0xd3, 0x4d, 255);

        public static CaptureZone Spawn(FGConfig.ZoneDef def, Vector3 center, Transform parent)
        {
            var go = new GameObject("Zona " + def.name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var z = go.AddComponent<CaptureZone>();
            z.def = def;
            var t = go.transform;
            // Mostrador, postes y asta: una malla compartida por todas las zonas
            var stall = new List<FGMesh.Part> { FGMesh.P(PrimitiveType.Cube, new Vector3(0, 0.55f, 0), new Vector3(3, 1.1f, 1.4f), FGMaterials.Lit("#5a3a24", 0.2f)) };
            foreach (var p in new[] { new Vector2(-1.8f, -1.1f), new Vector2(1.8f, -1.1f), new Vector2(-1.8f, 1.1f), new Vector2(1.8f, 1.1f) })
                stall.Add(FGMesh.P(PrimitiveType.Cube, new Vector3(p.x, 1.3f, p.y), new Vector3(0.1f, 2.5f, 0.1f), FGMaterials.Lit("#cccccc", 0.6f)));
            stall.Add(FGMesh.P(PrimitiveType.Cylinder, new Vector3(2.6f, 3.5f, 0), new Vector3(0.12f, 3.5f, 0.12f), FGMaterials.Lit("#dddddd", 0.6f)));
            FGMesh.Bake(t, "Kiosco", stall, "kioscoZona");
            z.canopyMat = FGMaterials.Instance(FGMaterials.Lit("toldoZona", Fed, null, 0.3f));
            Planter.Part(t, PrimitiveType.Cube, new Vector3(0, 2.6f, 0), new Vector3(3.8f, 0.25f, 2.4f), z.canopyMat);
            z.flagMat = FGMaterials.Instance(FGMaterials.Lit("banderaZona", Fed, null, 0.3f));
            var flagGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(flagGo.GetComponent<Collider>());
            flagGo.transform.SetParent(t, false);
            flagGo.transform.localPosition = new Vector3(3.5f, 6.3f, 0);
            flagGo.transform.localScale = new Vector3(1.8f, 1.1f, 0.03f);
            flagGo.GetComponent<MeshRenderer>().sharedMaterial = z.flagMat;
            z.flag = flagGo.transform;
            // anillo en el piso y columna de luz visible desde lejos
            z.ringMat = FGMaterials.Instance(FGMaterials.Transparent("anilloZona", new Color(Fed.r, Fed.g, Fed.b, 0.35f)));
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(ring.GetComponent<Collider>());
            ring.transform.SetParent(t, false);
            ring.transform.localPosition = Vector3.up * 0.06f;
            ring.transform.localScale = new Vector3(FGConfig.CaptureRadius * 2, 0.005f, FGConfig.CaptureRadius * 2);
            ring.GetComponent<MeshRenderer>().sharedMaterial = z.ringMat;
            ring.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            z.beamMat = FGMaterials.Instance(FGMaterials.Transparent("columnaZona", new Color(Fed.r, Fed.g, Fed.b, 0.18f)));
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(beam.GetComponent<Collider>());
            beam.transform.SetParent(t, false);
            beam.transform.localPosition = Vector3.up * 60;
            beam.transform.localScale = new Vector3(2.4f, 60, 2.4f);
            beam.GetComponent<MeshRenderer>().sharedMaterial = z.beamMat;
            beam.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.55f, 0);
            col.size = new Vector3(3, 1.1f, 1.4f);
            return z;
        }

        public void SetState(bool isOurs, bool isTarget)
        {
            ours = isOurs;
            Color c = isOurs ? Ours : isTarget ? Target : Fed;
            canopyMat.color = isOurs ? Ours : Fed;
            flagMat.color = isOurs ? Ours : Fed;
            ringMat.color = new Color(c.r, c.g, c.b, 0.35f);
            beamMat.color = new Color(c.r, c.g, c.b, isTarget ? 0.3f : 0.15f);
        }

        void Update()
        {
            flag.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 2 + transform.position.x) * 14, 0);
        }
    }

    public class Stamp : MonoBehaviour
    {
        public Vector3 velocity;
        public float damage, life = 3;
        public Enemy owner;
        static Material mat;

        public static Stamp Spawn(Vector3 pos, Vector3 vel, float dmg, Enemy owner, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = "Sello";
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.44f, 0.06f, 0.44f);
            if (mat == null) mat = FGMaterials.Emissive("sello", FGMaterials.Hex("#c0392b"), 1.2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var s = go.AddComponent<Stamp>();
            s.velocity = vel;
            s.damage = dmg;
            s.owner = owner;
            return s;
        }

        /// <summary>Devuelve false cuando el sello desaparece.</summary>
        public bool Tick(float dt, GameManager g)
        {
            Vector3 from = transform.position;
            Vector3 to = from + velocity * dt;
            transform.Rotate(600 * dt, 0, 0, Space.Self);
            life -= dt;
            var p = g.player;
            Vector3 pc = p.transform.position + Vector3.up * 1.0f;
            Vector3 closest = from + Vector3.Project(pc - from, to - from);
            if (!p.dead && Vector3.Dot(closest - from, to - from) >= 0 && (closest - from).sqrMagnitude <= (to - from).sqrMagnitude &&
                Vector3.Distance(closest, pc) < 0.8f)
            {
                p.Damage(damage, from, 0, g);
                SFX.Play("stamp");
                return false;
            }
            if (Physics.Linecast(from, to, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) && !(hit.collider is CharacterController))
            {
                FX.Burst(hit.point, FGMaterials.Hex("#c0392b"), 4, 2);
                return false;
            }
            transform.position = to;
            return life > 0;
        }
    }
}
