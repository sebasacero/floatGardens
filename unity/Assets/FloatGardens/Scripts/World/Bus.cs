// Bus rojo articulado que recorre el carril exclusivo de la Av. Caracas y para en las estaciones.
using UnityEngine;

namespace FloatGardens
{
    public class Bus : MonoBehaviour
    {
        public int dir = 1;
        public float lane;
        float stop;
        float lastStation = float.NaN;
        const float Speed = 11f;

        public static Bus Create(Transform parent, float lane, float z, int dir)
        {
            var go = new GameObject("BusArticulado");
            go.transform.SetParent(parent, false);
            var red = FGMaterials.Lit("busRojo", FGMaterials.Hex("#c62828"), null, 0.55f, 0.2f);
            var win = FGMaterials.Lit("busVentana", FGMaterials.Hex("#1d2b36"), null, 0.9f);
            var dark = FGMaterials.Lit("#333333", 0.2f);
            foreach (float off in new[] { -5.2f, 5.2f })
            {
                Part(go.transform, new Vector3(0, 1.9f, off), new Vector3(2.5f, 3f, 9.6f), red);
                Part(go.transform, new Vector3(0, 2.3f, off), new Vector3(2.56f, 1f, 8.6f), win);
                Part(go.transform, new Vector3(0, 0.45f, off - 3), new Vector3(2.6f, 0.9f, 0.9f), dark);
                Part(go.transform, new Vector3(0, 0.45f, off + 3), new Vector3(2.6f, 0.9f, 0.9f), dark);
            }
            Part(go.transform, new Vector3(0, 1.8f, 0), new Vector3(2.3f, 2.6f, 1f), dark);
            var bus = go.AddComponent<Bus>();
            bus.dir = dir;
            bus.lane = lane;
            go.transform.position = new Vector3(lane, 0, z);
            go.transform.rotation = Quaternion.Euler(0, dir > 0 ? 0 : 180, 0);
            return bus;
        }

        static void Part(Transform parent, Vector3 pos, Vector3 size, Material m)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(parent, false);
            p.transform.localPosition = pos;
            p.transform.localScale = size;
            p.GetComponent<MeshRenderer>().sharedMaterial = m;
        }

        void Update()
        {
            if (stop > 0) { stop -= Time.deltaTime; return; }
            Vector3 p = transform.position;
            p.z += dir * Speed * Time.deltaTime;
            if (p.z > FGConfig.Half + 20) p.z = -FGConfig.Half - 20;
            if (p.z < -FGConfig.Half - 20) p.z = FGConfig.Half + 20;
            foreach (float sz in City.I.stationZ)
                if (Mathf.Abs(p.z - sz) < 0.4f && !Mathf.Approximately(lastStation, sz)) { stop = 3; lastStation = sz; }
            transform.position = p;

            // empuja al jugador fuera del bus
            var pl = PlayerController.I;
            if (pl != null && Mathf.Abs(pl.transform.position.x - lane) < 1.25f + 0.45f && Mathf.Abs(pl.transform.position.z - p.z) < 10.5f)
            {
                float side = Mathf.Sign(pl.transform.position.x - lane);
                if (side == 0) side = 1;
                pl.Teleport(new Vector3(lane + side * 1.8f, pl.transform.position.y, pl.transform.position.z), false);
            }
        }
    }
}
