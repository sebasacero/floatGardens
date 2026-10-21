// El computador de la bodega: canal acero-press, Wiki del Café, correo (con inscripciones a competencias),
// tienda (semillas, equipos, balsas, café verde) e inventario.
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class PCPanel : HPanel
    {
        public override float HoursCost => 0.25f;
        static readonly CultureInfo CO = CultureInfo.GetCultureInfo("es-CO");
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly string[] TabNames = { HConfig.Channel, "Wiki del Café", "Correo", "Tienda", "Inventario" };
        static readonly string[] Categories = { "Historia", "Bogotá", "Colombia", "Cultivo", "Beneficio", "Tostión", "Catación", "Preparación", "Competencias" };
        static int tab;
        string category = "Historia", entry, mail;

        protected override void Build()
        {
            var c = HUI.Window(root, "Computador", new Vector2(1180, 640), Close, HConfig.Channel + " · Float Gardens · " + HConfig.Money(S.money));
            var setTab = HUI.Tabs(c, TabNames, new Vector2(0.5f, 1), Vector2.zero, 1148, i => { tab = i; entry = null; mail = null; Rebuild(); });
            setTab(tab);
            var area = UIKit.Rect(c, "Area", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            area.offsetMax = new Vector2(0, -54);
            switch (tab)
            {
                case 0: Channel(area); break;
                case 1: Wiki(area); break;
                case 2: Mail(area); break;
                case 3: Shop(area); break;
                default: Inventory(area); break;
            }
        }

        // ============================ acero-press ============================

        static readonly string[] Fans = { "cafeteritx_93", "lamonapaisa", "barista_en_bici", "tintoconpan", "doña_ruana", "aeropressmania", "chapolera2000", "el_molinero", "neblina_cafe", "bogota_brews", "haruka.tokyo", "juan_yipao" };
        static readonly string[] Comments =
        {
            "¡Qué receta tan pila! La pruebo mañana.", "¿Y a cuánto hierve el agua en Bogotá? ¿91 °C? ¡No sabía!", "Esa taza se ve brutal.",
            "Yo la hago invertida y me queda más dulce.", "¿Café cultivado en una bodega? ¡Necesito visitarla!", "Me salió ácida, ¿muelo más fino?",
            "Mi abuelo era recolector en el Quindío. Gracias por contar estas historias.", "¿Qué molino usas?", "Hermoso ese tueste.",
            "¡Vamos por las 50.000 tazas!", "Nunca había pensado en la altura y el agua.", "Explícanos la carta de control, porfa.",
        };

        void Channel(RectTransform a)
        {
            var head = HUI.Card(a, new Vector2(0, 1), Vector2.zero, new Vector2(560, 150));
            UIKit.Label(head.transform, HConfig.Channel, 30, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(16, -10), new Vector2(520, 40));
            UIKit.Label(head.transform, "por " + S.playerName + " · documentando el camino a 50.000 tazas de AeroPress", 15, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(16, -46), new Vector2(530, 22), FontStyle.Normal);
            UIKit.Label(head.transform, S.followers.ToString("N0", CO) + " seguidores · mejor taza " + S.bestBrew.ToString("0.0", Inv), 18, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(16, -76), new Vector2(530, 26));
            var bar = UIKit.Bar(head.transform, "Meta", new Vector2(0, 0), new Vector2(16, 30), new Vector2(528, 10), HConfig.Gold, out _);
            bar.fillAmount = Mathf.Clamp01(S.cups / (float)HConfig.CupGoal);
            UIKit.Label(head.transform, S.cups.ToString("N0", CO) + " / 50.000 tazas", 14, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(16, 8), new Vector2(400, 20), FontStyle.Normal);

            // Escribir artículos sobre lo aprendido
            UIKit.Label(a, "Escribir un artículo (lo que aprendiste en la Wiki)", 16, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, -160), new Vector2(560, 24));
            var arts = HUI.ScrollList(a, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(560, -188));
            int count = 0;
            foreach (var id in S.wiki)
            {
                if (S.Has("art_" + id)) continue;
                var w = Catalog.Wiki(id);
                if (w == null) continue;
                var row = HUI.Row(arts, 54);
                HUI.RowText(row, w.title, 12, 0, new Vector2(380, 48), 15);
                string wid = id;
                HUI.RowButton(row, "Escribir", 8, new Vector2(110, 40), new Color(0.18f, 0.75f, 0.44f, 0.8f), Color.white, () => WriteArticle(wid), 15);
                count++;
            }
            if (count == 0)
            {
                var row = HUI.Row(arts, 70, new Color(1, 1, 1, 0.03f));
                HUI.RowText(row, "Ya escribiste sobre todo lo que sabes. Viaja, conversa con los mentores y desbloquea más entradas de la Wiki.", 12, 0, new Vector2(520, 64), 15, HUI.Muted);
            }

            // Feed de tazas publicadas
            UIKit.Label(a, "Tus últimas tazas", 16, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(1, 1), Vector2.zero, new Vector2(570, 24));
            var feed = HUI.ScrollList(a, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-570, 0), new Vector2(0, -28));
            if (S.brews.Count == 0)
            {
                var row = HUI.Row(feed, 70, new Color(1, 1, 1, 0.03f));
                HUI.RowText(row, "Todavía no hay tazas. Prepara una AeroPress en la barra y publícala.", 12, 0, new Vector2(530, 64), 15, HUI.Muted);
            }
            for (int i = S.brews.Count - 1; i >= 0; i--)
            {
                var b = S.brews[i];
                var lot = S.FindLot(b.lot);
                var row = HUI.Row(feed, 100);
                UIKit.Label(row, b.score.ToString("0.0", Inv), 30, b.score >= 80 ? UIKit.Yellow : UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -8), new Vector2(90, 40));
                UIKit.Label(row, (lot != null ? lot.Label : "Café") + " · día " + b.day, 14, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(104, -8), new Vector2(430, 22), FontStyle.Bold);
                UIKit.Label(row, b.dose.ToString("0.0", Inv) + " g · " + b.water.ToString("0", Inv) + " g · " + b.temp.ToString("0", Inv) + " °C · " + b.grind.ToString("0", Inv) + " µm · " + b.steep.ToString("0", Inv) + " s" + (b.inverted ? " · invertida" : ""),
                    13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(104, -32), new Vector2(430, 20), FontStyle.Normal);
                int likes = Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0f, b.score - 65f) * (1f + Mathf.Sqrt(S.followers) * 0.08f)));
                UIKit.Label(row, likes + " me gusta · " + Fans[(i * 7 + 3) % Fans.Length] + ": " + Comments[(i * 5 + 1) % Comments.Length], 13, UIKit.Mint, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(104, -54), new Vector2(430, 40), FontStyle.Italic);
            }
        }

        void WriteArticle(string id)
        {
            if (S.Has("art_" + id)) return;
            int gained = CoffeeModel.FollowersFor(80f, S.followers, G.rng);
            if (S.gear.Contains("camara")) gained = Mathf.RoundToInt(gained * 1.3f);
            S.Flag("art_" + id);
            G.AddFollowers(gained);
            G.Toast("Publicaste un artículo: +" + gained + " seguidores");
            G.Emit(HEvents.Publish, "", gained);
            G.AdvanceHours(1f);
            G.Save();
            Rebuild();
        }

        // ============================ WIKI ============================

        void Wiki(RectTransform a)
        {
            var cats = HUI.ScrollList(a, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(230, 0), 6);
            foreach (var cat in Categories)
            {
                int total = 0, open = 0;
                foreach (var w in LoreData.All) if (w.category == cat) { total++; if (S.wiki.Contains(w.id)) open++; }
                if (total == 0) continue;
                var row = HUI.Row(cats, 48, cat == category ? HUI.CardHi : HUI.CardColor);
                string cc = cat;
                var b = row.gameObject.AddComponent<Button>();
                b.onClick.AddListener(() => { category = cc; entry = null; Rebuild(); });
                HUI.RowText(row, cat + "  " + open + "/" + total, 10, 0, new Vector2(200, 44), 16, cat == category ? UIKit.Yellow : UIKit.Text, FontStyle.Bold);
            }

            var list = HUI.ScrollList(a, new Vector2(0, 0), new Vector2(0, 1), new Vector2(240, 0), new Vector2(560, 0), 6);
            foreach (var w in LoreData.All)
            {
                if (w.category != category) continue;
                bool open = S.wiki.Contains(w.id);
                bool unread = open && !S.wikiRead.Contains(w.id);
                var row = HUI.Row(list, 54, w.id == entry ? HUI.CardHi : HUI.CardColor);
                HUI.RowText(row, open ? w.title : "???  (sigue explorando)", 10, 0, new Vector2(290, 50), 15, open ? (unread ? UIKit.Yellow : UIKit.Text) : HUI.Muted, unread ? FontStyle.Bold : FontStyle.Normal);
                if (open)
                {
                    string id = w.id;
                    var b = row.gameObject.AddComponent<Button>();
                    b.onClick.AddListener(() => { entry = id; Rebuild(); });
                }
            }

            var view = HUI.Card(a, new Vector2(1, 1), Vector2.zero, new Vector2(578, 490));
            var e = Catalog.Wiki(entry);
            if (e == null || !S.wiki.Contains(e.id))
            {
                HUI.Paragraph(view.transform, "Elige una entrada. Las entradas se desbloquean al viajar, hablar con los mentores, superar retos y cumplir misiones.\n\nLlevas " + S.wiki.Count + " de " + LoreData.All.Count + ".",
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 200), 17, HUI.Muted, TextAnchor.MiddleCenter);
                return;
            }
            if (!S.wikiRead.Contains(e.id)) S.wikiRead.Add(e.id);
            UIKit.Label(view.transform, e.title, 22, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(16, -12), new Vector2(546, 60));
            string meta = (string.IsNullOrEmpty(e.year) ? "" : e.year) + (string.IsNullOrEmpty(e.place) ? "" : (string.IsNullOrEmpty(e.year) ? "" : " · ") + e.place);
            UIKit.Label(view.transform, meta, 14, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(16, -70), new Vector2(546, 40), FontStyle.Italic);
            var bodyList = HUI.ScrollList(view.transform, Vector2.zero, Vector2.one, new Vector2(8, 60), new Vector2(-8, -112), 4);
            var bodyRow = HUI.Row(bodyList, Mathf.Max(200, e.body.Length * 0.42f), new Color(0, 0, 0, 0));
            HUI.Paragraph(bodyRow, e.body, new Vector2(0, 1), new Vector2(8, -4), new Vector2(530, Mathf.Max(200, e.body.Length * 0.42f)), 16);
            float rx = 12;
            foreach (var rid in e.related)
            {
                var r = Catalog.Wiki(rid);
                if (r == null || !S.wiki.Contains(rid)) continue;
                string id = rid;
                string t = r.title.Length > 26 ? r.title.Substring(0, 24) + "…" : r.title;
                UIKit.Button(view.transform, t, new Vector2(0, 0), new Vector2(rx, 10), new Vector2(178, 40), new Color(1, 1, 1, 0.1f), UIKit.Text, () => { entry = id; Rebuild(); }, 13);
                rx += 186;
                if (rx > 400) break;
            }
        }

        // ============================ CORREO ============================

        void Mail(RectTransform a)
        {
            var list = HUI.ScrollList(a, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(470, 0), 6);
            // Competencias abiertas, fijadas arriba
            foreach (var comp in StoryData.Competitions)
            {
                if (comp.chapter > S.chapter || S.trophies.Contains(comp.id)) continue;
                var row = HUI.Row(list, 112, HUI.CardHi);
                UIKit.Label(row, comp.name, 17, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -8), new Vector2(430, 24));
                UIKit.Label(row, comp.city + " · " + Mathf.RoundToInt(comp.altitude).ToString("N0", CO) + " m (el agua hierve a " + HConfig.BoilingPointC(comp.altitude).ToString("0", Inv) + " °C) · " + comp.rounds + " rondas · premio " + HConfig.Money(comp.prize),
                    13, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -34), new Vector2(440, 40), FontStyle.Normal);
                string id = comp.id;
                UIKit.Button(row, "Inscribirme", new Vector2(1, 0), new Vector2(-10, 8), new Vector2(150, 38), UIKit.Yellow, UIKit.Ink, () => { Close(); G.StartCompetition(id); }, 16);
            }
            var items = new List<MailItem>();
            foreach (var m in StoryData.Mail) if (string.IsNullOrEmpty(m.requiresFlag) || S.Has(m.requiresFlag)) items.Add(m);
            items.Sort((x, y) => S.mailRead.Contains(x.id).CompareTo(S.mailRead.Contains(y.id)));
            foreach (var m in items)
            {
                bool unread = !S.mailRead.Contains(m.id);
                var row = HUI.Row(list, 62, m.id == mail ? HUI.CardHi : HUI.CardColor);
                UIKit.Label(row, m.from, 14, unread ? UIKit.Yellow : HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -6), new Vector2(440, 20), unread ? FontStyle.Bold : FontStyle.Normal);
                UIKit.Label(row, m.subject, 16, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -28), new Vector2(440, 26), unread ? FontStyle.Bold : FontStyle.Normal);
                string id = m.id;
                var b = row.gameObject.AddComponent<Button>();
                b.onClick.AddListener(() => { mail = id; if (!S.mailRead.Contains(id)) S.mailRead.Add(id); Rebuild(); });
            }

            var view = HUI.Card(a, new Vector2(1, 1), Vector2.zero, new Vector2(668, 490));
            MailItem open = null;
            foreach (var m in items) if (m.id == mail) open = m;
            if (open == null)
            {
                int unread = 0;
                foreach (var m in items) if (!S.mailRead.Contains(m.id)) unread++;
                HUI.Paragraph(view.transform, unread > 0 ? "Tienes " + unread + " correo" + (unread == 1 ? "" : "s") + " sin leer." : "No hay correos nuevos.",
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 100), 18, HUI.Muted, TextAnchor.MiddleCenter);
                return;
            }
            UIKit.Label(view.transform, open.subject, 22, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(18, -14), new Vector2(630, 34));
            UIKit.Label(view.transform, "De: " + open.from, 15, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(18, -52), new Vector2(630, 22), FontStyle.Normal);
            HUI.Paragraph(view.transform, G.Fill(open.body), new Vector2(0, 1), new Vector2(18, -86), new Vector2(630, 380), 17);
        }

        // ============================ TIENDA ============================

        void Shop(RectTransform a)
        {
            var list = HUI.ScrollList(a, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 6);
            Header(list, "Semillas");
            foreach (var v in VarietyData.All)
            {
                if (!string.IsNullOrEmpty(v.unlockRegion) && !S.stamps.Contains(v.unlockRegion)) continue;
                var vv = v;
                Item(list, v.name + " · " + v.origin, v.description + " Resistencia a la roya: " + Mathf.RoundToInt(v.rustResistance * 100) + " %. Potencial en taza: " + v.cupPotential.ToString("0", Inv) + " puntos. Tienes " + S.Seeds(v.id) + ".",
                    v.price, "Comprar", () =>
                    {
                        if (!G.Spend(vv.price, "semillas de " + vv.name)) return;
                        S.AddSeeds(vv.id, 1);
                        G.Emit(HEvents.Buy, vv.id);
                        G.Save();
                        Rebuild();
                    });
            }
            int locked = 0;
            foreach (var v in VarietyData.All) if (!string.IsNullOrEmpty(v.unlockRegion) && !S.stamps.Contains(v.unlockRegion)) locked++;
            if (locked > 0) Note(list, locked + " variedades más se desbloquean al superar los retos de los mentores en las regiones.");

            Header(list, "Equipos");
            bool any = false;
            foreach (var gear in GearData.All)
            {
                if (gear.chapter > S.chapter || S.gear.Contains(gear.id)) continue;
                any = true;
                var gg = gear;
                Item(list, gear.name, gear.desc, gear.price, "Comprar", () =>
                {
                    if (!G.Spend(gg.price, gg.name)) return;
                    S.gear.Add(gg.id);
                    G.Emit(HEvents.Buy, gg.id);
                    G.Save();
                    Rebuild();
                });
            }
            if (!any) Note(list, "Ya tienes todos los equipos disponibles. Llegan más en los próximos capítulos.");

            Header(list, "Huerta");
            if (S.garden.rafts.Count < HConfig.MaxRafts)
                Item(list, "Balsa flotante extra", "Una lámina de icopor más sobre la solución nutritiva, con su canastilla y aireación. Más balsas, más cosecha (y más energía). Tienes " + S.garden.rafts.Count + " de " + HConfig.MaxRafts + ".",
                    HConfig.RaftPrice, "Comprar", () =>
                    {
                        if (S.garden.rafts.Count >= HConfig.MaxRafts || !G.Spend(HConfig.RaftPrice, "balsa flotante")) return;
                        S.garden.rafts.Add(new Plant());
                        BodegaBuilder.RefreshPlants(S);
                        G.Emit(HEvents.Buy, "balsa");
                        G.Save();
                        Rebuild();
                    });
            else Note(list, "La huerta ya tiene todas las balsas que caben en la bodega.");

            Header(list, "Café verde de las regiones");
            bool anyRegion = false;
            foreach (var r in RegionData.All)
            {
                if (r.id == "bogota" || !S.visited.Contains(r.id)) continue;
                anyRegion = true;
                var rr = r;
                var v = Catalog.Variety(r.giftVariety);
                float pot = (v != null ? v.cupPotential : 84f) - 2f;
                Item(list, "1 kg de café verde de " + r.name, r.cupProfile + " Potencial: " + pot.ToString("0", Inv) + " puntos.", 60000, "Comprar", () =>
                {
                    if (!G.Spend(60000, "café verde de " + rr.name)) return;
                    G.GiveLot(v != null ? v.id : "caturra", rr.id, "verde", 1000f, pot, rr.cupProfile, "lavado");
                    G.Emit(HEvents.Buy, "verde_" + rr.id);
                    G.Save();
                    Rebuild();
                });
            }
            if (!anyRegion) Note(list, "Cuando visites una región podrás comprarle café verde a sus productores.");
        }

        static void Header(RectTransform list, string text)
        {
            var row = HUI.Row(list, 34, new Color(0, 0, 0, 0));
            HUI.RowText(row, text, 4, 0, new Vector2(600, 30), 18, UIKit.Yellow, FontStyle.Bold);
        }

        static void Note(RectTransform list, string text)
        {
            var row = HUI.Row(list, 44, new Color(1, 1, 1, 0.03f));
            HUI.RowText(row, text, 12, 0, new Vector2(1000, 40), 14, HUI.Muted);
        }

        void Item(RectTransform list, string title, string desc, int price, string action, System.Action buy)
        {
            var row = HUI.Row(list, 84);
            UIKit.Label(row, title, 16, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -8), new Vector2(820, 22), FontStyle.Bold);
            UIKit.Label(row, desc, 13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -32), new Vector2(860, 48), FontStyle.Normal);
            bool can = S.money >= price;
            var b = HUI.RowButton(row, action + " · " + HConfig.Money(price), 10, new Vector2(210, 44), can ? UIKit.Yellow : new Color(1, 1, 1, 0.08f), can ? UIKit.Ink : HUI.Muted, buy, 15);
            b.interactable = can;
        }

        // ============================ INVENTARIO ============================

        void Inventory(RectTransform a)
        {
            var list = HUI.ScrollList(a, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(740, 0), 6);
            string[] stages = { "cereza", "secando", "verde", "tostado" };
            string[] names = { "Cereza (para el beneficio)", "Secando en las camas", "Café verde (para tostar)", "Café tostado (para preparar)" };
            for (int k = 0; k < stages.Length; k++)
            {
                Header(list, names[k]);
                int n = 0;
                foreach (var l in S.lots)
                {
                    if (l.stage != stages[k] || l.grams < 0.5f) continue;
                    n++;
                    var row = HUI.Row(list, 58);
                    UIKit.Label(row, l.Label, 15, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -6), new Vector2(700, 22), FontStyle.Bold);
                    string detail = HConfig.Grams(l.grams) + " · " + (l.stage == "tostado"
                        ? "tostión " + l.roastScore.ToString("0", Inv) + " pts · tostado el día " + l.day
                        : l.stage == "secando" ? "listo el día " + l.readyDay : "potencial " + l.potential.ToString("0.0", Inv) + " pts") +
                        (string.IsNullOrEmpty(l.notes) ? "" : " · " + l.notes);
                    UIKit.Label(row, detail, 13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(12, -30), new Vector2(700, 22), FontStyle.Normal);
                }
                if (n == 0) Note(list, "Nada por ahora.");
            }

            var side = HUI.Card(a, new Vector2(1, 1), Vector2.zero, new Vector2(398, 490));
            string seeds = "Semillas\n";
            bool anySeed = false;
            foreach (var st in S.seeds)
            {
                if (st.count <= 0) continue;
                var v = Catalog.Variety(st.id);
                seeds += "• " + (v != null ? v.name : st.id) + ": " + st.count + "\n";
                anySeed = true;
            }
            if (!anySeed) seeds += "Ninguna\n";
            string gear = "\nEquipos\n";
            if (S.gear.Count == 0) gear += "Solo tu AeroPress (¡y es suficiente para empezar!)\n";
            foreach (var id in S.gear)
            {
                var gg = Catalog.Gear(id);
                gear += "• " + (gg != null ? gg.name : id) + "\n";
            }
            HUI.Paragraph(side.transform, seeds + gear + "\nBalsas: " + S.garden.rafts.Count + " de " + HConfig.MaxRafts + "\nPuntos Float ganados en la historia: " + S.puntosAwarded,
                new Vector2(0, 1), new Vector2(16, -14), new Vector2(366, 460), 16);
        }
    }
}
