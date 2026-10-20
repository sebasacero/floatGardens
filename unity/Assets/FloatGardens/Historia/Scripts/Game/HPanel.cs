// Base de todos los paneles (estaciones de la bodega, PC, retos, mapa, competencias).
// StoryGame los abre con OpenPanel<T>() y solo hay uno abierto a la vez.
using UnityEngine;

namespace FloatGardens.Historia
{
    public abstract class HPanel : MonoBehaviour
    {
        public RectTransform root;
        protected StoryGame G => StoryGame.I;
        protected HistoriaSave S => StoryGame.I.S;

        /// <summary>Horas de juego que cuesta abrir el panel (usar una estación gasta tiempo).</summary>
        public virtual float HoursCost => 0f;

        public void Open(RectTransform parent)
        {
            root = UIKit.Stretch(parent, GetType().Name);
            Build();
        }

        /// <summary>Construye la interfaz dentro de root.</summary>
        protected abstract void Build();

        /// <summary>Borra y vuelve a construir (después de un cambio grande).</summary>
        protected void Rebuild()
        {
            if (root == null) return;
            UIKit.Clear(root);
            Build();
        }

        public virtual void Tick(float dt) { }
        public virtual void OnClosed() { }
        public void Close() => G.ClosePanel(this);
    }
}
