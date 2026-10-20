// Piezas de interfaz del modo historia, construidas sobre UIKit (uGUI por código, sin prefabs).
// Todo es táctil: botones grandes y "steppers" (– valor +) en lugar de sliders.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public static class HUI
    {
        public static readonly Color PanelBg = new Color(0.07f, 0.045f, 0.03f, 0.96f);   // café oscuro
        public static readonly Color CardColor = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color CardHi = new Color(1f, 0.76f, 0.1f, 0.18f);
        public static readonly Color Muted = new Color(0.95f, 0.92f, 0.85f, 0.65f);
        public static readonly Color Dim = new Color(0.02f, 0.02f, 0.015f, 0.7f);

        /// <summary>Ventana centrada con título y botón de cerrar. Devuelve el área de contenido (debajo del título).</summary>
        public static RectTransform Window(Transform parent, string title, Vector2 size, Action onClose, string subtitle = null)
        {
            var dim = UIKit.Stretch(parent, "Fondo " + title).gameObject.AddComponent<Image>();
            dim.color = Dim;
            dim.raycastTarget = true; // bloquea clics al mundo
            var win = UIKit.Image(dim.transform, "Ventana", new Vector2(0.5f, 0.5f), Vector2.zero, size, PanelBg);
            win.raycastTarget = true;
            var bar = UIKit.Image(win.transform, "Barra", new Vector2(0.5f, 1f), new Vector2(0, -32), new Vector2(size.x, 64), new Color(0, 0, 0, 0.35f));
            UIKit.Image(bar.transform, "Acento", new Vector2(0.5f, 0f), Vector2.zero, new Vector2(size.x, 3), HConfig.Gold);
            UIKit.Label(bar.transform, title, 28, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(24, subtitle != null ? 9 : 0), new Vector2(size.x - 160, 40));
            if (subtitle != null)
                UIKit.Label(bar.transform, subtitle, 15, Muted, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(24, -17), new Vector2(size.x - 160, 22), FontStyle.Normal);
            if (onClose != null)
                UIKit.Button(bar.transform, "X", new Vector2(1f, 0.5f), new Vector2(-36, 0), new Vector2(52, 44), new Color(1, 1, 1, 0.1f), UIKit.Text, onClose, 24);
            var content = UIKit.Rect(win.transform, "Contenido", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0, -32), new Vector2(-32, -96));
            return content;
        }

        /// <summary>Texto de varios renglones alineado arriba a la izquierda dentro de un rectángulo.</summary>
        public static Text Paragraph(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 box, int size = 18, Color? color = null, TextAnchor align = TextAnchor.UpperLeft)
        {
            var t = UIKit.Label(parent, text, size, color ?? UIKit.Text, align, anchor, pos, box, FontStyle.Normal);
            t.lineSpacing = 1.1f;
            return t;
        }

        public static Image Card(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, bool highlight = false)
            => UIKit.Image(parent, "Tarjeta", anchor, pos, size, highlight ? CardHi : CardColor);

        /// <summary>Control "– valor +". Devuelve una función para refrescar el texto si el valor cambia por fuera.</summary>
        public static Action Stepper(Transform parent, string label, Vector2 anchor, Vector2 pos, float width,
            Func<float> get, Action<float> set, float step, float min, float max, Func<float, string> format, Action onChange = null)
        {
            var row = UIKit.At(parent, "Stepper " + label, anchor, pos, new Vector2(width, 46));
            UIKit.Label(row, label, 17, Muted, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), Vector2.zero, new Vector2(width - 210, 40), FontStyle.Normal);
            Text value = null;
            void Refresh() { value.text = format(get()); }
            void Change(float d)
            {
                set(Mathf.Clamp(Mathf.Round((get() + d) / step) * step, min, max));
                Refresh();
                SFX.Play("pickup", 0.5f);
                onChange?.Invoke();
            }
            UIKit.Button(row, "–", new Vector2(1f, 0.5f), new Vector2(-185, 0), new Vector2(46, 42), new Color(1, 1, 1, 0.12f), UIKit.Text, () => Change(-step), 26);
            var box = UIKit.Image(row, "Valor", new Vector2(1f, 0.5f), new Vector2(-103, 0), new Vector2(110, 42), new Color(0, 0, 0, 0.4f));
            value = UIKit.Label(box.transform, "", 19, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(108, 40));
            UIKit.Button(row, "+", new Vector2(1f, 0.5f), new Vector2(-23, 0), new Vector2(46, 42), new Color(1, 1, 1, 0.12f), UIKit.Text, () => Change(step), 26);
            Refresh();
            return Refresh;
        }

        /// <summary>Fila de pestañas. onSelect recibe el índice; devuelve una función para marcar la pestaña activa.</summary>
        public static Action<int> Tabs(Transform parent, string[] labels, Vector2 anchor, Vector2 pos, float width, Action<int> onSelect)
        {
            var row = UIKit.At(parent, "Pestanas", anchor, pos, new Vector2(width, 44));
            float w = width / labels.Length;
            var images = new List<Image>();
            for (int i = 0; i < labels.Length; i++)
            {
                int k = i;
                var b = UIKit.Button(row, labels[i], new Vector2(0f, 0.5f), new Vector2(w * i + w / 2f, 0), new Vector2(w - 6, 42), new Color(1, 1, 1, 0.08f), UIKit.Text, () => onSelect(k), 17);
                images.Add(b.GetComponent<Image>());
            }
            return active =>
            {
                for (int i = 0; i < images.Count; i++) images[i].color = i == active ? UIKit.Yellow * new Color(1, 1, 1, 0.9f) : new Color(1, 1, 1, 0.08f);
                for (int i = 0; i < images.Count; i++)
                {
                    var t = images[i].GetComponentInChildren<Text>();
                    if (t != null) t.color = i == active ? UIKit.Ink : UIKit.Text;
                }
            };
        }

        /// <summary>Lista vertical con scroll. Agrega filas con Row(content, alto).</summary>
        public static RectTransform ScrollList(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float spacing = 8f)
        {
            var view = UIKit.Rect(parent, "Lista", anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            view.offsetMin = offsetMin;
            view.offsetMax = offsetMax;
            var bg = view.gameObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.18f);
            bg.raycastTarget = true;
            view.gameObject.AddComponent<RectMask2D>();
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            var content = UIKit.Rect(view, "Filas", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = view;
            scroll.content = content;
            return content;
        }

        /// <summary>Fila para una ScrollList (los hijos se posicionan con anclas dentro de la fila).</summary>
        public static RectTransform Row(Transform content, float height, Color? color = null)
        {
            var go = new GameObject("Fila", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(content, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            var img = go.AddComponent<Image>();
            img.sprite = UIKit.White;
            img.color = color ?? CardColor;
            img.raycastTarget = true; // permite arrastrar la lista desde la fila
            return rt;
        }

        /// <summary>Botón anclado al borde derecho de una fila (rightOffset = distancia al borde).</summary>
        public static Button RowButton(RectTransform row, string label, float rightOffset, Vector2 size, Color bg, Color fg, Action onClick, int fontSize = 17)
            => UIKit.Button(row, label, new Vector2(1f, 0.5f), new Vector2(-rightOffset, 0), size, bg, fg, onClick, fontSize);

        /// <summary>Texto anclado al borde izquierdo de una fila.</summary>
        public static Text RowText(RectTransform row, string text, float leftOffset, float y, Vector2 box, int size = 17, Color? color = null, FontStyle style = FontStyle.Normal)
            => UIKit.Label(row, text, size, color ?? UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(leftOffset, y), box, style);

        /// <summary>Gráfica de líneas simple (curva de tostión, extracción en el tiempo...).</summary>
        public class LineChart
        {
            public RectTransform area;
            readonly List<Image> pool = new List<Image>();
            int used;

            public LineChart(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
            {
                var bg = UIKit.Image(parent, "Grafica", anchor, pos, size, new Color(0, 0, 0, 0.4f));
                area = bg.rectTransform;
            }

            public void Clear()
            {
                for (int i = 0; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
                used = 0;
            }

            Image Get()
            {
                if (used < pool.Count) { var im = pool[used++]; im.gameObject.SetActive(true); return im; }
                var img = UIKit.Image(area, "Trazo", Vector2.zero, Vector2.zero, Vector2.one, Color.white);
                img.rectTransform.pivot = new Vector2(0f, 0.5f);
                pool.Add(img);
                used++;
                return img;
            }

            /// <summary>Línea entre dos puntos normalizados (0 – 1) del área.</summary>
            public void Segment(Vector2 a, Vector2 b, Color c, float thickness = 3f)
            {
                var size = area.rect.size;
                if (size.x <= 0) size = area.sizeDelta;
                Vector2 pa = new Vector2(a.x * size.x, a.y * size.y), pb = new Vector2(b.x * size.x, b.y * size.y);
                var img = Get();
                img.color = c;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = Vector2.zero;
                rt.anchoredPosition = pa;
                Vector2 d = pb - pa;
                rt.sizeDelta = new Vector2(d.magnitude, thickness);
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            }

            /// <summary>Rectángulo semitransparente (zona ideal) en coordenadas normalizadas.</summary>
            public void Band(Rect r, Color c)
            {
                var size = area.rect.size;
                if (size.x <= 0) size = area.sizeDelta;
                var img = Get();
                img.color = c;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = Vector2.zero;
                rt.localRotation = Quaternion.identity;
                rt.anchoredPosition = new Vector2(r.xMin * size.x, (r.yMin + r.height / 2f) * size.y);
                rt.sizeDelta = new Vector2(r.width * size.x, r.height * size.y);
            }

            public void Polyline(IList<Vector2> pts, Color c, float thickness = 3f)
            {
                for (int i = 1; i < pts.Count; i++) Segment(pts[i - 1], pts[i], c, thickness);
            }
        }
    }
}
