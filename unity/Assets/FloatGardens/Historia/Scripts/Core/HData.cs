// Tipos de datos del modo historia y el catálogo que los indexa.
// El contenido (variedades, regiones, Wiki, misiones, diálogos...) vive en Scripts/Data/*.cs.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    /// <summary>Reto que propone el mentor de cada región (cada uno abre un minijuego).</summary>
    public enum Reto { Ninguno, Recoleccion, Plagas, Beneficio, Catacion, Tostion, Historia, Aeropress }

    public class Variety
    {
        public string id, name, origin, description;
        public float yield = 1f;           // multiplicador de cosecha (0,5 – 1,5)
        public float rustResistance;       // resistencia a la roya (0 – 1)
        public float cupPotential = 82f;   // puntaje SCA máximo que puede alcanzar bien cultivada y procesada
        public float growthDays = 10f;     // días de juego desde la semilla hasta la primera floración
        public Color cherryColor = new Color32(0xc0, 0x1f, 0x2f, 255);
        public string notes = "";          // notas de sabor típicas, separadas por coma
        public int price = HConfig.SeedPrice;
        public string unlockRegion = "";   // región cuyo mentor regala la semilla ("" = disponible desde el inicio)
    }

    /// <summary>Cómo se ve una finca de la región (lo usa FincaBuilder).</summary>
    public class FincaStyle
    {
        public Color ground = new Color32(0x5d, 0x7a, 0x3a, 255);
        public Color foliage = new Color32(0x2e, 0x6b, 0x2f, 255);
        public Color sky = new Color32(0x9d, 0xc4, 0xe8, 255);
        public Color fog = new Color32(0xc8, 0xd6, 0xde, 255);
        public float fogEnd = 260f;
        public float slope = 0.6f;        // 0 plano .. 1 muy empinado
        public float temperatureC = 20f;
        public bool shadeTrees = true, plantains = true, waxPalms, volcano, snowPeaks, jeep = true, mule, river, colonialTown;
    }

    public class Region
    {
        public string id, name, department, town;
        public float lat, lon;
        public float altitudeMin, altitudeMax;
        public int chapter;                 // capítulo de la historia en que se puede viajar allí
        public int travelHours;             // horas de bus desde Bogotá
        public string summary = "";         // una o dos frases para el mapa
        public string cupProfile = "", harvests = "", landscape = "";
        public Reto reto;
        public string mentorName = "", mentorRole = "";   // personajes ficticios
        public string giftVariety = "";     // semilla que regala el mentor al superar el reto
        public string[] wikiUnlocks = new string[0];
        public FincaStyle style = new FincaStyle();
        public float Altitude => (altitudeMin + altitudeMax) * 0.5f;
    }

    public class WikiEntry
    {
        public string id, category, title, year = "", place = "", body;
        public string[] related = new string[0];
    }

    public struct DialogLine
    {
        public string speaker, text;
        public DialogLine(string speaker, string text) { this.speaker = speaker; this.text = text; }
    }

    public class Mission
    {
        public string id, title, desc;
        public int chapter;
        public string evt;                  // HEvents.*
        public string arg = "";             // filtro opcional: id de región, variedad, competencia...
        public float target = 1f;           // número de veces, o umbral si threshold = true
        public bool threshold;              // true: se cumple cuando el valor del evento ≥ target
        public string startDialog = "", endDialog = "";   // ids de StoryData.Dialogs
        public int rewardMoney;
        public string[] rewardWiki = new string[0];
        public string rewardSeed = "";
        public int rewardSeedCount;
        public string[] rewardFlags = new string[0];
        public string next = "";            // id de la siguiente misión ("" = fin del capítulo)
    }

    public class Chapter
    {
        public int number;
        public string title, subtitle;
        public string introDialog = "";     // id de StoryData.Dialogs
        public string firstMission = "";
    }

    public class Competition
    {
        public string id, name, city;
        public int chapter, rounds = 3;
        public float rivalSkill = 0.5f;     // 0 – 1
        public float altitude = HConfig.BogotaAltitude; // altura de la sede: cambia el punto de ebullición del agua
        public int prize;
        public string trophy = "";
        public string introDialog = "", winDialog = "", loseDialog = "";
        public string[] wikiUnlocks = new string[0];
    }

    public class Rival
    {
        public string name, city, style;
        public float skill;                 // 0 – 1
    }

    public class Gear
    {
        public string id, name, desc;
        public int price;
        public int chapter;                 // aparece en la tienda desde este capítulo
    }

    public class MailItem
    {
        public string id, from, subject, body;
        public string requiresFlag = "";    // llega cuando la bandera existe ("" = desde el inicio)
    }

    public class QuizQuestion
    {
        public string question;
        public string[] options;
        public int correct;
        public string explanation;          // se muestra después de responder
        public string wiki = "";            // entrada relacionada
    }

    /// <summary>Nombres de los eventos de juego que cumplen misiones.</summary>
    public static class HEvents
    {
        public const string Brew = "brew";            // valor = puntaje de la taza (0 – 100)
        public const string Publish = "publish";      // valor = seguidores ganados
        public const string Plant = "plant";          // arg = variedad
        public const string Harvest = "harvest";      // valor = gramos de cereza
        public const string Process = "process";      // arg = método; valor = calidad del lote
        public const string Roast = "roast";          // arg = "claro" | "medio" | "oscuro"; valor = puntaje de tostión
        public const string Visit = "visit";          // arg = región
        public const string Reto = "reto";            // arg = región; valor = puntaje (0 – 100)
        public const string Wiki = "wiki";            // arg = entrada
        public const string Competition = "competition"; // arg = competencia; valor = 1 si ganó, 0 si no
        public const string Sleep = "sleep";
        public const string Buy = "buy";              // arg = id del objeto
        public const string Talk = "talk";            // arg = personaje o región
        public const string Cups = "cups";            // valor = total de tazas acumuladas
        public const string Followers = "followers";  // valor = total de seguidores
    }

    /// <summary>Índice del contenido. Todo se busca por id.</summary>
    public static class Catalog
    {
        static Dictionary<string, Variety> varieties;
        static Dictionary<string, Region> regions;
        static Dictionary<string, WikiEntry> wiki;
        static Dictionary<string, Mission> missions;
        static Dictionary<string, Competition> competitions;
        static Dictionary<string, Gear> gear;

        static Dictionary<string, T> Index<T>(IEnumerable<T> items, Func<T, string> key)
        {
            var d = new Dictionary<string, T>();
            foreach (var it in items)
            {
                string k = key(it);
                if (d.ContainsKey(k)) Debug.LogWarning("Float Gardens: id repetido en el contenido: " + k);
                d[k] = it;
            }
            return d;
        }

        static T Get<T>(Dictionary<string, T> d, string id) where T : class
            => id != null && d.TryGetValue(id, out var v) ? v : null;

        public static Variety Variety(string id) => Get(varieties ??= Index(VarietyData.All, v => v.id), id);
        public static Region Region(string id) => Get(regions ??= Index(RegionData.All, r => r.id), id);
        public static WikiEntry Wiki(string id) => Get(wiki ??= Index(LoreData.All, w => w.id), id);
        public static Mission Mission(string id) => Get(missions ??= Index(StoryData.Missions, m => m.id), id);
        public static Competition Competition(string id) => Get(competitions ??= Index(StoryData.Competitions, c => c.id), id);
        public static Gear Gear(string id) => Get(gear ??= Index(GearData.All, g => g.id), id);

        public static Chapter Chapter(int n)
        {
            foreach (var c in StoryData.Chapters) if (c.number == n) return c;
            return null;
        }

        public static DialogLine[] Dialog(string id)
            => !string.IsNullOrEmpty(id) && StoryData.Dialogs.TryGetValue(id, out var d) ? d : null;
    }

    /// <summary>Coordenadas geográficas → plano del mapa 3D de Colombia (x = este, z = norte, 1 grado = Scale unidades).</summary>
    public static class ColombiaGeo
    {
        public const float Scale = 10f;
        public const float LonOrigin = -73f, LatOrigin = 4.5f;   // centro aproximado del país

        public static Vector3 ToMap(float lat, float lon, float y = 0f)
            => new Vector3((lon - LonOrigin) * Scale, y, (lat - LatOrigin) * Scale);
    }
}
