// Menús del modo historia: pantalla de título (continuar, nueva historia con nombre, acerca de), pausa con el
// Pasaporte Cafetero y la pantalla final "De Bogotá al mundo".
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class TitleMenu : MonoBehaviour
    {
        public bool Visible { get; private set; }
        StoryGame g;
        Canvas canvas;
        RectTransform root;
        static readonly CultureInfo CO = CultureInfo.GetCultureInfo("es-CO");

        public void Build(StoryGame game)
        {
            g = game;
            canvas = UIKit.Canvas("Menus Historia", 50);
            canvas.transform.SetParent(transform, false);
            root = (RectTransform)canvas.transform;
            canvas.gameObject.SetActive(false);
        }

        void Open()
        {
            canvas.gameObject.SetActive(true);
            UIKit.Clear(root);
            Visible = true;
            FGInput.CursorLocked = false;
        }

        public void Hide()
        {
            UIKit.Clear(root);
            canvas.gameObject.SetActive(false);
            Visible = false;
        }

        Text Title(Transform parent, float y)
        {
            UIKit.Label(parent, "Float Gardens", 30, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(900, 40));
            var t = UIKit.Label(parent, "Tierra de Café", 72, HConfig.Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, y - 64), new Vector2(1000, 90));
            UIKit.Label(parent, HConfig.Subtitle, 24, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, y - 124), new Vector2(900, 34), FontStyle.Italic);
            return t;
        }

        static Button Big(Transform parent, string label, float y, Color bg, Color fg, System.Action a)
            => UIKit.Button(parent, label, new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(380, 58), bg, fg, a, 24);

        // ============================ TÍTULO ============================

        public void ShowTitle()
        {
            Open();
            var bg = UIKit.Stretch(root, "Velo").gameObject.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.02f, 0.01f, 0.45f);
            Title(root, -70);
            HUI.Paragraph(root, "Un barista de Bogotá, una AeroPress, un computador y un sueño: llegar a las 50.000 tazas y ser un referente del café en el mundo. A 2.600 metros el café no crece al aire libre... así que lo cultivamos en balsas flotantes.",
                new Vector2(0.5f, 1), new Vector2(0, -262), new Vector2(820, 70), 17, HUI.Muted, TextAnchor.UpperCenter);

            float y = -20;
            if (HState.Exists)
            {
                Big(root, "Continuar", y, UIKit.Yellow, UIKit.Ink, () => { Hide(); g.Continue(); });
                y -= 70;
            }
            Big(root, "Nueva historia", y, HState.Exists ? new Color(1, 1, 1, 0.14f) : UIKit.Yellow, HState.Exists ? UIKit.Text : UIKit.Ink, () =>
            {
                if (HState.Exists) ConfirmNew(); else NameEntry();
            });
            y -= 70;
            Big(root, "Acerca de", y, new Color(1, 1, 1, 0.1f), UIKit.Text, About);
            y -= 70;
            SoundButton(root, new Vector2(0, y));
            UIKit.Label(root, "Juego de ficción · No está afiliado ni respaldado por ninguna organización cafetera real.", 13, HUI.Muted,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1000, 22), FontStyle.Normal);
        }

        void SoundButton(Transform parent, Vector2 pos)
        {
            bool muted = SFX.I != null && SFX.I.muted;
            UIKit.Button(parent, muted ? "Sonido: No" : "Sonido: Sí", new Vector2(0.5f, 0.5f), pos, new Vector2(380, 50), new Color(1, 1, 1, 0.08f), UIKit.Text, () =>
            {
                if (SFX.I != null) SFX.I.muted = !SFX.I.muted;
                if (g.S != null) g.S.muted = SFX.I != null && SFX.I.muted;
                if (g.lugar == Lugar.Titulo) ShowTitle(); else ShowPause();
            }, 20);
        }

        void ConfirmNew()
        {
            Open();
            var c = HUI.Window(root, "¿Empezar de cero?", new Vector2(640, 300), ShowTitle);
            HUI.Paragraph(c, "Ya tienes una historia guardada. Si empiezas una nueva se borra ese progreso (los Puntos Float que ya ganaste se conservan).",
                new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(560, 100), 18, null, TextAnchor.UpperCenter);
            UIKit.Button(c, "Sí, nueva historia", new Vector2(0.5f, 0), new Vector2(-140, 20), new Vector2(250, 54), UIKit.Red, Color.white, NameEntry, 19);
            UIKit.Button(c, "Volver", new Vector2(0.5f, 0), new Vector2(140, 20), new Vector2(250, 54), new Color(1, 1, 1, 0.12f), UIKit.Text, ShowTitle, 19);
        }

        void NameEntry()
        {
            Open();
            var c = HUI.Window(root, "Nueva historia", new Vector2(700, 380), ShowTitle, "¿Cómo se llama tu barista?");
            HUI.Paragraph(c, "Así te van a llamar tus mentores y así se va a firmar tu canal, " + HConfig.Channel + ".",
                new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(620, 50), 17, HUI.Muted, TextAnchor.UpperCenter);
            var field = NameField(c, new Vector2(0, 20));
            UIKit.Button(c, "Comenzar en Bogotá", new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(360, 58), UIKit.Yellow, UIKit.Ink, () =>
            {
                string name = field.text;
                Hide();
                g.NewGame(name);
            }, 22);
        }

        static InputField NameField(Transform parent, Vector2 pos)
        {
            var bg = UIKit.Image(parent, "Nombre", new Vector2(0.5f, 0.5f), pos, new Vector2(420, 60), new Color(1, 1, 1, 0.92f));
            bg.raycastTarget = true;
            var text = UIKit.Label(bg.transform, "", 26, UIKit.Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 52), FontStyle.Bold);
            text.GetComponent<Shadow>().enabled = false;
            text.supportRichText = false;
            var ph = UIKit.Label(bg.transform, HConfig.DefaultName, 26, new Color(0, 0, 0, 0.35f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 52), FontStyle.Italic);
            ph.GetComponent<Shadow>().enabled = false;
            var f = bg.gameObject.AddComponent<InputField>();
            f.textComponent = text;
            f.placeholder = ph;
            f.characterLimit = 18;
            f.targetGraphic = bg;
            f.text = HConfig.DefaultName;
            return f;
        }

        void About()
        {
            Open();
            var c = HUI.Window(root, "Acerca de", new Vector2(980, 600), ShowTitle, HConfig.Title);
            HUI.Paragraph(c,
                "Float Gardens es un proyecto para cultivar café en las casas y bodegas de Bogotá, donde a 2.600 metros y unos 14 °C el cafeto no se da al aire libre: " +
                "las matas crecen en balsas hidropónicas flotantes, bajo luces LED y con el clima controlado.\n\n" +
                "En esta historia juegas a ser un barista bogotano con una AeroPress y un computador que quiere llegar a ser un referente mundial. " +
                "Recorres las regiones cafeteras de Colombia, aprendes de mentores (personajes ficticios) la historia, el cultivo, el beneficio, la tostión, la catación y la preparación, " +
                "y compites hasta el Mundial.\n\n" +
                "Lo que se enseña se basa en fuentes reales sobre la historia del café, la investigación cafetera colombiana (Cenicafé) y los protocolos de la Specialty Coffee Association (SCA). " +
                "Las simulaciones están simplificadas y comprimidas en el tiempo para que quepan en una partida.\n\n" +
                "Los Puntos Float que ganas aquí se suman al mismo saldo del juego de mundo abierto y se canjean por café gratis en los puntos de Float Gardens.\n\n" +
                "Juego de ficción. No está afiliado ni respaldado por ninguna organización cafetera real.",
                new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(900, 480), 17);
        }

        // ============================ PAUSA ============================

        public void ShowPause()
        {
            Open();
            var c = HUI.Window(root, "Pausa", new Vector2(520, 520), Hide, "Día " + g.S.day + " · " + HConfig.Clock(g.S.hour));
            float y = 150;
            UIKit.Button(c, "Continuar", new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(360, 54), UIKit.Yellow, UIKit.Ink, Hide, 22);
            y -= 66;
            UIKit.Button(c, "Guardar", new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(360, 54), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { g.Save(); g.Toast("Partida guardada"); }, 22);
            y -= 66;
            UIKit.Button(c, "Pasaporte Cafetero", new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(360, 54), new Color(1, 1, 1, 0.12f), UIKit.Text, Passport, 22);
            y -= 66;
            SoundButton(c, new Vector2(0, y));
            y -= 66;
            UIKit.Button(c, "Salir al título", new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(360, 54), new Color(1, 0.3f, 0.23f, 0.35f), UIKit.Text, () =>
            {
                g.Save();
                Hide();
                g.GoTo(Lugar.Titulo);
            }, 22);
        }

        void Passport()
        {
            Open();
            var s = g.S;
            var c = HUI.Window(root, "Pasaporte Cafetero", new Vector2(1120, 620), ShowPause,
                s.playerName + " · " + s.stamps.Count + " sellos · " + s.trophies.Count + " trofeos · " + s.puntosAwarded + " Puntos Float ganados");
            int i = 0;
            foreach (var r in RegionData.All)
            {
                int col = i % 5, row = i / 5;
                bool stamp = s.stamps.Contains(r.id), visited = s.visited.Contains(r.id), open = g.RegionUnlocked(r);
                var card = HUI.Card(c, new Vector2(0, 1), new Vector2(8 + col * 214, -8 - row * 128), new Vector2(206, 120), stamp);
                UIKit.Label(card.transform, r.name, 17, stamp ? UIKit.Yellow : UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -8), new Vector2(190, 44));
                string state = stamp ? "SELLO ✓" : visited ? "Visitada" : open ? "Por visitar" : "Capítulo " + r.chapter;
                UIKit.Label(card.transform, state, 15, stamp ? UIKit.Mint : HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(10, 8), new Vector2(190, 22), FontStyle.Normal);
                var v = Catalog.Variety(r.giftVariety);
                if (stamp && v != null)
                    UIKit.Label(card.transform, v.name, 14, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(10, -6), new Vector2(190, 20), FontStyle.Italic);
                if (stamp) UIKit.Image(card.transform, "Sello", new Vector2(1, 1), new Vector2(-10, -10), new Vector2(34, 34), HConfig.Red, UIKit.Circle);
                i++;
            }
            string trophies = "Trofeos: ";
            if (s.trophies.Count == 0) trophies += "todavía ninguno.";
            else
            {
                for (int k = 0; k < s.trophies.Count; k++)
                {
                    var comp = Catalog.Competition(s.trophies[k]);
                    trophies += (k > 0 ? " · " : "") + (comp != null ? (string.IsNullOrEmpty(comp.trophy) ? comp.name : comp.trophy) : s.trophies[k]);
                }
            }
            trophies += "\nVariedades sembradas o guardadas: " + CountVarieties() + " de " + VarietyData.All.Count + " · Mejor taza: " + s.bestBrew.ToString("0.0", CultureInfo.InvariantCulture) + " puntos";
            HUI.Paragraph(c, trophies, new Vector2(0, 0), new Vector2(10, 10), new Vector2(1060, 56), 16, HUI.Muted, TextAnchor.LowerLeft);
        }

        int CountVarieties()
        {
            var s = g.S;
            int n = 0;
            foreach (var v in VarietyData.All)
            {
                bool has = s.Seeds(v.id) > 0;
                if (!has) foreach (var p in s.garden.rafts) if (p.variety == v.id && !p.Empty) { has = true; break; }
                if (!has) foreach (var l in s.lots) if (l.variety == v.id) { has = true; break; }
                if (has) n++;
            }
            return n;
        }

        // ============================ FINAL ============================

        public void ShowEnding()
        {
            Open();
            var s = g.S;
            var bg = UIKit.Stretch(root, "Velo").gameObject.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.02f, 0.01f, 0.88f);
            bg.raycastTarget = true;
            UIKit.Label(root, "De Bogotá al mundo", 56, HConfig.Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(1100, 80));
            UIKit.Label(root, "Gracias por jugar, " + s.playerName + ".", 26, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1000, 40), FontStyle.Italic);
            string stats =
                "Días: " + s.day + "\n" +
                "Tazas preparadas: " + s.cups.ToString("N0", CO) + " de 50.000 (el sueño sigue)\n" +
                "Seguidores de " + HConfig.Channel + ": " + s.followers.ToString("N0", CO) + "\n" +
                "Sellos del Pasaporte Cafetero: " + s.stamps.Count + " de " + RegionData.All.Count + "\n" +
                "Trofeos: " + s.trophies.Count + " · Mejor taza: " + s.bestBrew.ToString("0.0", CultureInfo.InvariantCulture) + " puntos\n" +
                "Entradas de la Wiki del Café: " + s.wiki.Count + " de " + LoreData.All.Count;
            HUI.Paragraph(root, stats, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(760, 220), 22, null, TextAnchor.MiddleCenter);
            HUI.Paragraph(root, "Desde una bodega de Puente Aranda, con una AeroPress y un computador, Float Gardens llegó al mundo. Ahora la meta es la misma de siempre: la próxima taza.",
                new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(900, 60), 18, HUI.Muted, TextAnchor.MiddleCenter);
            UIKit.Button(root, "Seguir jugando", new Vector2(0.5f, 0), new Vector2(-200, 60), new Vector2(340, 58), UIKit.Yellow, UIKit.Ink, Hide, 22);
            UIKit.Button(root, "Volver al título", new Vector2(0.5f, 0), new Vector2(200, 60), new Vector2(340, 58), new Color(1, 1, 1, 0.14f), UIKit.Text, () =>
            {
                g.Save();
                Hide();
                g.GoTo(Lugar.Titulo);
            }, 22);
            SFX.Play("capture");
        }
    }
}
