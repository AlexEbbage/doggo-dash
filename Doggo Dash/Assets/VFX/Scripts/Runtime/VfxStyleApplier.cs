using System.Collections.Generic;
using UnityEngine;

namespace DoggoDash.Vfx
{
    public class VfxStyleApplier : MonoBehaviour
    {
        public VfxStyleProfile Profile;

        private readonly Dictionary<ParticleSystem, ParticleSystemCache> _particleCache = new();
        private readonly Dictionary<ParticleSystemRenderer, RendererCache> _rendererCache = new();
        private readonly MaterialPropertyBlock _propertyBlock = new();

        private void Awake()
        {
            ApplyProfile();
        }

        public void ApplyProfile()
        {
            if (Profile == null)
            {
                return;
            }

            CacheParticlesIfNeeded();
            CacheRenderersIfNeeded();

            foreach (var pair in _particleCache)
            {
                ApplyParticleSettings(pair.Key, pair.Value, Profile);
            }

            foreach (var pair in _rendererCache)
            {
                ApplyRendererSettings(pair.Key, pair.Value, Profile);
            }
        }

        private void CacheParticlesIfNeeded()
        {
            var systems = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var system in systems)
            {
                if (_particleCache.ContainsKey(system))
                {
                    continue;
                }

                var main = system.main;
                var noise = system.noise;
                var cache = new ParticleSystemCache
                {
                    StartSize = main.startSize,
                    StartSpeed = main.startSpeed,
                    StartColor = main.startColor,
                    NoiseStrength = noise.enabled ? noise.strength : new ParticleSystem.MinMaxCurve(0f)
                };
                _particleCache.Add(system, cache);
            }
        }

        private void CacheRenderersIfNeeded()
        {
            var renderers = GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (_rendererCache.ContainsKey(renderer))
                {
                    continue;
                }

                var cache = new RendererCache
                {
                    SortingLayerId = renderer.sortingLayerID,
                    SortingOrder = renderer.sortingOrder
                };
                _rendererCache.Add(renderer, cache);
            }
        }

        private void ApplyParticleSettings(ParticleSystem system, ParticleSystemCache cache, VfxStyleProfile profile)
        {
            var main = system.main;
            var settings = system.GetComponent<VfxRendererSettings>();
            var role = settings != null ? settings.ColorRole : VfxColorRole.Primary;

            if (settings == null || settings.AffectStartColor)
            {
                var startColor = ResolveColor(role, profile) * profile.Intensity;
                main.startColor = new ParticleSystem.MinMaxGradient(startColor);
            }
            else
            {
                main.startColor = cache.StartColor;
            }

            main.startSize = ScaleCurve(cache.StartSize, profile.GlobalScale);
            main.startSpeed = ScaleCurve(cache.StartSpeed, profile.GlobalScale);

            var noise = system.noise;
            if (noise.enabled)
            {
                noise.strength = ScaleCurve(cache.NoiseStrength, profile.Intensity);
            }
        }

        private void ApplyRendererSettings(ParticleSystemRenderer renderer, RendererCache cache, VfxStyleProfile profile)
        {
            var settings = renderer.GetComponent<VfxRendererSettings>();
            if (settings == null)
            {
                renderer.sortingLayerID = cache.SortingLayerId;
                renderer.sortingOrder = cache.SortingOrder;
                return;
            }

            var sortingLayer = settings.SortingRole == VfxSortingRole.Overlay
                ? profile.SortingLayerOverlay
                : profile.SortingLayerWorld;

            if (!string.IsNullOrWhiteSpace(sortingLayer))
            {
                renderer.sortingLayerName = sortingLayer;
            }

            renderer.sortingOrder = settings.SortingOrder;

            if (settings.UseGlowMaterialSwap && settings.AdditiveMaterial != null && settings.AlphaMaterial != null)
            {
                renderer.sharedMaterial = profile.UseGlow ? settings.AdditiveMaterial : settings.AlphaMaterial;
            }

            if (settings.AffectMaterialColor)
            {
                var role = settings.ColorRole;
                var tint = ResolveColor(role, profile) * profile.Intensity;

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_BaseColor", tint);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private static Color ResolveColor(VfxColorRole role, VfxStyleProfile profile)
        {
            return role switch
            {
                VfxColorRole.Secondary => profile.SecondaryColor,
                VfxColorRole.Accent => profile.AccentColor,
                VfxColorRole.Smoke => profile.SmokeColor,
                _ => profile.PrimaryColor
            };
        }

        private static ParticleSystem.MinMaxCurve ScaleCurve(ParticleSystem.MinMaxCurve curve, float multiplier)
        {
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.TwoConstants:
                    return new ParticleSystem.MinMaxCurve(curve.curveMultiplier,
                        curve.constantMin * multiplier, curve.constantMax * multiplier);
                case ParticleSystemCurveMode.Constant:
                    return new ParticleSystem.MinMaxCurve(curve.constant * multiplier);
                case ParticleSystemCurveMode.TwoCurves:
                    return new ParticleSystem.MinMaxCurve(curve.curveMultiplier * multiplier,
                        curve.curveMin, curve.curveMax);
                case ParticleSystemCurveMode.Curve:
                    return new ParticleSystem.MinMaxCurve(curve.curveMultiplier * multiplier, curve.curve);
                default:
                    return curve;
            }
        }

        private class ParticleSystemCache
        {
            public ParticleSystem.MinMaxCurve StartSize;
            public ParticleSystem.MinMaxCurve StartSpeed;
            public ParticleSystem.MinMaxGradient StartColor;
            public ParticleSystem.MinMaxCurve NoiseStrength;
        }

        private class RendererCache
        {
            public int SortingLayerId;
            public int SortingOrder;
        }
    }
}
