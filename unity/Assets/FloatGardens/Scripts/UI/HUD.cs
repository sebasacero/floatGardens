// HUD estilo shooter móvil, construido con uGUI desde código.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens
{
    public class HUD : MonoBehaviour
    {
        Canvas canvas;
        RectTransform root, mapHolder, compass, numbers, killfeed;
        RawImage mapImage;
        Text locDistrict, locStreet, objTitle, objSub, capName, toast, prompt, bannerTitle, bannerSub, wallet, points, hpText, cherries, ammo, ammoMax, north;
        Image hpFill, capFill, bossFill, reloadFill, vignette, lowHp, hit1, hit2;
        GameObject capture, bossBar, reload, bigMap, bannerGo;
        RectTransform[] cross = new RectTransform[4];
        readonly List<Image> dots = new List<Image>();
        readonly List<Text> compassLabels = new List<Text>();
        readonly List<float> compassBearings = new List<float>();
        Text compassObjective;
        RawImage bigMapImage;
        RectTransform bigMarkers;

        float toastT, bannerT, hitT, hurtT;
        const float MapZoom = 0.8f; // px de UI por metro
        const float MapSize = 170;

        class Floating { public Text text; public Vector3 world; public float t, life; public Enemy follow; }
        readonly List<Floating> floating = new List<Floating>();
        readonly List<(Text t, float time)> feed = new List<(Text, float)>();

        public void Build()
        {
            canvas = UIKit.Canvas("HUD", 10);
            canvas.transform.SetParent(transform, false);
            root = (RectTransform)canvas.transform;

            vignette = FullScreen("Daño", new Color(0.8f, 0.05f, 0.03f, 0));
            lowHp = FullScreen("VidaBaja", new Color(0.5f, 0, 0, 0));

            // ---- Minimapa (arriba izquierda) ----
            var mask = UIKit.Image(root, "Minimapa", new Vector2(0, 1), new Vector2(14, -14), new Vector2(MapSize, MapSize), Color.white, UIKit.Circle);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            mask.color = new Color32(0x20, 0x24, 0x2a, 255);
            var border = UIKit.Image(root, "Borde", new Vector2(0, 1), new Vector2(11, -11), new Vector2(MapSize + 6, MapSize + 6), new Color(1, 1, 1, 0.55f), UIKit.Circle);
            border.transform.SetSiblingIndex(mask.transform.GetSiblingIndex());
            mapHolder = UIKit.At(mask.transform, "Mapa", new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var mapRT = UIKit.At(mapHolder, "Textura", new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * 100);
            mapImage = mapRT.gameObject.AddComponent<RawImage>();
            mapImage.raycastTarget = false;
            UIKit.Label(mask.transform, "▲", 22, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
            north = UIKit.Label(root, "N", 15, Color.white, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.zero, new Vector2(20, 20));
            locDistrict = UIKit.Label(root, "", 19, UIKit.Mint, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(14, -192), new Vector2(320, 26));
            locStreet = UIKit.Label(root, "", 14, new Color(1, 1, 1, 0.85f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(14, -216), new Vector2(320, 22), FontStyle.Normal);

            // ---- Brújula y objetivo (arriba centro) ----
            var compBg = UIKit.Image(root, "Brujula", new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(380, 34), new Color(0, 0, 0, 0.45f));
            compBg.gameObject.AddComponent<RectMask2D>();
            compass = (RectTransform)compBg.transform;
            string[] names = { "N", "NE", "E", "SE", "S", "SO", "O", "NO" };
            for (int b = 0; b < 360; b += 15)
            {
                bool major = b % 45 == 0;
                var t = UIKit.Label(compass, major ? names[b / 45] : "|", major ? (b % 90 == 0 ? 18 : 14) : 11, b == 0 ? UIKit.Yellow : Color.white,
                    TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 30));
                compassLabels.Add(t);
                compassBearings.Add(b);
            }
            compassObjective = UIKit.Label(compass, "▼", 14, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), Vector2.zero, new Vector2(20, 14));
            UIKit.Image(compass, "Centro", new Vector2(0.5f, 1), Vector2.zero, new Vector2(2, 7), UIKit.Yellow);
            objTitle = UIKit.Label(root, "", 20, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -46), new Vector2(600, 26));
            objSub = UIKit.Label(root, "", 15, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(600, 22), FontStyle.Normal);
            capture = UIKit.Stretch(root, "Captura").gameObject;
            capName = UIKit.Label(capture.transform, "", 15, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -96), new Vector2(600, 22));
            capFill = UIKit.Bar(capture.transform, "BarraCaptura", new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(420, 10), UIKit.Green, out _);
            bossBar = UIKit.Stretch(root, "Jefe").gameObject;
            UIKit.Label(bossBar.transform, "DON MONOPOLIO", 18, UIKit.Red, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(500, 24));
            bossFill = UIKit.Bar(bossBar.transform, "BarraJefe", new Vector2(0.5f, 1), new Vector2(0, -166), new Vector2(460, 14), UIKit.Red, out _);
            toast = UIKit.Label(root, "", 17, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -196), new Vector2(700, 26));

            // ---- Billetera y kill feed (arriba derecha) ----
            wallet = UIKit.Label(root, "", 20, UIKit.Yellow, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-14, -14), new Vector2(240, 26));
            points = UIKit.Label(root, "", 18, UIKit.Mint, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-14, -40), new Vector2(240, 24));
            killfeed = UIKit.At(root, "KillFeed", new Vector2(1, 1), new Vector2(-14, -70), new Vector2(420, 160));

            // ---- Vida (abajo izquierda) y arma (abajo derecha) ----
            UIKit.Label(root, "BARISTA · FLOAT GARDENS", 15, Color.white, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(16, 74), new Vector2(300, 22));
            hpFill = UIKit.Bar(root, "Vida", new Vector2(0, 0), new Vector2(16, 52), new Vector2(240, 14), new Color32(0x38, 0xd0, 0x7e, 255), out _);
            hpText = UIKit.Label(root, "100", 20, Color.white, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(264, 48), new Vector2(60, 24));
            cherries = UIKit.Label(root, "", 20, new Color32(0xff, 0x8a, 0x7a, 255), TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(16, 20), new Vector2(240, 26));
            UIKit.Image(root, "PanelArma", new Vector2(1, 0), new Vector2(-10, 12), new Vector2(230, 86), new Color(0, 0, 0, 0.5f));
            UIKit.Label(root, "AEROPRESS X-1", 14, UIKit.Yellow, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(-22, 70), new Vector2(200, 20));
            ammo = UIKit.Label(root, "12", 46, Color.white, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(-80, 20), new Vector2(120, 50));
            ammoMax = UIKit.Label(root, "/ 12", 20, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleLeft, new Vector2(1, 0), new Vector2(-22, 24), new Vector2(56, 26));
            reload = UIKit.Stretch(root, "Recarga").gameObject;
            reloadFill = UIKit.Bar(reload.transform, "BarraRecarga", new Vector2(1, 0), new Vector2(-22, 100), new Vector2(200, 12), UIKit.Orange, out _);
            UIKit.Label(reload.transform, "PRENSANDO...", 12, Color.white, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(-22, 116), new Vector2(200, 16));

            // ---- Mira y marcador de impacto ----
            for (int i = 0; i < 4; i++)
                cross[i] = (RectTransform)UIKit.Image(root, "Mira", new Vector2(0.5f, 0.5f), Vector2.zero, i < 2 ? new Vector2(2, 9) : new Vector2(9, 2), Color.white).transform;
            UIKit.Image(root, "Punto", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(3, 3), Color.white);
            hit1 = UIKit.Image(root, "Impacto1", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, 30), new Color(1, 1, 1, 0));
            hit2 = UIKit.Image(root, "Impacto2", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, 30), new Color(1, 1, 1, 0));
            hit1.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            hit2.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);

            prompt = UIKit.Label(root, "", 20, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(700, 30));
            bannerGo = UIKit.Stretch(root, "Anuncio").gameObject;
            bannerTitle = UIKit.Label(bannerGo.transform, "", 46, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(1000, 60));
            bannerSub = UIKit.Label(bannerGo.transform, "", 19, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -282), new Vector2(900, 26));
            numbers = UIKit.Stretch(root, "Numeros");

            // ---- Mapa completo ----
            bigMap = UIKit.Image(root, "MapaCompleto", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4000, 4000), new Color(0, 0, 0, 0.6f)).gameObject;
            var bm = UIKit.At(bigMap.transform, "Mapa", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 620));
            bigMapImage = bm.gameObject.AddComponent<RawImage>();
            bigMarkers = UIKit.Stretch(bm, "Marcadores");
            UIKit.Label(bigMap.transform, "M / botón de mapa para cerrar · ⛰ Monserrate al oriente", 16, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -330), new Vector2(700, 24));
            bigMap.SetActive(false);
            Show(false);
        }

        Image FullScreen(string name, Color c)
        {
            var img = UIKit.Stretch(root, name).gameObject.AddComponent<Image>();
            img.sprite = UIKit.White;
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        public void Show(bool on) { canvas.gameObject.SetActive(on); }

        public void SetBigMap(bool on, GameManager g)
        {
            bigMap.SetActive(on);
            if (!on) return;
            bigMapImage.texture = City.I.minimap;
            UIKit.Clear(bigMarkers);
            float size = 620;
            Vector2 P(Vector3 w) { var uv = City.I.MinimapUV(w); return new Vector2((uv.x - 0.5f) * size, (uv.y - 0.5f) * size); }
            foreach (var z in g.zones)
            {
                Color c = z.ours ? UIKit.Green : z == g.targetZone ? UIKit.Yellow : UIKit.Red;
                UIKit.Image(bigMarkers, "Zona", new Vector2(0.5f, 0.5f), P(z.transform.position), new Vector2(16, 16), c, UIKit.Circle);
                UIKit.Label(bigMarkers, z.def.name, 14, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), P(z.transform.position) + Vector2.up * 16, new Vector2(220, 20));
            }
            UIKit.Image(bigMarkers, "HQ", new Vector2(0.5f, 0.5f), P(City.I.hqCenter), new Vector2(14, 14), UIKit.Green);
            UIKit.Label(bigMarkers, "Float Gardens HQ", 14, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), P(City.I.hqCenter) + Vector2.up * 16, new Vector2(220, 20));
            UIKit.Label(bigMarkers, g.AllCaptured ? "☠ Torre Arcadia" : "Torre Arcadia (bloqueada)", 14, g.AllCaptured ? UIKit.Yellow : UIKit.Red,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), P(City.I.bossArena) + Vector2.up * 30, new Vector2(260, 20));
            var arrow = UIKit.Label(bigMarkers, "▲", 22, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), P(g.player.transform.position), new Vector2(30, 30));
            arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, -g.player.yaw);
        }

        public void Tick(GameManager g, float dt)
        {
            var p = g.player;
            Vector3 pos = p.transform.position;

            // Minimapa: el mapa gira para que el jugador mire siempre hacia arriba
            if (mapImage.texture == null)
            {
                mapImage.texture = City.I.minimap;
                mapImage.rectTransform.sizeDelta = new Vector2(City.I.minimap.width, City.I.minimap.height) * MapZoom;
            }
            mapHolder.localRotation = Quaternion.Euler(0, 0, p.yaw);
            Vector2 uv = City.I.MinimapUV(pos);
            mapImage.rectTransform.anchoredPosition = -(uv - new Vector2(0.5f, 0.5f)) * mapImage.rectTransform.sizeDelta;
            int di = 0;
            void Dot(Vector3 w, Color c, float size)
            {
                Vector2 d = new Vector2(w.x - pos.x, w.z - pos.z) * MapZoom;
                if (d.magnitude > MapSize * 0.7f) return;
                if (di >= dots.Count) dots.Add(UIKit.Image(mapHolder, "Punto", new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one, Color.white, UIKit.Circle));
                var img = dots[di++];
                img.gameObject.SetActive(true);
                img.rectTransform.anchoredPosition = d;
                img.rectTransform.sizeDelta = Vector2.one * size;
                img.color = c;
            }
            foreach (var z in g.zones)
                Dot(z.transform.position, z.ours ? new Color(0.18f, 0.75f, 0.44f, 0.5f) : z == g.targetZone ? new Color(1, 0.83f, 0.3f, 0.5f) : new Color(0.88f, 0.25f, 0.23f, 0.5f), FGConfig.CaptureRadius * 2 * MapZoom);
            Dot(City.I.hqCenter, UIKit.Green, 12);
            foreach (var l in g.loot) Dot(l.transform.position, UIKit.Yellow, 5);
            foreach (var c in g.peds) if (c.IconVisible) Dot(c.transform.position, new Color32(0xff, 0xf3, 0xdc, 255), 5);
            foreach (var e in g.enemies)
                if (!e.Dead && (e.alerted || Vector3.Distance(e.transform.position, pos) < 45)) Dot(e.transform.position, UIKit.Red, e.isBoss ? 12 : 6);
            for (int i = di; i < dots.Count; i++) dots[i].gameObject.SetActive(false);
            float na = (90 + p.yaw) * Mathf.Deg2Rad;
            north.rectTransform.anchoredPosition = new Vector2(14 + MapSize / 2 + Mathf.Cos(na) * (MapSize / 2 - 10), -14 - MapSize / 2 + Mathf.Sin(na) * (MapSize / 2 - 10));

            char code = City.DistrictAt(pos);
            locDistrict.text = FGConfig.DistrictName(code).ToUpperInvariant();
            locDistrict.color = g.IsMarket(pos) ? UIKit.Mint : new Color32(0xff, 0x8a, 0x7a, 255);
            locStreet.text = City.StreetName(pos);

            // Brújula
            float heading = Mathf.Repeat(p.yaw, 360);
            float pxDeg = 380f / 120f;
            for (int i = 0; i < compassLabels.Count; i++)
            {
                float diff = Mathf.DeltaAngle(heading, compassBearings[i]);
                var t = compassLabels[i];
                bool vis = Mathf.Abs(diff) < 62;
                t.enabled = vis;
                if (vis) t.rectTransform.anchoredPosition = new Vector2(diff * pxDeg, 0);
            }
            var obj = g.Objective();
            compassObjective.enabled = obj.hasTarget;
            if (obj.hasTarget)
            {
                Vector3 to = obj.target - pos;
                float b = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                float diff = Mathf.Clamp(Mathf.DeltaAngle(heading, b), -58, 58);
                compassObjective.rectTransform.anchoredPosition = new Vector2(diff * pxDeg, -6);
            }
            objTitle.text = obj.title.ToUpperInvariant();
            objSub.text = obj.hasTarget ? Mathf.RoundToInt(Vector3.Distance(Flat(obj.target), Flat(pos))) + " m · " + obj.sub : obj.sub;

            // Captura y jefe
            capture.SetActive(g.capturing != null);
            if (g.capturing != null)
            {
                capName.text = g.capturingContested ? g.capturing.def.name + " · ¡ELIMINA A LOS ENEMIGOS!" : "CAPTURANDO " + g.capturing.def.name.ToUpperInvariant();
                capName.color = g.capturingContested ? UIKit.Red : Color.white;
                capFill.fillAmount = g.capturing.progress;
                capFill.color = g.capturingContested ? UIKit.Red : UIKit.Green;
            }
            bossBar.SetActive(g.boss != null && !g.boss.Dead);
            if (g.boss != null) bossFill.fillAmount = Mathf.Clamp01(g.boss.hp / g.boss.maxHp);

            // Vida, cerezas, arma
            float frac = Mathf.Clamp01(p.hp / p.maxHp);
            hpFill.fillAmount = frac;
            hpFill.color = frac < 0.3f ? UIKit.Red : (Color)new Color32(0x38, 0xd0, 0x7e, 255);
            hpText.text = Mathf.CeilToInt(Mathf.Max(0, p.hp)).ToString();
            cherries.text = "Cerezas: " + g.cherries;
            ammo.text = p.ammo.ToString();
            ammo.color = p.ammo <= 3 ? UIKit.Red : Color.white;
            ammoMax.text = "/ " + p.magSize;
            reload.SetActive(p.reloading > 0);
            if (p.reloading > 0) reloadFill.fillAmount = 1 - p.reloading / FGConfig.ReloadTime;
            wallet.text = FGConfig.Money(g.save.money);
            points.text = "★ " + g.save.puntos + " Puntos Float";
            prompt.text = g.prompt ?? "";

            // Mira
            float gap = 6 + p.spread * 60;
            Color cc = p.aiming ? UIKit.Yellow : Color.white;
            cross[0].anchoredPosition = new Vector2(0, gap + 4.5f);
            cross[1].anchoredPosition = new Vector2(0, -gap - 4.5f);
            cross[2].anchoredPosition = new Vector2(-gap - 4.5f, 0);
            cross[3].anchoredPosition = new Vector2(gap + 4.5f, 0);
            foreach (var c in cross) c.GetComponent<Image>().color = cc;
            hitT -= dt;
            float ha = Mathf.Clamp01(hitT / 0.25f);
            hit1.color = new Color(hit1.color.r, hit1.color.g, hit1.color.b, ha);
            hit2.color = hit1.color;
            float hs = 1 + ha * 0.4f;
            hit1.rectTransform.localScale = hit2.rectTransform.localScale = Vector3.one * hs;

            // Efectos de pantalla
            hurtT -= dt;
            vignette.color = new Color(0.8f, 0.05f, 0.03f, Mathf.Clamp01(hurtT / 0.25f) * 0.35f);
            lowHp.color = new Color(0.5f, 0, 0, frac < 0.35f ? (0.35f - frac) * (0.8f + Mathf.Sin(Time.time * 6) * 0.2f) : 0);
            toastT -= dt;
            toast.color = new Color(1, 1, 1, Mathf.Clamp01(toastT / 0.4f));
            bannerT -= dt;
            float ba = Mathf.Clamp01(bannerT / 0.5f) * Mathf.Clamp01((2.8f - bannerT) / 0.25f);
            bannerGo.SetActive(bannerT > 0);
            bannerTitle.color = new Color(bannerTitle.color.r, bannerTitle.color.g, bannerTitle.color.b, ba);
            bannerSub.color = new Color(1, 1, 1, ba);
            bannerGo.transform.localScale = Vector3.one * (1 + Mathf.Max(0, bannerT - 2.55f) * 1.2f);

            // Kill feed
            for (int i = feed.Count - 1; i >= 0; i--)
            {
                float age = Time.time - feed[i].time;
                if (age > 5) { Destroy(feed[i].t.gameObject); feed.RemoveAt(i); continue; }
            }
            for (int i = 0; i < feed.Count; i++) feed[feed.Count - 1 - i].t.rectTransform.anchoredPosition = new Vector2(0, -i * 26);

            // Números de daño y globos de diálogo (proyectados desde el mundo)
            for (int i = floating.Count - 1; i >= 0; i--)
            {
                var f = floating[i];
                f.t += dt;
                Vector3 w = f.follow != null ? f.follow.transform.position + Vector3.up * (f.follow.isBoss ? 6.4f : 2.7f) : f.world + Vector3.up * f.t * 1.2f;
                Vector3 sp = g.cam.WorldToScreenPoint(w);
                bool vis = sp.z > 0 && (f.follow == null || Vector3.Distance(f.follow.transform.position, pos) < 45);
                f.text.enabled = vis;
                if (vis)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(numbers, sp, null, out Vector2 lp);
                    f.text.rectTransform.anchoredPosition = lp;
                    var c = f.text.color;
                    c.a = Mathf.Clamp01((f.life - f.t) / 0.4f);
                    f.text.color = c;
                }
                if (f.t > f.life) { Destroy(f.text.gameObject); floating.RemoveAt(i); }
            }
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        public void Toast(string text) { toast.text = text; toastT = 2.6f; }

        public void Banner(string title, string sub, bool ours)
        {
            bannerTitle.text = title;
            bannerSub.text = sub;
            bannerTitle.color = ours ? UIKit.Mint : (Color)new Color32(0xff, 0x8a, 0x7a, 255);
            bannerT = 2.8f;
        }

        public void HitMarker(bool head, bool ko)
        {
            hitT = 0.25f;
            Color c = ko ? UIKit.Red : head ? UIKit.Yellow : Color.white;
            hit1.color = hit2.color = c;
        }

        public void Hurt(float amount) { hurtT = Mathf.Clamp(0.15f + amount / 60f, 0.15f, 0.5f); }

        public void DamageNumber(Vector3 world, float amount, bool crit)
        {
            var t = UIKit.Label(numbers, Mathf.RoundToInt(amount).ToString(), crit ? 30 : 22, crit ? UIKit.Yellow : Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 34));
            floating.Add(new Floating { text = t, world = world + new Vector3(Random.Range(-0.2f, 0.2f), 0.3f, 0), life = 0.9f });
        }

        public void Bubble(Enemy e, string text)
        {
            var t = UIKit.Label(numbers, text, 15, new Color32(0x22, 0x22, 0x22, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 26));
            t.GetComponent<Shadow>().enabled = false;
            var bg = t.gameObject.AddComponent<Outline>();
            bg.effectColor = Color.white;
            bg.effectDistance = new Vector2(2, -2);
            floating.Add(new Floating { text = t, follow = e, life = 2.2f });
        }

        public void KillFeed(string text)
        {
            var t = UIKit.Label(killfeed, text, 15, Color.white, TextAnchor.MiddleRight, new Vector2(1, 1), Vector2.zero, new Vector2(420, 24));
            t.supportRichText = true;
            feed.Add((t, Time.time));
            while (feed.Count > 5) { Destroy(feed[0].t.gameObject); feed.RemoveAt(0); }
        }
    }
}
