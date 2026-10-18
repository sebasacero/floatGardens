// Atmósfera bogotana: cielo, sol con sombras, niebla, posprocesado URP y lluvia (con calles mojadas).
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
        static readonly Color DryFog = new Color32(0xa9, 0xbc, 0xcb, 255);
        static readonly Color WetFog = new Color32(0x8e, 0x9b, 0xa6, 255);

        public void Setup(Camera cam)
        {
            // Sol
            var sunGo = new GameObject("Sol");
            sunGo.transform.SetParent(transform, false);
            sunGo.transform.rotation = Quaternion.Euler(48, -35, 0);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color32(0xff, 0xf1, 0xdc, 255);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            RenderSettings.sun = sun;

            // Cielo procedural (plantilla FG_Sky en Resources)
            sky = FGMaterials.New("FG_Sky", "CieloBogota");
            if (sky.shader != null && sky.shader.name == "Skybox/Procedural")
            {
                sky.SetFloat("_SunSize", 0.04f);
                sky.SetFloat("_AtmosphereThickness", 1.15f);
                sky.SetColor("_SkyTint", new Color32(0x9d, 0xb4, 0xc8, 255));
                sky.SetColor("_GroundColor", new Color32(0x55, 0x5a, 0x55, 255));
                sky.SetFloat("_Exposure", 1.15f);
                RenderSettings.skybox = sky;
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color32(0xc9, 0xd8, 0xe6, 255);
            RenderSettings.ambientEquatorColor = new Color32(0x9a, 0xa6, 0xae, 255);
            RenderSettings.ambientGroundColor = new Color32(0x4a, 0x4f, 0x45, 255);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 40;
            RenderSettings.fogEndDistance = 430;
            RenderSettings.fogColor = DryFog;
            DynamicGI.UpdateEnvironment();

            // Posprocesado URP
            var volGo = new GameObject("Posprocesado");
            volGo.transform.SetParent(transform, false);
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.8f);
            bloom.threshold.Override(1.0f);
            bloom.scatter.Override(0.65f);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.25f);
            color.contrast.Override(12f);
            color.saturation.Override(10f);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            volume.profile = profile;

            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            }

            BuildRain(cam.transform);
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
            timer -= dt;
            if (timer > 0) return;
            Raining = !Raining;
            timer = Raining ? Random.Range(35f, 60f) : Random.Range(50f, 90f);
            if (Raining) rain.Play(); else rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            RenderSettings.fogColor = Raining ? WetFog : DryFog;
            RenderSettings.fogEndDistance = Raining ? 300 : 430;
            sun.intensity = Raining ? 0.7f : 1.25f;
            if (sky != null) sky.SetFloat("_Exposure", Raining ? 0.8f : 1.15f);
            if (color != null) color.saturation.Override(Raining ? -5f : 10f);
            // asfalto mojado: más brillante y reflectivo
            if (City.I != null && City.I.asphaltMat != null) City.I.asphaltMat.SetFloat("_Smoothness", Raining ? 0.62f : 0.12f);
            hud.Toast(Raining ? "Empezó a llover en Bogotá" : "Escampó");
        }
    }
}
