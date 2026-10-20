// El barista del modo historia: camina en tercera persona (sin disparos), mira con el mouse o deslizando,
// y usa estaciones con E. La cámara orbita detrás y no atraviesa paredes.
using UnityEngine;

namespace FloatGardens.Historia
{
    [RequireComponent(typeof(CharacterController))]
    public class StoryPlayer : MonoBehaviour
    {
        public CharacterController cc;
        public Humanoid rig;
        public Transform aeropress;
        public float yaw, pitch = 12f;
        public Interactable focus;     // estación al alcance
        StoryGame g;
        float vy, phase, moving, camDist = 4f;

        public static StoryPlayer Create(StoryGame game, GameObject modelPrefab)
        {
            var go = new GameObject("Barista");
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.85f;
            cc.radius = 0.38f;
            cc.center = new Vector3(0, 0.93f, 0);
            cc.stepOffset = 0.4f;
            cc.slopeLimit = 55f;
            var p = go.AddComponent<StoryPlayer>();
            p.cc = cc;
            p.g = game;
            p.rig = Humanoid.Create("Modelo", new HumanoidStyle
            {
                shirt = FGMaterials.Hex("#f5f1e8"), pants = FGMaterials.Hex("#2c2a27"), apron = FGMaterials.Hex("#4a2c1d"),
                cap = FGMaterials.Hex("#2fbf71"), hair = FGMaterials.Hex("#2a1a10"), skin = FGMaterials.Hex("#c98f62"),
            }, go.transform, modelPrefab);
            p.aeropress = Humanoid.MakeAeroPress(p.rig.rightHand); // siempre con su AeroPress en la mano
            return p;
        }

        public void Teleport(Vector3 pos, float newYaw)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
            yaw = newYaw;
            pitch = 12f;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            vy = 0f;
            Follow(1f, true);
        }

        /// <summary>Movimiento, cámara e interacción (solo cuando no hay paneles ni diálogos abiertos).</summary>
        public void Tick(float dt)
        {
            if (FGInput.ClickedAnywhere() && !FGInput.CursorLocked && !FGInput.IsTouchDevice && !PointerOverUI()) FGInput.CursorLocked = true;
            var look = FGInput.Look();
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, -25f, 60f);

            Vector2 mv = FGInput.Move();
            bool sprint = FGInput.Held(FGAction.Sprint) || mv.magnitude > 0.95f && FGInput.IsTouchDevice;
            float speed = sprint ? 6.2f : 3.6f;
            Quaternion yRot = Quaternion.Euler(0, yaw, 0);
            Vector3 wish = yRot * new Vector3(mv.x, 0, mv.y);
            if (cc.isGrounded) vy = -2f;
            if (cc.isGrounded && FGInput.Pressed(FGAction.Jump)) { vy = 6f; SFX.Play("jump", 0.4f); }
            vy -= 20f * dt;
            cc.Move((wish * speed + Vector3.up * vy) * dt);

            if (wish.sqrMagnitude > 0.01f)
            {
                Quaternion face = Quaternion.LookRotation(wish);
                transform.rotation = Quaternion.Slerp(transform.rotation, face, Mathf.Min(1f, dt * 12f));
            }
            moving = Mathf.Lerp(moving, wish.magnitude * (sprint ? 1.2f : 0.7f), Mathf.Min(1f, dt * 10f));
            phase += dt * (sprint ? 11f : 7.5f) * Mathf.Clamp01(wish.magnitude);
            rig.Animate(phase, moving, false);

            focus = Interactable.Nearest(transform.position);
            if (focus != null && FGInput.Pressed(FGAction.Use) && focus.onUse != null)
            {
                SFX.Play("pickup", 0.6f);
                focus.onUse();
            }
            Follow(dt, false);
        }

        /// <summary>Quieto mientras hay un panel o diálogo abierto (la cámara sigue encuadrando).</summary>
        public void Idle(float dt)
        {
            moving = Mathf.Lerp(moving, 0f, Mathf.Min(1f, dt * 10f));
            rig.Animate(phase, moving, false);
            focus = null;
            Follow(dt, false);
        }

        void Follow(float dt, bool snap)
        {
            var cam = g != null ? g.cam : Camera.main;
            if (cam == null) return;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pivot = transform.position + Vector3.up * 1.6f;
            Vector3 back = rot * Vector3.back;
            float want = 4f;
            if (Physics.SphereCast(pivot, 0.25f, back, out RaycastHit hit, want + 0.3f, ~0, QueryTriggerInteraction.Ignore) && hit.collider != cc)
                want = Mathf.Max(0.6f, hit.distance - 0.15f);
            camDist = snap ? want : Mathf.Lerp(camDist, want, Mathf.Min(1f, dt * (want < camDist ? 25f : 5f)));
            cam.transform.SetPositionAndRotation(pivot + back * camDist, rot);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 62f, Mathf.Min(1f, dt * 6f));
        }

        static bool PointerOverUI()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }
    }
}
