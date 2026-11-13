// Vista cenital ("devlog"): cámara casi vertical de lente larga que sigue al barista, mira asistida tipo
// twin-stick y dos temas (noche: piso carbón, maqueta blanca a media luz; día: maqueta blanca).
// Es el equivalente Unity de js3d/topdown.js. Cómo engancharla está en unity/VISTA_CENITAL.md.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public static class TopDownMode
    {
        const string Key = "fg.cenital", KeyTheme = "fg.cenital.tema";

        /// <summary>Activa por defecto en pantallas táctiles; el jugador la cambia en el lobby o en pausa.</summary>
        public static bool Enabled
        {
            get => PlayerPrefs.GetInt(Key, FGInput.IsTouchDevice ? 1 : 0) == 1;
            set => PlayerPrefs.SetInt(Key, value ? 1 : 0);
        }
        public static bool Night
        {
            get => PlayerPrefs.GetString(KeyTheme, "noche") == "noche";
            set => PlayerPrefs.SetString(KeyTheme, value ? "noche" : "dia");
        }
    }

    public class TopDownCamera : MonoBehaviour
    {
        public PlayerController target;
        public Camera cam;

        [Header("Encuadre")]
        public float fov = 24f;          // lente larga: el personaje se ve pequeño y sin perspectiva
        public float distance = 58f;
        public float tiltDegrees = 9f;   // casi vertical; la cámara mira al norte (+z del mundo)
        public float followLag = 4.5f;

        [Header("Mira asistida")]
        public float autoAimRange = 42f;
        public float autoAimConeDegrees = 36f;
        public float snapConeDegrees = 11f;

        [Header("Piso")]
        public float gridMeters = 8f;
        public Material gridMaterial;    // opcional: material transparente con la retícula (se crea solo si falta)

        Vector3 follow;
        bool following;
        float shake;
        Transform grid, blob, aimLine;
        public Enemy Target { get; private set; }

        public void Shake(float amount) { shake = Mathf.Max(shake, amount); }

        void Awake()
        {
            BuildDecor();
        }

        // ---------- escena: retícula, sombra blanda y marcador de mira ----------
        void BuildDecor()
        {
            var root = new GameObject("CenitalDecor").transform;
            root.SetParent(transform.parent, false);

            grid = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            Object.Destroy(grid.GetComponent<Collider>());
            grid.name = "Reticula";
            grid.SetParent(root, false);
            grid.rotation = Quaternion.Euler(90, 0, 0);
            grid.localScale = new Vector3(400, 400, 1);
            if (gridMaterial == null) gridMaterial = MakeGridMaterial();
            grid.GetComponent<Renderer>().sharedMaterial = gridMaterial;

            blob = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            Object.Destroy(blob.GetComponent<Collider>());
            blob.name = "SombraBlanda";
            blob.SetParent(root, false);
            blob.rotation = Quaternion.Euler(90, 0, 0);
            blob.localScale = new Vector3(1.5f, 1.5f, 1);
            blob.GetComponent<Renderer>().sharedMaterial = MakeFlat(new Color(0, 0, 0, 0.28f), true);

            aimLine = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            Object.Destroy(aimLine.GetComponent<Collider>());
            aimLine.name = "Mira";
            aimLine.SetParent(root, false);
            aimLine.rotation = Quaternion.Euler(90, 0, 0);
            aimLine.GetComponent<Renderer>().sharedMaterial = MakeFlat(new Color32(0xd6, 0xe4, 0x8a, 230), false);
            aimLine.gameObject.SetActive(false);
        }

        static Material MakeFlat(Color c, bool soft)
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent");
            var m = new Material(sh) { color = c };
            // URP/Unlit: modo transparente
            if (m.HasProperty("_Surface")) { m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetOverrideTag("RenderType", "Transparent"); m.renderQueue = 3000; m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); m.SetInt("_ZWrite", 0); }
            if (soft)
            {
                // círculo con borde suave
                var t = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(1.15f - d * 1.15f)));
                }
                t.Apply();
                m.mainTexture = t;
            }
            return m;
        }

        Material MakeGridMaterial()
        {
            var t = new Texture2D(128, 128, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
            var px = new Color[128 * 128];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(1, 1, 1, 0);
            for (int i = 0; i < 128; i++) { px[i] = Color.white; px[i * 128] = Color.white; px[i + 128] = Color.white; px[i * 128 + 1] = Color.white; }
            t.SetPixels(px);
            t.Apply();
            var m = MakeFlat(Color.white, false);
            m.mainTexture = t;
            m.mainTextureScale = new Vector2(400 / gridMeters, 400 / gridMeters);
            return m;
        }

        // ---------- tema ----------
        /// <summary>Aplica noche o día a la luz, la niebla y el piso. Llama con la luz principal y los renderers del piso/andenes.</summary>
        public void ApplyTheme(bool night, Light sun, IEnumerable<Renderer> paving)
        {
            Color sky = night ? new Color32(0x23, 0x27, 0x2c, 255) : new Color32(0xe3, 0xe9, 0xe8, 255);
            cam.backgroundColor = sky;
            RenderSettings.fogColor = sky;
            RenderSettings.fogStartDistance = night ? 90 : 110;
            RenderSettings.fogEndDistance = night ? 520 : 650;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = night ? new Color32(0xaa, 0xb4, 0xc8, 255) * 0.5f : new Color32(0xee, 0xf4, 0xff, 255) * 0.75f;
            RenderSettings.ambientEquatorColor = night ? new Color32(0x55, 0x5c, 0x66, 255) * 0.5f : new Color32(0xc8, 0xd0, 0xcc, 255) * 0.75f;
            RenderSettings.ambientGroundColor = night ? new Color32(0x15, 0x18, 0x1b, 255) : new Color32(0x8f, 0x9a, 0x88, 255) * 0.75f;
            if (sun != null) sun.intensity = night ? 0.72f : 1.0f;
            Color floor = night ? new Color32(0x26, 0x2b, 0x30, 255) : Color.white;
            Color pave = night ? new Color32(0x3a, 0x40, 0x46, 255) : Color.white;
            bool first = true;
            foreach (var r in paving)
            {
                if (r == null) continue;
                foreach (var m in r.materials) m.color = first ? floor : pave;
                first = false;
            }
            gridMaterial.color = night ? new Color(1, 1, 1, 0.09f) : new Color(0.17f, 0.19f, 0.18f, 0.14f);
        }

        // ---------- mira ----------
        /// <summary>Yaw del jugador para este cuadro: joystick derecho / mouse, o asistida al enemigo más cercano.</summary>
        public float Aim(float dt, Vector2 aimPixels, bool aimActive, IEnumerable<Enemy> enemies)
        {
            var p = target;
            float? yaw = null;
            bool manual = false;
            if (aimActive && aimPixels.magnitude > 18)
            {
                // pantalla: derecha = +x, arriba = +z (la cámara mira al norte)
                yaw = Mathf.Atan2(aimPixels.x, aimPixels.y) * Mathf.Rad2Deg;
                manual = true;
            }
            else if (!FGInput.IsTouchDevice)
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(Vector3.up, new Vector3(0, p.transform.position.y + 1.2f, 0));
                if (plane.Raycast(ray, out float t))
                {
                    Vector3 pt = ray.GetPoint(t) - p.transform.position;
                    yaw = Mathf.Atan2(pt.x, pt.z) * Mathf.Rad2Deg;
                    manual = true;
                }
            }
            bool want = FGInput.Held(FGAction.Fire) || Time.time - p.lastShot < 1.2f || manual;
            Enemy best = null;
            if (want)
            {
                float bd = float.MaxValue;
                float cone = manual ? snapConeDegrees : autoAimConeDegrees;
                float reference = yaw ?? p.yaw;
                foreach (var e in enemies)
                {
                    if (e == null || e.dead) continue;
                    Vector3 d3 = e.transform.position - p.transform.position; d3.y = 0;
                    float d = d3.magnitude;
                    if (d > autoAimRange) continue;
                    float ey = Mathf.Atan2(d3.x, d3.z) * Mathf.Rad2Deg;
                    float diff = Mathf.Abs(Mathf.DeltaAngle(reference, ey));
                    if (manual && diff > cone) continue;
                    if (!manual && diff > cone) d += diff * 0.1f;
                    if (d >= bd) continue;
                    // línea de visión: nada entre el pecho del barista y el del enemigo (o solo el propio enemigo)
                    bool clear = !Physics.Linecast(p.transform.position + Vector3.up * 1.3f, e.Chest, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) || hit.transform.IsChildOf(e.transform);
                    if (clear) { bd = d; best = e; }
                }
            }
            Target = best;
            if (best != null)
            {
                Vector3 d3 = best.transform.position - p.transform.position;
                float ty = Mathf.Atan2(d3.x, d3.z) * Mathf.Rad2Deg;
                yaw = manual ? ty : Mathf.LerpAngle(p.yaw, ty, Mathf.Min(1, dt * 16));
            }
            if (yaw == null)
            {
                Vector2 mv = FGInput.Move();
                yaw = mv.magnitude > 0.2f ? Mathf.Atan2(mv.x, mv.y) * Mathf.Rad2Deg : p.yaw;
            }
            // marcador en el piso
            bool show = manual || best != null || FGInput.Held(FGAction.Fire);
            aimLine.gameObject.SetActive(show);
            if (show)
            {
                float len = best != null ? Vector3.Distance(Flat(best.transform.position), Flat(p.transform.position)) : 6f;
                Vector3 dir = Quaternion.Euler(0, yaw.Value, 0) * Vector3.forward;
                aimLine.position = p.transform.position + dir * (len / 2) + Vector3.up * 0.04f;
                aimLine.rotation = Quaternion.Euler(90, yaw.Value, 0);
                aimLine.localScale = new Vector3(0.1f, len, 1);
                aimLine.GetComponent<Renderer>().sharedMaterial.color = best != null ? new Color32(0xd6, 0xe4, 0x8a, 230) : new Color(1, 1, 1, 0.35f);
            }
            return yaw.Value;
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        /// <summary>Rayo del disparo en cenital: sale del pecho, horizontal, o al torso del objetivo asistido.</summary>
        public Ray ShotRay()
        {
            Vector3 origin = target.transform.position + Vector3.up * 1.3f;
            Vector3 dir = Quaternion.Euler(0, target.yaw, 0) * Vector3.forward;
            if (Target != null) dir = (Target.Chest - origin).normalized;
            return new Ray(origin, dir);
        }

        // ---------- cámara ----------
        public void Follow(float dt)
        {
            var p = target;
            float lead = Target != null ? 4.5f : 2.5f;
            Vector3 want = p.transform.position + Quaternion.Euler(0, p.yaw, 0) * Vector3.forward * lead;
            if (!following) { follow = want; following = true; }
            follow = Vector3.Lerp(follow, want, Mathf.Min(1, dt * followLag));
            float tilt = tiltDegrees * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(follow.x, p.transform.position.y + Mathf.Cos(tilt) * distance, follow.z - Mathf.Sin(tilt) * distance);
            if (shake > 0)
            {
                pos += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * shake * 2;
                shake = Mathf.Max(0, shake - dt * 2);
            }
            transform.position = pos;
            transform.LookAt(new Vector3(follow.x, p.transform.position.y, follow.z));
            float targetFov = p.aiming || Target != null ? 22 : p.sprinting ? 26 : fov;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Mathf.Min(1, dt * 6));
            PlaceDecor(p.transform.position, p.Grounded);
        }

        void PlaceDecor(Vector3 pos, bool grounded)
        {
            float g = gridMeters;
            grid.position = new Vector3(Mathf.Round(pos.x / g) * g, 0.02f, Mathf.Round(pos.z / g) * g);
            float groundY = Physics.Raycast(pos + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 30, ~0, QueryTriggerInteraction.Ignore) ? hit.point.y : 0;
            blob.position = new Vector3(pos.x, groundY + 0.03f, pos.z);
            float h = Mathf.Max(0, pos.y - groundY);
            float sc = Mathf.Max(0.45f, 1 - h * 0.12f);
            blob.localScale = new Vector3(1.5f * sc, 1.5f * sc, 1);
        }

        /// <summary>Lobby: el barista visto desde arriba, con un giro lento.</summary>
        public void Lobby(Vector3 spawn, float t)
        {
            float d = 44, tilt = 14 * Mathf.Deg2Rad;
            float a = Mathf.Sin(t * 0.18f) * 0.5f;
            transform.position = spawn + new Vector3(Mathf.Sin(a) * Mathf.Sin(tilt) * d, Mathf.Cos(tilt) * d, -Mathf.Cos(a) * Mathf.Sin(tilt) * d);
            transform.LookAt(spawn + new Vector3(0, 0.6f, 1.5f));
            cam.fieldOfView = fov;
            PlaceDecor(spawn, true);
        }

        public void SetVisible(bool on)
        {
            grid.gameObject.SetActive(on);
            blob.gameObject.SetActive(on);
            if (!on) aimLine.gameObject.SetActive(false);
        }
    }
}
