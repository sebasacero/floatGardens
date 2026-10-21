// Float Gardens: Tierra de Café — modo historia. Constantes, balance y datos físicos que usa todo el juego.
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class HConfig
    {
        public const string Title = "Float Gardens: Tierra de Café";
        public const string Subtitle = "De Bogotá al mundo";
        public const string SaveKey = "floatgardens_historia_v1";
        public const string DefaultName = "Acero";
        public const string Channel = "acero-press";
        public const int CupGoal = 50000; // la meta de acero-press: 50.000 tazas de café en AeroPress

        // Bogotá: 2.600 metros más cerca de las estrellas (y del punto de ebullición)
        public const float BogotaAltitude = 2600f;
        public const float BogotaOutdoorTemp = 14f;

        /// <summary>Punto de ebullición del agua (°C) a una altitud (m). Baja ~1 °C cada 300 m: en Bogotá hierve a ~91 °C.</summary>
        public static float BoilingPointC(float altitudeM) => 100f - Mathf.Max(0f, altitudeM) / 300f;

        // Tiempo de juego
        public const float StartHour = 7f;
        public const float SleepHour = 22f;
        public const float HoursPerBrew = 0.25f;
        public const float HoursPerPanel = 0.5f;   // usar una estación (cultivo, beneficio, tostión) gasta media hora
        public const float RealSecondsPerGameHour = 60f; // caminando por la bodega el reloj avanza despacio

        // Economía (pesos colombianos)
        public const int StartMoney = 400000;
        public const int TintoPrice = 3000;        // un tinto en la calle
        public const int SpecialtyCupPrice = 9000; // una taza de especialidad
        public const int EnergyCostPerDay = 6000;  // luces LED + calefacción para que el café no sufra el frío bogotano
        public const int BusFarePerHour = 4000;    // flota intermunicipal

        // Cultivo
        public const int StartRafts = 4;          // balsas flotantes al empezar
        public const int MaxRafts = 12;
        public const int RaftPrice = 120000;
        public const int SeedPrice = 15000;

        // Conversiones de masa (gramos)
        public const float CherryToParchment = 0.2f;   // ~5 kg de cereza → 1 kg de pergamino seco
        public const float ParchmentToGreen = 0.8f;    // la trilla quita el pergamino: ~80 % es almendra
        public const float RoastLoss = 0.15f;          // en la tostión se pierde ~15 % del peso
        public const float DosePerCup = 15f;           // gramos de café tostado por AeroPress

        // Puntos Float (se suman al mismo guardado del mundo abierto y se canjean por café gratis)
        public const int PuntosPerChapter = 150;
        public const int PuntosPerTrophy = 300;

        public static string Money(int n) => FGConfig.Money(n);

        public static string Grams(float g) => g >= 1000f ? (g / 1000f).ToString("0.0#") + " kg" : Mathf.RoundToInt(g) + " g";

        public static string Clock(float hour)
        {
            int h = Mathf.FloorToInt(hour) % 24;
            int m = Mathf.FloorToInt((hour - Mathf.Floor(hour)) * 60f);
            return h.ToString("00") + ":" + m.ToString("00");
        }

        // Paleta de la historia (complementa la de UIKit)
        public static readonly Color Coffee = new Color32(0x4a, 0x2c, 0x1d, 255);
        public static readonly Color Cherry = new Color32(0xc0, 0x1f, 0x2f, 255);
        public static readonly Color Leaf = new Color32(0x2e, 0x7d, 0x32, 255);
        public static readonly Color Cream = new Color32(0xf5, 0xe6, 0xc8, 255);
        public static readonly Color Gold = new Color32(0xf2, 0xb7, 0x05, 255);   // amarillo de la bandera
        public static readonly Color Blue = new Color32(0x1f, 0x4e, 0xa8, 255);   // azul de la bandera
        public static readonly Color Red = new Color32(0xce, 0x11, 0x26, 255);    // rojo de la bandera
    }
}
