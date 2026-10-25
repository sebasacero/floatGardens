// Dron fumigador de las agrocorporaciones: patrulla en círculos sobre un barrio con un reflector que barre las calles.
// Ambientación, no enemigo. De noche el reflector se enciende con fuerza.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class Drone : MonoBehaviour
    {
        Vector3 center;
        float radius, height, speed, phase;
        Light spot;
        Material beacon;
        Transform[] rotors;
        static readonly Color BeaconRed = new Color(1f, 0.12f, 0.1f);

        public static Drone Create(Transform parent, Vector3 center, float radius, float height, float speed, float phase)
        {
            var go = new GameObject("Dron AgroCorp");
            go.transform.SetParent(parent, false);
            var d = go.AddComponent<Drone>();
            d.center = center; d.radius = radius; d.height = height; d.speed = speed; d.phase = phase;

            var dark = FGMaterials.Lit("dronCuerpo", FGMaterials.Hex("#23262b"), null, 0.55f, 0.8f);
            var green = FGMaterials.Lit("dronTanque", FGMaterials.Hex("#5fbf4a"), null, 0.6f, 0.2f);
            // Cuerpo, tanque y brazos en una malla compartida; las hélices giran, así que van aparte
            var body = new List<FGMesh.Part>
            {
                FGMesh.P(PrimitiveType.Cube, Vector3.zero, new Vector3(1.4f, 0.35f, 1.4f), dark),
                FGMesh.P(PrimitiveType.Cylinder, new Vector3(0, -0.55f, 0), new Vector3(0.9f, 0.35f, 0.9f), green),
            };
            d.rotors = new Transform[4];
            int i = 0;
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                {
                    body.Add(FGMesh.P(PrimitiveType.Cube, new Vector3(sx * 0.9f, 0, sz * 0.9f), Quaternion.Euler(0, sz * sx * 45, 0), new Vector3(0.9f, 0.08f, 0.08f), dark));
                    d.rotors[i++] = Part(go.transform, PrimitiveType.Cylinder, new Vector3(sx * 1.25f, 0.12f, sz * 1.25f), new Vector3(1.1f, 0.015f, 1.1f), dark);
                }
            FGMesh.Bake(go.transform, "Cuerpo", body, "dron");
            var beaconMat = FGMaterials.Instance(FGMaterials.Emissive("baliza", BeaconRed, 3f));
            Part(go.transform, PrimitiveType.Cube, new Vector3(0, 0.28f, -0.5f), new Vector3(0.18f, 0.18f, 0.18f), beaconMat);
            d.beacon = beaconMat;

            var lightGo = new GameObject("Reflector");
            lightGo.transform.SetParent(go.transform, false);
            lightGo.transform.localPosition = new Vector3(0, -0.3f, 0.4f);
            lightGo.transform.localRotation = Quaternion.Euler(78, 0, 0);
            d.spot = lightGo.AddComponent<Light>();
            d.spot.type = LightType.Spot;
            d.spot.spotAngle = 34;
            d.spot.innerSpotAngle = 18;
            d.spot.range = 90;
            d.spot.color = FGMaterials.Hex("#e8f4ff");
            d.spot.intensity = 0;
            d.spot.shadows = LightShadows.None;
            return d;
        }

        static Transform Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material m)
        {
            var p = GameObject.CreatePrimitive(type);
            Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(parent, false);
            p.transform.localPosition = pos;
            p.transform.localScale = scale;
            p.GetComponent<MeshRenderer>().sharedMaterial = m;
            return p.transform;
        }

        void Update()
        {
            float dt = Time.deltaTime, t = Time.time;
            phase += speed * dt / radius;
            Vector3 pos = center + new Vector3(Mathf.Cos(phase), 0, Mathf.Sin(phase)) * radius + Vector3.up * (height + Mathf.Sin(t * 0.7f + phase) * 1.5f);
            Vector3 tangent = new Vector3(-Mathf.Sin(phase), 0, Mathf.Cos(phase));
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(tangent) * Quaternion.Euler(6, 0, -14); // se inclina en la curva
            foreach (var r in rotors) r.Rotate(0, 1800 * dt, 0, Space.Self);
            float night = City.I != null ? City.I.Night : 0;
            spot.intensity = Mathf.Lerp(0.3f, 14f, night);
            bool on = Mathf.Repeat(t * 1.6f + phase, 1f) < 0.12f;
            FGMaterials.SetEmission(beacon, BeaconRed, on ? 5f : 0.2f);
        }
    }

    /// <summary>Holograma publicitario: gira despacio, parpadea y se ve más de noche.</summary>
    public class Hologram : MonoBehaviour
    {
        Material mat;
        Color color;
        float seed;

        public void Init(Material m, Color c) { mat = m; color = c; seed = Random.value * 100; }

        void Update()
        {
            transform.Rotate(0, 9 * Time.deltaTime, 0);
            float t = Time.time + seed;
            float flicker = 0.85f + 0.15f * Mathf.Sin(t * 17) * Mathf.Sin(t * 3.1f);
            if (Mathf.Repeat(t, 4.7f) < 0.08f) flicker *= 0.3f; // corte de señal
            float night = City.I != null ? City.I.Night : 0;
            float glow = Mathf.Lerp(1.1f, 2.4f, night) * flicker;
            mat.color = new Color(color.r * glow, color.g * glow, color.b * glow, Mathf.Lerp(0.16f, 0.34f, night) * flicker);
        }
    }
}
