// PLACEHOLDER. Firmas obligatorias.
using UnityEngine;
namespace FloatGardens.Historia
{
    public static class FincaBuilder
    {
        /// <summary>Finca cafetera de la región. Interactables: mentor (en el NPC), volver (camino de regreso al bus).</summary>
        public static SceneInfo Build(Transform root, Region r, HistoriaSave s) { return new SceneInfo { spawn = Vector3.zero, altitude = r.Altitude }; }
    }
}
