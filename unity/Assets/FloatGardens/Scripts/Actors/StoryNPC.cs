// Personaje de la historia: se queda en su sitio, mira al jugador cuando se acerca y lleva su nombre encima
// (con un "!" amarillo cuando tiene misión para ti).
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens
{
    public class StoryNPC : MonoBehaviour
    {
        public NPCDef def;
        public Humanoid rig;
        Text label;
        Transform labelRoot;
        bool marker;
        float phase;

        public static StoryNPC Spawn(NPCDef def, Vector3 pos, Transform parent)
        {
            var go = new GameObject("NPC " + def.name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var n = go.AddComponent<StoryNPC>();
            n.def = def;
            n.rig = Humanoid.Create("Modelo", def.style, go.transform);
            n.phase = Random.value * 10;

            // Etiqueta flotante (canvas en el mundo que siempre mira a la cámara)
            var canvasGo = new GameObject("Nombre", typeof(RectTransform));
            canvasGo.transform.SetParent(go.transform, false);
            canvasGo.transform.localPosition = new Vector3(0, 2.45f, 0);
            canvasGo.transform.localScale = Vector3.one * 0.01f;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 6;
            ((RectTransform)canvasGo.transform).sizeDelta = new Vector2(400, 90);
            n.labelRoot = canvasGo.transform;
            UIKit.Image(canvasGo.transform, "Fondo", new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(300, 44), new Color(0, 0, 0, 0.55f));
            UIKit.Label(canvasGo.transform, def.name, 26, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(300, 44));
            n.label = UIKit.Label(canvasGo.transform, "!", 46, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 44), new Vector2(60, 46));
            n.label.gameObject.SetActive(false);
            return n;
        }

        public void SetMarker(bool on)
        {
            marker = on;
            if (label != null) label.gameObject.SetActive(on);
        }

        void Update()
        {
            var cam = Camera.main;
            if (cam == null) return;
            // La etiqueta mira a la cámara; el personaje, al jugador si está cerca
            labelRoot.rotation = Quaternion.LookRotation(labelRoot.position - cam.transform.position);
            if (marker) labelRoot.localPosition = new Vector3(0, 2.45f + Mathf.Sin(Time.time * 3 + phase) * 0.08f, 0);
            var g = GameManager.I;
            if (g != null && g.player != null)
            {
                Vector3 to = g.player.transform.position - transform.position;
                to.y = 0;
                if (to.sqrMagnitude < 36 && to.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), Time.deltaTime * 4);
            }
            rig.Animate(Time.time * 1.5f + phase, 0.04f, false);
        }
    }
}
