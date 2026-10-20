// PLACEHOLDER (lo reemplaza el mapa de Colombia). Firmas obligatorias.
using System;
using UnityEngine;
namespace FloatGardens.Historia
{
    public class ColombiaMap : MonoBehaviour
    {
        public SceneInfo info;
        public static ColombiaMap Build(Transform root, HistoriaSave s)
        {
            var m = new GameObject("MapaColombia").AddComponent<ColombiaMap>();
            m.transform.SetParent(root, false);
            m.info = new SceneInfo { walkable = false, cameraPos = new Vector3(0, 120, -60), cameraLook = Vector3.zero, altitude = HConfig.BogotaAltitude };
            return m;
        }
        public void Tick(float dt) { }
        /// <summary>Resalta una región (al pasar el mouse por la lista del MapPanel).</summary>
        public void Focus(string regionId) { }
        /// <summary>Anima el bus desde Bogotá (o la región actual) hasta la región; llama onDone al llegar.</summary>
        public void AnimateTravel(Region to, Action onDone) { onDone?.Invoke(); }
    }
}
