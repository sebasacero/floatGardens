// Cámara sobre el hombro estilo shooter móvil, con colisión contra paredes, zoom al apuntar y sacudidas.
// En celular (FGStyle.Miniatura) es una cámara de teleobjetivo desde lo alto: la ciudad se ve como una maqueta.
using UnityEngine;

namespace FloatGardens
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public PlayerController target;
        public Camera cam;
        public float Distance { get; private set; } = 3.6f;
        /// <summary>Distancia de la cámara al punto enfocado: la profundidad de campo y la bruma de la maqueta la siguen.</summary>
        public float Focus { get; private set; } = 60f;
        float shake;

        // Maqueta: lente larga (FOV 16-22°) lejos del barista. Si un edificio lo tapa, la cámara sube hacia cenital.
        const float MiniDistance = 58f, MiniAimDistance = 46f, MiniPitch = 42f, MiniMaxPitch = 74f;
        const float MiniFov = 22f, MiniAimFov = 18f, LobbyDistance = 420f;
        float miniPitch = MiniPitch;
        static readonly RaycastHit[] hits = new RaycastHit[16];

        public void Shake(float amount) { shake = Mathf.Max(shake, amount); }

        /// <summary>Encuadre de juego.</summary>
        public void Follow(float dt)
        {
            if (FGStyle.Miniatura) { FollowMiniature(dt); return; }
            var p = target;
            float targetFov = p.aiming ? 48 : p.sprinting ? 76 : 70;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Mathf.Min(1, dt * 10));
            float dist = p.aiming ? 2.3f : 3.6f;
            float side = p.aiming ? 0.95f : 1.05f;
            Quaternion rot = Quaternion.Euler(-(p.pitch + p.recoil), p.yaw, 0);
            Vector3 right = Quaternion.Euler(0, p.yaw, 0) * Vector3.right;
            Vector3 pivot = p.transform.position + Vector3.up * (p.aiming ? 1.8f : 1.95f) + right * side;
            Vector3 back = rot * Vector3.back;
            float d = dist;
            if (Physics.SphereCast(pivot, 0.25f, back, out RaycastHit hit, dist + 0.3f, ~0, QueryTriggerInteraction.Ignore) && hit.collider != p.cc)
                d = Mathf.Max(0.4f, hit.distance - 0.1f);
            Distance = Mathf.Lerp(Distance, d, Mathf.Min(1, dt * (d < Distance ? 30 : 6)));
            Vector3 pos = pivot + back * Distance;
            pos.y = Mathf.Max(0.3f, pos.y);
            if (shake > 0)
            {
                pos += Random.insideUnitSphere * shake;
                shake = Mathf.Max(0, shake - dt * 2);
            }
            transform.SetPositionAndRotation(pos, rot);
        }

        /// <summary>Encuadre del lobby: el barista posa frente al HQ.</summary>
        public void Lobby(Vector3 spawn, float t)
        {
            if (FGStyle.Miniatura) { LobbyMiniature(spawn, t); return; }
            float a = Mathf.Sin(t * 0.25f) * 0.35f;
            Vector3 pos = spawn + new Vector3(1.2f + Mathf.Sin(a) * 4.5f, 1.65f, -Mathf.Cos(a) * 4.5f);
            transform.position = pos;
            transform.LookAt(spawn + new Vector3(-1.6f, 1.5f, 0));
            cam.fieldOfView = 55;
        }

        /// <summary>Encuadre de juego en la maqueta: desde atrás y arriba del barista, mirando un poco adelante.</summary>
        void FollowMiniature(float dt)
        {
            var p = target;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, p.aiming ? MiniAimFov : MiniFov, Mathf.Min(1, dt * 6));
            float dist = p.aiming ? MiniAimDistance : MiniDistance;
            Vector3 pivot = p.transform.position + Vector3.up * 1.2f + p.Forward * (p.aiming ? 6f : 4f);
            float want = MiniMaxPitch;
            for (float a = MiniPitch; a < MiniMaxPitch; a += 8f)
                if (!Blocked(pivot, Quaternion.Euler(a, p.yaw, 0) * Vector3.back, dist)) { want = a; break; }
            miniPitch = Mathf.Lerp(miniPitch, want, Mathf.Min(1, dt * (want > miniPitch ? 4f : 1.5f)));
            Quaternion rot = Quaternion.Euler(miniPitch, p.yaw, 0);
            Distance = Mathf.Lerp(Distance, dist, Mathf.Min(1, dt * 6));
            Focus = Distance;
            Vector3 pos = pivot + rot * Vector3.back * Distance;
            if (shake > 0)
            {
                pos += Random.insideUnitSphere * shake * 3f; // a esta distancia la sacudida tiene que ser mayor
                shake = Mathf.Max(0, shake - dt * 2);
            }
            transform.SetPositionAndRotation(pos, rot);
        }

        /// <summary>Lobby en la maqueta: paneo lento de dron sobre la ciudad, con los cerros y la Virgen al fondo.</summary>
        void LobbyMiniature(Vector3 spawn, float t)
        {
            float yaw = 95f + Mathf.Sin(t * 0.03f) * 18f;
            Vector3 aim = spawn + new Vector3(70f + Mathf.Sin(t * 0.05f) * 50f, 12f, Mathf.Cos(t * 0.04f) * 60f);
            Quaternion rot = Quaternion.Euler(24f, yaw, 0);
            transform.SetPositionAndRotation(aim + rot * Vector3.back * LobbyDistance, rot);
            cam.fieldOfView = 16;
            Focus = LobbyDistance;
            Distance = MiniDistance; // al empezar la partida la cámara baja directo a su distancia de juego
        }

        // ¿Algo sólido (que no sea un personaje) tapa al barista desde esta dirección?
        static bool Blocked(Vector3 from, Vector3 dir, float dist)
        {
            int n = Physics.SphereCastNonAlloc(from, 1.2f, dir, hits, dist, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
                if (!(hits[i].collider is CharacterController)) return true;
            return false;
        }
    }
}
