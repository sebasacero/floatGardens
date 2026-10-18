// Ayudas para construir la interfaz (uGUI) desde código, sin prefabs.
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FloatGardens
{
    public static class UIKit
    {
        static Font font;
        static Sprite white, circle;

        // Unity 2022.2+ y Unity 6 traen "LegacyRuntime.ttf" como fuente integrada.
        public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Sprite White
        {
            get
            {
                if (white != null) return white;
                var t = new Texture2D(4, 4);
                var px = new Color32[16];
                for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
                t.SetPixels32(px);
                t.Apply();
                return white = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle != null) return circle;
                FGTextures.Init();
                var t = FGTextures.Circle;
                return circle = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
            }
        }

        public static readonly Color Yellow = new Color32(0xff, 0xc2, 0x1a, 255);
        public static readonly Color Orange = new Color32(0xff, 0x7a, 0x1a, 255);
        public static readonly Color Green = new Color32(0x2f, 0xbf, 0x71, 255);
        public static readonly Color Mint = new Color32(0x7d, 0xff, 0xb0, 255);
        public static readonly Color Red = new Color32(0xff, 0x4b, 0x3a, 255);
        public static readonly Color Ink = new Color32(0x0b, 0x1a, 0x14, 255);
        public static readonly Color Panel = new Color(0.03f, 0.07f, 0.055f, 0.85f);
        public static readonly Color Text = new Color32(0xf3, 0xf1, 0xea, 255);

        public static Canvas Canvas(string name, int order)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.6f;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Rectángulo anclado a un punto (anchor = pivot).</summary>
        public static RectTransform At(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
            => Rect(parent, name, anchor, anchor, anchor, pos, size);

        public static RectTransform Stretch(Transform parent, string name)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return rt;
        }

        public static Image Image(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var rt = At(parent, name, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite ? sprite : White;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 pos, Vector2 box, FontStyle style = FontStyle.Bold)
        {
            var rt = At(parent, "Texto", anchor, pos, box);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.75f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        public static Button Button(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg, Color fg, Action onClick, int fontSize = 22)
        {
            var img = Image(parent, "Boton " + label, anchor, pos, size, bg);
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1);
            colors.disabledColor = new Color(1, 1, 1, 0.35f);
            b.colors = colors;
            b.onClick.AddListener(new UnityAction(onClick));
            var t = Label(img.transform, label, fontSize, fg, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(10, 4));
            t.GetComponent<Shadow>().enabled = fg != Ink;
            return b;
        }

        public static Image Bar(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color fill, out Image background)
        {
            background = Image(parent, name, anchor, pos, size, new Color(0, 0, 0, 0.55f));
            var f = Image(background.transform, "Relleno", new Vector2(0, 0.5f), Vector2.zero, size, fill);
            f.type = UnityEngine.UI.Image.Type.Filled;
            f.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            f.fillOrigin = 0;
            f.fillAmount = 1;
            return f;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
