// Huerta flotante: clima de la bodega (temperatura, luz, humedad, pH y nutrientes) y las balsas una por una:
// sembrar, cosechar, tratar la roya y la broca.
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class GardenPanel : HPanel
    {
        public override float HoursCost => HConfig.HoursPerPanel;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        Image envBar;
        Text envText;
        int choosing = -1;   // balsa en la que se está eligiendo semilla
        int confirmPull = -1;

        protected override void Build()
        {
            var c = HUI.Window(root, "Huerta flotante", new Vector2(1180, 640), Close,
                "Bogotá está a 2.600 m y unos 14 °C: aquí el café crece adentro, en balsas hidropónicas bajo luces LED");

            // ---- Clima ----
            UIKit.Label(c, "Clima de la bodega", 18, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.zero, new Vector2(400, 26));
            var g = S.garden;
            float y = -32;
            HUI.Stepper(c, "Temperatura", new Vector2(0, 1), new Vector2(0, y), 400, () => g.tempC, v => g.tempC = v, 0.5f, 10f, 30f, v => v.ToString("0.0", Inv) + " °C", UpdateEnv); y -= 48;
            HUI.Stepper(c, "Luz LED", new Vector2(0, 1), new Vector2(0, y), 400, () => g.lightHours, v => g.lightHours = v, 1f, 0f, 24f, v => v.ToString("0", Inv) + " h", UpdateEnv); y -= 48;
            HUI.Stepper(c, "Humedad", new Vector2(0, 1), new Vector2(0, y), 400, () => g.humidity, v => g.humidity = v, 5f, 30f, 95f, v => v.ToString("0", Inv) + " %", UpdateEnv); y -= 48;
            HUI.Stepper(c, "pH de la solución", new Vector2(0, 1), new Vector2(0, y), 400, () => g.ph, v => g.ph = v, 0.1f, 4f, 8f, v => v.ToString("0.0", Inv), UpdateEnv); y -= 48;
            HUI.Stepper(c, "Nutrientes (EC)", new Vector2(0, 1), new Vector2(0, y), 400, () => g.ec, v => g.ec = v, 0.1f, 0.2f, 3.5f, v => v.ToString("0.0", Inv) + " mS/cm", UpdateEnv); y -= 56;
            envBar = UIKit.Bar(c, "Clima", new Vector2(0, 1), new Vector2(0, y), new Vector2(400, 14), UIKit.Green, out _);
            envText = HUI.Paragraph(c, "", new Vector2(0, 1), new Vector2(0, y - 22), new Vector2(400, 90), 15);
            HUI.Paragraph(c, "Ideal: 18 – 22 °C, 10 – 14 h de luz, 60 – 80 % de humedad, pH 5,5 – 6,5 y EC 1,0 – 2,0. " +
                "Entre 17 y 19,5 °C la cereza madura más despacio y el grano sale más denso, como en la altura de las montañas. " +
                "Más de 85 % de humedad le abre la puerta a la roya.",
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(400, 110), 14, HUI.Muted, TextAnchor.LowerLeft);
            UpdateEnv();

            // ---- Balsas ----
            int cereza = S.LotsAt("cereza").Count;
            int seeds = 0;
            foreach (var st in S.seeds) seeds += st.count;
            UIKit.Label(c, "Balsas (" + g.rafts.Count + " de " + HConfig.MaxRafts + ") · " + seeds + " semillas · " + cereza + " lote" + (cereza == 1 ? "" : "s") + " de cereza para el beneficio",
                16, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(1, 1), Vector2.zero, new Vector2(700, 26));
            var list = HUI.ScrollList(c, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-710, 0), new Vector2(0, -32));
            for (int i = 0; i < g.rafts.Count; i++) RaftRow(list, i);
            if (g.rafts.Count < HConfig.MaxRafts)
            {
                var row = HUI.Row(list, 48, new Color(1, 1, 1, 0.03f));
                HUI.RowText(row, "¿Más balsas? Cómpralas en la tienda del computador.", 14, 0, new Vector2(600, 40), 15, HUI.Muted);
            }
        }

        void UpdateEnv()
        {
            float env = CoffeeModel.Environment(S.garden, out string warn);
            int active = 0;
            foreach (var p in S.garden.rafts) if (!p.Empty) active++;
            if (envBar != null)
            {
                envBar.fillAmount = env;
                envBar.color = env > 0.8f ? UIKit.Green : env > 0.5f ? UIKit.Yellow : UIKit.Red;
            }
            if (envText != null)
                envText.text = "Condiciones: " + Mathf.RoundToInt(env * 100) + " %" + (string.IsNullOrEmpty(warn) ? " · ¡clima de montaña!" : "\n" + warn) +
                               "\nEnergía diaria: " + HConfig.Money(CoffeeModel.DailyEnergyCost(S.garden, active)) + (active == 0 ? " (sin matas no se cobra)" : "");
            BodegaBuilder.RefreshPlants(S);
        }

        void RaftRow(RectTransform list, int i)
        {
            var p = S.garden.rafts[i];
            bool chooser = choosing == i;
            var row = HUI.Row(list, chooser ? 150 : 104);
            var v = Catalog.Variety(p.variety);
            string name = p.Empty ? "Balsa " + (i + 1) + " · vacía" : "Balsa " + (i + 1) + " · " + (v != null ? v.name : p.variety);
            UIKit.Label(row, name, 17, p.Empty ? HUI.Muted : UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -6), new Vector2(330, 24));
            if (!p.Empty)
            {
                string stage = CoffeeModel.StageName(p.stage) + (p.roya ? " · ROYA" : "") + (p.broca ? " · BROCA" : "") + (p.harvests > 0 ? " · " + p.harvests + " cosecha" + (p.harvests == 1 ? "" : "s") : "");
                UIKit.Label(row, stage, 15, p.roya || p.broca ? UIKit.Orange : UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -30), new Vector2(330, 20), FontStyle.Normal);
                var bar = UIKit.Bar(row, "Etapa", new Vector2(0, 1), new Vector2(12, -56), new Vector2(150, 8), HConfig.Gold, out _);
                bar.fillAmount = Mathf.Clamp01(p.progress);
                var hb = UIKit.Bar(row, "Salud", new Vector2(0, 1), new Vector2(176, -56), new Vector2(150, 8), UIKit.Green, out _);
                hb.fillAmount = p.health;
                hb.color = p.health > 0.6f ? UIKit.Green : p.health > 0.3f ? UIKit.Yellow : UIKit.Red;
                UIKit.Label(row, "etapa · salud", 12, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -70), new Vector2(320, 16), FontStyle.Normal);
                UIKit.Label(row, CoffeeModel.StageRealTime(p.stage), 13, HUI.Muted, TextAnchor.UpperLeft, new Vector2(0, 0), new Vector2(12, 4), new Vector2(380, 20), FontStyle.Italic);
            }

            // Acciones (a la derecha)
            float x = 10;
            if (p.Empty)
            {
                Act(row, ref x, chooser ? "Cancelar" : "Sembrar", UIKit.Yellow, UIKit.Ink, () => { choosing = chooser ? -1 : i; Rebuild(); });
            }
            else
            {
                if (p.stage >= PlantStage.CerezaVerde) Act(row, ref x, p.stage == PlantStage.CerezaVerde ? "Cosechar (verdes)" : "Cosechar", p.stage == PlantStage.CerezaVerde ? UIKit.Orange : UIKit.Yellow, UIKit.Ink, () => Harvest(i));
                if (p.roya) Act(row, ref x, "Tratar roya · $20.000", UIKit.Orange, UIKit.Ink, () => { if (G.Spend(20000, "tratamiento contra la roya")) { p.roya = false; G.Toast("Roya controlada. Las variedades resistentes, como Castillo, la aguantan mejor."); Rebuild(); BodegaBuilder.RefreshPlants(S); } });
                if (p.broca) Act(row, ref x, "Broca · $15.000", UIKit.Orange, UIKit.Ink, () => { if (G.Spend(15000, "trampas y Beauveria bassiana")) { p.broca = false; G.Toast("Broca controlada con trampas y el hongo Beauveria bassiana. Y recuerda el Re-Re."); Rebuild(); BodegaBuilder.RefreshPlants(S); } });
                if (confirmPull == i)
                    Act(row, ref x, "¿Seguro? Arrancar", UIKit.Red, Color.white, () => { S.garden.rafts[i] = new Plant(); confirmPull = -1; Rebuild(); BodegaBuilder.RefreshPlants(S); });
                else
                    Act(row, ref x, "Arrancar", new Color(1, 1, 1, 0.1f), UIKit.Text, () => { confirmPull = i; Rebuild(); });
            }

            if (chooser)
            {
                float sx = 12;
                int shown = 0;
                foreach (var st in S.seeds)
                {
                    if (st.count <= 0) continue;
                    var vv = Catalog.Variety(st.id);
                    if (vv == null) continue;
                    string id = st.id;
                    UIKit.Button(row, vv.name + " (" + st.count + ")", new Vector2(0, 0), new Vector2(sx, 8), new Vector2(160, 40), new Color(0.18f, 0.75f, 0.44f, 0.8f), Color.white, () => Plant(i, id), 15);
                    sx += 168;
                    if (++shown >= 4) break;
                }
                if (shown == 0) HUI.RowText(row, "No tienes semillas. Cómpralas en la tienda del computador o gánalas en los retos de los mentores.", 12, -50, new Vector2(660, 40), 15, UIKit.Orange);
            }
        }

        static void Act(RectTransform row, ref float x, string label, Color bg, Color fg, System.Action a)
        {
            float w = Mathf.Max(120, label.Length * 8.5f + 20);
            UIKit.Button(row, label, new Vector2(1, 1), new Vector2(-x, -10), new Vector2(w, 40), bg, fg, a, 15);
            x += w + 8;
        }

        void Plant(int i, string variety)
        {
            if (S.Seeds(variety) <= 0) return;
            var p = S.garden.rafts[i];
            p.variety = variety;
            p.stage = PlantStage.Semilla;
            p.progress = 0f;
            p.health = 1f;
            p.roya = p.broca = false;
            p.harvests = 0;
            S.AddSeeds(variety, -1);
            choosing = -1;
            SFX.Play("pickup");
            var v = Catalog.Variety(variety);
            G.Toast("Sembraste " + (v != null ? v.name : variety) + ". En la vida real, una semilla tarda unos dos meses en germinar.");
            G.Emit(HEvents.Plant, variety);
            G.Save();
            Rebuild();
            BodegaBuilder.RefreshPlants(S);
        }

        void Harvest(int i)
        {
            var p = S.garden.rafts[i];
            bool green = p.stage == PlantStage.CerezaVerde;
            var lot = CoffeeModel.Harvest(p, S.garden, S);
            if (lot == null) return;
            SFX.Play("cash", 0.6f);
            G.Toast("Cosechaste " + HConfig.Grams(lot.grams) + " de cereza (" + Mathf.RoundToInt(lot.ripePct * 100) + " % maduras)" +
                    (green ? ". ¡Las verdes dañan la taza!" : ". Llévala al beneficio el mismo día."));
            G.Emit(HEvents.Harvest, "", lot.grams);
            G.Save();
            Rebuild();
            BodegaBuilder.RefreshPlants(S);
        }

        public override void OnClosed() { BodegaBuilder.RefreshPlants(S); G.Save(); }
    }
}
