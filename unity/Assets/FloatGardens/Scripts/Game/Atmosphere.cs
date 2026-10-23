// Atmósfera bogotana: cielo, sol con sombras, niebla, posprocesado URP y lluvia (con calles mojadas).
// Los valores salen del perfil visual de la plataforma (FGStyle.Current).
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
        FGStyleProfile style;

        public void Setup(Camera cam)
        {
            style = FGStyle.Current;
            var l = style.lighting;

            // Sol
            var sunGo = new GameObject("Sol");
            sunGo.transform.SetParent(transform, false);
            sunGo.transform.rotation = Quaternion.Euler(48, -35, 0);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = l.sunColor;
            sun.intensity = l.sunIntensity;
            sun.shadows = l.sunShadows;
            sun.shadowStrength = l.shadowStrength;
            RenderSettings.sun = sun;

            // Cielo procedural (plantilla FG_Sky en Resources)
            sky = FGMaterials.New("FG_Sky", "CieloBogota");
            if (sky.shader != null && sky.shader.name == "Skybox/Procedural")
            {
                sky.SetFloat("_SunSize", 0.04f);
                sky.SetFloat("_AtmosphereThickness", l.atmosphereThickness);
                sky.SetColor("_SkyTint", l.skyTint);
                sky.SetColor("_GroundColor", l.skyGround);
                sky.SetFloat("_Exposure", l.skyExposure);
                RenderSettings.skybox = sky;
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = l.ambientSky;
            RenderSettings.ambientEquatorColor = l.ambientEquator;
            RenderSettings.ambientGroundColor = l.ambientGround;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = l.fogStart;
            RenderSettings.fogEndDistance = l.fogEnd;
            RenderSettings.fogColor = l.fogDry;
            DynamicGI.UpdateEnvironment();

            // Posprocesado URP según el perfil de la plataforma
            var volGo = new GameObject("Posprocesado");
            volGo.transform.SetParent(transform, false);
            var volume = volGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10;
            volume.profile = BuildPost(style.post);

            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null)
            {
                data.renderPostProcessing = true;
                data.antialiasing = style.post.antialiasing;
            }

            BuildRain(cam.transform);
        }

        VolumeProfile BuildPost(FGStyleProfile.Post f)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(f.bloomIntensity);
            bloom.threshold.Override(f.bloomThreshold);
            bloom.scatter.Override(f.bloomScatter);
            bloom.tint.Override(f.bloomTint);
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(f.tonemapping);
            color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(f.postExposure);
            color.contrast.Override(f.contrast);
            color.saturation.Override(f.saturation);
            color.colorFilter.Override(f.colorFilter);
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(f.vignette);
            vignette.color.Override(f.vignetteColor);
            if (f.filmGrain > 0)
            {
                var grain = profile.Add<FilmGrain>(true);
                grain.type.Override(FilmGrainLookup.Medium3);
                grain.intensity.Override(f.filmGrain);
            }
            if (f.chromaticAberration > 0)
                profile.Add<ChromaticAberration>(true).intensity.Override(f.chromaticAberration);
            if (f.splitShadows != Color.gray || f.splitHighlights != Color.gray)
            {
                var split = profile.Add<SplitToning>(true);
                split.shadows.Override(f.splitShadows);
                split.highlights.Override(f.splitHighlights);
                split.balance.Override(f.splitBalance);
            }
            return profile;
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
            var l = style.lighting;
            RenderSettings.fogColor = Raining ? l.fogWet : l.fogDry;
            RenderSettings.fogEndDistance = Raining ? l.fogEnd * 0.7f : l.fogEnd;
            sun.intensity = Raining ? l.sunIntensity * 0.56f : l.sunIntensity;
            if (sky != null) sky.SetFloat("_Exposure", Raining ? l.skyExposure * 0.7f : l.skyExposure);
            if (color != null) color.saturation.Override(Raining ? style.post.saturation - 15f : style.post.saturation);
            // asfalto mojado: más brillante y reflectivo
            if (City.I != null && City.I.asphaltMat != null)
                City.I.asphaltMat.SetFloat("_Smoothness", Mathf.Clamp01((Raining ? 0.62f : 0.12f) * style.materials.smoothnessScale));
            hud.Toast(Raining ? "Empezó a llover en Bogotá" : "Escampó");
        }
    }
}
