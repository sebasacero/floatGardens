// Estación o personaje con el que se interactúa con E (o el botón táctil). StoryGame conecta la acción según el id:
// cultivo, beneficio, tostadora, aeropress, pc, cama, puerta, volver, bodega, mentor, jueces
// (cualquier otro id reproduce el diálogo de StoryData.Dialogs con ese mismo id, si existe).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();

        public string id;
        public string prompt;      // "Preparar AeroPress", "Hablar con Doña Rosa"...
        public float radius = 2.2f;
        public Action onUse;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public static Interactable Add(GameObject go, string id, string prompt, float radius = 2.2f)
        {
            var it = go.AddComponent<Interactable>();
            it.id = id;
            it.prompt = prompt;
            it.radius = radius;
            return it;
        }

        /// <summary>La estación más cercana dentro de su radio, o null.</summary>
        public static Interactable Nearest(Vector3 pos)
        {
            Interactable best = null;
            float bestD = float.MaxValue;
            foreach (var it in All)
            {
                if (it == null || !it.isActiveAndEnabled) continue;
                Vector3 d = it.transform.position - pos;
                d.y = 0f;
                float dist = d.magnitude;
                if (dist <= it.radius && dist < bestD) { best = it; bestD = dist; }
            }
            return best;
        }
    }
}
