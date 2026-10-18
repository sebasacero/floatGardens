// Efectos de sonido sintetizados en tiempo de ejecución (sin archivos de audio).
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class SFX : MonoBehaviour
    {
        public static SFX I;
        const int Rate = 22050;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource[] pool;
        int next;
        public bool muted;

        void Awake()
        {
            I = this;
            pool = new AudioSource[10];
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = gameObject.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
                pool[i].spatialBlend = 0;
            }
            var rng = new System.Random(7);
            clips["shot"] = Make("shot", 0.14f, (t, d) => Noise(rng) * Env(t, d, 0.003f) * 0.6f + Square(220 - 900 * t, t) * Env(t, 0.08f, 0.002f) * 0.25f);
            clips["hit"] = Make("hit", 0.07f, (t, d) => Tri(1100, t) * Env(t, d, 0.002f) * 0.5f);
            clips["head"] = Make("head", 0.08f, (t, d) => Tri(1500, t) * Env(t, d, 0.002f) * 0.5f);
            clips["ko"] = Make("ko", 0.3f, (t, d) => Tri(t < 0.09f ? 660 : 990, t) * Env(t, d, 0.004f) * 0.45f);
            clips["reload"] = Make("reload", 1.0f, (t, d) =>
                (t < 0.08f ? Noise(rng) * Env(t, 0.08f, 0.002f) : 0) +
                (t > 0.38f && t < 0.45f ? Noise(rng) * Env(t - 0.38f, 0.07f, 0.002f) : 0) +
                (t > 0.9f ? Square(500, t) * Env(t - 0.9f, 0.08f, 0.002f) * 0.3f : 0));
            clips["empty"] = Make("empty", 0.06f, (t, d) => Square(300, t) * Env(t, d, 0.002f) * 0.3f);
            clips["hurt"] = Make("hurt", 0.2f, (t, d) => Saw(140 - 300 * t, t) * Env(t, d, 0.004f) * 0.5f);
            clips["pickup"] = Make("pickup", 0.09f, (t, d) => Mathf.Sin(2 * Mathf.PI * (880 + 4000 * t) * t) * Env(t, d, 0.003f) * 0.35f);
            clips["cash"] = Make("cash", 0.22f, (t, d) => Tri(t < 0.07f ? 1046 : 1568, t) * Env(t, d, 0.003f) * 0.35f);
            clips["capture"] = Make("capture", 0.6f, (t, d) => Tri(new[] { 523f, 659f, 784f, 1046f }[Mathf.Min(3, (int)(t / 0.12f))], t) * Env(t, d, 0.004f) * 0.4f);
            clips["stamp"] = Make("stamp", 0.16f, (t, d) => Noise(rng) * Env(t, d, 0.002f) * 0.45f * (1 - t * 4));
            clips["jump"] = Make("jump", 0.08f, (t, d) => Mathf.Sin(2 * Mathf.PI * (300 + 2500 * t) * t) * Env(t, d, 0.003f) * 0.2f);
        }

        delegate float Synth(float t, float dur);

        static AudioClip Make(string name, float dur, Synth fn)
        {
            int n = Mathf.CeilToInt(dur * Rate);
            float[] data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(fn(i / (float)Rate, dur), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Env(float t, float dur, float attack) => t < attack ? t / attack : Mathf.Clamp01(1 - (t - attack) / Mathf.Max(0.001f, dur - attack));
        static float Noise(System.Random r) => (float)(r.NextDouble() * 2 - 1);
        static float Square(float f, float t) => Mathf.Sin(2 * Mathf.PI * f * t) >= 0 ? 1 : -1;
        static float Tri(float f, float t) => 1 - 4 * Mathf.Abs(Mathf.Repeat(f * t + 0.25f, 1f) - 0.5f);
        static float Saw(float f, float t) => 2 * Mathf.Repeat(f * t, 1f) - 1;

        public static void Play(string name, float volume = 1f, float pitchJitter = 0.05f)
        {
            if (I == null || I.muted || !I.clips.TryGetValue(name, out var clip)) return;
            var src = I.pool[I.next];
            I.next = (I.next + 1) % I.pool.Length;
            src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            src.PlayOneShot(clip, volume);
        }
    }
}
