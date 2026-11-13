// Pantallas de la campaña: el Códice del Café, el mapa de expediciones por Colombia y el Laboratorio AeroPress (el PC).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens
{
    public partial class Menus
    {
        string codexCategory = "Historia";
        string mapRegion = "bogota";
        readonly BrewRecipe recipe = new BrewRecipe();
        BrewResult lastBrew;
        static Sprite colombia;

        // ---------- Códice ----------
        void CodexScreen()
        {
            var s = g.save;
            var p = Panel(960, 640, "CÓDICE DEL CAFÉ");
            int total = Codex.Entries.Length, have = 0;
            foreach (var e in Codex.Entries) if (s.codex.Contains(e.id)) have++;
            UIKit.Label(p, have + " de " + total + " entradas · se desbloquean con misiones, expediciones y recetas", 15, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(900, 22), FontStyle.Normal);

            // Categorías a la izquierda
            for (int i = 0; i < Codex.Categories.Length; i++)
            {
                string cat = Codex.Categories[i];
                int n = 0, k = 0;
                foreach (var e in Codex.Entries) if (e.category == cat) { n++; if (s.codex.Contains(e.id)) k++; }
                Btn(p, cat + "  " + k + "/" + n, new Vector2(0, 1), new Vector2(24, -92 - i * 50), new Vector2(210, 44), cat == codexCategory, () => { codexCategory = cat; Show("codex"); }, 16);
            }

            // Entradas a la derecha, con scroll
            var content = UIKit.Scroll(p, new Vector2(0, 1), new Vector2(250, -92), new Vector2(686, 470), out _);
            float y = -8;
            foreach (var e in Codex.Entries)
            {
                if (e.category != codexCategory) continue;
                bool open = s.codex.Contains(e.id);
                float h = open ? 112 : 40;
                var card = UIKit.Image(content, "Entrada", new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(660, h), new Color(0, 0, 0, open ? 0.35f : 0.18f));
                UIKit.Label(card.transform, open ? e.title : "???  (todavía no la aprendes)", 17, open ? UIKit.Yellow : new Color(1, 1, 1, 0.45f), TextAnchor.MiddleLeft,
                    new Vector2(0, 1), new Vector2(14, -6), new Vector2(630, 26));
                if (open)
                    UIKit.Label(card.transform, e.text, 14, Color.white, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(14, -34), new Vector2(630, 76), FontStyle.Normal);
                y -= h + 8;
            }
            content.sizeDelta = new Vector2(0, Mathf.Max(470, -y + 8));
            Btn(p, "Volver", new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(220, 46), true, Back);
        }

        // ---------- Mapa de Colombia ----------
        static Sprite Colombia()
        {
            if (colombia != null) return colombia;
            const int W = 260, H = 350;
            var px = new Color32[W * H];
            var land = new Color32(0x2f, 0x8f, 0x45, 255);
            var coast = new Color32(0x7d, 0xff, 0xb0, 255);
            var pts = Codex.Outline;
            for (int y = 0; y < H; y++)
            {
                float lat = Mathf.Lerp(-4.5f, 13f, (y + 0.5f) / H);
                // intersecciones de la fila con el contorno (regla par-impar)
                var xs = new List<float>();
                for (int i = 0; i < pts.Length; i++)
                {
                    var a = pts[i]; var b = pts[(i + 1) % pts.Length];
                    if ((a.x <= lat && b.x > lat) || (b.x <= lat && a.x > lat))
                        xs.Add(a.y + (lat - a.x) / (b.x - a.x) * (b.y - a.y));
                }
                xs.Sort();
                for (int k = 0; k + 1 < xs.Count; k += 2)
                {
                    int x0 = Mathf.Clamp(Mathf.RoundToInt(Mathf.InverseLerp(-79.5f, -66.5f, xs[k]) * W), 0, W);
                    int x1 = Mathf.Clamp(Mathf.RoundToInt(Mathf.InverseLerp(-79.5f, -66.5f, xs[k + 1]) * W), 0, W);
                    for (int x = x0; x < x1; x++) px[y * W + x] = (x - x0 < 2 || x1 - x < 3) ? coast : land;
                }
            }
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            t.SetPixels32(px);
            t.Apply();
            return colombia = Sprite.Create(t, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f));
        }

        static Vector2 MapPos(float lat, float lon, Vector2 size)
        {
            return new Vector2((Mathf.InverseLerp(-79.5f, -66.5f, lon) - 0.5f) * size.x, (Mathf.InverseLerp(-4.5f, 13f, lat) - 0.5f) * size.y);
        }

        void MapScreen()
        {
            var s = g.save;
            var p = Panel(980, 660, "EXPEDICIONES POR COLOMBIA");
            UIKit.Label(p, "Cada región abre variedades para tu laboratorio y páginas del Códice. Se desbloquean con los capítulos de la historia.",
                15, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(920, 22), FontStyle.Normal);
            var mapSize = new Vector2(380, 512);
            var map = UIKit.Image(p, "Colombia", new Vector2(0, 1), new Vector2(40, -90), mapSize, new Color(1, 1, 1, 0.9f), Colombia());
            map.rectTransform.pivot = new Vector2(0, 1);
            var mapCenter = map.transform;
            foreach (var r in Codex.Regions)
            {
                bool open = s.regions.Contains(r.id) || r.chapter == 0;
                var pos = MapPos(r.lat, r.lon, mapSize) + new Vector2(mapSize.x / 2, -mapSize.y / 2);
                var id = r.id;
                var b = UIKit.Button(mapCenter, "", new Vector2(0, 1), pos, new Vector2(22, 22), open ? (id == mapRegion ? UIKit.Yellow : UIKit.Mint) : new Color(0, 0, 0, 0.7f), Color.white, () => { mapRegion = id; Show("mapa"); }, 10);
                b.image.sprite = UIKit.Circle;
                UIKit.Label(mapCenter, r.name, 12, open ? Color.white : new Color(1, 1, 1, 0.5f), TextAnchor.MiddleLeft, new Vector2(0, 1), pos + new Vector2(14, 0), new Vector2(200, 18));
            }

            var r0 = Codex.GetRegion(mapRegion) ?? Codex.Regions[0];
            bool unlocked = s.regions.Contains(r0.id) || r0.chapter == 0;
            var card = UIKit.Image(p, "Region", new Vector2(1, 1), new Vector2(-40, -90), new Vector2(500, 512), new Color(0, 0, 0, 0.3f));
            card.rectTransform.pivot = new Vector2(1, 1);
            var c = card.transform;
            UIKit.Label(c, r0.name, 28, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(20, -14), new Vector2(460, 36));
            UIKit.Label(c, r0.dept + " · " + r0.altitude, 15, UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(20, -50), new Vector2(460, 22));
            UIKit.Label(c, r0.profile, 16, Color.white, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -80), new Vector2(460, 90), FontStyle.Normal);
            string vars = "";
            foreach (var vid in r0.varieties) { var v = Codex.GetVariety(vid); vars += "• " + v.name + " (" + v.origin + "): " + v.notes + "\n"; }
            UIKit.Label(c, unlocked ? "Variedades que te regala" : "Variedades (bloqueadas)", 17, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(20, -180), new Vector2(460, 24));
            UIKit.Label(c, vars, 14, unlocked ? Color.white : new Color(1, 1, 1, 0.5f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -208), new Vector2(460, 150), FontStyle.Normal);
            string codexNames = "";
            foreach (var cid in r0.codex) { var e = Codex.Get(cid); if (e != null) codexNames += (codexNames == "" ? "" : " · ") + e.title; }
            UIKit.Label(c, "Códice: " + codexNames, 13, new Color(1, 1, 1, 0.8f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -370), new Vector2(460, 60), FontStyle.Normal);
            UIKit.Label(c, unlocked ? "✓ Expedición abierta" : "Se abre en: " + r0.unlockHint, 16, unlocked ? UIKit.Mint : UIKit.Orange, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(20, 16), new Vector2(460, 26));

            int open2 = 0;
            foreach (var r in Codex.Regions) if (s.regions.Contains(r.id) || r.chapter == 0) open2++;
            UIKit.Label(p, open2 + " de " + Codex.Regions.Length + " regiones · Después de Colombia: Oslo, Melbourne, Tokio y Addis Abeba (próximamente)", 13,
                new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(900, 20), FontStyle.Normal);
            Btn(p, "Volver", new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(220, 46), true, Back);
        }

        // ---------- Laboratorio AeroPress ----------
        void LabScreen()
        {
            var s = g.save;
            var p = Panel(980, 660, "LABORATORIO AEROPRESS · TU PC");
            var varieties = g.story.AvailableVarieties();
            if (!varieties.Exists(v => v.id == recipe.variety)) recipe.variety = varieties[0].id;
            var cur = Codex.GetVariety(recipe.variety);

            // Variedades (columna izquierda)
            UIKit.Label(p, "VARIEDAD", 15, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(30, -66), new Vector2(200, 22));
            for (int i = 0; i < varieties.Count && i < 10; i++)
            {
                var v = varieties[i];
                float best = s.BestBrew(v.id);
                Btn(p, v.name + (best > 0 ? "  " + best.ToString("0.0") : ""), new Vector2(0, 1), new Vector2(30, -92 - i * 44), new Vector2(210, 40), v.id == recipe.variety, () => { recipe.variety = v.id; lastBrew = null; Show("lab"); }, 15);
            }
            UIKit.Label(p, cur.origin + "\n" + cur.notes, 13, new Color(1, 1, 1, 0.85f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(30, -96 - Mathf.Min(varieties.Count, 10) * 44), new Vector2(210, 120), FontStyle.Normal);

            // Receta (centro)
            float x = 270, w = 400;
            UIKit.Label(p, "RECETA", 15, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(x, -66), new Vector2(200, 22));
            Row(p, x, -96, w, "Café", recipe.dose.ToString("0") + " g", 12, 20, recipe.dose, true, v => recipe.dose = v);
            Row(p, x, -150, w, "Agua", recipe.water.ToString("0") + " ml   (1:" + recipe.Ratio.ToString("0.0") + ")", 150, 260, recipe.water, true, v => recipe.water = Mathf.Round(v / 5) * 5);
            Row(p, x, -204, w, "Molienda", recipe.grind.ToString("0.0") + "  (1 fina · 10 gruesa)", 1, 10, recipe.grind, false, v => recipe.grind = Mathf.Round(v * 2) / 2);
            Row(p, x, -258, w, "Temperatura", recipe.temp.ToString("0") + " °C", 78, 100, recipe.temp, true, v => recipe.temp = v);
            Row(p, x, -312, w, "Tiempo", Mathf.FloorToInt(recipe.time / 60) + ":" + (recipe.time % 60).ToString("00"), 30, 240, recipe.time, true, v => recipe.time = Mathf.Round(v / 5) * 5);
            Btn(p, recipe.inverted ? "Método: invertido" : "Método: estándar", new Vector2(0, 1), new Vector2(x, -372), new Vector2(195, 42), recipe.inverted, () => { recipe.inverted = !recipe.inverted; Show("lab"); }, 15);
            Btn(p, recipe.bloom ? "Bloom: sí (30 s)" : "Bloom: no", new Vector2(0, 1), new Vector2(x + 205, -372), new Vector2(195, 42), recipe.bloom, () => { recipe.bloom = !recipe.bloom; Show("lab"); }, 15);
            Btn(p, "PREPARAR Y CATAR", new Vector2(0, 1), new Vector2(x, -430), new Vector2(w, 54), true, () =>
            {
                lastBrew = Brewing.Evaluate(recipe, Random.Range(-0.75f, 0.75f));
                g.OnBrew(recipe.Clone(), lastBrew);
                SFX.Play(lastBrew.specialty ? "capture" : "pickup");
                if (g.State == GameManager.GameState.Lab) Show("lab");
            }, 20);
            UIKit.Label(p, "Cambia una variable a la vez. Fino, caliente y largo extraen más; grueso, frío y corto, menos. Rango dorado: 18-22 % de extracción y 1,15-1,35 % de TDS.",
                12, new Color(1, 1, 1, 0.7f), TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(x, -494), new Vector2(w, 60), FontStyle.Normal);

            // Catación (derecha)
            var card = UIKit.Image(p, "Cata", new Vector2(1, 1), new Vector2(-30, -66), new Vector2(250, 500), new Color(0, 0, 0, 0.35f));
            card.rectTransform.pivot = new Vector2(1, 1);
            var c = card.transform;
            UIKit.Label(c, "CATACIÓN", 15, UIKit.Yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(230, 22));
            if (lastBrew == null)
            {
                UIKit.Label(c, "Prepara una taza para catarla.\n\nEscala de 100 puntos:\n80+ especialidad\n86+ Taza de Excelencia\n90+ nivel mundial", 15, Color.white, TextAnchor.UpperCenter,
                    new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(220, 200), FontStyle.Normal);
            }
            else
            {
                var b = lastBrew;
                UIKit.Label(c, b.score.ToString("0.00"), 56, b.specialty ? UIKit.Mint : Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(230, 64));
                UIKit.Label(c, b.verdict, 16, b.score >= 86 ? UIKit.Yellow : b.specialty ? UIKit.Mint : UIKit.Orange, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -108), new Vector2(230, 24));
                UIKit.Label(c, "Extracción " + b.extraction.ToString("0.0") + " %   ·   TDS " + b.tds.ToString("0.00") + " %", 13, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0, -136), new Vector2(230, 20), FontStyle.Normal);
                UIKit.Label(c, b.notes, 14, Color.white, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(0, -166), new Vector2(222, 120), FontStyle.Normal);
                UIKit.Label(c, "Consejo: " + b.tip, 14, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0.5f, 1), new Vector2(0, -290), new Vector2(222, 80), FontStyle.Normal);
                int pts = b.score >= 75 ? Mathf.RoundToInt((b.score - 70) * 3) : 0;
                UIKit.Label(c, pts > 0 ? "+" + pts + " ★ Puntos Float" : "Desde 75 puntos ganas ★", 14, UIKit.Mint, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(230, 20));
            }
            UIKit.Label(c, "Récord: " + (s.bestBrew > 0 ? s.bestBrew.ToString("0.00") : "—") + "   ·   Tazas: " + s.brewCount, 13, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(230, 20), FontStyle.Normal);

            Btn(p, g.State == GameManager.GameState.Lab ? "Volver al juego" : "Volver", new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(240, 46), false,
                () => { if (g.State == GameManager.GameState.Lab) g.Play(); else Back(); });
        }

        void Row(Transform p, float x, float y, float w, string name, string value, float min, float max, float cur, bool whole, System.Action<float> set)
        {
            UIKit.Label(p, name, 15, Color.white, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(x, y), new Vector2(160, 22));
            var valueLabel = UIKit.Label(p, value, 15, UIKit.Mint, TextAnchor.MiddleRight, new Vector2(0, 1), new Vector2(x + w - 240, y), new Vector2(240, 22));
            UIKit.Slider(p, new Vector2(0, 1), new Vector2(x, y - 26), new Vector2(w, 14), min, max, cur, whole, v =>
            {
                set(v);
                lastBrew = null;
                // refrescar solo el texto del valor (sin reconstruir la pantalla mientras se arrastra)
                valueLabel.text = name == "Café" ? recipe.dose.ToString("0") + " g"
                    : name == "Agua" ? recipe.water.ToString("0") + " ml   (1:" + recipe.Ratio.ToString("0.0") + ")"
                    : name == "Molienda" ? recipe.grind.ToString("0.0") + "  (1 fina · 10 gruesa)"
                    : name == "Temperatura" ? recipe.temp.ToString("0") + " °C"
                    : Mathf.FloorToInt(recipe.time / 60) + ":" + (recipe.time % 60).ToString("00");
            });
        }
    }
}
