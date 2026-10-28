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
        const float DryFogDensity = 0.0042f, WetFogDensity = 0.0068f, NightFogDensity = 0.0052f;
        const float DrySaturation = 8f, WetSaturation = -12f, NightSaturation = 16f;

        // Luces reales de la calle: un grupo de luces puntuales que se van moviendo a los postes y neones
        // más cercanos a la cámara (así la noche ilumina de verdad sin tener 800 luces en la escena)
        const int LampLights = 14, NeonLights = 10;
        Light[] lampPool, neonPool;
        float lightTimer;
        readonly List<KeyValuePair<float, int>> nearest = new List<KeyValuePair<float, int>>();

        public void Setup(Camera cam)
        {
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
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

            BuildReflections();
            BuildStreetLights();
            if (FGMaterials.Urp) BuildPostProcessing(cam);
            else Debug.LogWarning("Float Gardens: el proyecto no tiene URP activo, así que no hay posprocesado. " +
                                  "Corre el menú Float Gardens > Configurar todo y crear escena.");

            BuildRain(cam.transform);
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

            // Destellos de lente del sol y los neones (usa el bloom)
            flare = profile.Add<ScreenSpaceLensFlare>(true);
            flare.intensity.Override(0.35f);

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
            dof.highQualitySampling.Override(true);

            // Barrido de cámara al girar rápido
            var blur = profile.Add<MotionBlur>(true);
            blur.mode.Override(MotionBlurMode.CameraOnly);
            blur.quality.Override(MotionBlurQuality.Medium);
            blur.intensity.Override(0.25f);
            blur.clamp.Override(0.04f);

            volume.profile = profile;

            cam.allowHDR = true;
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
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
            probe.resolution = 128;
            probe.size = new Vector3(160, 80, 160);
            probe.nearClipPlane = 0.5f;
            probe.farClipPlane = 260;
            probe.hdr = true;
            probe.shadowDistance = 60;
            probe.clearFlags = ReflectionProbeClearFlags.Skybox;
            probe.importance = 1;
        }

        void UpdateReflections(float dt, Camera cam)
        {
            if (probe == null) return;
            probeTimer -= dt;
            if (probeTimer > 0) return;
            probeTimer = 1.5f;
            probe.transform.position = cam.transform.position + Vector3.up * 2;
            probe.RenderProbe();
        }

        void BuildStreetLights()
        {
            var root = new GameObject("LucesDeCalle").transform;
            root.SetParent(transform, false);
            lampPool = MakePool(root, LampLights, "Poste", 16f);
            neonPool = MakePool(root, NeonLights, "Neon", 12f);
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
            nearest.Sort((a, b) => a.Key.CompareTo(b.Key));
            for (int k = 0; k < lampPool.Length; k++)
            {
                var l = lampPool[k];
                if (k >= nearest.Count) { l.enabled = false; continue; }
                l.enabled = true;
                l.transform.position = lamps[nearest[k].Value] - Vector3.up * 0.5f;
                l.color = new Color(1f, 0.82f, 0.58f);
                l.intensity = 3.4f * Night;
            }
            var neons = City.I.neonSpots;
            nearest.Clear();
            for (int i = 0; i < neons.Count; i++) nearest.Add(new KeyValuePair<float, int>((neons[i].pos - eye).sqrMagnitude, i));
            nearest.Sort((a, b) => a.Key.CompareTo(b.Key));
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
            main.maxParticles = 4000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.82f, 0.87f, 0.92f, 0.55f);
            var vel = rain.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-1.5f);
            vel.y = new ParticleSystem.MinMaxCurve(-26f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            var em = rain.emission;
            em.rateOverTime = 2200;
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
            Color dayFog = Raining ? WetFog : DryFog;
            RenderSettings.fogColor = Color.Lerp(dayFog, NightFog, night);
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
                flare.intensity.Override(Mathf.Lerp(0.35f, 0.7f, night));
            }

            if (Mathf.Abs(night - lastNight) > 0.01f)
            {
                lastNight = night;
                if (City.I != null) City.I.SetNight(night);
            }
        }

        public void Tick(float dt, Camera cam, HUD hud)
        {
            if (rain != null) rain.transform.position = cam.transform.position + Vector3.up * 22;
            UpdateReflections(dt, cam);
            UpdateStreetLights(dt, cam);

            Hour = Mathf.Repeat(Hour + dt * HoursPerSecond, 24f);
            UpdateDaylight();
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
            if (dof != null) dof.gaussianStart.Override(Raining ? 45f : 70f);
            probeTimer = 0;
            // asfalto mojado: más brillante y reflectivo
            if (City.I != null && City.I.asphaltMat != null) { City.I.asphaltMat.SetFloat("_Smoothness", Raining ? 0.7f : 0.12f); City.I.asphaltMat.SetFloat("_Glossiness", Raining ? 0.7f : 0.12f); }
            hud.Toast(Raining ? "Empezó a llover en Bogotá" : "Escampó");
        }
    }
}
