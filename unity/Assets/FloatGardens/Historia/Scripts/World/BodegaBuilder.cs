// PLACEHOLDER (lo reemplaza el constructor de la bodega). Firmas obligatorias.
using UnityEngine;
namespace FloatGardens.Historia
{
    public static class BodegaBuilder
    {
        /// <summary>Bodega de Float Gardens en Bogotá con las estaciones (Interactable ids: cultivo, beneficio, tostadora, aeropress, pc, cama, puerta).</summary>
        public static SceneInfo Build(Transform root, HistoriaSave s) { return new SceneInfo { spawn = Vector3.zero }; }
        /// <summary>Bogotá como región del mapa (centro / La Candelaria / feria). Interactables: mentor, bodega, volver.</summary>
        public static SceneInfo BuildCity(Transform root, HistoriaSave s) { return new SceneInfo { spawn = Vector3.zero }; }
        /// <summary>Refresca las plantas visibles en las balsas según el guardado (lo llama GardenPanel al cerrar).</summary>
        public static void RefreshPlants(HistoriaSave s) { }
    }
}
