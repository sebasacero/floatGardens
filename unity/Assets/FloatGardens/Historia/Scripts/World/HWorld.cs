// Utilidades para construir los escenarios de la historia con primitivas: cajas, cilindros, texto 3D y personajes.
using UnityEngine;
using UnityEngine.Rendering;

namespace FloatGardens.Historia
{
    public static class HWorld
    {
        public static Material M(string hex, float smooth = 0.15f) => FGMaterials.Lit(hex, smooth);

        public static Transform Group(Transform parent, string name, Vector3 pos = default, float yaw = 0f)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localRotation = Quaternion.Euler(0, yaw, 0);
            return t;
        }

        static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 pos, Vector3 scale, Material m, bool collider, bool shadows)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = m;
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Caja con su centro en pos.</summary>
        public static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 size, Material m, bool collider = false, bool shadows = true)
            => Prim(PrimitiveType.Cube, parent, name, pos, size, m, collider, shadows);

        /// <summary>Caja apoyada en el suelo (pos = centro de la base).</summary>
        public static GameObject Block(Transform parent, string name, Vector3 basePos, Vector3 size, Material m, bool collider = true)
            => Box(parent, name, basePos + Vector3.up * size.y * 0.5f, size, m, collider);

        /// <summary>Cilindro vertical: diámetro y alto reales, pos = centro.</summary>
        public static GameObject Cyl(Transform parent, string name, Vector3 pos, float diameter, float height, Material m, bool collider = false)
            => Prim(PrimitiveType.Cylinder, parent, name, pos, new Vector3(diameter, height * 0.5f, diameter), m, collider, true);

        public static GameObject Ball(Transform parent, string name, Vector3 pos, float diameter, Material m)
            => Prim(PrimitiveType.Sphere, parent, name, pos, Vector3.one * diameter, m, false, true);

        public static GameObject Cone(Transform parent, string name, Vector3 basePos, float radius, float height, Material m, int segments = 7)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = basePos;
            go.AddComponent<MeshFilter>().sharedMesh = MeshBuilder.Cone(segments, radius, height);
            go.AddComponent<MeshRenderer>().sharedMaterial = m;
            return go;
        }

        /// <summary>Texto en el mundo (letreros, murales, pantallas). size ≈ alto de letra en metros.</summary>
        public static TextMesh Text3D(Transform parent, string text, Vector3 pos, Quaternion rot, float size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject("Texto " + (text.Length > 12 ? text.Substring(0, 12) : text));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            var tm = go.AddComponent<TextMesh>();
            tm.font = UIKit.Font;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = size / 6.4f;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = UIKit.Font.material;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return tm;
        }

        /// <summary>Luz puntual sin sombras (barata en celular).</summary>
        public static Light Lamp(Transform parent, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject("Luz");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            return l;
        }

        /// <summary>Personaje no jugable que respira, mira al barista cuando se acerca y saluda.</summary>
        public static Humanoid Npc(Transform parent, string name, HumanoidStyle style, Vector3 pos, float yaw)
        {
            var h = Humanoid.Create(name, style, parent);
            h.transform.localPosition = pos;
            h.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            h.gameObject.AddComponent<NpcIdle>().rig = h;
            return h;
        }

        /// <summary>Sombrero aguadeño (ala ancha de paja con cinta negra), ruana, poncho o mochila sobre un Humanoid low-poly.</summary>
        public static void Hat(Humanoid h, Color straw, Color band, float brim = 0.72f)
        {
            if (h.body == null) return;
            Box(h.body, "Ala", new Vector3(0, 2.0f, 0), new Vector3(brim, 0.03f, brim), FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(straw), straw, null, 0.2f));
            Box(h.body, "Copa", new Vector3(0, 2.1f, 0), new Vector3(0.36f, 0.18f, 0.36f), FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(straw), straw, null, 0.2f));
            Box(h.body, "Cinta", new Vector3(0, 2.04f, 0), new Vector3(0.37f, 0.05f, 0.37f), FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(band), band, null, 0.2f));
        }

        public static void Ruana(Humanoid h, Color c)
        {
            if (h.body == null) return;
            var m = FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(c), c, null, 0.1f);
            Box(h.body, "Ruana", new Vector3(0, 1.3f, 0), new Vector3(0.95f, 0.62f, 0.42f), m);
        }

        public static void Bag(Humanoid h, Color c)
        {
            if (h.body == null) return;
            var m = FGMaterials.Lit("h" + ColorUtility.ToHtmlStringRGB(c), c, null, 0.2f);
            var strap = Box(h.body, "Correa", new Vector3(0, 1.3f, 0), new Vector3(0.05f, 0.8f, 0.4f), m);
            strap.transform.localRotation = Quaternion.Euler(0, 0, -32);
            Box(h.body, "Bolso", new Vector3(-0.36f, 0.95f, 0.05f), new Vector3(0.12f, 0.3f, 0.28f), m);
        }

        /// <summary>Cafeto low-poly: arbusto oscuro y brillante con cerezas.</summary>
        public static void CoffeeShrub(MeshBuilder leaves, MeshBuilder cherries, Vector3 basePos, float height, System.Random rng)
        {
            var cone = MeshBuilder.Primitive(PrimitiveType.Sphere);
            for (int k = 0; k < 3; k++)
            {
                float y = basePos.y + height * (0.3f + k * 0.28f);
                float r = height * (0.5f - k * 0.12f);
                leaves.Add(cone, Matrix4x4.TRS(new Vector3(basePos.x, y, basePos.z), Quaternion.Euler(0, (float)rng.NextDouble() * 180f, 0), new Vector3(r * 1.6f, r * 1.1f, r * 1.6f)));
            }
            var cube = MeshBuilder.Primitive(PrimitiveType.Cube);
            int n = 5 + rng.Next(5);
            for (int k = 0; k < n; k++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float y = basePos.y + height * (0.25f + (float)rng.NextDouble() * 0.55f);
                float rr = height * 0.42f;
                cherries.Add(cube, Matrix4x4.TRS(new Vector3(basePos.x + Mathf.Cos(a) * rr, y, basePos.z + Mathf.Sin(a) * rr), Quaternion.identity, Vector3.one * 0.07f));
            }
        }
    }

    /// <summary>Animación de reposo de los personajes: respiran, voltean a mirar al barista y saludan con el brazo.</summary>
    public class NpcIdle : MonoBehaviour
    {
        public Humanoid rig;
        float phase, baseYaw;
        bool init;

        void Update()
        {
            if (rig == null) return;
            if (!init) { baseYaw = transform.eulerAngles.y; phase = Random.value * 10f; init = true; }
            phase += Time.deltaTime;
            float targetYaw = baseYaw;
            var g = StoryGame.I;
            bool near = false;
            if (g != null && g.player != null)
            {
                Vector3 d = g.player.transform.position - transform.position;
                d.y = 0f;
                if (d.sqrMagnitude < 36f && d.sqrMagnitude > 0.01f) { targetYaw = Quaternion.LookRotation(d).eulerAngles.y; near = d.sqrMagnitude < 9f; }
            }
            float yaw = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, Mathf.Min(1f, Time.deltaTime * 3f));
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            if (rig.animator != null) { rig.Animate(0f, 0f, false); return; }
            if (rig.body != null) rig.body.localPosition = new Vector3(0, Mathf.Sin(phase * 1.6f) * 0.012f, 0);
            if (rig.armR != null) rig.armR.localRotation = near ? Quaternion.Euler(-150f + Mathf.Sin(phase * 8f) * 18f, 0, 12f) : Quaternion.Euler(Mathf.Sin(phase) * 3f, 0, 0);
        }
    }
}
