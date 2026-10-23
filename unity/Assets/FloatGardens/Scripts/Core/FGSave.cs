// Guardado local (PlayerPrefs + JSON) y cupones de café gratis.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    [Serializable]
    public class Voucher
    {
        public string code;
        public long date;
    }

    [Serializable]
    public class UpgradeLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class BrewRecord
    {
        public string variety;
        public float score;
    }

    [Serializable]
    public class SaveData
    {
        public int money;
        public int puntos;
        public int puntosTotal;
        public int cherries = FGConfig.StartCherries;
        public int kos;
        public int sales;
        public bool bossDefeated;
        public bool muted;
        public List<string> captured = new List<string>();
        public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
        public List<Voucher> vouchers = new List<Voucher>();
        // Campaña, Códice y expediciones
        public int chapter, mission, missionProgress;
        public bool introSeen;
        public List<string> codex = new List<string>();
        public List<string> regions = new List<string>();
        public List<string> varieties = new List<string>();
        public List<BrewRecord> brews = new List<BrewRecord>();
        public int brewCount;
        public float bestBrew;

        public float BestBrew(string variety)
        {
            float best = 0;
            foreach (var b in brews) if (b.variety == variety) best = Mathf.Max(best, b.score);
            return best;
        }

        public int Level(string id)
        {
            foreach (var u in upgrades) if (u.id == id) return u.level;
            return 0;
        }

        public void SetLevel(string id, int level)
        {
            foreach (var u in upgrades) if (u.id == id) { u.level = level; return; }
            upgrades.Add(new UpgradeLevel { id = id, level = level });
        }
    }

    public static class FGSave
    {
        public const string Key = "floatgardens_unity_v1";
        const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static SaveData Load()
        {
            try
            {
                string raw = PlayerPrefs.GetString(Key, "");
                if (!string.IsNullOrEmpty(raw))
                {
                    var data = JsonUtility.FromJson<SaveData>(raw) ?? new SaveData();
                    // Partidas anteriores al rebranding: la entrada "coffeeland" del Códice ahora es "arcadia".
                    int old = data.codex.IndexOf("coffeeland");
                    if (old >= 0) { if (data.codex.Contains("arcadia")) data.codex.RemoveAt(old); else data.codex[old] = "arcadia"; }
                    return data;
                }
            }
            catch (Exception e) { Debug.LogWarning("Float Gardens: no se pudo leer el guardado. " + e.Message); }
            return new SaveData();
        }

        public static void Write(SaveData data)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        // Cupón con dígito de control, mismo formato que la versión web (FG-XXX-XXX-XX).
        // OJO: para regalar café de verdad hace falta un backend que registre cada código y lo marque como usado.
        public static string CreateVoucher()
        {
            var rng = new System.Random(Guid.NewGuid().GetHashCode());
            char[] body = new char[6];
            for (int i = 0; i < 6; i++) body[i] = Alphabet[rng.Next(Alphabet.Length)];
            string b = new string(body);
            return "FG-" + b.Substring(0, 3) + "-" + b.Substring(3) + "-" + Checksum(b);
        }

        public static bool IsValidFormat(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            string[] parts = code.Trim().ToUpperInvariant().Split('-');
            if (parts.Length != 4 || parts[0] != "FG" || parts[1].Length != 3 || parts[2].Length != 3) return false;
            return Checksum(parts[1] + parts[2]) == parts[3];
        }

        static string Checksum(string body)
        {
            int h = 7;
            foreach (char ch in body) h = (h * 31 + ch) % 1024;
            return Alphabet[h % 32].ToString() + Alphabet[(h >> 5) % 32];
        }
    }
}
