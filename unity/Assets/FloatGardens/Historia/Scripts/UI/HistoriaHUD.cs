// HUD del modo historia: reloj y lugar, altura y punto de ebullición, misión activa, plata, tazas, seguidores,
// aviso de estación cercana, avisos flotantes, tarjetas de capítulo y de lugar, y controles táctiles.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class HistoriaHUD : MonoBehaviour
    {
        StoryGame g;
        Canvas canvas;
        RectTransform root, touchRoot;
        Text clock, place, altitude, objTitle, objDesc, objProgress, money, cups, followers, prompt;
        Image cupsBar, objCard, promptBg;
        GameObject touchUse;
        readonly List<Text> toasts = new List<Text>();
        readonly List<float> toastTimes = new List<float>();
        readonly Queue<string> pending = new Queue<string>();
        CanvasGroup chapterGroup, placeGroup;
        Text chapterNum, chapterTitle, chapterSub, placeTitle, placeSub;
        float chapterT = -1f, placeT = -1f;
        int lastMoney = int.MinValue, lastCups = -1, lastFollowers = -1, lastDay = -1, lastMinute = -1;
        string lastPrompt;
        bool visible;

        const float ToastLife = 3.2f;

        public void Build(StoryGame game)
        {
            g = game;
            canvas = UIKit.Canvas("HUD Historia", 15);
            canvas.transform.SetParent(transform, false);
            root = (RectTransform)canvas.transform;

            // Arriba a la izquierda: día, hora, lugar y altura
            var info = UIKit.Image(root, "Info", new Vector2(0, 1), new Vector2(16, -14), new Vector2(330, 86), UIKit.Panel);
            clock = UIKit.Label(info.transform, "", 22, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(14, -8), new Vector2(310, 30));
            place = UIKit.Label(info.transform, "", 17, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(14, -36), new Vector2(310, 24));
            altitude = UIKit.Label(info.transform, "", 14, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(14, -60), new Vector2(310, 20), FontStyle.Normal);

            // Misión activa
            objCard = UIKit.Image(root, "Mision", new Vector2(0, 1), new Vector2(16, -110), new Vector2(330, 96), new Color(0.07f, 0.045f, 0.03f, 0.82f));
            UIKit.Image(objCard.transform, "Acento", new Vector2(0, 0.5f), Vector2.zero, new Vector2(4, 96), HConfig.Gold);
            objTitle = UIKit.Label(objCard.transform, "", 17, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(14, -8), new Vector2(306, 24));
            objDesc = UIKit.Label(objCard.transform, "", 14, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(14, -32), new Vector2(306, 46), FontStyle.Normal);
            objProgress = UIKit.Label(objCard.transform, "", 13, UIKit.Mint, TextAnchor.LowerRight, new Vector2(1, 0), new Vector2(-10, 6), new Vector2(200, 18));
            objCard.gameObject.SetActive(false);

            // Arriba a la derecha: plata, tazas y seguidores
            var stats = UIKit.Image(root, "Stats", new Vector2(1, 1), new Vector2(-16, -14), new Vector2(290, 96), UIKit.Panel);
            money = UIKit.Label(stats.transform, "", 22, UIKit.Mint, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-14, -8), new Vector2(270, 30));
            cups = UIKit.Label(stats.transform, "", 16, UIKit.Text, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-14, -38), new Vector2(270, 22));
            cupsBar = UIKit.Bar(stats.transform, "Meta", new Vector2(1, 1), new Vector2(-14, -62), new Vector2(262, 5), HConfig.Gold, out _);
            followers = UIKit.Label(stats.transform, "", 15, HUI.Muted, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-14, -72), new Vector2(270, 20), FontStyle.Normal);

            // Aviso de estación cercana
            promptBg = UIKit.Image(root, "Aviso", new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(520, 46), new Color(0, 0, 0, 0.62f));
            prompt = UIKit.Label(promptBg.transform, "", 20, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(510, 42));
            promptBg.gameObject.SetActive(false);

            // Avisos flotantes (se apilan hacia arriba)
            for (int i = 0; i < 3; i++)
            {
                var t = UIKit.Label(root, "", 17, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -22 - i * 30), new Vector2(700, 28));
                t.gameObject.SetActive(false);
                toasts.Add(t);
                toastTimes.Add(0f);
            }

            // Tarjeta de capítulo
            var ch = UIKit.Image(root, "Capitulo", new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(760, 200), new Color(0.05f, 0.03f, 0.02f, 0.88f));
            chapterGroup = ch.gameObject.AddComponent<CanvasGroup>();
            chapterGroup.blocksRaycasts = false;
            UIKit.Image(ch.transform, "Linea", new Vector2(0.5f, 0.5f), new Vector2(0, 22), new Vector2(520, 2), HConfig.Gold);
            chapterNum = UIKit.Label(ch.transform, "", 18, HConfig.Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 58), new Vector2(700, 30));
            chapterTitle = UIKit.Label(ch.transform, "", 40, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -12), new Vector2(740, 56));
            chapterSub = UIKit.Label(ch.transform, "", 18, HUI.Muted, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(720, 28), FontStyle.Italic);
            ch.gameObject.SetActive(false);

            // Tarjeta de lugar (tercio inferior)
            var pc = UIKit.Image(root, "Lugar", new Vector2(0, 0), new Vector2(40, 190), new Vector2(560, 84), new Color(0.05f, 0.03f, 0.02f, 0.78f));
            placeGroup = pc.gameObject.AddComponent<CanvasGroup>();
            placeGroup.blocksRaycasts = false;
            UIKit.Image(pc.transform, "Acento", new Vector2(0, 0.5f), Vector2.zero, new Vector2(5, 84), HConfig.Gold);
            placeTitle = UIKit.Label(pc.transform, "", 30, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(20, -8), new Vector2(530, 40));
            placeSub = UIKit.Label(pc.transform, "", 16, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(20, 10), new Vector2(530, 26), FontStyle.Normal);
            pc.gameObject.SetActive(false);

            BuildTouch();
            Show(false);
        }

        void BuildTouch()
        {
            var tc = UIKit.Canvas("Tactil Historia", 18);
            tc.transform.SetParent(transform, false);
            touchRoot = (RectTransform)tc.transform;
            var look = UIKit.Rect(touchRoot, "Mirar", new Vector2(0.45f, 0), new Vector2(1, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var lookImg = look.gameObject.AddComponent<Image>();
            lookImg.color = new Color(0, 0, 0, 0);
            look.gameObject.AddComponent<TouchLook>();

            var stickBg = UIKit.Image(touchRoot, "Joystick", new Vector2(0, 0), new Vector2(60, 40), new Vector2(170, 170), new Color(1, 1, 1, 0.12f), UIKit.Circle);
            stickBg.raycastTarget = true;
            var knob = UIKit.Image(stickBg.transform, "Perilla", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72, 72), new Color(1, 0.76f, 0.1f, 0.8f), UIKit.Circle);
            stickBg.gameObject.AddComponent<TouchStick>().knob = knob.rectTransform;

            touchUse = TouchBtn("E", FGAction.Use, new Vector2(1, 0), new Vector2(-70, 60), 110, new Color(0.18f, 0.75f, 0.44f, 0.75f)).gameObject;
            TouchBtn("SALTO", FGAction.Jump, new Vector2(1, 0), new Vector2(-200, 40), 76, new Color(0, 0, 0, 0.45f));
            TouchBtn("II", FGAction.Pause, new Vector2(0.5f, 1), new Vector2(0, -14), 54, new Color(0, 0, 0, 0.45f));
            touchRoot.gameObject.SetActive(false);
        }

        Image TouchBtn(string label, FGAction action, Vector2 anchor, Vector2 pos, float size, Color color)
        {
            var img = UIKit.Image(touchRoot, label, anchor, pos, Vector2.one * size, color, UIKit.Circle);
            img.raycastTarget = true;
            UIKit.Label(img.transform, label, label.Length > 2 ? 15 : 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * size);
            img.gameObject.AddComponent<TouchButton>().action = action;
            return img;
        }

        public void Show(bool on)
        {
            visible = on;
            root.gameObject.SetActive(on);
            UpdateTouch();
            if (on) Refresh();
        }

        public void OnPlaceChanged()
        {
            lastDay = -1;
            Refresh();
        }

        void UpdateTouch()
        {
            bool touch = visible && FGInput.IsTouchDevice && g != null && g.scene != null && g.scene.walkable && !g.UIBusy;
            if (touchRoot.gameObject.activeSelf != touch)
            {
                touchRoot.gameObject.SetActive(touch);
                if (!touch) FGInput.ResetTouch();
            }
        }

        string PlaceName()
        {
            if (g == null) return "";
            switch (g.lugar)
            {
                case Lugar.Bodega: return "Bodega Float Gardens · Puente Aranda";
                case Lugar.Mapa: return "Mapa de Colombia";
                case Lugar.Competencia: return g.run != null ? g.run.comp.name : "Competencia";
                case Lugar.Region:
                    var r = g.CurrentRegion;
                    return r != null ? r.name + " · " + r.town : "";
                default: return "";
            }
        }

        public void Refresh()
        {
            if (g == null || g.S == null) return;
            var s = g.S;
            int minute = Mathf.FloorToInt(s.hour * 60f);
            if (s.day != lastDay || minute != lastMinute)
            {
                lastDay = s.day;
                lastMinute = minute;
                clock.text = "Día " + s.day + " · " + HConfig.Clock(s.hour);
                place.text = PlaceName();
                float alt = g.Altitude;
                altitude.text = Mathf.RoundToInt(alt).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " m · el agua hierve a "
                                + HConfig.BoilingPointC(alt).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + " °C";
            }
            if (s.money != lastMoney) { lastMoney = s.money; money.text = HConfig.Money(s.money); money.color = s.money < 0 ? UIKit.Red : UIKit.Mint; }
            if (s.cups != lastCups)
            {
                lastCups = s.cups;
                cups.text = "Tazas: " + s.cups.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " / 50.000";
                cupsBar.fillAmount = Mathf.Clamp01(s.cups / (float)HConfig.CupGoal);
            }
            if (s.followers != lastFollowers)
            {
                lastFollowers = s.followers;
                followers.text = HConfig.Channel + " · " + s.followers.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO")) + " seguidores";
            }
            if (objCard.gameObject.activeSelf) objProgress.text = g.MissionProgressText();
        }

        public void Objective(string title, string desc)
        {
            bool on = !string.IsNullOrEmpty(title);
            objCard.gameObject.SetActive(on);
            if (!on) return;
            objTitle.text = g.Fill(title);
            objDesc.text = g.Fill(desc);
            objProgress.text = g.MissionProgressText();
        }

        public void Toast(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            pending.Enqueue(msg);
        }

        public void ChapterCard(Chapter c)
        {
            if (c == null) return;
            chapterNum.text = c.number == 0 ? "PRÓLOGO" : "CAPÍTULO " + c.number;
            chapterTitle.text = c.title;
            chapterSub.text = c.subtitle;
            chapterGroup.gameObject.SetActive(true);
            chapterGroup.alpha = 0f;
            chapterT = 0f;
            SFX.Play("capture", 0.7f);
        }

        public void PlaceCard(string title, string subtitle)
        {
            placeTitle.text = title;
            placeSub.text = subtitle;
            placeGroup.gameObject.SetActive(true);
            placeGroup.alpha = 0f;
            placeT = 0f;
        }

        public void Tick(float dt)
        {
            if (g == null) return;
            UpdateTouch();

            // Aviso de estación (E · ...)
            string p = null;
            if (visible && !g.UIBusy && g.player != null && g.player.focus != null) p = g.player.focus.prompt;
            if (p != lastPrompt)
            {
                lastPrompt = p;
                promptBg.gameObject.SetActive(p != null);
                if (p != null) prompt.text = (FGInput.IsTouchDevice ? "E · " : "[E]  ") + p;
            }
            if (touchUse != null) touchUse.SetActive(p != null);

            // Avisos flotantes: un hueco libre se llena con el siguiente pendiente
            for (int i = 0; i < toasts.Count; i++)
            {
                if (toastTimes[i] > 0f)
                {
                    toastTimes[i] -= dt;
                    var c = toasts[i].color;
                    c.a = Mathf.Clamp01(toastTimes[i] / 0.5f);
                    toasts[i].color = c;
                    if (toastTimes[i] <= 0f) toasts[i].gameObject.SetActive(false);
                }
                else if (pending.Count > 0)
                {
                    toasts[i].text = pending.Dequeue();
                    toasts[i].color = UIKit.Text;
                    toasts[i].gameObject.SetActive(true);
                    toastTimes[i] = ToastLife;
                }
            }

            if (chapterT >= 0f)
            {
                chapterT += dt;
                chapterGroup.alpha = chapterT < 0.6f ? chapterT / 0.6f : chapterT > 3.6f ? Mathf.Clamp01((4.4f - chapterT) / 0.8f) : 1f;
                if (chapterT > 4.4f) { chapterT = -1f; chapterGroup.gameObject.SetActive(false); }
            }
            if (placeT >= 0f)
            {
                placeT += dt;
                placeGroup.alpha = placeT < 0.4f ? placeT / 0.4f : placeT > 3f ? Mathf.Clamp01((3.6f - placeT) / 0.6f) : 1f;
                if (placeT > 3.6f) { placeT = -1f; placeGroup.gameObject.SetActive(false); }
            }

            if (visible) Refresh();
        }
    }
}
