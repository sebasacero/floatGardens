// Tostadora de tambor: curva en vivo de la temperatura del grano (BT), primer y segundo crack, porcentaje de desarrollo.
// Con las sondas de la tienda también se ven la temperatura del aire (ET) y la tasa de aumento (RoR).
// También es el reto de Doña Lucía en el Cauca.
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class RoastPanel : HPanel
    {
        public string retoRegion = ""; // si no está vacío, el panel funciona como reto del mentor de esa región

        public override float HoursCost => 0.5f;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        const float Speed = 6f, MaxTime = 900f, MaxTemp = 250f;
        bool Reto => !string.IsNullOrEmpty(retoRegion);
        bool Pro => S.gear.Contains("tostadora_pro");

        enum Phase { Setup, Roasting, Result }
        Phase phase = Phase.Setup;
        List<Lot> lots = new List<Lot>();
        int lotIndex;
        float batch = 300f;
        Lot sample;
        CoffeeModel.RoastSim sim;
        CoffeeModel.RoastResult result;
        HUI.LineChart chart;
        Text readout, heatText, phaseText;
        float crackTimer;
        int lastPoints;
        readonly List<Vector2> bt = new List<Vector2>(), et = new List<Vector2>(), ror = new List<Vector2>();

        Lot Current => Reto ? sample : lots.Count > 0 ? lots[Mathf.Clamp(lotIndex, 0, lots.Count - 1)] : null;

        protected override void Build()
        {
            if (Reto && sample == null)
            {
                var r = Catalog.Region(retoRegion);
                var v = r != null ? Catalog.Variety(r.giftVariety) : null;
                sample = new Lot { id = "muestra", variety = v != null ? v.id : "caturra", origin = retoRegion, stage = "verde", grams = 300f, potential = 86f, process = "lavado", notes = v != null ? v.notes : "" };
                batch = 300f;
            }
            lots = new List<Lot>();
            foreach (var l in S.LotsAt("verde")) if (l.grams >= 100f) lots.Add(l);
            var c = HUI.Window(root, Reto ? "Reto de tostión" : "Tostión", new Vector2(1180, 640), phase == Phase.Roasting ? (System.Action)null : Close,
                Reto ? "Meta de Doña Lucía: un tueste medio con 18 – 22 % de desarrollo, sin quemar" : "Secado, Maillard y desarrollo: escucha el primer crack");
            if (phase == Phase.Result) { BuildResult(c); return; }
            if (phase == Phase.Roasting) { BuildRoasting(c); return; }
            BuildSetup(c);
        }

        void BuildSetup(RectTransform c)
        {
            var lot = Current;
            var card = HUI.Card(c, new Vector2(0, 1), Vector2.zero, new Vector2(560, 70));
            string label = lot == null ? "No tienes café verde (al menos 100 g). Procesa tus cerezas en el beneficio o compra café verde en la tienda." :
                           (Reto ? "Muestra de Doña Lucía: " : "") + lot.Label + " · " + HConfig.Grams(lot.grams) + " de café verde";
            UIKit.Label(card.transform, label, 15, lot == null ? UIKit.Orange : UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(lots.Count > 1 && !Reto ? 440 : 540, 62), FontStyle.Normal);
            if (!Reto && lots.Count > 1)
            {
                UIKit.Button(card.transform, "<", new Vector2(1, 0.5f), new Vector2(-62, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + lots.Count - 1) % lots.Count; Rebuild(); }, 22);
                UIKit.Button(card.transform, ">", new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + 1) % lots.Count; Rebuild(); }, 22);
            }
            HUI.Paragraph(c,
                "Fases del tueste\n" +
                "• Secado: el grano pierde agua y pasa de verde a amarillo (hacia 150 °C).\n" +
                "• Maillard: se dora, huele a pan y caramelo. Termina con el primer crack, un chasquido como de crispetas, cerca de 196 °C.\n" +
                "• Desarrollo: desde el primer crack hasta que descargas. En especialidad se busca que sea del 15 al 25 % del tiempo total.\n" +
                "Hacia 224 °C llega el segundo crack y los aceites salen a la superficie: tueste oscuro.\n\n" +
                "Consejo: arranca con buen fuego y bájalo poco a poco para que la curva suba cada vez más despacio. Si subes el fuego después del crack, la curva rebota (flick).",
                new Vector2(0, 1), new Vector2(0, -84), new Vector2(560, 360), 15);
            if (lot == null) return;
            if (!Reto)
            {
                float max = Mathf.Min(1000f, Mathf.Floor(lot.grams / 50f) * 50f);
                batch = Mathf.Clamp(batch, 100f, Mathf.Max(100f, max));
                HUI.Stepper(c, "Tamaño de la tanda", new Vector2(1, 1), Vector2.zero, 560, () => batch, v => batch = v, 50f, 100f, Mathf.Max(100f, max), v => v.ToString("0", Inv) + " g");
                HUI.Paragraph(c, "Se pierde ~15 % del peso en el tueste: de " + batch.ToString("0", Inv) + " g de verde salen unos " + (batch * 0.85f).ToString("0", Inv) + " g de tostado.",
                    new Vector2(1, 1), new Vector2(0, -56), new Vector2(560, 50), 15, HUI.Muted);
            }
            UIKit.Button(c, "Cargar el tambor", new Vector2(1, 0), Vector2.zero, new Vector2(560, 64), UIKit.Yellow, UIKit.Ink, StartRoast, 24);
        }

        void StartRoast()
        {
            if (Current == null) return;
            sim = new CoffeeModel.RoastSim { heat = 0.65f };
            bt.Clear(); et.Clear(); ror.Clear();
            lastPoints = 0;
            phase = Phase.Roasting;
            SFX.Play("reload", 0.6f);
            Rebuild();
        }

        void BuildRoasting(RectTransform c)
        {
            chart = new HUI.LineChart(c, new Vector2(0, 1), Vector2.zero, new Vector2(780, 470));
            // guías: crack y zona de desarrollo
            chart.Band(new Rect(0, CoffeeModel.YellowingC / MaxTemp - 0.002f, 1, 0.004f), new Color(1, 1, 0.4f, 0.35f));
            chart.Band(new Rect(0, CoffeeModel.FirstCrackC / MaxTemp - 0.002f, 1, 0.004f), new Color(1, 0.6f, 0.2f, 0.5f));
            chart.Band(new Rect(0, CoffeeModel.SecondCrackC / MaxTemp - 0.002f, 1, 0.004f), new Color(1, 0.3f, 0.2f, 0.5f));
            UIKit.Label(chart.area, "150 °C fin del secado", 12, HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(6, CoffeeModel.YellowingC / MaxTemp * 470 + 2), new Vector2(240, 16), FontStyle.Normal);
            UIKit.Label(chart.area, "196 °C primer crack", 12, HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(6, CoffeeModel.FirstCrackC / MaxTemp * 470 + 2), new Vector2(240, 16), FontStyle.Normal);
            UIKit.Label(chart.area, "224 °C segundo crack", 12, HUI.Muted, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(6, CoffeeModel.SecondCrackC / MaxTemp * 470 + 2), new Vector2(240, 16), FontStyle.Normal);
            UIKit.Label(c, Pro ? "Grano (BT) · aire (ET) · RoR ×10" : "Temperatura del grano (BT)", 14, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(0, 104), new Vector2(780, 20), FontStyle.Normal);

            // Controles a la derecha
            var panel = HUI.Card(c, new Vector2(1, 1), Vector2.zero, new Vector2(350, 470));
            readout = UIKit.Label(panel.transform, "", 20, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(16, -14), new Vector2(320, 200), FontStyle.Bold);
            phaseText = UIKit.Label(panel.transform, "", 16, UIKit.Yellow, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(16, -214), new Vector2(320, 60), FontStyle.Normal);
            UIKit.Label(panel.transform, "Fuego", 16, HUI.Muted, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 176), new Vector2(320, 22), FontStyle.Normal);
            heatText = UIKit.Label(panel.transform, "", 34, UIKit.Orange, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0, 130), new Vector2(160, 50));
            UIKit.Button(panel.transform, "–", new Vector2(0.5f, 0), new Vector2(-115, 130), new Vector2(80, 64), new Color(1, 1, 1, 0.14f), UIKit.Text, () => SetHeat(-0.05f), 34);
            UIKit.Button(panel.transform, "+", new Vector2(0.5f, 0), new Vector2(115, 130), new Vector2(80, 64), new Color(1, 1, 1, 0.14f), UIKit.Text, () => SetHeat(0.05f), 34);
            UIKit.Button(panel.transform, "Descargar", new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(320, 70), UIKit.Yellow, UIKit.Ink, Drop, 26);
            UIKit.Label(c, "Reloj de tostión a ×" + Speed.ToString("0", Inv) + " de velocidad", 13, HUI.Muted, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(0, 120), new Vector2(350, 20), FontStyle.Normal);
            RefreshRoast(true);
        }

        void SetHeat(float d)
        {
            if (sim == null) return;
            sim.heat = Mathf.Clamp01(Mathf.Round((sim.heat + d) * 20f) / 20f);
            SFX.Play("pickup", 0.4f);
            RefreshRoast(false);
        }

        public override void Tick(float dt)
        {
            if (phase != Phase.Roasting || sim == null) return;
            float step = dt * Speed;
            // sub-pasos de 1 s de tostión para que la simulación sea estable
            while (step > 0f)
            {
                float s = Mathf.Min(1f, step);
                bool before = sim.firstCrack >= 0f;
                sim.Step(s);
                if (!before && sim.firstCrack >= 0f) { SFX.Play("head", 0.8f); G.Toast("¡Primer crack! Empieza el desarrollo."); }
                step -= s;
            }
            // chasquidos durante el crack
            bool cracking = (sim.firstCrack >= 0f && sim.time - sim.firstCrack < 90f) || (sim.secondCrack >= 0f && sim.time - sim.secondCrack < 60f);
            if (cracking)
            {
                crackTimer -= dt;
                if (crackTimer <= 0f) { SFX.Play("hit", 0.35f, 0.3f); crackTimer = 0.08f + Random.value * 0.35f; }
            }
            if (sim.time >= MaxTime + 120f) Drop(); // el tambor no espera para siempre
            RefreshRoast(false);
        }

        void RefreshRoast(bool full)
        {
            if (sim == null) return;
            if (heatText != null) heatText.text = Mathf.RoundToInt(sim.heat * 100) + " %";
            if (readout != null)
            {
                string t = Mathf.FloorToInt(sim.time / 60f) + ":" + Mathf.FloorToInt(sim.time % 60f).ToString("00");
                readout.text = "Tiempo  " + t + "\nGrano   " + sim.bt.ToString("0", Inv) + " °C" +
                               (Pro ? "\nAire    " + sim.et.ToString("0", Inv) + " °C\nRoR     " + sim.ror.ToString("0.0", Inv) + " °C/min" : "") +
                               (sim.firstCrack >= 0f ? "\nDesarrollo " + sim.DevRatio.ToString("0", Inv) + " %" : "");
            }
            if (phaseText != null)
                phaseText.text = sim.secondCrack >= 0f ? "Segundo crack: ¡tueste oscuro! Descarga ya." :
                                 sim.firstCrack >= 0f ? "Desarrollo: decide cuándo descargar." :
                                 sim.yellowTime >= 0f ? "Maillard: el grano se dora y huele a pan." : "Secado: el grano pierde agua.";
            if (chart == null) return;
            int n = sim.curve.Count;
            if (!full && n == lastPoints) return;
            for (int i = lastPoints; i < n; i++)
            {
                var p = sim.curve[i];
                bt.Add(new Vector2(Mathf.Clamp01(p.x / MaxTime), Mathf.Clamp01(p.y / MaxTemp)));
                ror.Add(new Vector2(Mathf.Clamp01(p.x / MaxTime), Mathf.Clamp01(p.z * 10f / MaxTemp)));
                et.Add(new Vector2(Mathf.Clamp01(p.x / MaxTime), Mathf.Clamp01(sim.et / MaxTemp)));
            }
            // solo se dibujan los tramos nuevos (la gráfica reutiliza los anteriores)
            for (int i = Mathf.Max(1, lastPoints); i < n; i++)
            {
                chart.Segment(bt[i - 1], bt[i], HConfig.Gold, 3f);
                if (Pro)
                {
                    chart.Segment(et[i - 1], et[i], new Color(0.4f, 0.7f, 1f, 0.8f), 2f);
                    chart.Segment(ror[i - 1], ror[i], new Color(0.6f, 1f, 0.6f, 0.8f), 2f);
                }
            }
            lastPoints = n;
        }

        void Drop()
        {
            if (sim == null || phase != Phase.Roasting) return;
            sim.Drop();
            result = CoffeeModel.Evaluate(sim);
            SFX.Play("cash", 0.6f);
            phase = Phase.Result;
            if (!Reto)
            {
                var green = Current;
                if (green != null)
                {
                    float grams = Mathf.Min(batch, green.grams);
                    var roasted = S.NewLot(green.variety, green.origin, "verde", grams, green.potential);
                    roasted.process = green.process;
                    roasted.notes = green.notes;
                    roasted.ripePct = green.ripePct;
                    roasted.moisture = green.moisture;
                    roasted.fermentHours = green.fermentHours;
                    green.grams -= grams;
                    CoffeeModel.ApplyRoast(roasted, result, S.day);
                }
                G.Emit(HEvents.Roast, result.name, result.score);
                G.Save();
            }
            Rebuild();
        }

        void BuildResult(RectTransform c)
        {
            float retoScore = Mathf.Clamp(result.score - (result.name != "medio" ? 20f : 0f), 0f, 100f);
            var big = HUI.Card(c, new Vector2(0.5f, 1), Vector2.zero, new Vector2(900, 120), result.score >= 80f);
            UIKit.Label(big.transform, result.score.ToString("0", Inv), 56, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(160, 80));
            UIKit.Label(big.transform, "puntos de tostión", 15, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(26, -40), new Vector2(200, 22), FontStyle.Normal);
            string t = Mathf.FloorToInt(result.totalTime / 60f) + ":" + Mathf.FloorToInt(result.totalTime % 60f).ToString("00");
            UIKit.Label(big.transform, "Tueste " + result.name + " · descarga a " + result.dropTemp.ToString("0", Inv) + " °C · " + t + " · desarrollo " + result.devRatio.ToString("0", Inv) + " %",
                18, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(240, 0), new Vector2(640, 60));
            HUI.Paragraph(c, result.verdict, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(900, 120), 17);
            if (Reto)
            {
                HUI.Paragraph(c, "Calificación del reto: " + Mathf.RoundToInt(retoScore) + " / 100" + (result.name != "medio" ? " (Doña Lucía pidió un tueste medio)" : "") + (retoScore >= 60 ? " · ¡superado!" : ""),
                    new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(900, 50), 18, retoScore >= 60 ? UIKit.Mint : UIKit.Orange);
                UIKit.Button(c, "Intentar otra vez", new Vector2(0.5f, 0), new Vector2(-230, 0), new Vector2(420, 60), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { phase = Phase.Setup; sim = null; Rebuild(); }, 20);
                string region = retoRegion;
                UIKit.Button(c, "Entregar a Doña Lucía", new Vector2(0.5f, 0), new Vector2(230, 0), new Vector2(420, 60), UIKit.Yellow, UIKit.Ink, () => { Close(); G.RetoDone(region, retoScore); }, 22);
                return;
            }
            HUI.Paragraph(c, "Tu café tostado quedó en el inventario. Déjalo reposar uno o dos días: recién tostado suelta CO2 y la extracción sale irregular.",
                new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(900, 50), 16, UIKit.Mint);
            UIKit.Button(c, "Tostar otra tanda", new Vector2(0.5f, 0), new Vector2(-230, 0), new Vector2(420, 60), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { phase = Phase.Setup; sim = null; Rebuild(); }, 20);
            UIKit.Button(c, "Listo", new Vector2(0.5f, 0), new Vector2(230, 0), new Vector2(420, 60), UIKit.Yellow, UIKit.Ink, Close, 22);
        }
    }
}
