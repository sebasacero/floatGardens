// La ciencia del café, simplificada para jugar: crecimiento de la planta, beneficio, tostión y extracción en AeroPress.
// Los números siguen referencias reales (SCA, Cenicafé), comprimidos en el tiempo para que quepan en una partida.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class CoffeeModel
    {
        // ======================= CULTIVO =======================

        /// <summary>Fracción de growthDays que dura cada etapa antes de la primera floración.</summary>
        static readonly float[] StageShare = { 0f, 0.12f, 0.08f, 0.10f, 0.25f, 0.45f };
        public const float FloweringDays = 1f, GreenCherryDays = 3f, RipeWindowDays = 2f, OverripeDays = 2f;

        public static string StageName(PlantStage s)
        {
            switch (s)
            {
                case PlantStage.Semilla: return "Semilla germinando";
                case PlantStage.Fosforito: return "Fosforito";
                case PlantStage.Chapola: return "Chapola";
                case PlantStage.Almacigo: return "Almácigo";
                case PlantStage.Arbusto: return "Arbusto";
                case PlantStage.Floracion: return "Floración";
                case PlantStage.CerezaVerde: return "Cereza verde";
                case PlantStage.CerezaMadura: return "Cereza madura";
                case PlantStage.Sobremadura: return "Sobremadura";
                default: return "Balsa vacía";
            }
        }

        /// <summary>Lo que dura cada etapa en la vida real (para enseñar mientras se juega).</summary>
        public static string StageRealTime(PlantStage s)
        {
            switch (s)
            {
                case PlantStage.Semilla: return "En la vida real: ~2 meses para germinar.";
                case PlantStage.Fosforito: return "El tallito sale con la semilla encima, como un fósforo.";
                case PlantStage.Chapola: return "Se abren las dos primeras hojas (cotiledones): parece una mariposa.";
                case PlantStage.Almacigo: return "En la vida real pasa ~6 meses en bolsa antes de ir al campo.";
                case PlantStage.Arbusto: return "En la vida real tarda ~18 meses más hasta la primera cosecha.";
                case PlantStage.Floracion: return "Flores blancas con olor a jazmín. Se autopoliniza.";
                case PlantStage.CerezaVerde: return "De la flor a la cereza madura pasan ~7 a 8 meses.";
                case PlantStage.CerezaMadura: return "¡A cosechar! Solo las cerezas maduras dan buena taza.";
                case PlantStage.Sobremadura: return "Se están pasando: pierden calidad y atraen la broca.";
                default: return "";
            }
        }

        static float Band(float v, float idealMin, float idealMax, float zeroMin, float zeroMax)
        {
            if (v >= idealMin && v <= idealMax) return 1f;
            if (v < idealMin) return Mathf.Clamp01(Mathf.InverseLerp(zeroMin, idealMin, v));
            return Mathf.Clamp01(Mathf.InverseLerp(zeroMax, idealMax, v));
        }

        /// <summary>Qué tan bien está el clima de la bodega para el café (0 – 1) y el aviso más importante.</summary>
        public static float Environment(Garden g, out string warning)
        {
            float t = Band(g.tempC, 18f, 22f, 10f, 30f);
            float l = Band(g.lightHours, 10f, 14f, 4f, 20f);
            float h = Band(g.humidity, 60f, 80f, 30f, 98f);
            float p = Band(g.ph, 5.5f, 6.5f, 4f, 8f);
            float e = Band(g.ec, 1.0f, 2.0f, 0.2f, 3.5f);
            float min = Mathf.Min(t, Mathf.Min(l, Mathf.Min(h, Mathf.Min(p, e))));
            warning = "";
            if (min < 0.999f)
            {
                if (min == t) warning = g.tempC < 18f ? "Hace frío para el café (ideal 18 – 22 °C)." : "Hace demasiado calor (ideal 18 – 22 °C).";
                else if (min == l) warning = g.lightHours < 10f ? "Le falta luz (ideal 10 – 14 h)." : "Demasiadas horas de luz estresan la planta.";
                else if (min == h) warning = g.humidity < 60f ? "El aire está muy seco (ideal 60 – 80 %)." : "Demasiada humedad: la roya ama la humedad.";
                else if (min == p) warning = "El pH de la solución está fuera de rango (ideal 5,5 – 6,5).";
                else warning = g.ec < 1f ? "Faltan nutrientes en el agua (EC ideal 1,0 – 2,0)." : "Demasiados nutrientes: las raíces se queman.";
            }
            return (t + l + h + p + e) / 5f * Mathf.Lerp(0.5f, 1f, min);
        }

        /// <summary>Costo diario de mantener el clima: calentar una bodega bogotana a 20 °C no es gratis.</summary>
        public static int DailyEnergyCost(Garden g, int activeRafts)
        {
            float heat = Mathf.Max(0f, g.tempC - HConfig.BogotaOutdoorTemp) * 600f;
            float light = g.lightHours * 250f;
            return Mathf.RoundToInt(HConfig.EnergyCostPerDay * 0.25f + (heat + light) * Mathf.Max(1, activeRafts) / 4f);
        }

        /// <summary>Avanza un día en una balsa. Devuelve un aviso si algo importante pasó (plaga, cerezas listas...).</summary>
        public static string GrowDay(Plant p, Garden g, System.Random rng, float boost = 1f)
        {
            if (p.Empty) return null;
            var v = Catalog.Variety(p.variety);
            float env = Environment(g, out _);
            float growthDays = v != null ? v.growthDays : 10f;
            string note = null;

            // Plagas
            float rustRes = v != null ? v.rustResistance : 0.3f;
            if (!p.roya && p.stage >= PlantStage.Almacigo)
            {
                float chance = 0.035f * (1f - rustRes) * (g.humidity > 85f ? 2.2f : 1f) * (p.health < 0.5f ? 1.5f : 1f);
                if (rng.NextDouble() < chance) { p.roya = true; note = "¡Roya! Manchas naranjas en las hojas"; }
            }
            if (!p.broca && p.stage >= PlantStage.CerezaVerde)
            {
                float chance = p.stage == PlantStage.Sobremadura ? 0.18f : 0.03f;
                if (rng.NextDouble() < chance) { p.broca = true; note = "¡Broca! Un escarabajo se metió en las cerezas"; }
            }
            if (p.roya) p.health = Mathf.Max(0.05f, p.health - 0.08f);
            else p.health = Mathf.Min(1f, p.health + (env > 0.8f ? 0.05f : env > 0.5f ? 0.01f : -0.04f));

            float speed = env * Mathf.Lerp(0.4f, 1f, p.health) * boost;
            float duration;
            switch (p.stage)
            {
                case PlantStage.Floracion: duration = FloweringDays; break;
                case PlantStage.CerezaVerde: duration = GreenCherryDays; break;
                case PlantStage.CerezaMadura: duration = RipeWindowDays; speed = 1f; break;
                case PlantStage.Sobremadura: duration = OverripeDays; speed = 1f; break;
                default: duration = Mathf.Max(0.5f, StageShare[(int)p.stage] * growthDays); break;
            }
            p.progress += speed / duration;
            if (p.progress >= 1f)
            {
                p.progress = 0f;
                if (p.stage == PlantStage.Sobremadura)
                {
                    // las cerezas se cayeron: se pierde la cosecha y la planta vuelve a florecer
                    p.stage = PlantStage.Floracion;
                    p.broca = true;
                    note = "Las cerezas se cayeron al agua. Re-Re: recoger y repasar evita la broca";
                }
                else
                {
                    p.stage++;
                    if (p.stage == PlantStage.Floracion && p.harvests == 0) note = "¡Primera floración! Huele a jazmín";
                    if (p.stage == PlantStage.CerezaMadura) note = "Cerezas maduras listas para cosechar";
                }
            }
            return note;
        }

        /// <summary>Cosecha una balsa. Devuelve el lote de cereza o null si no hay frutos.</summary>
        public static Lot Harvest(Plant p, Garden g, HistoriaSave s)
        {
            if (p.stage < PlantStage.CerezaVerde) return null;
            var v = Catalog.Variety(p.variety);
            float ripe = p.stage == PlantStage.CerezaMadura ? 0.95f : p.stage == PlantStage.Sobremadura ? 0.6f : 0.25f + p.progress * 0.3f;
            float grams = 350f * (v != null ? v.yield : 1f) * Mathf.Lerp(0.4f, 1f, p.health) * (p.broca ? 0.7f : 1f);
            float potential = (v != null ? v.cupPotential : 80f) - (1f - ripe) * 12f - (1f - p.health) * 5f - (p.broca ? 3f : 0f);
            // maduración lenta en clima fresco = grano más denso (el mismo efecto de la altura en las montañas)
            if (g.tempC >= 17f && g.tempC <= 19.5f) potential += 1.5f;
            var lot = s.NewLot(p.variety, "floatgardens", "cereza", grams, potential);
            lot.ripePct = ripe;
            lot.notes = v != null ? v.notes : "";
            p.stage = PlantStage.Floracion;
            p.progress = 0f;
            p.broca = false;
            p.harvests++;
            return lot;
        }

        // ======================= BENEFICIO =======================

        public static readonly string[] Methods = { "lavado", "honey", "natural", "anaerobico" };

        public static string MethodName(string m)
        {
            switch (m)
            {
                case "lavado": return "Lavado";
                case "honey": return "Honey (enmielado)";
                case "natural": return "Natural";
                case "anaerobico": return "Anaeróbico";
                default: return m;
            }
        }

        /// <summary>Horas de fermentación ideales por método (mín, máx).</summary>
        public static Vector2 IdealFerment(string method)
        {
            switch (method)
            {
                case "lavado": return new Vector2(12f, 18f);
                case "honey": return new Vector2(0f, 12f);
                case "natural": return new Vector2(0f, 24f);
                case "anaerobico": return new Vector2(48f, 96f);
                default: return new Vector2(12f, 18f);
            }
        }

        /// <summary>Humedad del pergamino (%) después de secar cierto número de días. Ideal: 10 – 12 %.</summary>
        public static float MoistureAfter(string method, float dryDays)
        {
            float k = method == "natural" ? 0.2f : method == "honey" ? 0.26f : 0.32f; // la fruta entera seca más despacio
            return 8f + 47f * Mathf.Exp(-k * Mathf.Max(0f, dryDays));
        }

        public struct ProcessResult
        {
            public float potential, moisture, greenGrams;
            public string notes, verdict;
        }

        /// <summary>Simula despulpado, fermentación, lavado, secado y trilla de un lote de cereza.</summary>
        public static ProcessResult Process(Lot cherry, string method, float fermentHours, float dryDays)
        {
            var r = new ProcessResult();
            float pot = cherry.potential;
            var ideal = IdealFerment(method);
            var verdict = new List<string>();
            string notes;
            switch (method)
            {
                case "honey": notes = "dulce, miel, panela"; pot += 0.5f; break;
                case "natural": notes = "frutos rojos, vino, chocolate"; pot += 1f; break;
                case "anaerobico": notes = "licor, frutas tropicales, especias"; pot += 1.5f; break;
                default: notes = "acidez brillante, taza limpia"; break;
            }
            if (fermentHours < ideal.x)
            {
                pot -= Mathf.Min(2f, (ideal.x - fermentHours) * 0.15f);
                verdict.Add("Fermentación corta: el mucílago no se soltó del todo.");
            }
            else if (fermentHours > ideal.y)
            {
                float over = fermentHours - ideal.y;
                pot -= Mathf.Min(8f, over * (method == "anaerobico" ? 0.08f : 0.3f));
                verdict.Add(over > 12f ? "¡Sobrefermentado! Sabor a vinagre (defecto)." : "Fermentación un poco larga: notas alcohólicas.");
            }
            else
            {
                pot += method == "anaerobico" ? 1f : 0.5f;
                verdict.Add("Fermentación en su punto.");
            }

            r.moisture = MoistureAfter(method, dryDays);
            if (r.moisture > 12.5f) { pot -= 3f; verdict.Add("Quedó húmedo (" + r.moisture.ToString("0.0") + " %): riesgo de moho."); }
            else if (r.moisture < 9.5f) { pot -= 1.5f; verdict.Add("Sobresecado (" + r.moisture.ToString("0.0") + " %): se quiebra en la trilla."); }
            else verdict.Add("Secado perfecto: " + r.moisture.ToString("0.0") + " % de humedad.");

            if (method == "natural" && cherry.ripePct < 0.8f) { pot -= 2f; verdict.Add("Las cerezas verdes en un natural dan sabores astringentes."); }

            r.potential = Mathf.Clamp(pot, 60f, 96f);
            r.greenGrams = cherry.grams * HConfig.CherryToParchment * HConfig.ParchmentToGreen;
            r.notes = string.IsNullOrEmpty(cherry.notes) ? notes : cherry.notes + ", " + notes;
            r.verdict = string.Join(" ", verdict);
            return r;
        }

        // ======================= TOSTIÓN =======================

        public const float YellowingC = 150f, FirstCrackC = 196f, SecondCrackC = 224f;

        /// <summary>Simulador de tostión: la temperatura del aire (ET) sigue al fuego y el grano (BT) sigue al aire.</summary>
        public class RoastSim
        {
            public float time, bt = 25f, et = 200f, heat = 0.55f, ror;
            public float yellowTime = -1f, firstCrack = -1f, secondCrack = -1f, dropTime = -1f;
            public float maxEarlyEt;
            public readonly List<Vector3> curve = new List<Vector3>(); // (tiempo, BT, RoR)
            float lastSample = -999f, prevRor = -999f;
            public bool flick;
            public bool Dropped => dropTime >= 0f;

            /// <summary>dt en segundos de tostión (el panel puede acelerar el reloj).</summary>
            public void Step(float dt)
            {
                if (Dropped) return;
                float targetEt = 170f + 140f * Mathf.Clamp01(heat);
                et += (targetEt - et) * Mathf.Min(1f, dt / 45f);
                float exo = firstCrack >= 0f && secondCrack < 0f ? 0.04f : 0f; // el primer crack libera calor
                float dBt = (et - bt) / 300f + exo;
                bt += dBt * dt;
                time += dt;
                ror = dBt * 60f;
                if (time < 120f) maxEarlyEt = Mathf.Max(maxEarlyEt, et);
                if (yellowTime < 0f && bt >= YellowingC) yellowTime = time;
                if (firstCrack < 0f && bt >= FirstCrackC) firstCrack = time;
                if (secondCrack < 0f && bt >= SecondCrackC) secondCrack = time;
                // flick: subir el fuego después del crack hace que la curva vuelva a subir
                float baseRor = (et - bt) / 300f * 60f;
                if (firstCrack >= 0f && time - firstCrack > 5f)
                {
                    if (prevRor < -900f) prevRor = baseRor;
                    else
                    {
                        if (baseRor > prevRor + 0.6f) flick = true;
                        prevRor = Mathf.Min(prevRor, baseRor);
                    }
                }
                if (time - lastSample >= 5f) { curve.Add(new Vector3(time, bt, ror)); lastSample = time; }
            }

            public void Drop() { if (!Dropped) dropTime = time; }

            public float DevRatio => firstCrack < 0f || time <= 0f ? 0f : (time - firstCrack) / time * 100f;
        }

        public struct RoastResult
        {
            public float score, level, devRatio, dropTemp, totalTime;
            public string name, verdict;
        }

        public static string RoastName(float level) => level < 0.3f ? "claro" : level < 0.6f ? "medio" : "oscuro";

        public static RoastResult Evaluate(RoastSim s)
        {
            var r = new RoastResult { dropTemp = s.bt, totalTime = s.time, devRatio = s.DevRatio };
            r.level = Mathf.Clamp01(Mathf.InverseLerp(198f, 235f, s.bt));
            r.name = RoastName(r.level);
            float score = 100f;
            var v = new List<string>();
            if (s.firstCrack < 0f)
            {
                score -= 45f;
                v.Add("Lo sacaste antes del primer crack: café crudo, sabor a pasto y cereal.");
            }
            else
            {
                if (r.devRatio < 15f) { score -= (15f - r.devRatio) * 2.5f; v.Add("Poco desarrollo (" + r.devRatio.ToString("0") + " %): ácido y vegetal."); }
                else if (r.devRatio > 25f) { score -= (r.devRatio - 25f) * 2f; v.Add("Mucho desarrollo (" + r.devRatio.ToString("0") + " %): se pierde el origen."); }
                else v.Add("Desarrollo de " + r.devRatio.ToString("0") + " %: en el rango ideal (15 – 25 %).");
            }
            if (s.time < 360f) { score -= (360f - s.time) / 10f; v.Add("Tostión muy rápida: el centro del grano quedó crudo."); }
            else if (s.time > 900f) { score -= (s.time - 900f) / 15f; v.Add("Tostión muy lenta: sabor horneado y plano."); }
            if (s.maxEarlyEt > 285f) { score -= 10f; v.Add("Demasiado fuego al principio: quemaduras en la superficie (scorching)."); }
            if (s.flick) { score -= 6f; v.Add("La curva subió después del crack (flick): sabores ahumados."); }
            if (s.firstCrack >= 0f && (s.et - s.bt) / 300f * 60f < 1.5f) { score -= 6f; v.Add("La curva se desplomó al final (crash): taza plana."); }
            if (s.bt > 232f) { score -= (s.bt - 232f) * 4f; v.Add("¡Carbonizado! Pasó del segundo crack: amargo y aceitoso."); }
            if (v.Count == 1 && score > 90f) v.Add("¡Curva de libro!");
            r.score = Mathf.Clamp(score, 0f, 100f);
            r.verdict = string.Join(" ", v);
            return r;
        }

        public static void ApplyRoast(Lot green, RoastResult r, int day)
        {
            green.stage = "tostado";
            green.roastLevel = r.level;
            green.roast = r.name;
            green.roastScore = r.score;
            green.devRatio = r.devRatio;
            green.grams *= 1f - HConfig.RoastLoss;
            green.day = day;
        }

        // ======================= EXTRACCIÓN (AEROPRESS) =======================

        public struct BrewParams
        {
            public float dose, water, temp, grind, steep, stirs, dilute; // g, g, °C, µm, s, vueltas, g de agua para diluir
            public bool inverted, metal;

            public static BrewParams Default => new BrewParams { dose = 15f, water = 220f, temp = 88f, grind = 650f, steep = 90f, stirs = 10f, dilute = 0f };
        }

        public struct BrewResult
        {
            public float ey, tds, beverage, score, brewQuality, waterTemp;
            public bool under, over, weak, strong;
            public string verdict, taste;
        }

        public const float IdealEyMin = 18f, IdealEyMax = 22f, IdealTdsMin = 1.15f, IdealTdsMax = 1.55f;

        /// <summary>Extracción (%) a los t segundos de contacto. Sirve para la curva en vivo del panel.
        /// Dos fases: los sólidos de la superficie salen casi de inmediato; el resto depende de molienda, temperatura y agitación.</summary>
        public static float ExtractionAt(BrewParams p, float roastLevel, float waterTemp, float t)
        {
            float grind = Mathf.Clamp(p.grind, 200f, 1400f);
            t = Mathf.Max(0f, t);
            float k = 0.00617f * (650f / grind) * Mathf.Exp((waterTemp - 93f) / 26f)
                      * (1f + 0.03f * Mathf.Min(p.stirs, 15f)) * Mathf.Lerp(0.88f, 1.18f, roastLevel);
            float ey = 10f * (1f - Mathf.Exp(-t / 15f)) + 18f * (1f - Mathf.Exp(-k * t));
            if (grind < 350f) ey += (350f - grind) / 100f; // finos: se tapa el filtro y se sobreextrae
            return ey;
        }

        /// <summary>Prepara una AeroPress. altitude = altura donde se prepara (en Bogotá el agua hierve a ~91 °C).</summary>
        public static BrewResult Brew(BrewParams p, Lot lot, float altitude, int today)
        {
            var r = new BrewResult();
            float boil = HConfig.BoilingPointC(altitude);
            r.waterTemp = Mathf.Min(p.temp, boil);
            float roastLevel = lot != null ? lot.roastLevel : 0.5f;
            float contact = p.steep + (p.inverted ? 30f : 20f); // el método estándar gotea antes de presionar
            r.ey = ExtractionAt(p, roastLevel, r.waterTemp, contact);
            float retained = 2f * p.dose;                         // el café molido retiene ~2 g de agua por gramo
            r.beverage = Mathf.Max(1f, p.water - retained) + Mathf.Max(0f, p.dilute);
            r.tds = p.dose * r.ey / 100f / r.beverage * 100f;
            if (p.metal) r.tds += 0.04f;                          // el filtro metálico deja pasar aceites y finos

            r.under = r.ey < IdealEyMin; r.over = r.ey > IdealEyMax;
            r.weak = r.tds < IdealTdsMin; r.strong = r.tds > IdealTdsMax;
            float eyScore = 1f - Mathf.Clamp01((Mathf.Abs(r.ey - 20f) - 2f) / 4f);
            float tdsScore = 1f - Mathf.Clamp01((Mathf.Abs(r.tds - 1.35f) - 0.2f) / 0.4f);
            r.brewQuality = eyScore * 0.6f + tdsScore * 0.4f;

            float potential = lot != null ? lot.potential : 78f;
            float roastFactor = lot != null && lot.stage == "tostado" ? Mathf.Lerp(0.88f, 1f, lot.roastScore / 100f) : 0.9f;
            float cup = potential * roastFactor - (roastLevel > 0.75f ? (roastLevel - 0.75f) * 20f : 0f);
            var taste = new List<string>();
            if (lot != null && lot.stage == "tostado")
            {
                int age = today - lot.day;
                if (age < 1) { cup -= 1.5f; taste.Add("muy fresco: el CO2 hace la extracción irregular"); }
                else if (age > 30) { cup -= 3f; taste.Add("oxidado: el café perdió aromas"); }
            }
            r.score = Mathf.Clamp(cup * Mathf.Lerp(0.72f, 1f, r.brewQuality), 0f, 100f);

            if (r.under) taste.Add("ácido, salado y corto (subextraído)");
            else if (r.over) taste.Add("amargo, astringente y seco (sobreextraído)");
            if (r.weak) taste.Add("aguado, sin cuerpo");
            else if (r.strong) taste.Add("muy intenso y pesado");
            if (!r.under && !r.over && !r.weak && !r.strong)
                taste.Add("dulce, balanceado y limpio" + (lot != null && !string.IsNullOrEmpty(lot.notes) ? ": " + lot.notes : ""));
            if (p.metal) taste.Add("más cuerpo por el filtro metálico");
            if (p.temp > boil + 0.5f) taste.Add("el agua no pasa de " + boil.ToString("0") + " °C a esta altura");
            r.taste = string.Join("; ", taste);
            r.verdict = r.score >= 88f ? "¡Taza de campeonato!" : r.score >= 84f ? "Taza excelente" : r.score >= 80f ? "Café de especialidad"
                      : r.score >= 72f ? "Buen tinto, pero se puede mejorar" : "Hay que ajustar la receta";
            return r;
        }

        // ======================= CATACIÓN =======================

        public static readonly string[] CuppingAttributes =
            { "Fragancia/Aroma", "Sabor", "Sabor residual", "Acidez", "Cuerpo", "Balance", "Uniformidad", "Taza limpia", "Dulzor", "Puntaje del catador" };

        /// <summary>Atributos del formato de catación SCA (cada uno de 6 a 10) que suman el puntaje del lote.</summary>
        public static float[] Cupping(Lot l, int seed)
        {
            var rng = new System.Random(seed);
            float target = Mathf.Clamp(l.stage == "tostado" ? l.potential * Mathf.Lerp(0.88f, 1f, l.roastScore / 100f) : l.potential, 60f, 100f);
            var a = new float[10];
            float acidity = l.process == "lavado" ? 0.3f : 0f, body = l.process == "natural" || l.process == "anaerobico" ? 0.3f : 0f;
            acidity -= l.roastLevel * 0.5f; body += l.roastLevel * 0.4f;
            float baseVal = target / 10f;
            float sum = 0f;
            for (int i = 0; i < 10; i++)
            {
                float v = baseVal + (float)(rng.NextDouble() - 0.5) * 0.5f;
                if (i == 3) v += acidity;
                if (i == 4) v += body;
                a[i] = Mathf.Clamp(Mathf.Round(v * 4f) / 4f, 6f, 10f);
                sum += a[i];
            }
            // ajusta el último atributo para que la suma cuadre con el puntaje del lote
            a[9] = Mathf.Clamp(Mathf.Round((a[9] + target - sum) * 4f) / 4f, 6f, 10f);
            return a;
        }

        // ======================= REDES (acero-press) =======================

        /// <summary>Seguidores que gana una publicación según la taza y la fama actual.</summary>
        public static int FollowersFor(float score, int followers, System.Random rng)
        {
            float quality = Mathf.Max(0f, score - 70f);
            float reach = 3f + Mathf.Sqrt(Mathf.Max(0, followers)) * 0.6f;
            return Mathf.Max(1, Mathf.RoundToInt(quality * reach * (0.8f + (float)rng.NextDouble() * 0.4f) / 4f));
        }
    }
}
