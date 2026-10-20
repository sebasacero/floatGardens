// Guardado del modo historia (PlayerPrefs + JSON). Es independiente del mundo abierto,
// pero los Puntos Float que se ganan aquí se suman al mismo guardado (FGSave) para canjear café.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public enum PlantStage { Vacia, Semilla, Fosforito, Chapola, Almacigo, Arbusto, Floracion, CerezaVerde, CerezaMadura, Sobremadura }

    [Serializable]
    public class Plant
    {
        public string variety = "";
        public PlantStage stage = PlantStage.Vacia;
        public float progress;        // 0 – 1 dentro de la etapa actual
        public float health = 1f;     // 0 – 1
        public bool roya, broca;      // roya (Hemileia vastatrix) y broca (Hypothenemus hampei)
        public int harvests;          // cosechas que ha dado
        public bool Empty => stage == PlantStage.Vacia;
    }

    /// <summary>Clima controlado de la bodega: el café no crece al aire libre en Bogotá (2.600 m, ~14 °C).</summary>
    [Serializable]
    public class Garden
    {
        public float tempC = 20f;      // ideal ~18 – 22 °C
        public float lightHours = 12f; // fotoperiodo de las luces LED
        public float humidity = 70f;   // %
        public float ph = 6f;          // solución nutritiva
        public float ec = 1.4f;        // conductividad (mS/cm)
        public List<Plant> rafts = new List<Plant>();
    }

    /// <summary>Un lote de café que avanza por la cadena: cereza → pergamino → verde → tostado.</summary>
    [Serializable]
    public class Lot
    {
        public string id;
        public string variety = "";
        public string origin = "";     // id de región o "floatgardens"
        public string stage = "cereza"; // "cereza" | "secando" | "verde" | "tostado"
        public float grams;
        public float potential = 80f;  // puntaje SCA alcanzable con este café verde (lo fijan variedad, cultivo y beneficio)
        public float ripePct = 1f;     // fracción de cerezas maduras al recoger
        public string process = "";    // "lavado" | "honey" | "natural" | "anaerobico"
        public float fermentHours;
        public float moisture;         // % de humedad del pergamino
        public string roast = "";      // "claro" | "medio" | "oscuro"
        public float roastLevel;       // 0 claro – 1 muy oscuro
        public float roastScore;       // 0 – 100
        public float devRatio;         // % de desarrollo después del primer crack
        public string notes = "";      // notas de sabor del lote
        public int day;                // día en que se creó (para el reposo después de tostar)
        public int readyDay;           // stage "secando": día en que el pergamino queda seco y trillado

        public string Label
        {
            get
            {
                var v = Catalog.Variety(variety);
                var r = Catalog.Region(origin);
                string o = r != null ? r.name : origin == "floatgardens" ? "Float Gardens" : origin;
                return (v != null ? v.name : variety) + " · " + o + (string.IsNullOrEmpty(process) ? "" : " · " + process)
                    + (string.IsNullOrEmpty(roast) ? "" : " · tueste " + roast);
            }
        }
    }

    [Serializable]
    public class BrewRecord
    {
        public string lot;
        public float dose, water, temp, grind, steep, stirs, dilute;
        public bool inverted, metal;
        public float ey, tds, score;
        public int day;
    }

    [Serializable] public class Stack { public string id; public int count; }

    [Serializable]
    public class HistoriaSave
    {
        public string playerName = HConfig.DefaultName;
        public int day = 1;
        public float hour = HConfig.StartHour;
        public int money = HConfig.StartMoney;
        public int cups;               // tazas preparadas (meta: 50.000)
        public int followers;          // seguidores de acero-press
        public int chapter;            // 0 = prólogo
        public string mission = "";    // misión activa
        public float missionProgress;
        public string location = "bodega";
        public string region = "";     // región actual si location == "region"
        public List<string> flags = new List<string>();
        public List<string> wiki = new List<string>();
        public List<string> wikiRead = new List<string>();
        public List<string> visited = new List<string>();
        public List<string> stamps = new List<string>();    // sellos del Pasaporte Cafetero (retos superados)
        public List<string> trophies = new List<string>();
        public List<string> gear = new List<string>();
        public List<string> mailRead = new List<string>();
        public List<string> missionsDone = new List<string>();
        public List<Stack> seeds = new List<Stack>();
        public Garden garden = new Garden();
        public List<Lot> lots = new List<Lot>();
        public List<BrewRecord> brews = new List<BrewRecord>();
        public float bestBrew;
        public int nextLotId = 1;
        public int puntosAwarded;      // Puntos Float ya sumados al guardado general
        public bool muted;
        public bool tutorialDone;

        public bool Has(string flag) => flags.Contains(flag);
        public void Flag(string flag) { if (!flags.Contains(flag)) flags.Add(flag); }

        public int Seeds(string variety)
        {
            foreach (var s in seeds) if (s.id == variety) return s.count;
            return 0;
        }

        public void AddSeeds(string variety, int n)
        {
            foreach (var s in seeds) if (s.id == variety) { s.count = Mathf.Max(0, s.count + n); return; }
            if (n > 0) seeds.Add(new Stack { id = variety, count = n });
        }

        public Lot NewLot(string variety, string origin, string stage, float grams, float potential)
        {
            var l = new Lot { id = "L" + (nextLotId++).ToString("000"), variety = variety, origin = origin, stage = stage, grams = grams, potential = potential, day = day };
            lots.Add(l);
            return l;
        }

        public Lot FindLot(string id)
        {
            foreach (var l in lots) if (l.id == id) return l;
            return null;
        }

        public List<Lot> LotsAt(string stage)
        {
            var r = new List<Lot>();
            foreach (var l in lots) if (l.stage == stage && l.grams > 0.5f) r.Add(l);
            return r;
        }
    }

    public static class HState
    {
        public static bool Exists => PlayerPrefs.HasKey(HConfig.SaveKey);

        public static HistoriaSave Load()
        {
            try
            {
                string raw = PlayerPrefs.GetString(HConfig.SaveKey, "");
                if (!string.IsNullOrEmpty(raw))
                {
                    var s = JsonUtility.FromJson<HistoriaSave>(raw);
                    if (s != null) { Normalize(s); return s; }
                }
            }
            catch (Exception e) { Debug.LogWarning("Float Gardens: no se pudo leer el guardado de la historia. " + e.Message); }
            return NewGame(HConfig.DefaultName);
        }

        public static HistoriaSave NewGame(string name)
        {
            var s = new HistoriaSave { playerName = string.IsNullOrWhiteSpace(name) ? HConfig.DefaultName : name.Trim() };
            Normalize(s);
            return s;
        }

        static void Normalize(HistoriaSave s)
        {
            s.garden ??= new Garden();
            while (s.garden.rafts.Count < HConfig.StartRafts) s.garden.rafts.Add(new Plant());
        }

        public static void Write(HistoriaSave s)
        {
            PlayerPrefs.SetString(HConfig.SaveKey, JsonUtility.ToJson(s));
            PlayerPrefs.Save();
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(HConfig.SaveKey);
            PlayerPrefs.Save();
        }

        /// <summary>Suma Puntos Float al guardado general (el mismo del mundo abierto) para canjear café gratis.</summary>
        public static void AwardPuntos(HistoriaSave s, int puntos)
        {
            if (puntos <= 0) return;
            var main = FGSave.Load();
            main.puntos += puntos;
            main.puntosTotal += puntos;
            FGSave.Write(main);
            s.puntosAwarded += puntos;
        }
    }
}
