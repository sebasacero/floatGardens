// Personaje humanoide low-poly hecho de primitivas, con animación procedural.
// Si asignas un modelo animado (p. ej. de Mixamo) en FGBootstrap, se usa ese en su lugar.
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

            h.legL = Pivot(h.body, new Vector3(-0.13f, 0.85f, 0), Box(0.22f, 0.85f, 0.26f, s.pants, new Vector3(0, -0.42f, 0)));
            h.legR = Pivot(h.body, new Vector3(0.13f, 0.85f, 0), Box(0.22f, 0.85f, 0.26f, s.pants, new Vector3(0, -0.42f, 0)));
            Attach(h.body, Box(0.62f, 0.72f, 0.34f, s.shirt, new Vector3(0, 1.22f, 0)));
            h.head = Attach(h.body, Box(0.36f, 0.38f, 0.36f, s.skin, new Vector3(0, 1.79f, 0)));
            h.armL = Pivot(h.body, new Vector3(-0.4f, 1.52f, 0), Box(0.17f, 0.66f, 0.2f, s.shirt, new Vector3(0, -0.3f, 0)));
            h.armR = Pivot(h.body, new Vector3(0.4f, 1.52f, 0), Box(0.17f, 0.66f, 0.2f, s.shirt, new Vector3(0, -0.3f, 0)));
            Attach(h.armL, Box(0.15f, 0.14f, 0.17f, s.skin, new Vector3(0, -0.66f, 0)));
            Attach(h.armR, Box(0.15f, 0.14f, 0.17f, s.skin, new Vector3(0, -0.66f, 0)));
            h.rightHand = new GameObject("Mano").transform; // ancla sin escala para el arma
            h.rightHand.SetParent(h.armR, false);
            h.rightHand.localPosition = new Vector3(0, -0.66f, 0);
            Attach(h.body, Box(0.06f, 0.06f, 0.02f, Color.black, new Vector3(-0.08f, 1.82f, 0.185f)));
            Attach(h.body, Box(0.06f, 0.06f, 0.02f, Color.black, new Vector3(0.08f, 1.82f, 0.185f)));
            if (s.hair.HasValue) Attach(h.body, Box(0.38f, 0.1f, 0.38f, s.hair.Value, new Vector3(0, 2.0f, 0)));
            if (s.apron.HasValue) Attach(h.body, Box(0.5f, 0.75f, 0.04f, s.apron.Value, new Vector3(0, 1.1f, 0.19f)));
            if (s.tie.HasValue) Attach(h.body, Box(0.08f, 0.4f, 0.03f, s.tie.Value, new Vector3(0, 1.32f, 0.18f)));
            if (s.mustache) Attach(h.body, Box(0.2f, 0.05f, 0.03f, FGMaterials.Hex("#2a1a10"), new Vector3(0, 1.7f, 0.185f)));
            if (s.cap.HasValue)
            {
                Attach(h.body, Box(0.4f, 0.14f, 0.4f, s.cap.Value, new Vector3(0, 2.03f, 0)));
                Attach(h.body, Box(0.36f, 0.04f, 0.22f, s.cap.Value, new Vector3(0, 1.97f, 0.26f)));
            }
            Color red = FGMaterials.Hex("#c0392b");
            if (s.hat == "fedora")
            {
                Attach(h.body, Box(0.6f, 0.04f, 0.6f, FGMaterials.Hex("#222222"), new Vector3(0, 1.99f, 0)));
                Attach(h.body, Box(0.38f, 0.2f, 0.38f, FGMaterials.Hex("#222222"), new Vector3(0, 2.1f, 0)));
                Attach(h.body, Box(0.39f, 0.05f, 0.39f, red, new Vector3(0, 2.03f, 0)));
            }
            else if (s.hat == "beret") Attach(h.body, Box(0.44f, 0.1f, 0.44f, FGMaterials.Hex("#8f1f1a"), new Vector3(0.03f, 2.02f, 0)));
            else if (s.hat == "tophat")
            {
                Attach(h.body, Box(0.62f, 0.04f, 0.62f, FGMaterials.Hex("#111111"), new Vector3(0, 1.99f, 0)));
                Attach(h.body, Box(0.38f, 0.5f, 0.38f, FGMaterials.Hex("#111111"), new Vector3(0, 2.25f, 0)));
                Attach(h.body, Box(0.39f, 0.07f, 0.39f, red, new Vector3(0, 2.05f, 0)));
            }
            if (s.sash.HasValue)
            {
                var sash = Attach(h.body, Box(0.08f, 0.85f, 0.36f, s.sash.Value, new Vector3(0, 1.22f, 0)));
                sash.localRotation = Quaternion.Euler(0, 0, 34);
            }
            if (s.badge) Attach(h.body, Box(0.1f, 0.1f, 0.03f, FGMaterials.Hex("#e6c34a"), new Vector3(0.17f, 1.4f, 0.18f)));
            return h;
        }

        static Transform Box(float w, float hgt, float d, Color c, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(go.GetComponent<Collider>());
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(w, hgt, d);
            go.GetComponent<MeshRenderer>().sharedMaterial = FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(c), c, null, 0.25f);
            return go.transform;
        }

        static Transform Pivot(Transform parent, Vector3 pos, Transform child)
        {
            var p = new GameObject("Pivote").transform;
            p.SetParent(parent, false);
            p.localPosition = pos;
            child.SetParent(p, false);
            return p;
        }

        static Transform Attach(Transform parent, Transform child)
        {
            child.SetParent(parent, false);
            return child;
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
