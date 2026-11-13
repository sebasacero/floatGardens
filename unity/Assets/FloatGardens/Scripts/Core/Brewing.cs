// Modelo de extracción del AeroPress: con las cinco variables (proporción, molienda, temperatura, tiempo y método)
// calcula el porcentaje de extracción y el TDS, y de ahí un puntaje de cata sobre 100, con notas de sabor.
// Simplificado pero fiel a la lógica real: fino, caliente y largo extrae más; grueso, frío y corto extrae menos.
using UnityEngine;

namespace FloatGardens
{
    public class BrewRecipe
    {
        // La receta inicial es la de cualquier principiante: mucho café, agua hirviendo, molienda gruesa y afán.
        // Da unos 70 puntos; el juego consiste en aprender a subirla.
        public string variety = "castillo";
        public float dose = 18f;     // gramos de café
        public float water = 250f;   // mililitros
        public float grind = 8f;     // 1 fina .. 10 gruesa
        public float temp = 97f;     // grados
        public float time = 45f;     // segundos de contacto
        public bool inverted = false;
        public bool bloom = false;   // prehumedecer 30 s

        public float Ratio => water / Mathf.Max(1, dose);
        public BrewRecipe Clone() => (BrewRecipe)MemberwiseClone();
    }

    public class BrewResult
    {
        public float extraction, tds, score;
        public string verdict, notes, tip;
        public bool specialty => score >= 80;
    }

    public static class Brewing
    {
        public static BrewResult Evaluate(BrewRecipe r, float luck = 0)
        {
            var v = Codex.GetVariety(r.variety);
            // Porcentaje de extracción: parte de 20 % con la receta de referencia y se mueve con cada variable
            float ey = 20.2f
                     + (5.5f - r.grind) * 1.35f                     // fino extrae más
                     + (r.temp - 92f) * 0.22f                       // caliente extrae más
                     + (Mathf.Sqrt(r.time) - Mathf.Sqrt(90f)) * 0.9f // el tiempo rinde cada vez menos
                     + (r.bloom ? 0.6f : 0f)
                     + (r.inverted ? 0.4f : 0f)
                     - Mathf.Max(0, r.Ratio - 15f) * 0.08f;         // con mucha agua la extracción sube poco
            // Molienda muy fina y tiempo largo: astringencia extra (finos)
            float fines = Mathf.Max(0, 3.5f - r.grind) * Mathf.Max(0, (r.time - 60f) / 60f) * 0.8f;
            ey += fines;
            ey = Mathf.Clamp(ey, 10f, 30f);
            // TDS: lo extraído repartido en el agua (café en taza ≈ 1,0 ≈ 1 g/ml)
            float tds = ey / 100f * r.dose / r.water * 100f;

            // Puntaje: cerca del rango dorado (EY 18-22, TDS 1,15-1,35) y de lo que prefiere la variedad
            float score = 90f;
            score -= Penalty(ey, 18.5f, 21.5f, 3f);
            score -= Penalty(tds, 1.18f, 1.38f, 30f);
            score -= Mathf.Abs(r.temp - v.temp) * 0.5f;
            score -= Mathf.Abs(r.grind - v.grind) * 1.6f;
            score -= Penalty(r.Ratio, 14f, 17f, 2f);
            if (r.time < 45) score -= 2.5f;
            if (r.time > 200) score -= 2f;
            if (!r.bloom) score -= 1.5f;
            score += luck;
            score = Mathf.Clamp(score, 55f, 97f);

            var res = new BrewResult { extraction = ey, tds = tds, score = Mathf.Round(score * 4f) / 4f };
            res.notes = Notes(v, ey, tds, r);
            res.tip = Tip(v, ey, tds, r);
            res.verdict = score >= 90 ? "Nivel campeonato mundial" : score >= 86 ? "Taza de Excelencia" : score >= 80 ? "Café de especialidad" : score >= 72 ? "Buen tinto" : "Para aprender";
            return res;
        }

        static float Penalty(float x, float lo, float hi, float perUnit)
        {
            if (x < lo) return (lo - x) * perUnit;
            if (x > hi) return (x - hi) * perUnit;
            return 0;
        }

        static string Notes(Codex.Variety v, float ey, float tds, BrewRecipe r)
        {
            if (ey < 17.5f) return "Subextraído: ácido, aguado, salado. Las notas de " + v.name + " apenas se asoman.";
            if (ey > 22.5f) return "Sobreextraído: amargo, seco, astringente. Se perdió el dulzor de " + v.name + ".";
            if (tds < 1.1f) return "Balanceado pero ligero: sabe a " + v.notes.Split('.')[0].ToLowerInvariant() + ", con poco cuerpo.";
            if (tds > 1.45f) return "Intenso y pesado: " + v.notes.Split('.')[0] + ", pero cuesta distinguir las capas.";
            return "¡Dulce y limpio! " + v.notes;
        }

        static string Tip(Codex.Variety v, float ey, float tds, BrewRecipe r)
        {
            if (ey < 17.5f) return r.grind > v.grind + 0.5f ? "Muele más fino." : r.temp < v.temp - 2 ? "Sube la temperatura." : "Dale más tiempo o agita.";
            if (ey > 22.5f) return r.grind < v.grind - 0.5f ? "Muele más grueso." : r.temp > v.temp + 2 ? "Baja la temperatura." : "Acorta el tiempo.";
            if (tds < 1.1f) return "Menos agua o más café: acércate a 1:15.";
            if (tds > 1.45f) return "Más agua o menos café: acércate a 1:16.";
            if (Mathf.Abs(r.temp - v.temp) > 2) return v.name + " prefiere el agua a " + v.temp + " °C.";
            if (Mathf.Abs(r.grind - v.grind) > 1) return v.name + " pide una molienda " + (r.grind > v.grind ? "más fina." : "más gruesa.");
            return "Receta redonda. Repítela tres veces: la consistencia también se califica.";
        }
    }
}
