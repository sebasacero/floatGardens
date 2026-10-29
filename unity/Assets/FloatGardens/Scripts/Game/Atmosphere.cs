// Atmósfera bogotana con look de cine y ciclo de día y noche: sol bajo con sombras largas, bruma, reflejos en
// tiempo real, etalonaje (tonos fríos en sombras y cálidos en luces), grano de película, profundidad de campo,
// lluvia, y de noche una ciudad de neón: la luna, el cielo oscuro, los postes y los letreros iluminando de verdad.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FloatGardens
{
    public class Atmosphere : MonoBehaviour
    {
        public Light sun;
        public bool Raining { get; private set; }
        /// <summary>Hora del día (0-24). Arranca en la hora dorada y un día completo dura 14 minutos.</summary>
        public float Hour = 17.3f;
        /// <summary>0 = pleno día, 1 = noche cerrada.</summary>
        public float Night { get; private set; }
        /// <summary>0-1: cuánto sol rasante hay (amanecer y atardecer tiñen las nubes).</summary>
        public float Dusk { get; private set; }
        Scenery scenery;
        const float HoursPerSecond = 24f / 840f;

        ParticleSystem rain;
        Material sky;
        float timer = 40;
        ColorAdjustments color;
        DepthOfField dof;
        Bloom bloom;
        SplitToning split;
        Vignette vignette;
        ScreenSpaceLensFlare flare;
        ReflectionProbe probe;
        float probeTimer;
        Vector3 probePos = new Vector3(float.MaxValue, 0, 0);
        float probeNight = -1;
        bool probeRain;
        float daylightTimer;
        float lastNight = -1;
        bool wasNight;

        static readonly Color DryFog = new Color32(0xb4, 0xc2, 0xcc, 255);
        static readonly Color WetFog = new Color32(0x8a, 0x96, 0xa0, 255);
        static readonly Color NightFog = new Color32(0x16, 0x17, 0x2a, 255);
        static readonly Color SunDry = new Color32(0xff, 0xe2, 0xc0, 255);
        static readonly Color SunWet = new Color32(0xd6, 0xde, 0xe8, 255);
        static readonly Color SunDusk = new Color32(0xff, 0x8e, 0x45, 255);
        static readonly Color Moon = new Color32(0x8f, 0xa8, 0xff, 255);
        static readonly Color SkyDay = new Color32(0x92, 0xae, 0xc9, 255);
        static readonly Color SkyNight = new Color32(0x2c, 0x1f, 0x4e, 255);
        static readonly Color SplitDay = new Color(0.36f, 0.52f, 0.58f);
        static readonly Color SplitNight = new Color(0.42f, 0.32f, 0.64f);
        static readonly Color LampColor = new Color(1f, 0.82f, 0.58f);
        // Bruma más ligera que antes: los cerros y las nubes del horizonte se ven desde la ciudad en vez de borrarse
        const float DryFogDensity = 0.0026f, WetFogDensity = 0.0046f, NightFogDensity = 0.0036f;

        // Maqueta (versión móvil, FGStyle.Miniatura): bruma lineal verde-gris que sigue el foco de la cámara,
        // etalonaje más apagado (sombras verde azulado, luces ámbar) y tilt-shift con la profundidad de campo Bokeh.
        static readonly Color MiniHaze = new Color32(0x9f, 0xb2, 0xae, 255);
        static readonly Color MiniHazeWet = new Color32(0x86, 0x96, 0x95, 255);
        static readonly Color MiniHazeNight = new Color32(0x18, 0x20, 0x2e, 255);
        static readonly Color MiniSplitDay = new Color32(0x5f, 0x8c, 0x8a, 255);
        static readonly Color MiniSplitNight = new Color32(0x4a, 0x5f, 0x9a, 255);
        const float MiniSaturation = -14f, MiniNightSaturation = -8f;
        Camera cam;
        ThirdPersonCamera rig;
        const float DrySaturation = 8f, WetSaturation = -12f, NightSaturation = 16f;

        // Luces reales de la calle: un grupo de luces puntuales que se van moviendo a los postes y neones
        // más cercanos a la cámara (así la noche ilumina de verdad sin tener 800 luces en la escena)
        const int LampLights = 14, NeonLights = 10;
        // En celular cada luz puntual cuesta en todos los objetos que toca: menos luces, sondas más pequeñas y sin los
        // efectos de cámara más caros (los que menos se notan en una pantalla pequeña).
        static bool Mobile => Application.isMobilePlatform;
        Light[] lampPool, neonPool;
        float lightTimer;
        readonly List<KeyValuePair<float, int>> nearest = new List<KeyValuePair<float, int>>();

        public void Setup(Camera cam)
        {
            this.cam = cam;
            // Sol (de noche hace de luna, desde el lado contrario)
            var sunGo = new GameObject("Sol");
            sunGo.transform.SetParent(transform, false);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = SunDry;
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.shadowBias = 0.04f;
            sun.shadowNormalBias = 0.3f;
            RenderSettings.sun = sun;

            // Cielo procedural (plantilla FG_Sky en Resources)
            sky = FGMaterials.New("FG_Sky", "CieloBogota");
            if (sky.shader != null && sky.shader.name == "Skybox/Procedural")
            {
                sky.SetFloat("_SunSize", 0.035f);
                sky.SetFloat("_SunSizeConvergence", 6f);
                sky.SetFloat("_AtmosphereThickness", 1.25f);
                sky.SetColor("_SkyTint", SkyDay);
                sky.SetColor("_GroundColor", new Color32(0x4d, 0x52, 0x4e, 255));
                sky.SetFloat("_Exposure", 1.1f);
                RenderSettings.skybox = sky;
            }
            else sky = null;
            // Luz ambiente en tres tonos: sombras azuladas de día, violeta de neón de noche
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 1f;
            // Bruma de montaña: niebla exponencial, la ciudad se pierde suave hacia los cerros
            RenderSettings.fog = true;
            RenderSettings.fogMode = FGStyle.Miniatura ? FogMode.Linear : FogMode.ExponentialSquared;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

            BuildReflections();
            BuildStreetLights();
            if (FGMaterials.Urp) BuildPostProcessing(cam);
            else Debug.LogWarning("Float Gardens: el proyecto no tiene URP activo, así que no hay posprocesado. " +
                                  "Corre el menú Float Gardens > Configurar todo y crear escena.");

            BuildRain(cam.transform);
            scenery = gameObject.AddComponent<Scenery>();
            scenery.Setup(cam, City.I);
            UpdateDaylight();
            wasNight = Night > 0.5f;
        }

        void BuildPostProcessing(Camera cam)
        {
            var volGo = new GameObject("Posprocesado");
            volGo.transform.SetParent(transform, false);
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.9f);
            bloom.threshold.Override(0.95f);
            bloom.scatter.Override(0.72f);
            bloom.tint.Override(new Color(1f, 0.93f, 0.85f));

            // Destellos de lente del sol y los neones (usa el bloom). En celular se omite: es un pase de pantalla completa.
            if (!Mobile)
            {
                flare = profile.Add<ScreenSpaceLensFlare>(true);
                flare.intensity.Override(0.35f);
            }

            // Etalonaje: blancos un poco cálidos, sombras verde azulado, luces ámbar
            color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.1f);
            color.contrast.Override(18f);
            color.saturation.Override(DrySaturation);
            var wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(6f);
            wb.tint.Override(2f);
            split = profile.Add<SplitToning>(true);
            split.shadows.Override(SplitDay);
            split.highlights.Override(new Color(0.86f, 0.68f, 0.5f));
            split.balance.Override(-15f);
            var lgg = profile.Add<LiftGammaGain>(true);
            lgg.lift.Override(new Vector4(0.98f, 1f, 1.03f, -0.03f));
            lgg.gain.Override(new Vector4(1.03f, 1f, 0.97f, 0.02f));

            // Lente: viñeta, aberración cromática leve y grano de película
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);
            var chroma = profile.Add<ChromaticAberration>(true);
            chroma.intensity.Override(0.08f);
            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.2f);
            grain.response.Override(0.8f);

            // Profundidad de campo: solo desenfoca lo lejano, nunca la zona de combate
            dof = profile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Gaussian);
            dof.gaussianStart.Override(70f);
            dof.gaussianEnd.Override(260f);
            dof.gaussianMaxRadius.Override(1f);
            dof.highQualitySampling.Override(!Mobile);
            if (FGStyle.Miniatura)
            {
                // Tilt-shift: lente larga muy abierta enfocada en el barista; lo de adelante y lo de atrás se desenfoca y
                // la ciudad parece una maqueta. LateUpdate mueve el foco con la cámara.
                dof.mode.Override(DepthOfFieldMode.Bokeh);
                dof.focusDistance.Override(90f);
                dof.focalLength.Override(200f);
                dof.aperture.Override(2.8f);
                dof.bladeCount.Override(6);
                color.contrast.Override(10f);
                bloom.threshold.Override(0.92f);
                bloom.scatter.Override(0.5f);
                split.highlights.Override(new Color32(0xd8, 0xb4, 0x8a, 255));
                split.balance.Override(-20f);
                chroma.active = false;
            }

            // Barrido de cámara al girar rápido (no en celular: cuesta mucho y casi no se nota en pantalla pequeña)
            if (!Mobile)
            {
                var blur = profile.Add<MotionBlur>(true);
                blur.mode.Override(MotionBlurMode.CameraOnly);
                blur.quality.Override(MotionBlurQuality.Medium);
                blur.intensity.Override(0.25f);
                blur.clamp.Override(0.04f);
            }

            volume.profile = profile;

            cam.allowHDR = true;
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = Mobile ? AntialiasingQuality.Low : AntialiasingQuality.High;
                data.dithering = true;
                data.stopNaN = true;
            }
        }

        // Sonda de reflejos que sigue al jugador: el agua, el vidrio y el asfalto mojado reflejan la ciudad de verdad
        void BuildReflections()
        {
            var go = new GameObject("Reflejos");
            go.transform.SetParent(transform, false);
            probe = go.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.resolution = Mobile ? 64 : 128;
            probe.size = new Vector3(160, 80, 160);
            probe.nearClipPlane = 0.5f;
            probe.farClipPlane = 260;
            probe.hdr = true;
            probe.shadowDistance = 60;
            probe.clearFlags = ReflectionProbeClearFlags.Skybox;
            probe.importance = 1;
        }

        // La sonda renderiza la ciudad 6 veces (una por cara del cubo): solo se repite cuando algo cambió de verdad
        // (la cámara se movió, cambió la luz o empezó a llover), y como máximo cada 1,5 s.
        void UpdateReflections(float dt, Camera cam)
        {
            if (probe == null) return;
            probeTimer -= dt;
            if (probeTimer > 0) return;
            Vector3 pos = cam.transform.position + Vector3.up * 2;
            bool moved = (pos - probePos).sqrMagnitude > 8f * 8f;
            bool relit = Mathf.Abs(Night - probeNight) > 0.04f || Raining != probeRain;
            if (!moved && !relit && probeTimer > -12f) return; // y de todas formas cada ~12 s (nubes, neones)
            probeTimer = 1.5f;
            probePos = pos;
            probeNight = Night;
            probeRain = Raining;
            probe.transform.position = pos;
            probe.RenderProbe();
        }

        void BuildStreetLights()
        {
            var root = new GameObject("LucesDeCalle").transform;
            root.SetParent(transform, false);
            lampPool = MakePool(root, Mobile ? 6 : LampLights, "Poste", 16f);
            neonPool = MakePool(root, Mobile ? 4 : NeonLights, "Neon", 12f);
        }

        static Light[] MakePool(Transform root, int count, string name, float range)
        {
            var pool = new Light[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject(name + " " + i);
                go.transform.SetParent(root, false);
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = range;
                l.intensity = 0;
                l.shadows = LightShadows.None;
                l.enabled = false;
                pool[i] = l;
            }
            return pool;
        }

        void UpdateStreetLights(float dt, Camera cam)
        {
            lightTimer -= dt;
            if (lightTimer > 0 || City.I == null) return;
            lightTimer = 0.4f;
            if (Night < 0.03f)
            {
                foreach (var l in lampPool) l.enabled = false;
                foreach (var l in neonPool) l.enabled = false;
                return;
            }
            Vector3 eye = cam.transform.position;
            var lamps = City.I.lampHeads;
            nearest.Clear();
            for (int i = 0; i < lamps.Count; i++) nearest.Add(new KeyValuePair<float, int>((lamps[i] - eye).sqrMagnitude, i));
            nearest.Sort(ByDistance);
            for (int k = 0; k < lampPool.Length; k++)
            {
                var l = lampPool[k];
                if (k >= nearest.Count) { l.enabled = false; continue; }
                l.enabled = true;
                l.transform.position = lamps[nearest[k].Value] - Vector3.up * 0.5f;
                l.color = LampColor;
                l.intensity = 3.4f * Night;
            }
            var neons = City.I.neonSpots;
            nearest.Clear();
            for (int i = 0; i < neons.Count; i++) nearest.Add(new KeyValuePair<float, int>((neons[i].pos - eye).sqrMagnitude, i));
            nearest.Sort(ByDistance);
            for (int k = 0; k < neonPool.Length; k++)
            {
                var l = neonPool[k];
                if (k >= nearest.Count) { l.enabled = false; continue; }
                var spot = neons[nearest[k].Value];
                l.enabled = true;
                l.transform.position = spot.pos;
                l.color = spot.color;
                l.intensity = 2.4f * Night;
            }
        }

        static readonly System.Comparison<KeyValuePair<float, int>> ByDistance = (a, b) => a.Key.CompareTo(b.Key);

        void BuildRain(Transform camT)
        {
            var go = new GameObject("Lluvia");
            go.transform.SetParent(transform, false);
            rain = go.AddComponent<ParticleSystem>();
            rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = rain.main;
            main.loop = true;
            main.startLifetime = 1.4f;
            main.startSpeed = 0;
            main.startSize = 0.05f;
            main.maxParticles = Mobile ? 2000 : 4000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.82f, 0.87f, 0.92f, 0.55f);
            var vel = rain.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-1.5f);
            vel.y = new ParticleSystem.MinMaxCurve(-26f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            var em = rain.emission;
            em.rateOverTime = Mobile ? 1100 : 2200;
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(70, 1, 70);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.03f;
            r.lengthScale = 2f;
            r.sharedMaterial = FGMaterials.Particles("Lluvia");
        }

        /// <summary>Sol, luna, cielo, niebla y etalonaje según la hora y la lluvia.</summary>
        void UpdateDaylight()
        {
            float elev = Mathf.Sin((Hour - 6f) / 24f * Mathf.PI * 2f); // 1 al mediodía, -1 a medianoche
            float night = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.12f, -0.1f, elev));
            Night = night;
            float angle = (Hour - 6f) / 24f * 360f;
            bool moon = elev < 0;
            sun.transform.rotation = Quaternion.Euler(moon ? angle + 180f : angle, -40f, 0);
            float rainK = Raining ? 0.5f : 1f;
            Dusk = moon ? 0f : (1 - Mathf.Clamp01(elev / 0.3f)) * (1 - night);
            if (!moon)
            {
                float strength = Mathf.Clamp01(elev / 0.35f);
                float horizon = 1 - Mathf.Clamp01(elev / 0.3f);
                Color c = Color.Lerp(SunDry, SunDusk, horizon * 0.85f);
                sun.color = Raining ? Color.Lerp(c, SunWet, 0.7f) : c;
                sun.intensity = Mathf.Lerp(0.12f, 1.6f, strength) * rainK;
                sun.shadowStrength = 0.85f;
            }
            else
            {
                sun.color = Moon;
                sun.intensity = (0.05f + 0.22f * Mathf.Clamp01(-elev / 0.25f)) * rainK;
                sun.shadowStrength = 0.6f;
            }

            if (sky != null)
            {
                sky.SetFloat("_Exposure", Mathf.Lerp(Raining ? 0.75f : 1.1f, 0.05f, night));
                sky.SetColor("_SkyTint", Color.Lerp(SkyDay, SkyNight, night));
            }
            Color dayFog = FGStyle.Miniatura ? (Raining ? MiniHazeWet : MiniHaze) : Raining ? WetFog : DryFog;
            RenderSettings.fogColor = Color.Lerp(dayFog, FGStyle.Miniatura ? MiniHazeNight : NightFog, night);
            RenderSettings.fogDensity = Mathf.Lerp(Raining ? WetFogDensity : DryFogDensity, NightFogDensity, night);
            float dim = Raining ? 0.8f : 1f;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color32(0xc9, 0xd8, 0xe6, 255), new Color32(0x2a, 0x2b, 0x4c, 255), night) * dim;
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color32(0x9a, 0xa6, 0xae, 255), new Color32(0x24, 0x1c, 0x36, 255), night) * dim;
            RenderSettings.ambientGroundColor = Color.Lerp(new Color32(0x4a, 0x4f, 0x45, 255), new Color32(0x0c, 0x0b, 0x14, 255), night) * dim;

            if (color != null)
            {
                color.postExposure.Override(Mathf.Lerp(0.1f, 0.45f, night));
                color.saturation.Override(Mathf.Lerp(Raining ? WetSaturation : DrySaturation, NightSaturation, night));
                bloom.intensity.Override(Mathf.Lerp(0.9f, 1.6f, night));
                split.shadows.Override(Color.Lerp(SplitDay, SplitNight, night));
                vignette.intensity.Override(Mathf.Lerp(0.3f, 0.4f, night));
                if (FGStyle.Miniatura)
                {
                    color.postExposure.Override(Mathf.Lerp(0f, 0.05f, night));
                    color.saturation.Override(Mathf.Lerp(Raining ? MiniSaturation - 8f : MiniSaturation, MiniNightSaturation, night));
                    bloom.intensity.Override(Mathf.Lerp(0.55f, 0.9f, night));
                    split.shadows.Override(Color.Lerp(MiniSplitDay, MiniSplitNight, night));
                    vignette.intensity.Override(Mathf.Lerp(0.25f, 0.3f, night));
                }
                if (flare != null) flare.intensity.Override(Mathf.Lerp(0.35f, 0.7f, night));
            }

            if (Mathf.Abs(night - lastNight) > 0.01f)
            {
                lastNight = night;
                if (City.I != null) City.I.SetNight(night);
            }
        }

        // La maqueta enfoca y despeja la bruma alrededor de lo que mira la cámara (también en el lobby, donde no hay Tick)
        void LateUpdate()
        {
            if (!FGStyle.Miniatura || cam == null) return;
            if (rig == null) rig = cam.GetComponent<ThirdPersonCamera>();
            if (rig == null) return;
            float focus = rig.Focus;
            RenderSettings.fogStartDistance = focus * 0.75f;
            RenderSettings.fogEndDistance = focus * 3.2f;
            if (dof != null)
            {
                dof.focusDistance.Override(focus);
                // 200 mm jugando (unos 90 m), 300 mm en el paneo del lobby (420 m): el desenfoque se nota igual en los dos
                dof.focalLength.Override(Mathf.Lerp(200f, 300f, Mathf.InverseLerp(90f, 420f, focus)));
            }
        }

        public void Tick(float dt, Camera cam, HUD hud)
        {
            if (rain != null) rain.transform.position = cam.transform.position + Vector3.up * 22;
            UpdateReflections(dt, cam);
            UpdateStreetLights(dt, cam);
            if (scenery != null) scenery.Tick(dt, Night, Dusk, Raining, cam);

            Hour = Mathf.Repeat(Hour + dt * HoursPerSecond, 24f);
            // Un día dura 14 minutos: el sol avanza 0,04° en una décima de segundo. Recalcular cielo, niebla, luz
            // ambiente y posprocesado 10 veces por segundo se ve igual y evita hacerlo 60+ veces.
            daylightTimer -= dt;
            if (daylightTimer <= 0)
            {
                daylightTimer = 0.1f;
                UpdateDaylight();
            }
            bool isNight = Night > 0.5f;
            if (isNight != wasNight)
            {
                wasNight = isNight;
                hud.Toast(isNight ? "Cae la noche sobre Bogotá" : "Amanece en Bogotá");
            }

            timer -= dt;
            if (timer > 0) return;
            Raining = !Raining;
            timer = Raining ? Random.Range(35f, 60f) : Random.Range(50f, 90f);
            if (Raining) rain.Play(); else rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            daylightTimer = 0; // niebla y luz de lluvia al instante
            if (dof != null) dof.gaussianStart.Override(Raining ? 45f : 70f);
            probeTimer = 0;
            // asfalto mojado: más brillante y reflectivo
            if (City.I != null && City.I.asphaltMat != null) { City.I.asphaltMat.SetFloat("_Smoothness", Raining ? 0.7f : 0.12f); City.I.asphaltMat.SetFloat("_Glossiness", Raining ? 0.7f : 0.12f); }
            hud.Toast(Raining ? "Empezó a llover en Bogotá" : "Escampó");
        }
    }
}
