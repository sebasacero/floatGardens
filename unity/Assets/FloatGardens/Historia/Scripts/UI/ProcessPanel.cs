// Beneficio: de la cereza al café verde. Despulpado, fermentación, lavado, secado y trilla.
// También es el reto de Don Aurelio (lavado clásico en Cundinamarca) y de Don Hernando (notas frutales en el Huila).
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens.Historia
{
    public class ProcessPanel : HPanel
    {
        public string retoRegion = ""; // si no está vacío, el panel funciona como reto del mentor de esa región

        public override float HoursCost => 0.5f;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        bool Reto => !string.IsNullOrEmpty(retoRegion);

        List<Lot> lots = new List<Lot>();
        int lotIndex;
        Lot sample;
        string method = "lavado";
        float ferment = 14f, dryDays = 10f;
        Text preview;
        bool done;
        CoffeeModel.ProcessResult last;
        float retoScore;

        Lot Current => Reto ? sample : lots.Count > 0 ? lots[Mathf.Clamp(lotIndex, 0, lots.Count - 1)] : null;
        float DryFactor => S.gear.Contains("marquesina") && !Reto ? 1.3f : 1f;

        protected override void Build()
        {
            if (Reto && sample == null)
            {
                var r = Catalog.Region(retoRegion);
                var v = r != null ? Catalog.Variety(r.giftVariety) : null;
                sample = new Lot { id = "muestra", variety = v != null ? v.id : "caturra", origin = retoRegion, stage = "cereza", grams = 2000f, potential = v != null ? v.cupPotential : 84f, ripePct = 0.95f, notes = v != null ? v.notes : "" };
                if (retoRegion == "huila") method = "natural";
            }
            lots = S.LotsAt("cereza");
            var c = HUI.Window(root, Reto ? "Reto de beneficio" : "Beneficio", new Vector2(1180, 640), Close,
                Reto ? Goal() : "De la cereza al café verde: despulpar, fermentar, lavar, secar y trillar");
            if (done) { BuildResult(c); return; }

            var lot = Current;
            // Izquierda: la explicación del proceso
            HUI.Paragraph(c,
                "1. Despulpado: se le quita la cáscara y la pulpa a la cereza el mismo día de la cosecha. Queda el grano cubierto de mucílago, la 'miel'.\n" +
                "2. Fermentación: los microorganismos descomponen el mucílago. Muy corta: no se suelta. Muy larga: sabor a vinagre.\n" +
                "3. Lavado: se lava el mucílago con agua (en el lavado). En el honey se seca con parte de la miel y en el natural se seca la cereza entera.\n" +
                "4. Secado: hasta 10 – 12 % de humedad. Húmedo, se enmohece; sobresecado, se quiebra.\n" +
                "5. Trilla: se quita el pergamino y queda la almendra: el café verde. De 5 kg de cereza sale cerca de 1 kg de pergamino seco.",
                new Vector2(0, 1), Vector2.zero, new Vector2(500, 330), 15);
            HUI.Paragraph(c, "En la bodega fría de Bogotá la fermentación va más despacio que en una finca templada: los microorganismos trabajan menos con frío.",
                new Vector2(0, 0), Vector2.zero, new Vector2(500, 60), 14, HUI.Muted, TextAnchor.LowerLeft);

            // Derecha: controles
            var card = HUI.Card(c, new Vector2(1, 1), Vector2.zero, new Vector2(610, 64));
            string label = lot == null ? "No hay cerezas para procesar. Cosecha cerezas maduras en la huerta flotante." :
                           (Reto ? "Muestra del mentor: " : "") + lot.Label + " · " + HConfig.Grams(lot.grams) + " de cereza · " + Mathf.RoundToInt(lot.ripePct * 100) + " % maduras";
            UIKit.Label(card.transform, label, 15, lot == null ? UIKit.Orange : UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(lots.Count > 1 && !Reto ? 480 : 590, 56), FontStyle.Normal);
            if (!Reto && lots.Count > 1)
            {
                UIKit.Button(card.transform, "<", new Vector2(1, 0.5f), new Vector2(-62, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + lots.Count - 1) % lots.Count; Rebuild(); }, 22);
                UIKit.Button(card.transform, ">", new Vector2(1, 0.5f), new Vector2(-12, 0), new Vector2(44, 44), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { lotIndex = (lotIndex + 1) % lots.Count; Rebuild(); }, 22);
            }
            if (lot == null) return;

            // Método
            bool anaerobicOk = Reto || S.gear.Contains("tanque_anaerobico");
            float bx = 0;
            foreach (var m in CoffeeModel.Methods)
            {
                string id = m;
                bool locked = m == "anaerobico" && !anaerobicOk;
                bool on = method == m;
                var b = UIKit.Button(c, CoffeeModel.MethodName(m) + (locked ? " (tanque)" : ""), new Vector2(1, 1), new Vector2(-(610 - 152 * 4) - (3 - bx) * 152, -76), new Vector2(146, 46),
                    on ? UIKit.Yellow : new Color(1, 1, 1, 0.1f), on ? UIKit.Ink : UIKit.Text, () =>
                    {
                        if (locked) { G.Toast("El proceso anaeróbico necesita el tanque sellado de la tienda."); return; }
                        method = id;
                        var ideal = CoffeeModel.IdealFerment(id);
                        ferment = Mathf.Round((ideal.x + ideal.y) / 2f / 2f) * 2f;
                        Rebuild();
                    }, 15);
                b.interactable = !locked;
                bx++;
            }
            var ide = CoffeeModel.IdealFerment(method);
            HUI.Stepper(c, "Fermentación (ideal " + ide.x.ToString("0", Inv) + " – " + ide.y.ToString("0", Inv) + " h)", new Vector2(1, 1), new Vector2(0, -136), 610,
                () => ferment, v => ferment = v, 2f, 0f, 150f, v => v.ToString("0", Inv) + " h", UpdatePreview);
            HUI.Stepper(c, "Días de secado" + (DryFactor > 1f ? " (marquesina: +30 %)" : ""), new Vector2(1, 1), new Vector2(0, -188), 610,
                () => dryDays, v => dryDays = v, 1f, 3f, 30f, v => v.ToString("0", Inv) + " días", UpdatePreview);
            preview = HUI.Paragraph(c, "", new Vector2(1, 1), new Vector2(0, -246), new Vector2(610, 150), 16);
            UpdatePreview();
            UIKit.Button(c, Reto ? "Procesar la muestra" : "Procesar el lote", new Vector2(1, 0), Vector2.zero, new Vector2(610, 60), UIKit.Yellow, UIKit.Ink, DoProcess, 22);
        }

        string Goal()
        {
            if (retoRegion == "huila") return "Meta de Don Hernando: notas frutales sin defectos (natural o anaeróbico en su punto, secado a 10 – 12 %)";
            return "Meta de Don Aurelio: un lavado clásico perfecto (fermentación en su punto y secado a 10 – 12 %)";
        }

        void UpdatePreview()
        {
            if (preview == null) return;
            float moisture = CoffeeModel.MoistureAfter(method, dryDays * DryFactor);
            var ide = CoffeeModel.IdealFerment(method);
            string f = ferment < ide.x ? "corta: el mucílago no se soltará del todo" : ferment > ide.y ? (ferment - ide.y > 12 ? "¡demasiado larga: riesgo de vinagre!" : "un poco larga: notas alcohólicas") : "en su punto";
            string h = moisture > 12.5f ? "húmedo: riesgo de moho" : moisture < 9.5f ? "sobresecado: se quiebra en la trilla" : "perfecto";
            preview.text = CoffeeModel.MethodName(method) + ": " + MethodNote(method) +
                           "\nFermentación " + f + "." +
                           "\nHumedad final estimada: " + moisture.ToString("0.0", Inv) + " % (" + h + ")." +
                           (Reto ? "" : "\nEl pergamino estará seco y trillado en " + GameDays() + " día" + (GameDays() == 1 ? "" : "s") + " de juego.");
        }

        static string MethodNote(string m)
        {
            switch (m)
            {
                case "honey": return "se seca con parte del mucílago; taza dulce, a miel y panela.";
                case "natural": return "se seca la cereza entera; frutos rojos y vino, pero exige cerezas maduras y buen secado.";
                case "anaerobico": return "fermenta en tanque sellado, sin oxígeno; sabores intensos, más riesgo.";
                default: return "el sello de Colombia: acidez brillante y taza limpia.";
            }
        }

        int GameDays() => Mathf.Max(1, Mathf.CeilToInt(dryDays / 5f));

        void DoProcess()
        {
            var lot = Current;
            if (lot == null) return;
            last = CoffeeModel.Process(lot, method, ferment, dryDays * DryFactor);
            if (Reto)
            {
                bool methodOk = retoRegion == "huila" ? method == "natural" || method == "anaerobico" : method == "lavado";
                retoScore = Mathf.Clamp((last.potential - 78f) * 9f, 0f, 100f);
                if (!methodOk) retoScore = Mathf.Min(retoScore, 40f);
                if (last.moisture > 12.5f || last.moisture < 9.5f) retoScore = Mathf.Min(retoScore, 55f);
            }
            else
            {
                lot.process = method;
                lot.fermentHours = ferment;
                lot.moisture = last.moisture;
                lot.potential = last.potential;
                lot.notes = last.notes;
                lot.grams = last.greenGrams;
                lot.stage = "secando";
                lot.readyDay = S.day + GameDays();
                G.Emit(HEvents.Process, method, last.potential);
                G.Save();
            }
            done = true;
            SFX.Play(last.potential >= 84f ? "capture" : "pickup");
            Rebuild();
        }

        void BuildResult(RectTransform c)
        {
            var big = HUI.Card(c, new Vector2(0.5f, 1), Vector2.zero, new Vector2(900, 120), last.potential >= 84f);
            UIKit.Label(big.transform, last.potential.ToString("0.0", Inv), 56, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(200, 80));
            UIKit.Label(big.transform, "puntos de potencial en taza", 15, HUI.Muted, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(26, -40), new Vector2(260, 22), FontStyle.Normal);
            UIKit.Label(big.transform, CoffeeModel.MethodName(method) + " · " + ferment.ToString("0", Inv) + " h de fermentación · " + last.moisture.ToString("0.0", Inv) + " % de humedad",
                18, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(300, 18), new Vector2(580, 30));
            UIKit.Label(big.transform, "Notas: " + last.notes, 15, UIKit.Mint, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(300, -18), new Vector2(580, 40), FontStyle.Normal);
            HUI.Paragraph(c, last.verdict, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(900, 90), 17);
            if (Reto)
            {
                HUI.Paragraph(c, Goal() + "\nCalificación del reto: " + Mathf.RoundToInt(retoScore) + " / 100" + (retoScore >= 60 ? " · ¡superado!" : " · inténtalo otra vez"),
                    new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(900, 70), 17, retoScore >= 60 ? UIKit.Mint : UIKit.Orange);
                UIKit.Button(c, "Intentar otra vez", new Vector2(0.5f, 0), new Vector2(-230, 0), new Vector2(420, 60), new Color(1, 1, 1, 0.12f), UIKit.Text, () => { done = false; Rebuild(); }, 20);
                string region = retoRegion;
                float score = retoScore;
                UIKit.Button(c, "Entregar al mentor", new Vector2(0.5f, 0), new Vector2(230, 0), new Vector2(420, 60), UIKit.Yellow, UIKit.Ink, () => { Close(); G.RetoDone(region, score); }, 22);
                return;
            }
            HUI.Paragraph(c, "El lote quedó en las camas de secado: " + HConfig.Grams(last.greenGrams) + " de café verde estarán listos el día " + (S.day + GameDays()) +
                             ". Mientras tanto puedes dormir, viajar o preparar café.", new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(900, 60), 17, UIKit.Mint);
            UIKit.Button(c, lots.Count > 1 ? "Procesar otro lote" : "Listo", new Vector2(0.5f, 0), Vector2.zero, new Vector2(420, 60), UIKit.Yellow, UIKit.Ink, () =>
            {
                done = false;
                if (S.LotsAt("cereza").Count == 0) Close(); else Rebuild();
            }, 22);
        }
    }
}
