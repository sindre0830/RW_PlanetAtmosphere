using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using Verse.Noise;

namespace RW_PlanetAtmosphere
{
    [StaticConstructorOnStartup]
    internal static class ShaderLoader
    {
        public static RenderTexture translucentLUT = null;
        public static RenderTexture scatterLUT = null;
        public readonly static Material materialLUT = null;
        public readonly static Mesh mesh = null;        private static Shader SkyBox_LUT = null;
        private static Shader TranslucentGenrater = null;
        private static Shader ScatterGenrater = null;
        private static Material materialTranslucentGenrater = null;
        private static Material materialScatterGenrater = null;
        private static GameObject sky = null;
        private static MeshFilter meshFilter = null;
        private static MeshRenderer meshRenderer = null;
        private static PlanetAtmosphere planetAtmosphere = null;

        public static bool isEnable => materialLUT != null && (materialLUT.shader?.isSupported ?? false);
        static ShaderLoader()
        {
            uint loadedCount = 0;
            List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
            foreach (ModContentPack pack in runningModsListForReading)
            {
                //Log.Message($"{pack.PackageId},{pack.assetBundles.loadedAssetBundles?.Count}");
                if (pack.PackageId.Equals("rwnodetree.rwplanetatmosphere") && !pack.assetBundles.loadedAssetBundles.NullOrEmpty())
                {
                    //Log.Message($"{pack.PackageId} found, try to load shader");
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        SkyBox_LUT = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\SkyBox_LUT.shader");
                        if (SkyBox_LUT != null && SkyBox_LUT.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        TranslucentGenrater = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\TranslucentGenrater.shader");
                        if (TranslucentGenrater != null && TranslucentGenrater.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    foreach (AssetBundle assetBundle in pack.assetBundles.loadedAssetBundles)
                    {
                        // Log.Message($"Loading shader in {assetBundle.name}");
                        ScatterGenrater = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\ScatterGenrater.shader");
                        if (ScatterGenrater != null && ScatterGenrater.isSupported)
                        {
                            loadedCount++;
                            break;
                        }
                    }
                    break;
                }
            }
            if (loadedCount >= 3)
            {
                materialLUT = new Material(SkyBox_LUT)
                {
                    renderQueue = 3545
                };

                mesh = new Mesh();
                
                sky = new GameObject("RW_PlanetAtmosphere");
                meshFilter = sky.AddComponent<MeshFilter>();
                meshRenderer = sky.AddComponent<MeshRenderer>();
                planetAtmosphere = sky.AddComponent<PlanetAtmosphere>();
                Object.DontDestroyOnLoad(sky);
                sky.layer = WorldCameraManager.WorldLayer;
                meshFilter.mesh = mesh;
                meshRenderer.material = materialLUT;
                WorldCameraManager.WorldCamera.fieldOfView = 20;
                WorldCameraManager.WorldSkyboxCamera.fieldOfView = 20;

                WorldMaterials.Rivers.shader = ShaderDatabase.Transparent;
                WorldMaterials.RiversBorder.shader = ShaderDatabase.Transparent;
                WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldOcean.shader;

                WorldMaterials.Rivers.renderQueue = 3530;
                WorldMaterials.RiversBorder.renderQueue = 3520;
                WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

                WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
                WorldMaterials.RiversBorder.color = new Color(0,0,0,0);

                WorldMaterials.WorldOcean.mainTexture = SolidColorMaterials.NewSolidColorTexture(Color.black);
                WorldMaterials.UngeneratedPlanetParts.mainTexture = WorldMaterials.WorldOcean.mainTexture;

                planetAtmosphere.materialsTest.Add(WorldMaterials.WorldOcean);
                planetAtmosphere.materialsTest.Add(WorldMaterials.UngeneratedPlanetParts);
                planetAtmosphere.materialsTest.Add(WorldMaterials.Stars);
                planetAtmosphere.materialsTest.Add(WorldMaterials.Rivers);
                planetAtmosphere.materialsTest.Add(WorldMaterials.RiversBorder);
                planetAtmosphere.materialsTest.Add(WorldMaterials.WorldTerrain);
                planetAtmosphere.materialsTest.Add(WorldMaterials.WorldIce);

                // WorldMaterials.WorldOcean.color = new Color32(1,2,4,255);
                // WorldMaterials.UngeneratedPlanetParts.color = new Color32(1,2,4,255);
                // WorldMaterials.Rivers.color = new Color32(1,2,4,255);
            }
        }


        private class PlanetAtmosphere : MonoBehaviour
        {
            public readonly List<Material> materialsTest = new List<Material>();
            void parmUpdated()
            {
                if(!AtmosphereSettings.updated && isEnable)
                {
                    materialLUT.SetFloat("mie_amount", AtmosphereSettings.mie_amount);
                    materialLUT.SetFloat("mie_absorb", AtmosphereSettings.mie_absorb);
                    materialLUT.SetFloat("H_Reayleigh", AtmosphereSettings.H_Reayleigh);
                    materialLUT.SetFloat("H_Mie", AtmosphereSettings.H_Mie);
                    materialLUT.SetFloat("H_OZone", AtmosphereSettings.H_OZone);
                    materialLUT.SetFloat("D_OZone", AtmosphereSettings.D_OZone);
                    materialLUT.SetFloat("minh", 99.85f);
                    materialLUT.SetFloat("maxh", 99.85f + Mathf.Max
                    (
                        AtmosphereSettings.H_OZone + AtmosphereSettings.D_OZone,
                        -Mathf.Log(0.00001f)*(Mathf.Max
                        (
                            AtmosphereSettings.reayleighScatterFactor.x,
                            AtmosphereSettings.reayleighScatterFactor.y,
                            AtmosphereSettings.reayleighScatterFactor.z
                        ) * AtmosphereSettings.H_Reayleigh),
                        -Mathf.Log(0.00001f)*(AtmosphereSettings.mie_amount * (AtmosphereSettings.mie_absorb + 1.0f) * AtmosphereSettings.H_Mie)
                    ));
                    materialLUT.SetVector("reayleighScatterFactor", AtmosphereSettings.reayleighScatterFactor);
                    materialLUT.SetVector("OZoneAbsorbFactor", AtmosphereSettings.OZoneAbsorbFactor);
                    Vector4 scatterLUTSize = AtmosphereSettings.scatterLUTSize * 16;
                    Vector2Int translucentLUTSize = Vector2Int.FloorToInt(AtmosphereSettings.translucentLUTSize) * 16;
                    Vector2Int scatterLUTSize2D = new Vector2Int((int)scatterLUTSize.x * (int)scatterLUTSize.z, (int)scatterLUTSize.y * (int)scatterLUTSize.w);
                    
                    if(translucentLUT == null || translucentLUT.width != translucentLUTSize.x || translucentLUT.height != translucentLUTSize.y)
                    {
                        if (translucentLUT != null) Destroy(translucentLUT);
                        translucentLUT = new RenderTexture(translucentLUTSize.x, translucentLUTSize.y, 0)
                        {
                            enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        translucentLUT.Create();
                    }
                    if(scatterLUT == null || scatterLUT.width != scatterLUTSize2D.x || scatterLUT.height != scatterLUTSize2D.y)
                    {
                        if (scatterLUT != null) Destroy(scatterLUT);
                        scatterLUT = new RenderTexture(scatterLUTSize2D.x, scatterLUTSize2D.y, 0)
                        {
                            enableRandomWrite = true,
                            useMipMap = false,
                            format = RenderTextureFormat.ARGBHalf,
                            wrapMode = TextureWrapMode.Clamp
                        };
                        scatterLUT.Create();
                    }

                    materialTranslucentGenrater = new Material(TranslucentGenrater);
                    materialScatterGenrater = new Material(ScatterGenrater);

                    materialTranslucentGenrater.SetTexture("scatterLUT", scatterLUT);
                    materialTranslucentGenrater.SetTexture("translucentLUT", translucentLUT);
                    materialTranslucentGenrater.SetVector("reayleighScatterFactor", materialLUT.GetVector("reayleighScatterFactor"));
                    materialTranslucentGenrater.SetVector("OZoneAbsorbFactor", materialLUT.GetVector("OZoneAbsorbFactor"));
                    materialTranslucentGenrater.SetFloat("mie_amount", materialLUT.GetFloat("mie_amount"));
                    materialTranslucentGenrater.SetFloat("mie_absorb", materialLUT.GetFloat("mie_absorb"));
                    materialTranslucentGenrater.SetFloat("minh", materialLUT.GetFloat("minh"));
                    materialTranslucentGenrater.SetFloat("maxh", materialLUT.GetFloat("maxh"));
                    materialTranslucentGenrater.SetFloat("H_Reayleigh", materialLUT.GetFloat("H_Reayleigh"));
                    materialTranslucentGenrater.SetFloat("H_Mie", materialLUT.GetFloat("H_Mie"));
                    materialTranslucentGenrater.SetFloat("H_OZone", materialLUT.GetFloat("H_OZone"));
                    materialTranslucentGenrater.SetFloat("D_OZone", materialLUT.GetFloat("D_OZone"));
                    Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);

                    materialScatterGenrater.SetTexture("scatterLUT", scatterLUT);
                    materialScatterGenrater.SetTexture("translucentLUT", translucentLUT);
                    materialScatterGenrater.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y , (int)scatterLUTSize.z, (int)scatterLUTSize.w));
                    materialScatterGenrater.SetVector("reayleighScatterFactor", materialLUT.GetVector("reayleighScatterFactor"));
                    materialScatterGenrater.SetVector("OZoneAbsorbFactor", materialLUT.GetVector("OZoneAbsorbFactor"));
                    materialScatterGenrater.SetFloat("mie_amount", materialLUT.GetFloat("mie_amount"));
                    materialScatterGenrater.SetFloat("mie_absorb", materialLUT.GetFloat("mie_absorb"));
                    materialScatterGenrater.SetFloat("minh", materialLUT.GetFloat("minh"));
                    materialScatterGenrater.SetFloat("maxh", materialLUT.GetFloat("maxh"));
                    materialScatterGenrater.SetFloat("H_Reayleigh", materialLUT.GetFloat("H_Reayleigh"));
                    materialScatterGenrater.SetFloat("H_Mie", materialLUT.GetFloat("H_Mie"));
                    materialScatterGenrater.SetFloat("H_OZone", materialLUT.GetFloat("H_OZone"));
                    materialScatterGenrater.SetFloat("D_OZone", materialLUT.GetFloat("D_OZone"));
                    Graphics.Blit(null, scatterLUT, materialScatterGenrater);

                    //int scatterIteratorComputeKernel = scatterIteratorCompute.FindKernel("CSMain");
                    //scatterIteratorCompute.Dispatch(scatterIteratorComputeKernel, scatterLUTSize.x, scatterLUTSize.y, scatterLUTSize.z << 4);

                    //materialLUT.SetFloat("mie_amount", 3.996f);
                    //materialLUT.SetFloat("mie_absorb", 1.11f);
                    //materialLUT.SetFloat("minh", 63.71393f);
                    materialLUT.SetTexture("translucentLUT", translucentLUT);
                    materialLUT.SetTexture("scatterLUT", scatterLUT);
                    materialLUT.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y , (int)scatterLUTSize.z, (int)scatterLUTSize.w));
                    AtmosphereSettings.updated = true;
                }
            }
            void Update()
            {
                if(isEnable && Find.World != null)
                {
                    parmUpdated();
                    materialLUT.SetFloat("exposure", AtmosphereSettings.exposure);
                    materialLUT.SetFloat("ground_refract", AtmosphereSettings.ground_refract);
                    materialLUT.SetFloat("ground_light", AtmosphereSettings.ground_light);
                    materialLUT.SetVector("SunColor", AtmosphereSettings.SunColor);
                    materialLUT.SetVector("mie_eccentricity", AtmosphereSettings.mie_eccentricity);
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                }
            }
        }
    }
}