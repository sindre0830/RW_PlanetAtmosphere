using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;

namespace RW_PlanetAtmosphere
{
    [StaticConstructorOnStartup]
    internal static class ShaderLoader
    {
        public static Material materialLUT = null;

        private static Vector2Int translucentLUTSize = new Vector2Int(16, 16);
        private static Vector4 scatterLUTSize = new Vector4( 8, 2, 2, 1);
        private static Shader SkyBox_LUT = null;
        private static Shader TranslucentGenrater = null;
        private static Shader ScatterGenrater = null;
        private static Material materialTranslucentGenrater = null;
        private static Material materialScatterGenrater = null;
        private static RenderTexture translucentLUT = null;
        private static RenderTexture scatterLUT = null;


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
                const float scale = 99.85f/63.71393f;
                materialLUT = new Material(SkyBox_LUT);
                materialLUT.SetVector("reayleighScatterFactor", materialLUT.GetVector("reayleighScatterFactor") / scale);
                materialLUT.SetVector("OZoneAbsorbFactor", materialLUT.GetVector("OZoneAbsorbFactor") / scale);
                materialLUT.SetFloat("mie_amount", materialLUT.GetFloat("mie_amount") / scale);
                materialLUT.SetFloat("mie_absorb", materialLUT.GetFloat("mie_absorb"));
                materialLUT.SetFloat("minh", materialLUT.GetFloat("minh") * scale);
                materialLUT.SetFloat("maxh", materialLUT.GetFloat("maxh") * scale);
                materialLUT.SetFloat("H_Reayleigh", materialLUT.GetFloat("H_Reayleigh") * scale);
                materialLUT.SetFloat("H_Mie", materialLUT.GetFloat("H_Mie") * scale);
                materialLUT.SetFloat("H_OZone", materialLUT.GetFloat("H_OZone") * scale);
                materialLUT.SetFloat("D_OZone", materialLUT.GetFloat("D_OZone") * scale);
                
                translucentLUT = new RenderTexture(translucentLUTSize.x << 4, translucentLUTSize.y << 4, 0)
                {
                    enableRandomWrite = true,
                    useMipMap = false,
                    format = RenderTextureFormat.ARGBHalf,
                    wrapMode = TextureWrapMode.Clamp
                };
                translucentLUT.Create();
                scatterLUT = new RenderTexture(((int)scatterLUTSize.x * (int)scatterLUTSize.z) << 8, ((int)scatterLUTSize.y * (int)scatterLUTSize.w) << 8, 0)
                {
                    enableRandomWrite = true,
                    useMipMap = false,
                    format = RenderTextureFormat.ARGBHalf,
                    wrapMode = TextureWrapMode.Clamp
                };
                scatterLUT.Create();

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
                materialScatterGenrater.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x << 4, (int)scatterLUTSize.y << 4, (int)scatterLUTSize.z << 4, (int)scatterLUTSize.w << 4));
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
                materialLUT.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x << 4, (int)scatterLUTSize.y << 4, (int)scatterLUTSize.z << 4, (int)scatterLUTSize.w << 4));
            }
        }
    }
}