// Menús estilo lobby de shooter móvil: lobby, tienda, Puntos Float, cómo jugar, pausa, derrota y victoria.
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens
{
    public partial class Menus : MonoBehaviour
    {
        Canvas canvas;
        RectTransform root;
        GameManager g;
        public string Current { get; private set; }
        string returnTo = "lobby";
        string lastCode;

        public void Build(GameManager gm)
        {
            g = gm;
            canvas = UIKit.Canvas("Menus", 30);
            canvas.transform.SetParent(transform, false);
            root = (RectTransform)canvas.transform;
        }

        public void Hide()
        {
            UIKit.Clear(root);
            canvas.gameObject.SetActive(false);
            Current = null;
        }

        public void Show(string name)
        {
            canvas.gameObject.SetActive(true);
            UIKit.Clear(root);
            Current = name;
            switch (name)
            {
                case "lobby": Lobby(); break;
                case "pause": Pause(); break;
                case "shop": Shop(); break;
                case "rewards": Rewards(); break;
                case "howto": HowTo(); break;
                case "dead": Dead(); break;
                case "victory": Victory(); break;
                case "reset": ResetConfirm(); break;
                case "codex": CodexScreen(); break;
                case "mapa": MapScreen(); break;
                case "lab": LabScreen(); break;
            }
        }

        void Go(string screen)
        {
            if (Current == "lobby" || Current == "pause") returnTo = Current;
            Show(screen);
        }

        void Back() { Show(returnTo); }

        RectTransform Panel(float w, float h, string title)
        {
            var dim = UIKit.Stretch(root, "Fondo").gameObject.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.05f, 0.04f, 0.72f);
            var p = UIKit.Image(root, "Panel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h), UIKit.Panel);
            UIKit.Image(p.transform, "Linea", new Vector2(0.5f, 1), Vector2.zero, new Vector2(w, 4), UIKit.Yellow);
            UIKit.Label(p.transform, title, 32, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(w - 40, 44));
            return p.rectTransform;
        }

        Button Btn(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, bool primary, System.Action onClick, int font = 22)
        {
            return UIKit.Button(parent, label, anchor, pos, size, primary ? UIKit.Yellow : new Color(1, 1, 1, 0.12f), primary ? UIKit.Ink : Color.white, onClick, font);
        }

        void PointsMeter(Transform parent, Vector2 pos, float width)
        {
            var s = g.save;
            UIKit.Label(parent, "★ " + s.puntos + " / " + FGConfig.RedeemCost + " Puntos Float", 20, UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), pos, new Vector2(width, 26));
            var fill = UIKit.Bar(parent, "Puntos", new Vector2(0.5f, 1), pos + new Vector2(0, -28), new Vector2(width, 10), UIKit.Yellow, out _);
            fill.fillAmount = Mathf.Clamp01(s.puntos / (float)FGConfig.RedeemCost);
            string hint = s.puntos >= FGConfig.RedeemCost ? "¡Tienes un café gratis para canjear!" : "Te faltan " + (FGConfig.RedeemCost - s.puntos) + " para un café gratis";
            UIKit.Label(parent, hint, 15, Color.white, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), pos + new Vector2(0, -48), new Vector2(width, 22), FontStyle.Normal);
        }

        // ---------- Pantallas ----------
        void Lobby()
        {
            var s = g.save;
            var shade = UIKit.Stretch(root, "Degradado").gameObject.AddComponent<Image>();
            shade.color = new Color(0.02f, 0.05f, 0.04f, 0.45f);
            UIKit.Label(root, "FLOAT GARDENS", 22, UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(24, -18), new Vector2(400, 30));
            UIKit.Label(root, FGConfig.Money(s.money) + "    ★ " + s.puntos, 22, UIKit.Yellow, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-24, -18), new Vector2(400, 30));

            var left = UIKit.At(root, "Izquierda", new Vector2(0, 0.5f), new Vector2(60, 0), new Vector2(520, 560));
            UIKit.Image(left, "Etiqueta", new Vector2(0.5f, 1), new Vector2(-150, -10), new Vector2(220, 30), UIKit.Green);
            UIKit.Label(left, "MUNDO ABIERTO · BOGOTÁ", 15, UIKit.Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(-150, -10), new Vector2(220, 30)).GetComponent<Shadow>().enabled = false;
            UIKit.Label(left, "LA CONQUISTA", 60, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(520, 70));
            UIKit.Label(left, "DEL CAFÉ", 60, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(0, -118), new Vector2(520, 70));
            UIKit.Label(left, "Un barista, una AeroPress y un PC en una ciudad donde el café solo crece adentro. Cultiva, vende, aprende de los que saben y arrebátale Bogotá a Coffee Land.",
                18, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(520, 80), FontStyle.Normal);
            int n = g.CapturedCount;
            UIKit.Label(left, "Zonas capturadas: " + n + "/" + g.zones.Count + "     Derrotados: " + s.kos + "     Ventas: " + s.sales + "     Cerezas: " + g.cherries,
                16, Color.white, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), new Vector2(0, -290), new Vector2(520, 24));
            var zf = UIKit.Bar(left, "Zonas", new Vector2(0.5f, 1), new Vector2(0, -318), new Vector2(520, 8), UIKit.Green, out _);
            zf.fillAmount = n / (float)g.zones.Count;
            PointsMeter(left, new Vector2(0, -346), 520);

            var right = UIKit.At(root, "Derecha", new Vector2(1, 0.5f), new Vector2(-60, -40), new Vector2(380, 360));
            bool started = n > 0 || s.kos > 0;
            Btn(right, started ? "CONTINUAR" : "JUGAR", new Vector2(0.5f, 1), Vector2.zero, new Vector2(380, 96), true, () => g.Play(), 44);
            Btn(right, "Mejoras", new Vector2(0, 1), new Vector2(0, -116), new Vector2(184, 48), false, () => Go("shop"));
            Btn(right, "Puntos Float", new Vector2(1, 1), new Vector2(0, -116), new Vector2(184, 48), false, () => Go("rewards"));
            Btn(right, "Cómo jugar", new Vector2(0, 1), new Vector2(0, -172), new Vector2(184, 48), false, () => Go("howto"));
            Btn(right, s.muted ? "Sonido: no" : "Sonido: sí", new Vector2(1, 1), new Vector2(0, -172), new Vector2(184, 48), false, () =>
            {
                s.muted = !s.muted;
                SFX.I.muted = s.muted;
                g.Persist();
                Show("lobby");
            });
            Btn(right, "Códice del Café", new Vector2(0, 1), new Vector2(0, -228), new Vector2(184, 48), false, () => Go("codex"));
            Btn(right, "Mapa de Colombia", new Vector2(1, 1), new Vector2(0, -228), new Vector2(184, 48), false, () => Go("mapa"));
            Btn(right, "Reiniciar mundo", new Vector2(0.5f, 1), new Vector2(0, -284), new Vector2(380, 40), false, () => Go("reset"), 16);
            if (g.story != null && !g.story.Finished)
                UIKit.Label(left, g.story.CurrentChapter.title + "   →   " + g.story.Current.title, 15, UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), new Vector2(0, -420), new Vector2(520, 40), FontStyle.Normal);
            UIKit.Label(root, "Juego de ficción y parodia · no afiliado a ninguna organización real", 14, new Color(1, 1, 1, 0.65f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(900, 22), FontStyle.Normal);
        }

        void Pause()
        {
            var p = Panel(420, 500, "PAUSA");
            Btn(p, "Seguir jugando", new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(340, 56), true, () => g.Play());
            Btn(p, "Mejoras", new Vector2(0, 1), new Vector2(40, -148), new Vector2(165, 50), false, () => Go("shop"), 18);
            Btn(p, "Puntos Float", new Vector2(1, 1), new Vector2(-40, -148), new Vector2(165, 50), false, () => Go("rewards"), 18);
            Btn(p, "Códice del Café", new Vector2(0, 1), new Vector2(40, -208), new Vector2(165, 50), false, () => Go("codex"), 18);
            Btn(p, "Mapa de Colombia", new Vector2(1, 1), new Vector2(-40, -208), new Vector2(165, 50), false, () => Go("mapa"), 18);
            Btn(p, "Cómo jugar", new Vector2(0.5f, 1), new Vector2(0, -268), new Vector2(340, 50), false, () => Go("howto"));
            Btn(p, "Volver al lobby", new Vector2(0.5f, 1), new Vector2(0, -328), new Vector2(340, 50), false, () => g.ToLobby());
            if (g.story != null && !g.story.Finished)
                UIKit.Label(p, g.story.Current.title, 14, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -400), new Vector2(380, 60), FontStyle.Normal);
        }

        void Shop()
        {
            var s = g.save;
            var p = Panel(860, 560, "MEJORAS");
            UIKit.Label(p, "Tienes " + FGConfig.Money(s.money) + " · gana plata vendiendo café, capturando zonas y derrotando enemigos", 16, Color.white,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -62), new Vector2(800, 24), FontStyle.Normal);
            for (int i = 0; i < FGConfig.Upgrades.Length; i++)
            {
                var u = FGConfig.Upgrades[i];
                int lvl = s.Level(u.id);
                bool maxed = lvl >= u.max;
                int cost = u.cost * (lvl + 1);
                float x = i % 2 == 0 ? -210 : 210, y = -100 - (i / 2) * 110;
                var card = UIKit.Image(p, "Mejora", new Vector2(0.5f, 1), new Vector2(x, y), new Vector2(400, 96), new Color(0, 0, 0, 0.3f));
                UIKit.Label(card.transform, u.icon, 18, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(60, 40));
                UIKit.Label(card.transform, u.name, 19, Color.white, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(76, -10), new Vector2(200, 24));
                UIKit.Label(card.transform, u.desc, 14, new Color(1, 1, 1, 0.8f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(76, -36), new Vector2(190, 40), FontStyle.Normal);
                UIKit.Label(card.transform, "Nivel " + lvl + "/" + u.max, 13, UIKit.Yellow, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(76, 8), new Vector2(120, 18));
                string id = u.id;
                var b = Btn(card.transform, maxed ? "MÁX" : FGConfig.Money(cost), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(120, 44), true, () =>
                {
                    int l = s.Level(id);
                    int c = FGConfig.Upgrades[System.Array.FindIndex(FGConfig.Upgrades, x2 => x2.id == id)].cost * (l + 1);
                    if (s.money >= c)
                    {
                        s.money -= c;
                        s.SetLevel(id, l + 1);
                        g.RefreshStats();
                        g.Persist();
                        SFX.Play("cash");
                    }
                    Show("shop");
                }, 17);
                b.interactable = !maxed && s.money >= cost;
            }
            Btn(p, g.State == GameManager.GameState.Shop ? "Volver al juego" : "Volver", new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(260, 52), true,
                () => { if (g.State == GameManager.GameState.Shop) g.Play(); else Back(); });
        }

        void Rewards()
        {
            var s = g.save;
            var p = Panel(760, 600, "PUNTOS FLOAT");
            PointsMeter(p, new Vector2(0, -70), 600);
            if (!string.IsNullOrEmpty(lastCode))
            {
                var v = UIKit.Image(p, "Cupon", new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(420, 96), new Color32(0xff, 0xf3, 0xdc, 255));
                UIKit.Label(v.transform, "¡Tu café gratis!", 18, UIKit.Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(400, 24)).GetComponent<Shadow>().enabled = false;
                UIKit.Label(v.transform, lastCode, 30, new Color32(0xc0, 0x39, 0x2b, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -2), new Vector2(400, 36)).GetComponent<Shadow>().enabled = false;
                UIKit.Label(v.transform, "Muéstralo en un punto oficial de Float Gardens", 14, UIKit.Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(400, 20), FontStyle.Normal).GetComponent<Shadow>().enabled = false;
            }
            var redeem = Btn(p, "Canjear café gratis (" + FGConfig.RedeemCost + " ★)", new Vector2(0.5f, 1), new Vector2(0, -262), new Vector2(380, 50), true, () =>
            {
                if (s.puntos < FGConfig.RedeemCost) return;
                s.puntos -= FGConfig.RedeemCost;
                lastCode = FGSave.CreateVoucher();
                s.vouchers.Add(new Voucher { code = lastCode, date = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                g.Persist();
                SFX.Play("cash");
                Show("rewards");
            });
            redeem.interactable = s.puntos >= FGConfig.RedeemCost;
            UIKit.Label(p, "Mis cupones", 18, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), new Vector2(-180, -330), new Vector2(340, 24));
            string list = "";
            for (int i = s.vouchers.Count - 1; i >= 0 && i >= s.vouchers.Count - 5; i--)
                list += s.vouchers[i].code + "   " + System.DateTimeOffset.FromUnixTimeMilliseconds(s.vouchers[i].date).ToLocalTime().ToString("dd/MM/yyyy") + "\n";
            UIKit.Label(p, list == "" ? "Aún no tienes cupones. ¡A farmear!" : list, 15, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(-180, -360), new Vector2(340, 130), FontStyle.Normal);
            UIKit.Label(p, "Puntos oficiales", 18, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0.5f, 1), new Vector2(180, -330), new Vector2(340, 24));
            UIKit.Label(p, string.Join("\n", FGConfig.OfficialPoints), 15, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(180, -360), new Vector2(340, 130), FontStyle.Normal);
            UIKit.Label(p, "Ganas puntos cosechando (+1 por cereza), vendiendo (+10), derrotando enemigos (+5), capturando zonas (+100) y venciendo al jefe (+300).",
                14, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(700, 40), FontStyle.Normal);
            Btn(p, "Volver", new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(220, 50), false, () => { lastCode = null; Back(); });
        }

        void HowTo()
        {
            var p = Panel(900, 640, "CÓMO JUGAR");
            string[] items =
            {
                "FARMEA: cosecha las plantas flotantes del HQ en Chapinero y abre cajas de café por la ciudad. Cada cereza suma Puntos Float.",
                "DISPARA: tu AeroPress dispara chorros de café a presión. Recargar gasta 1 cereza. Apunta a la cabeza para más daño.",
                "VENDE: los peatones con taza quieren café, pero solo te compran en barrios de Float Gardens. 2 cerezas = $8.000.",
                "CONQUISTA: entra a las zonas rojas, derrota las oleadas y quédate dentro hasta capturarlas. Cada zona abre un barrio nuevo.",
                "HISTORIA: habla con los personajes con [!] (Abuela Rosa, el Profe, Kaldi...). Cada misión enseña algo real del café y abre regiones de Colombia.",
                "LABORATORIO: en el PC del HQ afinas recetas de AeroPress: proporción, molienda, temperatura, tiempo y método. 80 puntos = especialidad.",
                "JEFE FINAL: con las 6 zonas, ve a la Plaza de Bolívar y derrota a Don Monopolio, CEO de Coffee Land.",
                "VIDA: se regenera si te cubres unos segundos. Si caes, vuelves al HQ y pierdes la mitad de tus cerezas.",
            };
            for (int i = 0; i < items.Length; i++)
            {
                float x = i % 2 == 0 ? -220 : 220, y = -70 - (i / 2) * 118;
                var card = UIKit.Image(p, "Paso", new Vector2(0.5f, 1), new Vector2(x, y), new Vector2(420, 108), new Color(0, 0, 0, 0.3f));
                UIKit.Label(card.transform, items[i], 14, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(396, 96), FontStyle.Normal);
            }
            string keys = FGInput.IsTouchDevice
                ? "Joystick: moverte (al fondo = correr) · Desliza a la derecha: mirar · DISPARO (arrastra para apuntar) · APUNTAR · SALTO · RECARGA · E acción · MAPA"
                : "WASD moverte · Mouse mirar · Clic disparar · Clic derecho apuntar · Shift correr · Espacio saltar · R recargar · E acción · M mapa · Esc pausa";
            UIKit.Label(p, keys, 15, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 86), new Vector2(800, 44), FontStyle.Normal);
            Btn(p, "Volver", new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(220, 50), true, Back);
        }

        void Dead()
        {
            var p = Panel(440, 260, "");
            UIKit.Label(p, "¡TE SELLARON!", 44, UIKit.Red, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(420, 56));
            UIKit.Label(p, "Coffee Land te llenó de formularios. Perdiste " + g.lastLost + " cerezas.", 16, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(400, 50), FontStyle.Normal);
            Btn(p, "Volver al HQ", new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(300, 56), true, () => g.Respawn());
        }

        void Victory()
        {
            var s = g.save;
            var p = Panel(760, 460, "");
            UIKit.Label(p, "¡BOGOTÁ ES DE FLOAT GARDENS!", 36, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(720, 50));
            UIKit.Label(p, "Don Monopolio probó el café y Coffee Land... abrió su propio invernadero flotante. Todo empezó con una AeroPress, un PC y una idea compartida. Ahora: las expediciones por Colombia.",
                17, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -96), new Vector2(680, 60), FontStyle.Normal);
            UIKit.Label(p, s.kos + " derrotados     " + s.sales + " cafés vendidos     ★ " + s.puntosTotal + " Puntos Float totales", 20, UIKit.Yellow,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(700, 30));
            PointsMeter(p, new Vector2(0, -220), 560);
            Btn(p, "Seguir farmeando", new Vector2(0.5f, 0), new Vector2(-130, 24), new Vector2(240, 54), true, () => g.Play());
            Btn(p, "Canjear", new Vector2(0.5f, 0), new Vector2(130, 24), new Vector2(240, 54), false, () => Go("rewards"));
        }

        void ResetConfirm()
        {
            var p = Panel(480, 280, "¿REINICIAR EL MUNDO?");
            UIKit.Label(p, "Se pierden las zonas capturadas, mejoras y cerezas. Tus Puntos Float, dinero y cupones se conservan.", 16, Color.white,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(440, 70), FontStyle.Normal);
            UIKit.Button(p, "Sí, reiniciar", new Vector2(0.5f, 0), new Vector2(-110, 24), new Vector2(200, 52), UIKit.Red, Color.white, () => g.ResetWorld());
            Btn(p, "Cancelar", new Vector2(0.5f, 0), new Vector2(110, 24), new Vector2(200, 52), true, Back);
        }
    }
}
