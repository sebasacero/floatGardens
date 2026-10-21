// Panel del mapa de Colombia: regiones por capítulo con altura, horas de bus y pasaje; al elegir una se resalta en el mapa.
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class MapPanel : HPanel
    {
        static readonly CultureInfo CO = CultureInfo.GetCultureInfo("es-CO");
        static string selected = "";

        protected override void Build()
        {
            // Panel lateral izquierdo: no tapa el mapa
            var side = UIKit.Rect(root, "Lateral", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(430, 0));
            side.offsetMin = new Vector2(0, 0);
            side.offsetMax = new Vector2(430, -112);
            var bg = side.gameObject.AddComponent<Image>();
            bg.sprite = UIKit.White;
            bg.color = HUI.PanelBg;
            bg.raycastTarget = true;
            UIKit.Label(side, "Mapa de Colombia", 26, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(16, -10), new Vector2(400, 36));
            UIKit.Label(side, "Día " + S.day + " · " + HConfig.Clock(S.hour) + " · " + HConfig.Money(S.money), 15, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(16, -44), new Vector2(400, 22), FontStyle.Normal);

            var list = HUI.ScrollList(side, Vector2.zero, Vector2.one, new Vector2(8, 220), new Vector2(-8, -72), 6);
            int lastChapter = -1;
            foreach (var r in RegionData.All)
            {
                if (r.chapter != lastChapter)
                {
                    lastChapter = r.chapter;
                    var ch = Catalog.Chapter(r.chapter);
                    var h = HUI.Row(list, 30, new Color(0, 0, 0, 0));
                    HUI.RowText(h, (r.chapter == 0 ? "Prólogo" : "Capítulo " + r.chapter) + (ch != null && r.chapter > 0 ? " — " + ch.title : ""), 4, 0, new Vector2(380, 28), 15, HConfig.Gold, FontStyle.Bold);
                }
                Row(list, r);
            }

            // Detalle de la región elegida
            var detail = HUI.Card(side, new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(414, 144), true);
            var sel = Catalog.Region(selected);
            if (sel != null)
            {
                UIKit.Label(detail.transform, sel.name + " · " + sel.town, 17, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -6), new Vector2(394, 22));
                HUI.Paragraph(detail.transform, sel.summary + "\nTaza: " + sel.cupProfile, new Vector2(0, 1), new Vector2(10, -30), new Vector2(394, 110), 13);
            }
            else HUI.Paragraph(detail.transform, "Elige una región para ver su café. Las regiones se abren a medida que avanza la historia.", new Vector2(0, 1), new Vector2(10, -10), new Vector2(394, 120), 15, HUI.Muted);

            UIKit.Button(side, "Volver a la bodega", new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(414, 52), new Color(1, 1, 1, 0.14f), UIKit.Text, () => G.GoTo(Lugar.Bodega), 19);
            if (G.map != null) G.map.Focus(selected);
        }

        void Row(RectTransform list, Region r)
        {
            bool open = G.RegionUnlocked(r);
            bool stamp = S.stamps.Contains(r.id), visited = S.visited.Contains(r.id);
            var row = HUI.Row(list, 74, r.id == selected ? HUI.CardHi : HUI.CardColor);
            string id = r.id;
            var b = row.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() => { selected = id; Rebuild(); });
            UIKit.Label(row, r.name, 16, open ? UIKit.Text : HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -6), new Vector2(250, 22));
            string alt = Mathf.RoundToInt(r.altitudeMin).ToString("N0", CO) + "–" + Mathf.RoundToInt(r.altitudeMax).ToString("N0", CO) + " m";
            string trip = r.id == "bogota" ? "en la ciudad · gratis" : r.travelHours + " h de bus · " + HConfig.Money(r.travelHours * HConfig.BusFarePerHour);
            UIKit.Label(row, r.department + " · " + alt, 13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -28), new Vector2(260, 18), FontStyle.Normal);
            UIKit.Label(row, open ? trip + (stamp ? " · SELLO" : visited ? " · visitada" : "") : "Se abre en el capítulo " + r.chapter, 13, stamp ? UIKit.Mint : open ? UIKit.Text : HUI.Muted,
                TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -48), new Vector2(270, 18), FontStyle.Normal);
            if (open)
            {
                var go = HUI.RowButton(row, "Viajar", 8, new Vector2(100, 44), UIKit.Yellow, UIKit.Ink, () => { selected = id; G.Travel(id); }, 17);
                go.interactable = S.money >= r.travelHours * HConfig.BusFarePerHour;
            }
        }
    }
}
