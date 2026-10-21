// Atmósfera bogotana con look de cine: sol bajo con sombras largas, bruma, reflejos en tiempo real,
// etalonaje (tonos fríos en sombras y cálidos en luces), grano de película, profundidad de campo y lluvia.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FloatGardens
{
    public class Atmosphere : MonoBehaviour
    {
        public Light sun;
        public bool Raining { get; private set; }
        ParticleSystem rain;
        Material sky;
        float timer = 40;
        ColorAdjustments color;
        DepthOfField dof;
        ReflectionProbe probe;
        float probeTimer;
        static readonly Color DryFog = new Color32(0xb4, 0xc2, 0xcc, 255);
        static readonly Color WetFog = new Color32(0x8a, 0x96, 0xa0, 255);
        static readonly Color SunDry = new Color32(0xff, 0xe2, 0xc0, 255);
        static readonly Color SunWet = new Color32(0xd6, 0xde, 0xe8, 255);
        const float DryFogDensity = 0.0042f, WetFogDensity = 0.0068f;
        const float DrySaturation = 8f, WetSaturation = -12f;

        public void Setup(Camera cam)
        {
            // Sol de media tarde: bajo y cálido, para sombras largas
            var sunGo = new GameObject("Sol");
            sunGo.transform.SetParent(transform, false);
            sunGo.transform.rotation = Quaternion.Euler(32, -40, 0);
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
            bool hasSky = sky.shader != null && sky.shader.name == "Skybox/Procedural";
            if (hasSky)
            {
                sky.SetFloat("_SunSize", 0.035f);
                sky.SetFloat("_SunSizeConvergence", 6f);
                sky.SetFloat("_AtmosphereThickness", 1.25f);
                sky.SetColor("_SkyTint", new Color32(0x92, 0xae, 0xc9, 255));
                sky.SetColor("_GroundColor", new Color32(0x4d, 0x52, 0x4e, 255));
                sky.SetFloat("_Exposure", 1.1f);
                RenderSettings.skybox = sky;
            }
            // Luz ambiente tomada del cielo: las sombras quedan azuladas y el sol cálido (contraste de cine)
            if (hasSky)
            {
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1.0f;
            }
            else
            {
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color32(0xc9, 0xd8, 0xe6, 255);
                RenderSettings.ambientEquatorColor = new Color32(0x9a, 0xa6, 0xae, 255);
                RenderSettings.ambientGroundColor = new Color32(0x4a, 0x4f, 0x45, 255);
            }
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 1f;
            // Bruma de montaña: niebla exponencial, la ciudad se pierde suave hacia los cerros
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = DryFogDensity;
            RenderSettings.fogColor = DryFog;
            DynamicGI.UpdateEnvironment();
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

            BuildReflections();
            if (FGMaterials.Urp) BuildPostProcessing(cam);
            else Debug.LogWarning("Float Gardens: el proyecto no tiene URP activo, así que no hay posprocesado. " +
                                  "Corre el menú Float Gardens > Configurar todo y crear escena.");

            BuildRain(cam.transform);
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

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.9f);
            bloom.threshold.Override(0.95f);
            bloom.scatter.Override(0.72f);
            bloom.tint.Override(new Color(1f, 0.93f, 0.85f));

            // Destellos de lente del sol y los neones (usa el bloom)
            var flare = profile.Add<ScreenSpaceLensFlare>(true);
            flare.intensity.Override(0.35f);

            // Etalonaje: blancos un poco cálidos, sombras verde azulado, luces ámbar
            color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.1f);
            color.contrast.Override(18f);
            color.saturation.Override(DrySaturation);
            var wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(6f);
            wb.tint.Override(2f);
            var split = profile.Add<SplitToning>(true);
            split.shadows.Override(new Color(0.36f, 0.52f, 0.58f));
            split.highlights.Override(new Color(0.86f, 0.68f, 0.5f));
            split.balance.Override(-15f);
            var lgg = profile.Add<LiftGammaGain>(true);
            lgg.lift.Override(new Vector4(0.98f, 1f, 1.03f, -0.03f));
            lgg.gain.Override(new Vector4(1.03f, 1f, 0.97f, 0.02f));

            // Lente: viñeta, aberración cromática leve y grano de película
            var vignette = profile.Add<Vignette>(true);
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

        public void Tick(float dt, Camera cam, HUD hud)
        {
            if (rain != null) rain.transform.position = cam.transform.position + Vector3.up * 22;
            UpdateReflections(dt, cam);
            timer -= dt;
            if (timer > 0) return;
            Raining = !Raining;
            timer = Raining ? Random.Range(35f, 60f) : Random.Range(50f, 90f);
            if (Raining) rain.Play(); else rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            RenderSettings.fogColor = Raining ? WetFog : DryFog;
            RenderSettings.fogDensity = Raining ? WetFogDensity : DryFogDensity;
            sun.intensity = Raining ? 0.75f : 1.6f;
            sun.color = Raining ? SunWet : SunDry;
            if (sky != null) sky.SetFloat("_Exposure", Raining ? 0.75f : 1.1f);
            if (color != null) color.saturation.Override(Raining ? WetSaturation : DrySaturation);
            if (dof != null) dof.gaussianStart.Override(Raining ? 45f : 70f);
            DynamicGI.UpdateEnvironment();
            probeTimer = 0;
            // asfalto mojado: más brillante y reflectivo
            if (City.I != null && City.I.asphaltMat != null) { City.I.asphaltMat.SetFloat("_Smoothness", Raining ? 0.7f : 0.12f); City.I.asphaltMat.SetFloat("_Glossiness", Raining ? 0.7f : 0.12f); }
            hud.Toast(Raining ? "Empezó a llover en Bogotá" : "Escampó");
        }
    }
}
