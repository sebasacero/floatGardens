// Punto de entrada del modo historia "Float Gardens: Tierra de Café". El menú Float Gardens > Historia crea la escena
// con este componente; todo lo demás (bodega, mapa de Colombia, fincas, interfaz) se genera por código al dar Play.
using UnityEngine;

namespace FloatGardens.Historia
{
    [DisallowMultipleComponent]
    public class HistoriaBootstrap : MonoBehaviour
    {
        [Tooltip("Modelo animado opcional para el barista (humanoide con Animator: Speed, Aiming, Dead). Vacío = personaje low-poly.")]
        public GameObject playerModel;

        void Awake()
        {
            if (FindObjectsByType<HistoriaBootstrap>(FindObjectsSortMode.None).Length > 1) { Destroy(gameObject); return; }
            var g = gameObject.AddComponent<StoryGame>();
            g.playerModel = playerModel;
            g.Init();
        }
    }
}
