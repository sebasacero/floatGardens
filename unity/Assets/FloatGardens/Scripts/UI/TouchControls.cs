// Controles táctiles estilo CoD Mobile: joystick, deslizar para mirar, botones de disparo, apuntar, salto, recarga y acción.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FloatGardens
{
    public class TouchControls : MonoBehaviour
    {
        Canvas canvas;

        public void Build()
        {
            canvas = UIKit.Canvas("ControlesTactiles", 20);
            canvas.transform.SetParent(transform, false);
            var root = canvas.transform;

            // Zona para mirar: mitad derecha de la pantalla (debajo de los botones)
            var look = UIKit.Rect(root, "Mirar", new Vector2(0.42f, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var lookImg = look.gameObject.AddComponent<Image>();
            lookImg.color = new Color(0, 0, 0, 0);
            look.gameObject.AddComponent<TouchLook>();

            // Joystick
            var stickBg = UIKit.Image(root, "Joystick", new Vector2(0, 0), new Vector2(70, 50), new Vector2(170, 170), new Color(1, 1, 1, 0.12f), UIKit.Circle);
            stickBg.raycastTarget = true;
            var knob = UIKit.Image(stickBg.transform, "Perilla", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72, 72), new Color(1, 0.76f, 0.1f, 0.8f), UIKit.Circle);
            var stick = stickBg.gameObject.AddComponent<TouchStick>();
            stick.knob = knob.rectTransform;

            Button(root, "DISPARO", FGAction.Fire, new Vector2(-60, 60), 120, new Color(1, 0.3f, 0.23f, 0.5f), true);
            Button(root, "APUNTAR", FGAction.Aim, new Vector2(-200, 40), 76, new Color(0, 0, 0, 0.45f), false, toggle: true);
            Button(root, "SALTO", FGAction.Jump, new Vector2(-40, 210), 76, new Color(0, 0, 0, 0.45f), false);
            Button(root, "RECARGA", FGAction.Reload, new Vector2(-170, 150), 68, new Color(0, 0, 0, 0.45f), false);
            Button(root, "E", FGAction.Use, new Vector2(-290, 110), 80, new Color(0.18f, 0.75f, 0.44f, 0.6f), false);
            SmallButton(root, "MAPA", FGAction.Map, new Vector2(200, -14));
            SmallButton(root, "II", FGAction.Pause, new Vector2(262, -14));
            Show(false);
        }

        void Button(Transform root, string label, FGAction action, Vector2 pos, float size, Color color, bool dragLooks, bool toggle = false)
        {
            var img = UIKit.Image(root, label, new Vector2(1, 0), pos, Vector2.one * size, color, UIKit.Circle);
            img.raycastTarget = true;
            UIKit.Label(img.transform, label, label.Length > 2 ? 14 : 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * size);
            var b = img.gameObject.AddComponent<TouchButton>();
            b.action = action;
            b.dragLooks = dragLooks;
            b.toggle = toggle;
        }

        void SmallButton(Transform root, string label, FGAction action, Vector2 pos)
        {
            var img = UIKit.Image(root, label, new Vector2(0, 1), pos, new Vector2(54, 44), new Color(0, 0, 0, 0.45f));
            img.raycastTarget = true;
            UIKit.Label(img.transform, label, 14, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54, 44));
            img.gameObject.AddComponent<TouchButton>().action = action;
        }

        public void Show(bool on)
        {
            canvas.gameObject.SetActive(on);
            if (!on) FGInput.ResetTouch();
        }
    }

    public class TouchStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform knob;
        RectTransform rt;
        void Awake() { rt = (RectTransform)transform; }

        public void OnPointerDown(PointerEventData e) { OnDrag(e); }

        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out Vector2 lp);
            Vector2 v = lp / (rt.rect.width / 2);
            v = Vector2.ClampMagnitude(v, 1);
            FGInput.touchMove = v.magnitude < 0.12f ? Vector2.zero : v;
            knob.anchoredPosition = v * rt.rect.width * 0.28f;
        }

        public void OnPointerUp(PointerEventData e)
        {
            FGInput.touchMove = Vector2.zero;
            knob.anchoredPosition = Vector2.zero;
        }
    }

    public class TouchLook : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData e) { FGInput.touchLookPixels += e.delta; }
    }

    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public FGAction action;
        public bool dragLooks, toggle;
        Image img;
        Color baseColor;

        void Awake() { img = GetComponent<Image>(); baseColor = img.color; }

        public void OnPointerDown(PointerEventData e)
        {
            if (toggle) { FGInput.ToggleTouch(action); img.color = FGInput.Held(action) ? new Color(1, 0.76f, 0.1f, 0.6f) : baseColor; return; }
            FGInput.TouchDown(action);
            img.color = new Color(1, 0.76f, 0.1f, 0.6f);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (toggle) return;
            FGInput.TouchUp(action);
            img.color = baseColor;
        }

        // El botón de disparo también sirve para apuntar arrastrando (como en los shooters móviles)
        public void OnDrag(PointerEventData e)
        {
            if (dragLooks) FGInput.touchLookPixels += e.delta;
        }
    }
}
