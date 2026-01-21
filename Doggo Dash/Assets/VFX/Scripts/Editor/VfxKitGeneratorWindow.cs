using System.IO;
using DoggoDash.Vfx;
using UnityEditor;
using UnityEngine;

namespace DoggoDash.VfxEditor
{
    public class VfxKitGeneratorWindow : EditorWindow
    {
        private const string RootPath = "Assets/VFX";
        private const string MaterialsPath = RootPath + "/Materials";
        private const string TexturesPath = RootPath + "/Textures";
        private const string PrefabsPath = RootPath + "/Prefabs/Common";
        private const string ProfilesPath = RootPath + "/Profiles";

        private const string AdditiveMaterialName = "M_VFX_Additive";
        private const string AlphaMaterialName = "M_VFX_AlphaSoft";
        private const string CutoutMaterialName = "M_VFX_Cutout";
        private const string TrailAdditiveMaterialName = "M_VFX_Trail_Additive";

        [MenuItem("Tools/VFX/VFX Kit Generator")]
        public static void Open()
        {
            GetWindow<VfxKitGeneratorWindow>("VFX Kit Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Common VFX Kit", EditorStyles.boldLabel);
            if (GUILayout.Button("Regenerate Common VFX Kit"))
            {
                Regenerate();
            }
        }

        private static void Regenerate()
        {
            EnsureFolder(RootPath);
            EnsureFolder(MaterialsPath);
            EnsureFolder(TexturesPath);
            EnsureFolder(PrefabsPath);
            EnsureFolder(ProfilesPath);

            var additive = CreateOrUpdateMaterial(Path.Combine(MaterialsPath, AdditiveMaterialName + ".mat"),
                MaterialBlendMode.Additive, false);
            var alphaSoft = CreateOrUpdateMaterial(Path.Combine(MaterialsPath, AlphaMaterialName + ".mat"),
                MaterialBlendMode.Alpha, false);
            var cutout = CreateOrUpdateMaterial(Path.Combine(MaterialsPath, CutoutMaterialName + ".mat"),
                MaterialBlendMode.Alpha, true);
            var trailAdditive = CreateOrUpdateMaterial(Path.Combine(MaterialsPath, TrailAdditiveMaterialName + ".mat"),
                MaterialBlendMode.Additive, false);

            var casualProfile = CreateOrUpdateProfile(Path.Combine(ProfilesPath, "VfxStyleProfile_CasualRunner.asset"),
                new Color(1f, 0.86f, 0.55f, 1f),
                new Color(0.55f, 0.85f, 1f, 1f),
                new Color(1f, 0.45f, 0.2f, 1f),
                new Color(0.8f, 0.8f, 0.8f, 1f),
                0.9f,
                false,
                "Default",
                "Default",
                1f);

            var scifiProfile = CreateOrUpdateProfile(Path.Combine(ProfilesPath, "VfxStyleProfile_SciFiRunner.asset"),
                new Color(0.3f, 0.95f, 1f, 1f),
                new Color(0.9f, 0.2f, 1f, 1f),
                new Color(1f, 0.85f, 0.2f, 1f),
                new Color(0.6f, 0.7f, 0.8f, 1f),
                1.35f,
                true,
                "Default",
                "Default",
                1f);

            CreateImpactSparkSmall(Path.Combine(PrefabsPath, "P_ImpactSpark_Small.prefab"), additive, casualProfile);
            CreateImpactSparkBig(Path.Combine(PrefabsPath, "P_ImpactSpark_Big.prefab"), additive, alphaSoft, casualProfile);
            CreateDustPuffFootstep(Path.Combine(PrefabsPath, "P_DustPuff_Footstep.prefab"), alphaSoft, casualProfile);
            CreateDustPuffLand(Path.Combine(PrefabsPath, "P_DustPuff_Land.prefab"), alphaSoft, casualProfile);
            CreateMuzzleFlashShort(Path.Combine(PrefabsPath, "P_MuzzleFlash_Short.prefab"), additive, casualProfile);
            CreateSmokeTrailThin(Path.Combine(PrefabsPath, "P_SmokeTrail_Thin.prefab"), alphaSoft, casualProfile);
            CreateSmokeTrailThick(Path.Combine(PrefabsPath, "P_SmokeTrail_Thick.prefab"), alphaSoft, casualProfile);
            CreateExplosionSmall(Path.Combine(PrefabsPath, "P_Explosion_Small.prefab"), additive, alphaSoft, casualProfile);
            CreateExplosionMed(Path.Combine(PrefabsPath, "P_Explosion_Med.prefab"), additive, alphaSoft, cutout, casualProfile);
            CreateShockwaveRing(Path.Combine(PrefabsPath, "P_Shockwave_Ring.prefab"), additive, alphaSoft, casualProfile);
            CreatePickupSparkle(Path.Combine(PrefabsPath, "P_Pickup_Sparkle.prefab"), additive, trailAdditive, casualProfile);
            CreateAmbientDustMotes(Path.Combine(PrefabsPath, "P_Ambient_DustMotes.prefab"), alphaSoft, casualProfile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path);
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        private enum MaterialBlendMode
        {
            Alpha,
            Additive
        }

        private static Material CreateOrUpdateMaterial(string path, MaterialBlendMode mode, bool alphaClip)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = Shader.Find("Universal Render Pipeline/Unlit");
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_AlphaClip", alphaClip ? 1f : 0f);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_ZWrite", 0f);
            material.SetColor("_BaseColor", Color.white);

            var blendValue = mode == MaterialBlendMode.Additive ? 2f : 0f;
            material.SetFloat("_Blend", blendValue);

            if (alphaClip)
            {
                material.SetFloat("_Surface", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static VfxStyleProfile CreateOrUpdateProfile(string path, Color primary, Color secondary, Color accent,
            Color smoke, float intensity, bool useGlow, string sortingWorld, string sortingOverlay, float globalScale)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VfxStyleProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VfxStyleProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.PrimaryColor = primary;
            profile.SecondaryColor = secondary;
            profile.AccentColor = accent;
            profile.SmokeColor = smoke;
            profile.Intensity = intensity;
            profile.UseGlow = useGlow;
            profile.SortingLayerWorld = sortingWorld;
            profile.SortingLayerOverlay = sortingOverlay;
            profile.GlobalScale = globalScale;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void CreateImpactSparkSmall(string path, Material additive, VfxStyleProfile profile)
        {
            var root = new GameObject("P_ImpactSpark_Small");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Sparks");
            ConfigureImpactSpark(system, 0.25f, 0.06f, 0.12f, 3f, 7f, 0.05f, 0.12f, 12, 25f, 0.02f);
            ConfigureRenderer(system, additive, VfxColorRole.Primary, VfxSortingRole.Overlay, 10);

            SavePrefab(root, path);
        }

        private static void CreateImpactSparkBig(string path, Material additive, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_ImpactSpark_Big");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            CreateRootLifetimeSystem(root, 0.6f);

            var system = AddParticleSystem(root, "Sparks");
            ConfigureImpactSpark(system, 0.35f, 0.08f, 0.16f, 5f, 11f, 0.08f, 0.2f, 22, 35f, 0.04f);
            ConfigureRenderer(system, additive, VfxColorRole.Primary, VfxSortingRole.Overlay, 10);

            var smoke = AddParticleSystem(root, "SmokePuff");
            ConfigureSmokePuff(smoke, 0.3f, 0.15f, 0.3f, 0.1f, 0.2f, 0.15f, 0.35f, 4, 0.2f);
            ConfigureRenderer(smoke, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            var subEmitters = system.subEmitters;
            subEmitters.enabled = true;
            subEmitters.AddSubEmitter(smoke, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritNothing);

            SavePrefab(root, path);
        }

        private static void CreateDustPuffFootstep(string path, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_DustPuff_Footstep");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Dust");
            ConfigureDustPuff(system, 0.6f, 0.35f, 0.8f, 0.3f, 1.2f, 0.12f, 0.35f, 10, 12f, 0.08f, 0.35f, 0.25f);
            ConfigureRenderer(system, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateDustPuffLand(string path, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_DustPuff_Land");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Dust");
            ConfigureDustPuff(system, 0.8f, 0.5f, 1.2f, 0.6f, 2f, 0.25f, 0.75f, 22, 18f, 0.16f, 0.5f, 0.25f);
            ConfigureRenderer(system, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateMuzzleFlashShort(string path, Material additive, VfxStyleProfile profile)
        {
            var root = new GameObject("P_MuzzleFlash_Short");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Flash");
            ConfigureMuzzleFlash(system);
            ConfigureRenderer(system, additive, VfxColorRole.Accent, VfxSortingRole.Overlay, 15);

            SavePrefab(root, path);
        }

        private static void CreateSmokeTrailThin(string path, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_SmokeTrail_Thin");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Smoke");
            ConfigureSmokeTrail(system, 2f, 0.7f, 1.5f, 0.05f, 0.25f, 0.08f, 0.22f, 35, 10f, 0.02f, 0.6f, 0.35f, 400, false);
            ConfigureRenderer(system, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateSmokeTrailThick(string path, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_SmokeTrail_Thick");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Smoke");
            ConfigureSmokeTrail(system, 2f, 1f, 2.4f, 0.05f, 0.25f, 0.18f, 0.55f, 55, 10f, 0.02f, 0.9f, 0.35f, 650, true);
            ConfigureRenderer(system, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateExplosionSmall(string path, Material additive, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_Explosion_Small");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            CreateRootLifetimeSystem(root, 2f);

            var flash = AddParticleSystem(root, "Flash");
            ConfigureFlash(flash, 0.05f, 0.6f, 1f);
            ConfigureRenderer(flash, additive, VfxColorRole.Accent, VfxSortingRole.Overlay, 10);

            var sparks = AddParticleSystem(root, "Sparks");
            ConfigureImpactSpark(sparks, 0.3f, 0.08f, 0.18f, 6f, 14f, 0.05f, 0.14f, 18, 40f, 0.05f);
            ConfigureRenderer(sparks, additive, VfxColorRole.Primary, VfxSortingRole.Overlay, 10);

            var smoke = AddParticleSystem(root, "Smoke");
            ConfigureSmokeBurst(smoke, 1.5f, 0.9f, 1.8f, 0.4f, 1.6f, 0.6f, 1.4f, 10, 0.2f, 0.7f, 0.25f);
            ConfigureRenderer(smoke, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateExplosionMed(string path, Material additive, Material alphaSoft, Material cutout, VfxStyleProfile profile)
        {
            var root = new GameObject("P_Explosion_Med");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            CreateRootLifetimeSystem(root, 2.8f);

            var flash = AddParticleSystem(root, "Flash");
            ConfigureFlash(flash, 0.05f, 1.2f, 2f);
            ConfigureRenderer(flash, additive, VfxColorRole.Accent, VfxSortingRole.Overlay, 10);

            var sparks = AddParticleSystem(root, "Sparks");
            ConfigureImpactSpark(sparks, 0.35f, 0.1f, 0.2f, 8f, 18f, 0.06f, 0.18f, 28, 45f, 0.06f);
            ConfigureRenderer(sparks, additive, VfxColorRole.Primary, VfxSortingRole.Overlay, 10);

            var smoke = AddParticleSystem(root, "Smoke");
            ConfigureSmokeBurst(smoke, 2f, 1.2f, 2.6f, 0.4f, 1.6f, 1.2f, 2.6f, 18, 0.25f, 0.8f, 0.25f);
            ConfigureRenderer(smoke, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            var debris = AddParticleSystem(root, "Debris");
            ConfigureDebris(debris, 0.9f, 0.4f, 0.9f, 3f, 8f, 0.08f, 0.18f, 10);
            ConfigureRenderer(debris, cutout, VfxColorRole.Secondary, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static void CreateShockwaveRing(string path, Material additive, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_Shockwave_Ring");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Shockwave");
            ConfigureShockwave(system);

            var renderer = ConfigureRenderer(system, additive, VfxColorRole.Secondary, VfxSortingRole.World, 0);
            var settings = renderer.GetComponent<VfxRendererSettings>();
            settings.UseGlowMaterialSwap = true;
            settings.AdditiveMaterial = additive;
            settings.AlphaMaterial = alphaSoft;

            SavePrefab(root, path);
        }

        private static void CreatePickupSparkle(string path, Material additive, Material trailAdditive, VfxStyleProfile profile)
        {
            var root = new GameObject("P_Pickup_Sparkle");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "Sparkle");
            ConfigurePickupSparkle(system, trailAdditive);
            ConfigureRenderer(system, additive, VfxColorRole.Accent, VfxSortingRole.Overlay, 10);

            SavePrefab(root, path);
        }

        private static void CreateAmbientDustMotes(string path, Material alphaSoft, VfxStyleProfile profile)
        {
            var root = new GameObject("P_Ambient_DustMotes");
            var applier = root.AddComponent<VfxStyleApplier>();
            applier.Profile = profile;

            var system = AddParticleSystem(root, "DustMotes");
            ConfigureAmbientDust(system);
            ConfigureRenderer(system, alphaSoft, VfxColorRole.Smoke, VfxSortingRole.World, 0);

            SavePrefab(root, path);
        }

        private static ParticleSystem AddParticleSystem(GameObject root, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var system = go.AddComponent<ParticleSystem>();
            return system;
        }

        private static void CreateRootLifetimeSystem(GameObject root, float duration)
        {
            var system = root.AddComponent<ParticleSystem>();
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = duration;
            main.startSpeed = 0f;
            main.startSize = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.enabled = false;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void ConfigureImpactSpark(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int burstCount, float coneAngle, float radius)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = radius;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, ShrinkCurve());
        }

        private static void ConfigureSmokePuff(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int burstCount, float radius)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, GrowCurve(1f, 2f));
        }

        private static void ConfigureDustPuff(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int burstCount, float coneAngle, float radius,
            float noiseStrength, float noiseFrequency)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = radius;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, GrowCurve(1f, 2.2f));

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(noiseStrength);
            noise.frequency = noiseFrequency;
        }

        private static void ConfigureMuzzleFlash(ParticleSystem system)
        {
            var main = system.main;
            main.duration = 0.12f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 0.04f, 0.07f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 0f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 0.18f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)2) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.01f;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();
        }

        private static void ConfigureSmokeTrail(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int rate, float coneAngle, float radius,
            float noiseStrength, float noiseFrequency, int maxParticles, bool upwardVelocity)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;

            var emission = system.emission;
            emission.rateOverTime = rate;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = radius;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, GrowCurve(1f, 2.5f));

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(noiseStrength);
            noise.frequency = noiseFrequency;

            if (upwardVelocity)
            {
                var velocity = system.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = new ParticleSystem.MinMaxCurve(0.08f);
            }
        }

        private static void ConfigureFlash(ParticleSystem system, float lifetime, float sizeMin, float sizeMax)
        {
            var main = system.main;
            main.duration = lifetime;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)1) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, ShrinkCurve());
        }

        private static void ConfigureSmokeBurst(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int burstCount, float radius,
            float noiseStrength, float noiseFrequency)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, GrowCurve(1f, 3f));

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(noiseStrength);
            noise.frequency = noiseFrequency;
        }

        private static void ConfigureDebris(ParticleSystem system, float duration, float lifetimeMin, float lifetimeMax,
            float speedMin, float speedMax, float sizeMin, float sizeMax, int burstCount)
        {
            var main = system.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, sizeMin, sizeMax);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.2f;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.08f;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();
        }

        private static void ConfigureShockwave(ParticleSystem system)
        {
            var main = system.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = 0f;
            main.startSize = 0.8f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)1) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var sizeOverLifetime = system.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, GrowCurve(1f, 6f));

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();
        }

        private static void ConfigurePickupSparkle(ParticleSystem system, Material trailMaterial)
        {
            var main = system.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 0.3f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 0.2f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 0.03f, 0.1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)18) });

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.4f);

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var trails = system.trails;
            trails.enabled = true;
            trails.ratio = 0.4f;
            trails.lifetime = 0.08f;
            trails.dieWithParticles = true;
            trails.minVertexDistance = 0.02f;
            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, TaperCurve());

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.trailMaterial = trailMaterial;
        }

        private static void ConfigureAmbientDust(ParticleSystem system)
        {
            var main = system.main;
            main.duration = 10f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 4f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 0.03f, 0.08f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 250;

            var emission = system.emission;
            emission.rateOverTime = 6f;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 3f, 8f);

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = FadeOutGradient();

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.25f);
            noise.frequency = 0.12f;
        }

        private static ParticleSystemRenderer ConfigureRenderer(ParticleSystem system, Material material,
            VfxColorRole colorRole, VfxSortingRole sortingRole, int sortingOrder)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;

            var settings = system.gameObject.AddComponent<VfxRendererSettings>();
            settings.ColorRole = colorRole;
            settings.SortingRole = sortingRole;
            settings.SortingOrder = sortingOrder;
            settings.AffectStartColor = true;
            settings.AffectMaterialColor = true;
            settings.AdditiveMaterial = material;

            return renderer;
        }

        private static ParticleSystem.MinMaxGradient FadeOutGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            return new ParticleSystem.MinMaxGradient(gradient);
        }

        private static AnimationCurve ShrinkCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.7f, 0f),
                new Keyframe(1f, 0f));
        }

        private static AnimationCurve GrowCurve(float start, float end)
        {
            return new AnimationCurve(
                new Keyframe(0f, start),
                new Keyframe(1f, end));
        }

        private static AnimationCurve TaperCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f));
        }
    }
}
