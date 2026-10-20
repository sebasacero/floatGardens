// Laboratorio de AeroPress: receta, curva de extracción en vivo, preparación, carta de control (EY vs TDS),
// publicar en acero-press y vender la taza. También funciona como reto del mentor y como ronda de competencia.
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class BrewPanel : HPanel
    {
        public string retoRegion = ""; // si no está vacío, el panel funciona como reto del mentor de esa región

        static CoffeeModel.BrewParams recipe = CoffeeModel.BrewParams.Default; // se recuerda la última receta
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        enum Phase { Recipe, Brewing, Result }
        Phase phase = Phase.Recipe;
        List<Lot> lots = new List<Lot>();
        int lotIndex;
        Lot virtualLot;
        HUI.LineChart chart;
        Text gauge, lotLabel, stepText;
        Image stepBar;
        float brewT;
        CoffeeModel.BrewResult result;
        CoffeeModel.BrewParams used;
        bool published, sold;

        public override float HoursCost => HConfig.HoursPerBrew;
        bool Competition => G.lugar == Lugar.Competencia && G.run != null && !G.run.finished;
        bool Reto => !string.IsNullOrEmpty(retoRegion);
        bool Refractometer => S.gear.Contains("refractometro");
        float Boil => HConfig.BoilingPointC(G.Altitude);

        static readonly string[] Steps = { "Moliendo el café...", "Vertiendo el agua...", "Revolviendo...", "Dejando infusionar...", "Presionando el émbolo..." };

        Lot CurrentLot
        {
            get
            {
                if (lots.Count > 0) return lots[Mathf.Clamp(lotIndex, 0, lots.Count - 1)];
                return virtualLot;
            }
        }

        protected override void Build()
        {
            lots = S.LotsAt("tostado");
            if ((Reto || Competition) && virtualLot == null)
            {
                var r = Catalog.Region(retoRegion);
                var v = r != null ? Catalog.Variety(r.giftVariety) : null;
                virtualLot = new Lot
                {
                    id = "muestra", variety = v != null ? v.id : "caturra", origin = Reto ? retoRegion : "floatgardens", stage = "tostado",
                    grams = 500f, potential = Reto ? 86f : 85f, roast = "medio", roastLevel = 0.4f, roastScore = 85f, process = "lavado",
                    notes = v != null ? v.notes : "panela, cítricos", day = S.day - 4,
                };
                if (Reto) lots.Clear(); // en el reto se usa el café de la finca
            }
            string title = Reto ? "Reto AeroPress" : Competition ? "Ronda " + (G.run.round + 1) + " · " + G.run.comp.name : "AeroPress";
            string sub = Mathf.RoundToInt(G.Altitude).ToString("N0", CultureInfo.GetCultureInfo("es-CO")) + " m s. n. m. · el agua hierve a " + Boil.ToString("0", Inv) + " °C";
            var c = HUI.Window(root, title, new Vector2(1180, 640), Competition ? (System.Action)null : Close, sub);
            if (phase == Phase.Result) { BuildResult(c); return; }
            BuildRecipe(c);
        }

        // ============================ RECETA ============================

        void BuildRecipe(RectTransform c)
        {
            // Café
            var lotCard = HUI.Card(c, new Vector2(0, 1), new Vector2(0, 0), new Vector2(560, 70));
            UIKit.Label(lotCard.transform, "Café", 15, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -6), new Vector2(200, 20), FontStyle.Normal);
            lotLabel = UIKit.Label(lotCard.transform, "", 16, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(12, 8), new Vector2(430, 40), FontStyle.Bold);
            if (lots.Count > 1)
            {
                UIKit.Button(lotCard.transform, "<", new Vector2(1, 0.5f), new Vector2(-62, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + lots.Count - 1) % lots.Count; UpdateLot(); Redraw(); }, 22);
                UIKit.Button(lotCard.transform, ">", new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + 1) % lots.Count; UpdateLot(); Redraw(); }, 22);
            }
            UpdateLot();

            float y = -84;
            float step = -47;
            HUI.Stepper(c, "Dosis de café (g)", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.dose, v => recipe.dose = v, 0.5f, 10f, 25f, v => v.ToString("0.0", Inv) + " g", Redraw); y += step;
            HUI.Stepper(c, "Agua (g)", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.water, v => recipe.water = v, 5f, 100f, 260f, v => v.ToString("0", Inv) + " g", Redraw); y += step;
            HUI.Stepper(c, "Temperatura (máx. " + Boil.ToString("0", Inv) + " °C aquí)", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.temp, v => recipe.temp = v, 1f, 75f, 100f, v => v.ToString("0", Inv) + " °C", Redraw); y += step;
            HUI.Stepper(c, "Molienda", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.grind, v => recipe.grind = v, 50f, 250f, 1400f, v => GrindWord(v) + " " + v.ToString("0", Inv), Redraw); y += step;
            HUI.Stepper(c, "Tiempo de infusión (s)", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.steep, v => recipe.steep = v, 10f, 30f, 240f, v => Clock(v), Redraw); y += step;
            HUI.Stepper(c, "Revueltas", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.stirs, v => recipe.stirs = v, 1f, 0f, 20f, v => v.ToString("0", Inv), Redraw); y += step;
            HUI.Stepper(c, "Diluir después con agua (g)", new Vector2(0, 1), new Vector2(0, y), 560, () => recipe.dilute, v => recipe.dilute = v, 10f, 0f, 150f, v => v.ToString("0", Inv) + " g", Redraw); y += step;

            UIKit.Button(c, recipe.inverted ? "Método: invertido" : "Método: estándar", new Vector2(0, 1), new Vector2(0, y - 4), new Vector2(272, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { recipe.inverted = !recipe.inverted; Rebuild(); }, 17);
            bool metalOk = S.gear.Contains("filtro_metal");
            if (!metalOk) recipe.metal = false;
            var fb = UIKit.Button(c, recipe.metal ? "Filtro: metálico" : metalOk ? "Filtro: papel" : "Filtro: papel (metálico en la tienda)", new Vector2(0, 1), new Vector2(288, y - 4), new Vector2(272, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { if (metalOk) { recipe.metal = !recipe.metal; Rebuild(); } }, 15);
            fb.interactable = metalOk;

            // Derecha: curva de extracción
            UIKit.Label(c, "Extracción en el tiempo", 17, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(1, 1), Vector2.zero, new Vector2(560, 26));
            chart = new HUI.LineChart(c, new Vector2(1, 1), new Vector2(0, -30), new Vector2(560, 250));
            gauge = HUI.Paragraph(c, "", new Vector2(1, 1), new Vector2(0, -290), new Vector2(560, 120), 16);
            string tip = G.Altitude > 2000f
                ? "Tip: aquí el agua hierve a " + Boil.ToString("0", Inv) + " °C. Para extraer más sin pasarte de temperatura, muele un poco más fino o deja más tiempo."
                : "Tip: a esta altura el agua llega hasta " + Boil.ToString("0", Inv) + " °C. Con agua más caliente la extracción va más rápido: ¡ajusta la receta que aprendiste en Bogotá!";
            HUI.Paragraph(c, tip, new Vector2(1, 1), new Vector2(0, -410), new Vector2(560, 60), 14, HUI.Muted);

            var lot = CurrentLot;
            bool enough = lot != null && lot.grams >= recipe.dose;
            var brew = UIKit.Button(c, enough ? "Preparar" : "No alcanza el café", new Vector2(1, 0), new Vector2(0, 0), new Vector2(560, 60), enough ? UIKit.Yellow : new Color(1, 1, 1, 0.1f), enough ? UIKit.Ink : UIKit.Text, StartBrew, 24);
            brew.interactable = enough;
            if (lot == null)
                HUI.Paragraph(c, "No tienes café tostado. Tuesta café verde en la tostadora o compra café en la tienda del computador.", new Vector2(1, 0), new Vector2(0, 70), new Vector2(560, 50), 15, UIKit.Orange);
            Redraw();
        }

        void UpdateLot()
        {
            if (lotLabel == null) return;
            var l = CurrentLot;
            if (l == null) { lotLabel.text = "Sin café tostado"; return; }
            int age = S.day - l.day;
            lotLabel.text = l.Label + "\n" + HConfig.Grams(l.grams) + " · tostado hace " + Mathf.Max(0, age) + " día" + (age == 1 ? "" : "s");
        }

        static string GrindWord(float g) => g < 400 ? "fina" : g < 600 ? "media-fina" : g < 900 ? "media" : "gruesa";
        static string Clock(float s) => Mathf.FloorToInt(s / 60f) + ":" + Mathf.FloorToInt(s % 60f).ToString("00");

        readonly List<Vector2> pts = new List<Vector2>();

        void Redraw()
        {
            if (chart == null) return;
            var lot = CurrentLot;
            float roast = lot != null ? lot.roastLevel : 0.5f;
            float temp = Mathf.Min(recipe.temp, Boil);
            chart.Clear();
            const float MaxT = 300f, MaxEy = 28f;
            chart.Band(new Rect(0, CoffeeModel.IdealEyMin / MaxEy, 1, (CoffeeModel.IdealEyMax - CoffeeModel.IdealEyMin) / MaxEy), new Color(0.18f, 0.75f, 0.44f, 0.22f));
            pts.Clear();
            for (int i = 0; i <= 30; i++)
            {
                float t = MaxT * i / 30f;
                pts.Add(new Vector2(t / MaxT, Mathf.Clamp01(CoffeeModel.ExtractionAt(recipe, roast, temp, t) / MaxEy)));
            }
            chart.Polyline(pts, HConfig.Gold, 3f);
            float contact = recipe.steep + (recipe.inverted ? 30f : 20f);
            chart.Segment(new Vector2(contact / MaxT, 0), new Vector2(contact / MaxT, 1), new Color(1, 1, 1, 0.5f), 2f);
            float ey = CoffeeModel.ExtractionAt(recipe, roast, temp, contact);
            float bev = Mathf.Max(1f, recipe.water - 2f * recipe.dose) + recipe.dilute;
            float tds = recipe.dose * ey / 100f / bev * 100f + (recipe.metal ? 0.04f : 0f);
            string ratio = "Proporción 1:" + (recipe.water / Mathf.Max(1f, recipe.dose)).ToString("0.0", Inv);
            if (Refractometer)
                gauge.text = ratio + " · contacto " + Clock(contact) + "\nExtracción estimada: " + ey.ToString("0.0", Inv) + " % (ideal 18 – 22)\nTDS estimado: " + tds.ToString("0.00", Inv) + " % (ideal 1,15 – 1,55)";
            else
                gauge.text = ratio + " · contacto " + Clock(contact) + "\nExtracción: " + (ey < 18f ? "baja (sabrá ácida)" : ey > 22f ? "alta (sabrá amarga)" : "en la zona dulce") +
                             " · Fuerza: " + (tds < 1.15f ? "suave" : tds > 1.55f ? "intensa" : "equilibrada") + "\n(Con un refractómetro verías los números exactos.)";
            if (recipe.temp > Boil + 0.5f) gauge.text += "\nOjo: el agua no pasa de " + Boil.ToString("0", Inv) + " °C a esta altura.";
        }

        // ============================ PREPARACIÓN ============================

        void StartBrew()
        {
            var lot = CurrentLot;
            if (lot == null || lot.grams < recipe.dose) return;
            used = recipe;
            if (!S.gear.Contains("bascula")) used.dose = Mathf.Max(8f, used.dose + (float)(G.rng.NextDouble() * 2 - 1));
            if (!S.gear.Contains("hervidor_cuello")) used.temp = used.temp + (float)(G.rng.NextDouble() * 6 - 3);
            phase = Phase.Brewing;
            brewT = 0f;
            UIKit.Clear(root);
            var c = HUI.Window(root, "Preparando...", new Vector2(640, 260), null);
            stepText = UIKit.Label(c, Steps[0], 24, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(600, 40));
            stepBar = UIKit.Bar(c, "Progreso", new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(520, 14), HConfig.Gold, out _);
            stepBar.fillAmount = 0f;
            SFX.Play("reload", 0.5f);
        }

        public override void Tick(float dt)
        {
            if (phase != Phase.Brewing) return;
            brewT += dt;
            const float Dur = 2.4f;
            int k = Mathf.Min(Steps.Length - 1, Mathf.FloorToInt(brewT / Dur * Steps.Length));
            if (stepText != null) stepText.text = Steps[k];
            if (stepBar != null) stepBar.fillAmount = Mathf.Clamp01(brewT / Dur);
            if (brewT >= Dur) FinishBrew();
        }

        void FinishBrew()
        {
            var lot = CurrentLot;
            result = CoffeeModel.Brew(used, lot, G.Altitude, S.day);
            if (S.gear.Contains("molino_manual")) result.score = Mathf.Min(100f, result.score + 1.5f);
            if (lot != virtualLot) lot.grams = Mathf.Max(0f, lot.grams - used.dose);
            G.AddCups(1);
            S.brews.Add(new BrewRecord
            {
                lot = lot.id, dose = used.dose, water = used.water, temp = result.waterTemp, grind = used.grind, steep = used.steep, stirs = used.stirs,
                dilute = used.dilute, inverted = used.inverted, metal = used.metal, ey = result.ey, tds = result.tds, score = result.score, day = S.day,
            });
            while (S.brews.Count > 20) S.brews.RemoveAt(0);
            S.bestBrew = Mathf.Max(S.bestBrew, result.score);
            published = sold = false;
            phase = Phase.Result;
            SFX.Play(result.score >= 84f ? "capture" : "pickup");
            G.Emit(HEvents.Brew, "", result.score);
            G.Save();
            Rebuild();
        }

        // ============================ RESULTADO ============================

        void BuildResult(RectTransform c)
        {
            var lot = CurrentLot;
            var big = HUI.Card(c, new Vector2(0, 1), Vector2.zero, new Vector2(560, 150), result.score >= 84f);
            UIKit.Label(big.transform, result.score.ToString("0.0", Inv), 64, result.score >= 80f ? UIKit.Yellow : UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(20, 12), new Vector2(220, 80));
            UIKit.Label(big.transform, "puntos", 16, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(24, -38), new Vector2(200, 24), FontStyle.Normal);
            UIKit.Label(big.transform, result.verdict, 22, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(230, 16), new Vector2(320, 60));
            UIKit.Label(big.transform, lot != null ? lot.Label : "", 14, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 0.5f), new Vector2(230, -26), new Vector2(320, 44), FontStyle.Normal);

            string nums = Refractometer
                ? "Extracción: " + result.ey.ToString("0.0", Inv) + " % · TDS: " + result.tds.ToString("0.00", Inv) + " % · bebida " + Mathf.RoundToInt(result.beverage) + " g"
                : "Extracción ≈ " + Mathf.Round(result.ey).ToString("0", Inv) + " % · TDS ≈ " + result.tds.ToString("0.0", Inv) + " % (sin refractómetro, a ojo)";
            string recipeTxt = used.dose.ToString("0.0", Inv) + " g · " + used.water.ToString("0", Inv) + " g de agua a " + result.waterTemp.ToString("0", Inv) + " °C · molienda " + GrindWord(used.grind) +
                               " · " + Clock(used.steep) + " · " + used.stirs.ToString("0", Inv) + " revueltas" + (used.dilute > 0 ? " · +" + used.dilute.ToString("0", Inv) + " g de agua" : "") +
                               (used.inverted ? " · invertida" : "") + (used.metal ? " · filtro metálico" : "");
            HUI.Paragraph(c, "En la taza: " + result.taste + "\n\n" + nums + "\n" + recipeTxt, new Vector2(0, 1), new Vector2(0, -160), new Vector2(560, 200), 16);

            // Carta de control: EY (x) contra TDS (y) con la caja ideal
            UIKit.Label(c, "Carta de control de la preparación", 17, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(1, 1), Vector2.zero, new Vector2(560, 26));
            var cc = new HUI.LineChart(c, new Vector2(1, 1), new Vector2(0, -30), new Vector2(560, 300));
            const float x0 = 14f, x1 = 26f, y0 = 0.9f, y1 = 1.9f;
            cc.Band(new Rect((CoffeeModel.IdealEyMin - x0) / (x1 - x0), (CoffeeModel.IdealTdsMin - y0) / (y1 - y0), (CoffeeModel.IdealEyMax - CoffeeModel.IdealEyMin) / (x1 - x0), (CoffeeModel.IdealTdsMax - CoffeeModel.IdealTdsMin) / (y1 - y0)), new Color(0.18f, 0.75f, 0.44f, 0.3f));
            Vector2 pt = new Vector2(Mathf.Clamp01((result.ey - x0) / (x1 - x0)), Mathf.Clamp01((result.tds - y0) / (y1 - y0)));
            cc.Segment(pt + new Vector2(-0.025f, 0), pt + new Vector2(0.025f, 0), HConfig.Gold, 10f);
            UIKit.Label(cc.area, "subextraído", 13, HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(6, 4), new Vector2(150, 18), FontStyle.Normal);
            UIKit.Label(cc.area, "sobreextraído", 13, HUI.Muted, TextAnchor.LowerRight, new Vector2(1, 0), new Vector2(-6, 4), new Vector2(150, 18), FontStyle.Normal);
            UIKit.Label(cc.area, "fuerte", 13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(6, -4), new Vector2(150, 18), FontStyle.Normal);
            UIKit.Label(cc.area, "débil", 13, HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(6, 22), new Vector2(150, 18), FontStyle.Normal);

            // Botones según el modo
            if (Competition)
            {
                UIKit.Button(c, "Entregar a los jueces", new Vector2(1, 0), Vector2.zero, new Vector2(560, 60), UIKit.Yellow, UIKit.Ink, () => G.CompetitionBrew(result.score), 24);
                return;
            }
            if (Reto)
            {
                float retoScore = Mathf.Clamp01((result.score - 70f) / 15f) * 100f;
                HUI.Paragraph(c, "Meta del mentor: una AeroPress de 82 puntos o más con el café de la finca. Tu calificación del reto: " + Mathf.RoundToInt(retoScore) + " / 100.",
                    new Vector2(1, 0), new Vector2(0, 70), new Vector2(560, 50), 15, retoScore >= 60f ? UIKit.Mint : UIKit.Orange);
                UIKit.Button(c, "Otra taza", new Vector2(1, 0), new Vector2(-290, 0), new Vector2(270, 56), new Color(1, 1, 1, 0.12f), UIKit.Text, Again, 20);
                string region = retoRegion;
                UIKit.Button(c, "Entregar al mentor", new Vector2(1, 0), Vector2.zero, new Vector2(270, 56), UIKit.Yellow, UIKit.Ink, () => { Close(); G.RetoDone(region, retoScore); }, 20);
                return;
            }
            var pub = UIKit.Button(c, published ? "Publicado" : "Publicar en " + HConfig.Channel, new Vector2(1, 0), new Vector2(-380, 0), new Vector2(180, 56), new Color(0.18f, 0.75f, 0.44f, 0.8f), Color.white, Publish, 15);
            pub.interactable = !published;
            int price = result.score >= 80f ? HConfig.SpecialtyCupPrice : HConfig.TintoPrice;
            var sell = UIKit.Button(c, sold ? "Vendida" : "Vender · " + HConfig.Money(price), new Vector2(1, 0), new Vector2(-190, 0), new Vector2(180, 56), new Color(1, 1, 1, 0.14f), UIKit.Text, () =>
            {
                if (sold) return;
                sold = true;
                G.AddMoney(price, "taza vendida");
                Rebuild();
            }, 15);
            sell.interactable = !sold;
            UIKit.Button(c, "Otra taza", new Vector2(1, 0), Vector2.zero, new Vector2(180, 56), UIKit.Yellow, UIKit.Ink, Again, 18);
        }

        void Publish()
        {
            if (published) return;
            published = true;
            int gained = CoffeeModel.FollowersFor(result.score, S.followers, G.rng);
            if (S.gear.Contains("camara")) gained = Mathf.RoundToInt(gained * 1.3f);
            G.AddFollowers(gained);
            G.Toast("+" + gained + " seguidores en " + HConfig.Channel);
            G.Emit(HEvents.Publish, "", gained);
            G.Save();
            Rebuild();
        }

        void Again()
        {
            phase = Phase.Recipe;
            G.AdvanceHours(HConfig.HoursPerBrew);
            Rebuild();
        }
    }
}
