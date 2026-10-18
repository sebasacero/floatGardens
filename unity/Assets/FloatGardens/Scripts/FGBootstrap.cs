// Punto de entrada: pon este componente en un objeto vacío de la escena (el menú Float Gardens lo hace por ti).
// Todo lo demás (ciudad, personajes, HUD, menús, atmósfera) se genera por código al dar Play.
using UnityEngine;

namespace FloatGardens
{
    [DisallowMultipleComponent]
    public class FGBootstrap : MonoBehaviour
    {
        [Header("Modelos opcionales (humanoides con Animator: Speed, Aiming, Dead)")]
        [Tooltip("Modelo del barista, p. ej. un personaje de Mixamo. Si está vacío se usa el personaje low-poly.")]
        public GameObject playerModel;
        [Tooltip("Modelo de los inspectores y guardias de la Federación.")]
        public GameObject enemyModel;
        [Tooltip("Modelo de Don Monopolio (se escala x2.6).")]
        public GameObject bossModel;
        [Tooltip("Modelo de los peatones.")]
        public GameObject pedestrianModel;

        void Awake()
        {
            if (FindObjectsByType<FGBootstrap>(FindObjectsSortMode.None).Length > 1) { Destroy(gameObject); return; }
            var gm = gameObject.AddComponent<GameManager>();
            gm.playerModel = playerModel;
            gm.enemyModel = enemyModel;
            gm.bossModel = bossModel;
            gm.pedestrianModel = pedestrianModel;
            gm.Init();
        }
    }
}
