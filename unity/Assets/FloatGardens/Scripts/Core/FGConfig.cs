// Float Gardens: Bogotá — configuración y balance (misma lógica que la versión web).
using UnityEngine;

namespace FloatGardens
{
    public static class FGConfig
    {
        // Ciudad
        public const float Block = 40f;
        public const float Street = 14f;
        public const int Grid = 12;
        public const float Cell = Block + Street;
        public const float Half = Grid * Cell / 2f;

        // Jugador
        public const float WalkSpeed = 6f;
        public const float SprintSpeed = 9.5f;
        public const float JumpSpeed = 7f;
        public const float Gravity = 20f;
        public const float MaxHp = 100f;
        public const float RegenDelay = 4f;
        public const float RegenRate = 18f;

        // AeroPress X-1
        public const int MagSize = 12;
        public const float FireInterval = 0.14f;
        public const float Damage = 25f;
        public const float HeadshotMul = 1.6f;
        public const float Range = 140f;
        public const float ReloadTime = 1.4f;
        public const int CherriesPerReload = 1;

        // Economía
        public const int StartCherries = 15;
        public const int CherriesPerSale = 2;
        public const int SalePrice = 8000;
        public const int HarvestYield = 4;
        public const float RegrowTime = 25f;

        // Zonas
        public const float CaptureRadius = 16f;
        public const float CaptureTime = 22f;

        // Puntos Float (se canjean por café gratis en los puntos oficiales)
        public const int RedeemCost = 1500;
        public const int PointsPerCherry = 1;
        public const int PointsPerSale = 10;
        public const int PointsPerKO = 5;
        public const int PointsPerZone = 100;
        public const int PointsPerBoss = 300;
        public const int ZoneMoney = 20000;
        public const int KOMoney = 500;
        public const int BossMoney = 50000;

        // Mapa de barrios: fila 0 = norte, columna 0 = occidente. Al oriente están los cerros.
        // T Zona T · C Chapinero · U Teusaquillo · P Parque Simón Bolívar · I Centro Internacional
        // L La Candelaria · A Puente Aranda · R residencial · F Sede de la Federación · B Plaza de Bolívar
        public static readonly string[] DistrictMap =
        {
            "RRPPPRTTTTTT",
            "RPPPPRTTTTTT",
            "RPPPPRCCTTTT",
            "RPPPPUCCCCCC",
            "RRUUUUCCCCCC",
            "RRUUUUCCCCCC",
            "AAUUUUIIIIII",
            "AAAARRIIIIII",
            "AAAARRRLLFLL",
            "AAAARRRLLBLL",
            "AAARRRRLLLLL",
            "AARRRRRLLLLL",
        };

        public static string DistrictName(char code)
        {
            switch (code)
            {
                case 'T': return "Zona T";
                case 'C': return "Chapinero";
                case 'U': return "Teusaquillo";
                case 'P': return "Parque Simón Bolívar";
                case 'I': return "Centro Internacional";
                case 'L': return "La Candelaria";
                case 'A': return "Puente Aranda";
                case 'F': return "Sede de la Federación";
                case 'B': return "Plaza de Bolívar";
                default: return "Barrios del occidente";
            }
        }

        public static readonly string[] Carreras =
        {
            "Av. Boyacá", "Av. 68", "Carrera 50", "Av. NQS", "Carrera 24", "Carrera 17", "Av. Caracas",
            "Carrera 13", "Carrera 11", "Carrera 9", "Carrera 7", "Carrera 5", "Av. Circunvalar",
        };

        public static readonly string[] Calles =
        {
            "Calle 100", "Calle 93", "Calle 85", "Calle 72", "Calle 63", "Calle 53", "Calle 45", "Calle 26",
            "Calle 19", "Av. Jiménez", "Calle 6", "Calle 1", "Av. Primera de Mayo",
        };

        public static readonly Vector2Int HQ = new Vector2Int(9, 4); // (columna, fila)
        public const char HQDistrict = 'C';

        public struct ZoneDef
        {
            public string id, name;
            public int row, col;
            public char district;
            public ZoneDef(string id, int row, int col, string name, char district)
            {
                this.id = id; this.row = row; this.col = col; this.name = name; this.district = district;
            }
        }

        public static readonly ZoneDef[] Zones =
        {
            new ZoneDef("teusaquillo", 5, 3, "Parque Teusaquillo", 'U'),
            new ZoneDef("simon", 2, 2, "Lago del Simón Bolívar", 'P'),
            new ZoneDef("zonat", 1, 8, "Zona T", 'T'),
            new ZoneDef("centro", 7, 8, "Centro Internacional", 'I'),
            new ZoneDef("candelaria", 10, 8, "Chorro de Quevedo", 'L'),
            new ZoneDef("aranda", 9, 2, "Puente Aranda", 'A'),
        };

        public static readonly Vector2Int BossCell = new Vector2Int(9, 9);
        public const string BossName = "Don Monopolio";
        public const float BossHp = 3000f;

        public class EnemyType
        {
            public string name;
            public float hp, speed, melee, ranged, range, cooldown;
            public Color color;
            public string hat;
        }

        public static readonly EnemyType Inspector = new EnemyType
        {
            name = "Inspector", hp = 60, speed = 4.4f, melee = 10, color = new Color32(0x6d, 0x70, 0x78, 255), hat = "fedora",
        };

        public static readonly EnemyType Guardia = new EnemyType
        {
            name = "Guardia del Tinto", hp = 100, speed = 3.2f, ranged = 9, range = 30, cooldown = 2.2f, color = new Color32(0x33, 0x36, 0x3d, 255), hat = "beret",
        };

        public class Upgrade
        {
            public string id, icon, name, desc;
            public int max, cost;
        }

        public static readonly Upgrade[] Upgrades =
        {
            new Upgrade { id = "dano", icon = "+DMG", name = "Presión turbo", desc = "+20% de daño por nivel.", max = 3, cost = 20000 },
            new Upgrade { id = "cargador", icon = "+MAG", name = "Cámara XL", desc = "+4 disparos por cargador.", max = 3, cost = 15000 },
            new Upgrade { id = "vida", icon = "+HP", name = "Delantal reforzado", desc = "+25 de vida máxima.", max = 3, cost = 18000 },
            new Upgrade { id = "tenis", icon = "+VEL", name = "Tenis de barista", desc = "+8% de velocidad.", max = 3, cost = 12000 },
            new Upgrade { id = "cultivo", icon = "+CUL", name = "Hidroponía avanzada", desc = "+1 cereza por cosecha y crecen más rápido.", max = 3, cost = 16000 },
        };

        // Edita esta lista con las direcciones reales antes de publicar.
        public static readonly string[] OfficialPoints = { "Float Gardens — Punto oficial · Bogotá · Dirección por confirmar" };

        public static readonly string[] KOLines = { "¡Me quemé con el café!", "¡Llamaré a mi supervisor!", "¡Esto no se queda así!", "¡Retirada!" };
        public static readonly string[] EnemyLines = { "¡Café sin sello!", "¡Alto ahí, barista!", "¡Formulario 27-B!", "¡Multa por innovar!", "¡Aquí manda la Federación!" };
        public static readonly string[] BossLines = { "¡Sellado y decomisado!", "¡Formulario rechazado!", "¡Nadie compite conmigo!" };

        public static string Money(int n)
        {
            return "$" + n.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-CO"));
        }
    }
}
