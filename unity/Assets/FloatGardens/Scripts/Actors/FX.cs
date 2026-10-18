// Efectos: salpicaduras de café (un solo sistema de partículas) y trazadoras de los disparos.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class FX : MonoBehaviour
    {
        static FX I;
        ParticleSystem ps;
        readonly List<TracerShot> tracers = new List<TracerShot>();
        readonly Stack<Transform> tracerPool = new Stack<Transform>();
        Material tracerMat;

        class TracerShot { public Transform t; public Vector3 from, to; public float dist, d; }

        public static void Init(Transform parent)
        {
            var go = new GameObject("FX");
            go.transform.SetParent(parent, false);
            I = go.AddComponent<FX>();
            I.ps = go.AddComponent<ParticleSystem>();
            var main = I.ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.6f;
            main.startSize = 0.12f;
            main.gravityModifier = 1.8f;
            main.maxParticles = 800;
            main.playOnAwake = false;
            var emission = I.ps.emission;
            emission.enabled = false;
            var shape = I.ps.shape;
            shape.enabled = false;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = FGMaterials.Particles("Particula"); // respeta el color de cada partícula
            r.renderMode = ParticleSystemRenderMode.Mesh;
            r.mesh = MeshBuilder.Primitive(PrimitiveType.Cube);
            I.tracerMat = FGMaterials.Emissive("trazadora", FGMaterials.Hex("#a0612f"), 1.2f);
        }

        public static void Burst(Vector3 at, Color color, int count, float speed)
        {
            if (I == null) return;
            var ep = new ParticleSystem.EmitParams { position = at, startColor = color, applyShapeToPosition = false };
            for (int i = 0; i < count; i++)
            {
                ep.velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), Random.Range(-1f, 1f)) * speed;
                ep.startLifetime = Random.Range(0.3f, 0.7f);
                I.ps.Emit(ep, 1);
            }
        }

        public static void Tracer(Vector3 from, Vector3 to)
        {
            if (I == null) return;
            Transform t;
            if (I.tracerPool.Count > 0) t = I.tracerPool.Pop();
            else
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Destroy(go.GetComponent<Collider>());
                go.GetComponent<MeshRenderer>().sharedMaterial = I.tracerMat;
                go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.transform.localScale = new Vector3(0.1f, 0.35f, 0.1f);
                t = go.transform;
                t.SetParent(I.transform, false);
            }
            t.gameObject.SetActive(true);
            t.position = from;
            t.rotation = Quaternion.LookRotation(to - from) * Quaternion.Euler(90, 0, 0);
            I.tracers.Add(new TracerShot { t = t, from = from, to = to, dist = Vector3.Distance(from, to) });
        }

        void Update()
        {
            for (int i = tracers.Count - 1; i >= 0; i--)
            {
                var tr = tracers[i];
                tr.d += 160f * Time.deltaTime;
                float k = tr.dist > 0 ? Mathf.Min(1, tr.d / tr.dist) : 1;
                tr.t.position = Vector3.Lerp(tr.from, tr.to, k);
                if (k >= 1)
                {
                    tr.t.gameObject.SetActive(false);
                    tracerPool.Push(tr.t);
                    tracers.RemoveAt(i);
                }
            }
        }
    }
}
