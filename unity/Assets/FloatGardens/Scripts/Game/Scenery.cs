// Escenografía que rodea la ciudad y reacciona al clima y a la hora: nubes que derivan por el cielo y se tiñen al
// atardecer, estrellas de noche, conos de luz bajo los postes (se ven más con lluvia), salpicaduras de las gotas en
// el piso y asfalto que se moja y refleja. También genera las texturas y mallas de apoyo (ruido, bosque de los cerros).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FloatGardens
{
    public class Scenery : MonoBehaviour
    {
        public static Scenery I;

        Transform clouds, cloudsHigh, stars;
        Material cloudMat, cloudHighMat, starMat, coneMat;
        ParticleSystem splash;
        ParticleSystem.EmissionModule splashEmission;
        Material asphalt;
        float wet, colorTimer;
        Vector2 wind = new Vector2(0.004f, 0.0015f);

        static bool Mobile => Application.isMobilePlatform;
        static readonly Color CloudDay = new Color(1f, 1f, 1f, 0.92f);
        static readonly Color CloudDusk = new Color(1f, 0.66f, 0.46f, 0.95f);
        static readonly Color CloudNight = new Color(0.16f, 0.17f, 0.3f, 0.85f);
        static readonly Color CloudRain = new Color(0.62f, 0.66f, 0.7f, 1f);
        static readonly Color ConeColor = new Color(1f, 0.84f, 0.6f);

        public void Setup(Camera cam, City city)
        {
            I = this;
            BuildSky();
            if (city != null)
            {
                BuildLightCones(city.lampHeads);
                asphalt = city.asphaltMat;
            }
            BuildSplashes();
        }

        // ---------- Cielo: dos capas de nubes y una de estrellas sobre cúpulas que siguen a la cámara ----------
        void BuildSky()
        {
            var dome = Dome(28, 10);
            // La bruma de la ciudad también alcanza las cúpulas: por eso están cerca (380 m) y aplastadas, así las
            // nubes del cenit se ven limpias y las del horizonte se funden con la niebla, como pasa de verdad.
            cloudMat = SkyMaterial("Nubes", CloudTexture(512, 0.52f, 7), 3001);
            cloudMat.mainTextureScale = new Vector2(3f, 3f);
            clouds = SkyLayer("Nubes", dome, cloudMat, new Vector3(380, 150, 380));
            cloudHighMat = SkyMaterial("NubesAltas", CloudTexture(512, 0.62f, 11), 3000);
            cloudHighMat.mainTextureScale = new Vector2(5f, 5f);
            cloudsHigh = SkyLayer("NubesAltas", dome, cloudHighMat, new Vector3(400, 175, 400));
            starMat = SkyMaterial("Estrellas", StarTexture(1024, 512, 1800), 2999);
            starMat.color = new Color(1, 1, 1, 0);
            stars = SkyLayer("Estrellas", dome, starMat, new Vector3(410, 190, 410));
        }

        Material SkyMaterial(string name, Texture2D tex, int queue)
        {
            var m = FGMaterials.New("FG_UnlitTransparent", name);
            m.mainTexture = tex;
            m.renderQueue = queue;
            return m;
        }

        Transform SkyLayer(string name, Mesh mesh, Material mat, Vector3 scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return go.transform;
        }

        /// <summary>Media esfera de radio 1 vista desde adentro, con UV planos (las nubes se proyectan desde arriba).</summary>
        static Mesh Dome(int segments, int rings)
        {
            var v = new List<Vector3>();
            var uv = new List<Vector2>();
            var t = new List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float phi = r / (float)rings * Mathf.PI / 2f; // 0 = horizonte, PI/2 = cenit
                for (int s = 0; s <= segments; s++)
                {
                    float th = s / (float)segments * Mathf.PI * 2f;
                    var p = new Vector3(Mathf.Cos(th) * Mathf.Cos(phi), Mathf.Sin(phi), Mathf.Sin(th) * Mathf.Cos(phi));
                    v.Add(p);
                    uv.Add(new Vector2(p.x * 0.5f + 0.5f, p.z * 0.5f + 0.5f));
                }
            }
            int w = segments + 1;
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < segments; s++)
                {
                    int a = r * w + s, b = a + 1, c = a + w, d = c + 1;
                    t.Add(a); t.Add(b); t.Add(c);
                    t.Add(b); t.Add(d); t.Add(c);
                }
            var m = new Mesh { name = "Cupula" };
            m.SetVertices(v);
            m.SetUVs(0, uv);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // ---------- Conos de luz de los postes: una sola malla aditiva para todos ----------
        void BuildLightCones(List<Vector3> heads)
        {
            if (heads == null || heads.Count == 0) return;
            var cone = LightCone(14);
            var mb = new MeshBuilder();
            foreach (var h in heads)
                mb.Add(cone, Matrix4x4.TRS(h - Vector3.up * 0.1f, Quaternion.identity, new Vector3(4.6f, h.y - 0.3f, 4.6f)));
            coneMat = FGMaterials.New("FG_UnlitTransparent", "ConoDeLuz");
            coneMat.mainTexture = GradientTexture(8, 64, 1.7f);
            coneMat.color = new Color(ConeColor.r, ConeColor.g, ConeColor.b, 0);
            coneMat.renderQueue = 3002;
            // Aditivo: la luz suma sobre lo que hay detrás (URP Unlit expone las mezclas como propiedades)
            if (coneMat.HasProperty("_SrcBlend")) coneMat.SetFloat("_SrcBlend", (float)BlendMode.One);
            if (coneMat.HasProperty("_DstBlend")) coneMat.SetFloat("_DstBlend", (float)BlendMode.One);
            if (coneMat.HasProperty("_SrcBlendAlpha")) coneMat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            if (coneMat.HasProperty("_DstBlendAlpha")) coneMat.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
            var go = mb.Build("ConosDeLuz", coneMat, transform, false);
            var r = go.GetComponent<MeshRenderer>();
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        /// <summary>Cono con la punta en el origen y la base (radio 1) en y = -1; v = 1 en la punta, 0 en la base.</summary>
        static Mesh LightCone(int segments)
        {
            var v = new List<Vector3> { Vector3.zero };
            var uv = new List<Vector2> { new Vector2(0.5f, 1f) };
            var t = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a), -1f, Mathf.Sin(a)));
                uv.Add(new Vector2(i / (float)segments, 0f));
            }
            for (int i = 1; i <= segments; i++) { t.Add(0); t.Add(i); t.Add(i + 1); }
            var m = new Mesh { name = "ConoDeLuz" };
            m.SetVertices(v);
            m.SetUVs(0, uv);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // ---------- Salpicaduras de lluvia: anillos que se abren en el piso alrededor de la cámara ----------
        void BuildSplashes()
        {
            var go = new GameObject("Salpicaduras");
            go.transform.SetParent(transform, false);
            splash = go.AddComponent<ParticleSystem>();
            splash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = splash.main;
            main.loop = true;
            main.startLifetime = 0.4f;
            main.startSpeed = 0;
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startColor = new Color(0.85f, 0.9f, 0.95f, 0.5f);
            main.maxParticles = Mobile ? 300 : 700;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            splashEmission = splash.emission;
            splashEmission.rateOverTime = 0;
            var shape = splash.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(44, 0.01f, 44);
            var size = splash.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 0.15f), new Keyframe(1, 1f)));
            var col = splash.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                      new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            var mat = FGMaterials.Particles("Salpicadura");
            mat.mainTexture = RingTexture(32);
            r.sharedMaterial = mat;
            splash.Play();
        }

        /// <summary>night: 0 día, 1 noche. dusk: 0-1 cuánto sol rasante (atardecer o amanecer).</summary>
        public void Tick(float dt, float night, float dusk, bool raining, Camera cam)
        {
            Vector3 eye = cam.transform.position;
            Vector3 ground = new Vector3(eye.x, 0, eye.z);
            clouds.position = ground;
            cloudsHigh.position = ground;
            stars.position = ground;
            if (splash != null) splash.transform.position = ground + Vector3.up * 0.06f;

            cloudMat.mainTextureOffset += wind * dt;
            cloudHighMat.mainTextureOffset += wind * (0.45f * dt);
            stars.Rotate(0, dt * 0.15f, 0, Space.World);

            // El asfalto se moja despacio y se seca más despacio todavía
            wet = Mathf.MoveTowards(wet, raining ? 1f : 0f, dt * (raining ? 0.12f : 0.05f));

            colorTimer -= dt;
            if (colorTimer > 0) return;
            colorTimer = 0.1f;

            Color c = Color.Lerp(Color.Lerp(CloudDay, CloudDusk, dusk), CloudNight, night);
            if (raining) c = Color.Lerp(c, CloudRain * new Color(1, 1, 1, 1) * (1f - night * 0.8f), 0.8f);
            cloudMat.color = c;
            Color hi = c; hi.a *= 0.7f;
            cloudHighMat.color = hi;
            starMat.color = new Color(1, 1, 1, Mathf.Clamp01(night * 1.2f - 0.2f) * (raining ? 0.25f : 1f));

            if (coneMat != null)
            {
                float k = night * (raining ? 0.5f : 0.28f);
                coneMat.color = new Color(ConeColor.r * k, ConeColor.g * k, ConeColor.b * k, k);
            }
            if (asphalt != null)
            {
                asphalt.SetFloat("_Smoothness", Mathf.Lerp(0.12f, 0.8f, wet));
                asphalt.SetFloat("_Glossiness", Mathf.Lerp(0.12f, 0.8f, wet));
                asphalt.color = Color.Lerp(Color.white, new Color(0.55f, 0.56f, 0.58f), wet);
            }
            float rate = raining ? (Mobile ? 500 : 1300) : 0;
            if (Mathf.Abs(splashEmission.rateOverTime.constant - rate) > 1) splashEmission.rateOverTime = rate;
        }

        // ---------- Ruido y texturas generadas ----------
        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1274126177;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0x7fffffff) / (float)0x7fffffff;
            }
        }

        /// <summary>Ruido de valor suave que se repite cada period celdas (para texturas sin costuras).</summary>
        public static float Noise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            int X0 = ((x0 % period) + period) % period, X1 = (X0 + 1) % period;
            int Y0 = ((y0 % period) + period) % period, Y1 = (Y0 + 1) % period;
            float a = Hash(X0, Y0, seed), b = Hash(X1, Y0, seed), c = Hash(X0, Y1, seed), d = Hash(X1, Y1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        /// <summary>Ruido fractal 0-1: varias octavas de Noise, cada una el doble de fina y la mitad de fuerte.</summary>
        public static float Fbm(float x, float y, int period, int octaves, int seed)
        {
            float sum = 0, amp = 0.5f, norm = 0;
            for (int o = 0; o < octaves; o++)
            {
                sum += Noise(x, y, period, seed + o * 31) * amp;
                norm += amp;
                x *= 2; y *= 2; period *= 2; amp *= 0.5f;
            }
            return sum / norm;
        }

        static Texture2D CloudTexture(int n, float cover, int seed)
        {
            var px = new Color32[n * n];
            float cells = 4f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float f = Fbm(x / (float)n * cells, y / (float)n * cells, (int)cells, 6, seed);
                    float a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(cover, cover + 0.22f, f));
                    float shade = Mathf.Lerp(1f, 0.72f, Mathf.InverseLerp(cover + 0.1f, cover + 0.4f, f)); // panza más gris
                    byte v = (byte)(shade * 255);
                    px[y * n + x] = new Color32(v, v, v, (byte)(a * 255));
                }
            return Make(px, n, n, "Nubes", TextureWrapMode.Repeat);
        }

        static Texture2D StarTexture(int w, int h, int count)
        {
            var px = new Color32[w * h];
            var rng = new System.Random(77);
            for (int i = 0; i < count; i++)
            {
                int x = rng.Next(w), y = rng.Next(h);
                float b = (float)rng.NextDouble();
                b = b * b * b; // pocas estrellas brillantes, muchas tenues
                byte v = (byte)(60 + b * 195);
                Color32 c = rng.NextDouble() < 0.15 ? new Color32((byte)(v * 0.85f), (byte)(v * 0.9f), v, v) : new Color32(v, v, (byte)(v * 0.95f), v);
                px[y * w + x] = c;
                if (b > 0.6f)
                {
                    byte s = (byte)(v * 0.35f);
                    Color32 soft = new Color32(c.r, c.g, c.b, s);
                    if (x > 0) px[y * w + x - 1] = soft;
                    if (x < w - 1) px[y * w + x + 1] = soft;
                    if (y > 0) px[(y - 1) * w + x] = soft;
                    if (y < h - 1) px[(y + 1) * w + x] = soft;
                }
            }
            return Make(px, w, h, "Estrellas", TextureWrapMode.Repeat);
        }

        static Texture2D GradientTexture(int w, int h, float power)
        {
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                byte a = (byte)(Mathf.Pow(y / (float)(h - 1), power) * 255);
                for (int x = 0; x < w; x++) px[y * w + x] = new Color32(255, 255, 255, a);
            }
            return Make(px, w, h, "Degradado", TextureWrapMode.Clamp);
        }

        static Texture2D RingTexture(int n)
        {
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = 1 - Mathf.Clamp01(Mathf.Abs(d - 0.72f) / 0.16f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * a * 255));
                }
            return Make(px, n, n, "Anillo", TextureWrapMode.Clamp);
        }

        /// <summary>Textura de bosque para los cerros: copas verdes de distinto tono, claros y rocas en lo más alto.</summary>
        public static Texture2D ForestTexture(int n, out Texture2D normal)
        {
            var px = new Color32[n * n];
            var hgt = new float[n * n];
            Color dark = FGMaterials.Hex("#28502f"), mid = FGMaterials.Hex("#3b7338"), light = FGMaterials.Hex("#6a9a3c"), rock = FGMaterials.Hex("#6e6a5e");
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = x / (float)n, v = y / (float)n;
                    float canopy = Fbm(u * 24, v * 24, 24, 4, 5);      // copas individuales
                    float patches = Fbm(u * 3, v * 3, 3, 3, 9);        // manchas de especies
                    float h = canopy * 0.75f + patches * 0.25f;
                    Color c = Color.Lerp(dark, mid, Mathf.InverseLerp(0.3f, 0.6f, canopy));
                    c = Color.Lerp(c, light, Mathf.Clamp01((patches - 0.55f) * 3f) * canopy);
                    if (patches < 0.3f && canopy > 0.62f) c = Color.Lerp(c, rock, 0.7f);
                    px[y * n + x] = c;
                    hgt[y * n + x] = h;
                }
            var nm = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float l = hgt[y * n + (x + n - 1) % n], r = hgt[y * n + (x + 1) % n];
                    float d = hgt[((y + n - 1) % n) * n + x], u = hgt[((y + 1) % n) * n + x];
                    var nrm = new Vector3((l - r) * 6f, (d - u) * 6f, 1f).normalized;
                    nm[y * n + x] = new Color32((byte)((nrm.x * 0.5f + 0.5f) * 255), (byte)((nrm.y * 0.5f + 0.5f) * 255), (byte)((nrm.z * 0.5f + 0.5f) * 255), 255);
                }
            normal = Make(nm, n, n, "BosqueRelieve", TextureWrapMode.Repeat, true);
            return Make(px, n, n, "Bosque", TextureWrapMode.Repeat);
        }

        static Texture2D Make(Color32[] px, int w, int h, string name, TextureWrapMode wrap, bool linear = false)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, true, linear) { name = name, wrapMode = wrap, filterMode = FilterMode.Trilinear, anisoLevel = 4 };
            t.SetPixels32(px);
            t.Apply(true, true);
            return t;
        }
    }
}
