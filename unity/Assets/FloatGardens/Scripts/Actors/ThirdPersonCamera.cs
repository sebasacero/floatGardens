// Cámara sobre el hombro estilo shooter móvil, con colisión contra paredes, zoom al apuntar y sacudidas.
using UnityEngine;

namespace FloatGardens
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public PlayerController target;
        public Camera cam;
        public float Distance { get; private set; } = 3.6f;
        float shake;

        public void Shake(float amount) { shake = Mathf.Max(shake, amount); }

        /// <summary>Encuadre de juego.</summary>
        public void Follow(float dt)
        {
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
            float a = Mathf.Sin(t * 0.25f) * 0.35f;
            Vector3 pos = spawn + new Vector3(1.2f + Mathf.Sin(a) * 4.5f, 1.65f, -Mathf.Cos(a) * 4.5f);
            transform.position = pos;
            transform.LookAt(spawn + new Vector3(-1.6f, 1.5f, 0));
            cam.fieldOfView = 55;
        }
    }
}
