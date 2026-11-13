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
        static Font builtin, regular, semibold, light;
        static Sprite white, circle, harvest;

        // Unity 2022.2+ y Unity 6 traen "LegacyRuntime.ttf" como fuente integrada; es el respaldo
        // si no están las Manrope de Resources/Fonts (la misma tipografía de la web).
        public static Font Builtin => builtin != null ? builtin : builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        static Font Load(ref Font f, string name)
        {
            if (f == null) f = Resources.Load<Font>("Fonts/" + name);
            return f != null ? f : Builtin;
        }
        public static Font Font => Load(ref regular, "Manrope-Regular");
        public static Font SemiBold => Load(ref semibold, "Manrope-SemiBold");
        public static Font Light => Load(ref light, "Manrope-Light");

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

        // Tema "plataforma de control" (el mismo de web/index.html y css/glass.css): vidrio blanco
        // translúcido, texto gris oscuro, verde de cultivo para datos y un solo acento azul para alertas.
        public static readonly Color Ink = new Color32(0x2c, 0x30, 0x2e, 255);
        public static readonly Color Ink2 = new Color32(0x7b, 0x81, 0x7e, 255);
        public static readonly Color Ink3 = new Color32(0xa9, 0xae, 0xab, 255);
        public static readonly Color Leaf = new Color32(0x4f, 0x8f, 0x2c, 255);
        public static readonly Color Leaf2 = new Color32(0x8f, 0xbf, 0x45, 255);
        public static readonly Color Lime = new Color32(0xd6, 0xe4, 0x8a, 255);
        public static readonly Color Alert = new Color32(0x2a, 0x22, 0xf5, 255);
        public static readonly Color Cherry = new Color32(0xc0, 0x16, 0x1c, 255);
        public static readonly Color Glass = new Color(0.965f, 0.973f, 0.969f, 0.86f);
        public static readonly Color Track = new Color32(0xdf, 0xe3, 0xe0, 255);
        public static readonly Color Veil = new Color(0.79f, 0.81f, 0.80f, 0.6f);

        // Nombres de la paleta anterior, apuntando al tema nuevo para que todas las pantallas lo tomen.
        public static readonly Color Yellow = Leaf;
        public static readonly Color Orange = new Color32(0xb7, 0x79, 0x1f, 255);
        public static readonly Color Green = Leaf2;
        public static readonly Color Mint = Leaf;
        public static readonly Color Red = new Color32(0xd6, 0x45, 0x45, 255);
        public static readonly Color Panel = Glass;
        public static readonly Color Text = Ink;

        /// <summary>Degradado verde de la tarjeta de cosecha.</summary>
        public static Sprite Harvest
        {
            get
            {
                if (harvest != null) return harvest;
                const int n = 64;
                var t = new Texture2D(n, n) { wrapMode = TextureWrapMode.Clamp };
                Color a = new Color32(0x7f, 0xb3, 0x49, 255), b = new Color32(0x9c, 0xc6, 0x57, 255), c = new Color32(0xc8, 0xdd, 0x78, 255);
                var px = new Color[n * n];
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float k = (x + (n - 1 - y)) / (2f * (n - 1));
                        px[y * n + x] = k < 0.45f ? Color.Lerp(a, b, k / 0.45f) : Color.Lerp(b, c, (k - 0.45f) / 0.55f);
                    }
                t.SetPixels(px);
                t.Apply();
                return harvest = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
            }
        }

        /// <summary>Lleva los colores de la interfaz oscura anterior al tema claro: los fondos negros
        /// translúcidos pasan a vidrio blanco y los blancos translúcidos a blanco más sólido.</summary>
        public static Color ThemeBackground(Color c)
        {
            if (c.a <= 0.001f) return c;
            if (c.a < 0.97f && c.maxColorComponent < 0.12f) return new Color(Glass.r, Glass.g, Glass.b, Mathf.Lerp(0.55f, 0.92f, c.a));
            if (c.a < 0.5f && c.r > 0.95f && c.g > 0.95f && c.b > 0.95f) return new Color(1, 1, 1, 0.75f);
            return c;
        }

        /// <summary>El texto blanco de antes pasa a gris oscuro; el rosado de "zona enemiga" pasa a rojo.</summary>
        public static Color ThemeText(Color c)
        {
            if (c.r > 0.9f && c.g > 0.9f && c.b > 0.9f) return c.a >= 0.9f ? Ink : Ink2;
            if (c.r > 0.95f && c.g > 0.5f && c.g < 0.6f && c.b > 0.45f && c.b < 0.5f) return Red;
            return c;
        }

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
            img.color = ThemeBackground(color);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Tarjeta de vidrio, como las de la web.</summary>
        public static Image Card(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
            => Image(parent, name, anchor, pos, size, Glass);

        /// <summary>Tarjeta verde de cosecha (degradado).</summary>
        public static Image HarvestCard(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
            => Image(parent, name, anchor, pos, size, Color.white, Harvest);

        /// <summary>Texto blanco sobre una tarjeta de color (verde o azul), sin la conversión del tema.</summary>
        public static Text OnColor(Text t)
        {
            t.color = Color.white;
            t.GetComponent<Shadow>().enabled = false;
            return t;
        }

        /// <summary>Texto que flota sobre el mundo 3D sin tarjeta: blanco con sombra suave.</summary>
        public static Text WorldLabel(Transform parent, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 pos, Vector2 box, FontStyle style = FontStyle.Bold)
        {
            var t = Label(parent, text, size, color, align, anchor, pos, box, style);
            t.color = color;
            var sh = t.GetComponent<Shadow>();
            sh.enabled = true;
            sh.effectColor = new Color(0.12f, 0.16f, 0.14f, 0.55f);
            sh.effectDistance = new Vector2(0, -1.5f);
            return t;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 pos, Vector2 box, FontStyle style = FontStyle.Bold)
        {
            var rt = At(parent, "Texto", anchor, pos, box);
            var t = rt.gameObject.AddComponent<Text>();
            // Como en la web: números y títulos grandes en Manrope Light, etiquetas en SemiBold o Regular.
            t.font = size >= 34 ? Light : style == FontStyle.Bold ? SemiBold : Font;
            t.text = text;
            t.fontSize = size;
            t.color = ThemeText(color);
            t.alignment = align;
            t.fontStyle = t.font == Builtin ? style : FontStyle.Normal;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.75f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            sh.enabled = false;
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
            // Sobre un botón de color sólido (verde, azul, rojo) el texto va en blanco.
            Color.RGBToHSV(img.color, out _, out float sat, out _);
            if (img.color.a > 0.9f && sat > 0.35f) OnColor(t);
            return b;
        }

        public static Image Bar(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color fill, out Image background)
        {
            background = Image(parent, name, anchor, pos, size, Track);
            var f = Image(background.transform, "Relleno", new Vector2(0, 0.5f), Vector2.zero, size, fill);
            f.type = UnityEngine.UI.Image.Type.Filled;
            f.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            f.fillOrigin = 0;
            f.fillAmount = 1;
            return f;
        }

        /// <summary>Deslizador horizontal con etiqueta de valor; onChange recibe el valor.</summary>
        public static Slider Slider(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, float min, float max, float value, bool whole, Action<float> onChange)
        {
            var bg = Image(parent, "Deslizador", anchor, pos, size, Track);
            bg.raycastTarget = true;
            var s = bg.gameObject.AddComponent<Slider>();
            var fillArea = Rect(bg.transform, "Area", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var fill = Image(fillArea, "Relleno", new Vector2(0, 0.5f), Vector2.zero, new Vector2(10, size.y), Yellow);
            var fillRt = fill.rectTransform;
            fillRt.anchorMin = new Vector2(0, 0); fillRt.anchorMax = new Vector2(0, 1); fillRt.pivot = new Vector2(0, 0.5f);
            fillRt.anchoredPosition = Vector2.zero; fillRt.sizeDelta = new Vector2(0, 0);
            var handleArea = Rect(bg.transform, "Mango", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-16, 0));
            var handle = Image(handleArea, "Boton", new Vector2(0, 0.5f), Vector2.zero, new Vector2(18, size.y + 8), Color.white, Circle);
            handle.raycastTarget = true;
            s.fillRect = fillRt;
            s.handleRect = handle.rectTransform;
            s.targetGraphic = handle;
            s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            s.minValue = min;
            s.maxValue = max;
            s.wholeNumbers = whole;
            s.value = value;
            s.onValueChanged.AddListener(v => onChange(v));
            return s;
        }

        /// <summary>Lista con scroll vertical; devuelve el contenido donde se agregan los elementos (ancla arriba).</summary>
        public static RectTransform Scroll(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, out ScrollRect scroll)
        {
            var view = Image(parent, "Lista", anchor, pos, size, new Color(0, 0, 0, 0.25f));
            view.raycastTarget = true;
            view.gameObject.AddComponent<RectMask2D>();
            var content = Rect(view.transform, "Contenido", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, size.y));
            scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = view.rectTransform;
            scroll.horizontal = false;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30;
            return content;
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
