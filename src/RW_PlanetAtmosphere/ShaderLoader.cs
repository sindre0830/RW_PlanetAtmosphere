﻿using Verse;
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
        public readonly static Material materialSkyLUT = null;
        private static Mesh mesh = null;
        private static Shader SkyBox_LUT = null;
        private static Shader SkyBoxCloud_LUT = null;
        private static Shader TranslucentGenrater = null;
        private static Shader ScatterGenrater = null;
        private static Material materialTranslucentGenrater = null;
        private static Material materialScatterGenrater = null;
        private static GameObject sky = null;
        private static MeshFilter meshFilter = null;
        private static MeshRenderer meshRenderer = null;
        private static RenderTexture translucentLUT = null;
        private static RenderTexture scatterLUT = null;

        public static bool isEnable => materialSkyLUT != null && (materialSkyLUT.shader?.isSupported ?? false);
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
                        SkyBoxCloud_LUT = assetBundle.LoadAsset<Shader>(@"Assets\Data\RWNodeTree.RWPlanetAtmosphere\SkyBoxCloud_LUT.shader");
                        if (SkyBoxCloud_LUT != null && SkyBoxCloud_LUT.isSupported)
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
            if (loadedCount >= 4)
            {
                materialSkyLUT = new Material(SkyBox_LUT)
                {
                    renderQueue = 3555
                };

                mesh = new Mesh();
                
                sky = new GameObject("RW_PlanetAtmosphere");
                meshFilter = sky.AddComponent<MeshFilter>();
                meshRenderer = sky.AddComponent<MeshRenderer>();
                sky.AddComponent<PlanetAtmosphere>();
                Object.DontDestroyOnLoad(sky);
                sky.layer = WorldCameraManager.WorldLayer;
                meshFilter.mesh = mesh;
                meshRenderer.material = materialSkyLUT;
                WorldCameraManager.WorldCamera.fieldOfView = 20;
                WorldCameraManager.WorldSkyboxCamera.fieldOfView = 20;

                WorldMaterials.Rivers.shader = WorldMaterials.WorldTerrain.shader;
                WorldMaterials.WorldOcean.shader = WorldMaterials.WorldTerrain.shader;
                // WorldMaterials.RiversBorder.shader = WorldMaterials.WorldTerrain.shader;
                WorldMaterials.UngeneratedPlanetParts.shader = WorldMaterials.WorldTerrain.shader;

                WorldMaterials.Rivers.renderQueue = 3530;
                WorldMaterials.WorldOcean.renderQueue = 3500;
                // WorldMaterials.RiversBorder.renderQueue = 3520;
                WorldMaterials.UngeneratedPlanetParts.renderQueue = 3500;

                // WorldMaterials.Rivers.color = new Color(-65536,-65536,-65536,1);
                // WorldMaterials.RiversBorder.color = new Color(0,0,0,0);


                WorldMaterials.Rivers.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                WorldMaterials.WorldOcean.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                // WorldMaterials.RiversBorder.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");
                WorldMaterials.UngeneratedPlanetParts.mainTexture = ContentFinder<Texture2D>.Get("TerrainReplace/Water");

                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldOcean);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.UngeneratedPlanetParts);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.Stars);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.Rivers);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.RiversBorder);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldTerrain);
                // planetAtmosphere.materialsTest.Add(WorldMaterials.WorldIce);

                // WorldMaterials.WorldOcean.color = new Color32(1,2,4,255);
                // WorldMaterials.UngeneratedPlanetParts.color = new Color32(1,2,4,255);
                // WorldMaterials.Rivers.color = new Color32(1,2,4,255);
            }
        }


        private class PlanetAtmosphere : MonoBehaviour
        {
            // public readonly List<Material> materialsTest = new List<Material>();
            void parmUpdated()
            {
                if(!AtmosphereSettings.updated && isEnable)
                {
                    MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
                    for(int i= 0; i < renderers.Length; i++)
                    {
                        if(renderers[i].transform == transform) continue;
                        GameObject.Destroy(renderers[i].material);
                        GameObject.Destroy(renderers[i].gameObject);
                    }
                    float minh = 99.85f;
                    float maxh = 99.85f + Mathf.Max
                    (
                        AtmosphereSettings.H_OZone + AtmosphereSettings.D_OZone,
                        -Mathf.Log(0.00001f)*(Mathf.Max
                        (
                            AtmosphereSettings.reayleighScatterFactor.x,
                            AtmosphereSettings.reayleighScatterFactor.y,
                            AtmosphereSettings.reayleighScatterFactor.z
                        ) * AtmosphereSettings.H_Reayleigh),
                        -Mathf.Log(0.00001f)*(AtmosphereSettings.mie_amount * (AtmosphereSettings.mie_absorb + 1.0f) * AtmosphereSettings.H_Mie)
                    );
                    materialSkyLUT.SetFloat("mie_amount", AtmosphereSettings.mie_amount);
                    materialSkyLUT.SetFloat("mie_absorb", AtmosphereSettings.mie_absorb);
                    materialSkyLUT.SetFloat("H_Reayleigh", AtmosphereSettings.H_Reayleigh);
                    materialSkyLUT.SetFloat("H_Mie", AtmosphereSettings.H_Mie);
                    materialSkyLUT.SetFloat("H_OZone", AtmosphereSettings.H_OZone);
                    materialSkyLUT.SetFloat("D_OZone", AtmosphereSettings.D_OZone);
                    materialSkyLUT.SetFloat("minh", minh);
                    materialSkyLUT.SetFloat("maxh", maxh);
                    materialSkyLUT.SetVector("reayleighScatterFactor", AtmosphereSettings.reayleighScatterFactor);
                    materialSkyLUT.SetVector("OZoneAbsorbFactor", AtmosphereSettings.OZoneAbsorbFactor);
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
                            format = RenderTextureFormat.ARGBFloat,
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
                    materialTranslucentGenrater.SetVector("reayleighScatterFactor", materialSkyLUT.GetVector("reayleighScatterFactor"));
                    materialTranslucentGenrater.SetVector("OZoneAbsorbFactor", materialSkyLUT.GetVector("OZoneAbsorbFactor"));
                    materialTranslucentGenrater.SetFloat("mie_amount", materialSkyLUT.GetFloat("mie_amount"));
                    materialTranslucentGenrater.SetFloat("mie_absorb", materialSkyLUT.GetFloat("mie_absorb"));
                    materialTranslucentGenrater.SetFloat("minh", minh);
                    materialTranslucentGenrater.SetFloat("maxh", maxh);
                    materialTranslucentGenrater.SetFloat("H_Reayleigh", materialSkyLUT.GetFloat("H_Reayleigh"));
                    materialTranslucentGenrater.SetFloat("H_Mie", materialSkyLUT.GetFloat("H_Mie"));
                    materialTranslucentGenrater.SetFloat("H_OZone", materialSkyLUT.GetFloat("H_OZone"));
                    materialTranslucentGenrater.SetFloat("D_OZone", materialSkyLUT.GetFloat("D_OZone"));
                    Graphics.Blit(null, translucentLUT, materialTranslucentGenrater);

                    materialScatterGenrater.SetTexture("scatterLUT", scatterLUT);
                    materialScatterGenrater.SetTexture("translucentLUT", translucentLUT);
                    materialScatterGenrater.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y , (int)scatterLUTSize.z, (int)scatterLUTSize.w));
                    materialScatterGenrater.SetVector("reayleighScatterFactor", materialSkyLUT.GetVector("reayleighScatterFactor"));
                    materialScatterGenrater.SetVector("OZoneAbsorbFactor", materialSkyLUT.GetVector("OZoneAbsorbFactor"));
                    materialScatterGenrater.SetFloat("mie_amount", materialSkyLUT.GetFloat("mie_amount"));
                    materialScatterGenrater.SetFloat("mie_absorb", materialSkyLUT.GetFloat("mie_absorb"));
                    materialScatterGenrater.SetFloat("minh", minh);
                    materialScatterGenrater.SetFloat("maxh", maxh);
                    materialScatterGenrater.SetFloat("H_Reayleigh", materialSkyLUT.GetFloat("H_Reayleigh"));
                    materialScatterGenrater.SetFloat("H_Mie", materialSkyLUT.GetFloat("H_Mie"));
                    materialScatterGenrater.SetFloat("H_OZone", materialSkyLUT.GetFloat("H_OZone"));
                    materialScatterGenrater.SetFloat("D_OZone", materialSkyLUT.GetFloat("D_OZone"));
                    Graphics.Blit(null, scatterLUT, materialScatterGenrater);

                    //int scatterIteratorComputeKernel = scatterIteratorCompute.FindKernel("CSMain");
                    //scatterIteratorCompute.Dispatch(scatterIteratorComputeKernel, scatterLUTSize.x, scatterLUTSize.y, scatterLUTSize.z << 4);

                    //materialLUT.SetFloat("mie_amount", 3.996f);
                    //materialLUT.SetFloat("mie_absorb", 1.11f);
                    //materialLUT.SetFloat("minh", 63.71393f);
                    materialSkyLUT.SetTexture("translucentLUT", translucentLUT);
                    materialSkyLUT.SetTexture("scatterLUT", scatterLUT);
                    materialSkyLUT.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x, (int)scatterLUTSize.y , (int)scatterLUTSize.z, (int)scatterLUTSize.w));

                    
                    SphereGenerator.Generate(4, maxh, Vector3.forward, 360f, out var outVerts, out var outIndices);
                    mesh.vertices = outVerts.ToArray();
                    mesh.triangles = outIndices.ToArray();
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                    mesh.UploadMeshData(false);

                    for(int i = 0; i < AtmosphereSettings.cloudTexPath.Count; i++)
                    {
                        if(AtmosphereSettings.cloudTexPath[i] == null) continue;
                        Texture2D texture2D = ContentFinder<Texture2D>.Get(AtmosphereSettings.cloudTexPath[i]);
                        if(texture2D == null) continue;
                        Material cloud = new Material(SkyBoxCloud_LUT)
                        {
                            renderQueue = 3556
                        };
                        cloud.SetFloat("exposure", materialSkyLUT.GetFloat("exposure"));
                        cloud.SetFloat("ground_refract", materialSkyLUT.GetFloat("ground_refract"));
                        cloud.SetFloat("ground_light", materialSkyLUT.GetFloat("ground_light"));
                        cloud.SetFloat("mie_amount", materialSkyLUT.GetFloat("mie_amount"));
                        cloud.SetFloat("mie_absorb", materialSkyLUT.GetFloat("mie_absorb"));
                        cloud.SetFloat("minh", minh);
                        cloud.SetFloat("maxh", maxh);
                        cloud.SetVector("SunColor", materialSkyLUT.GetVector("SunColor"));
                        cloud.SetVector("mie_eccentricity", materialSkyLUT.GetVector("mie_eccentricity"));
                        cloud.SetVector("reayleighScatterFactor", materialSkyLUT.GetVector("reayleighScatterFactor"));
                        cloud.SetVector("scatterLUT_Size", new Vector4((int)scatterLUTSize.x << 4, (int)scatterLUTSize.y << 4, (int)scatterLUTSize.z << 4, (int)scatterLUTSize.w << 4));
                        cloud.SetTexture("cloudTexture", texture2D);
                        cloud.SetTexture("translucentLUT", translucentLUT);
                        cloud.SetTexture("scatterLUT", scatterLUT);
                        GameObject gameObject = new GameObject();
                        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
                        Transform transform = gameObject.transform;
                        filter.mesh = mesh;
                        renderer.material = cloud;
                        transform.parent = this.transform;
                        transform.localScale = Vector3.one * (0.05f * (maxh - minh) + minh) / maxh; 
                    }
                    AtmosphereSettings.updated = true;
                }
            }
            void Update()
            {
                if(isEnable && Find.World != null)
                {
                    parmUpdated();
                    materialSkyLUT.SetFloat("exposure", AtmosphereSettings.exposure);
                    materialSkyLUT.SetFloat("ground_refract", AtmosphereSettings.ground_refract);
                    materialSkyLUT.SetFloat("ground_light", AtmosphereSettings.ground_light);
                    materialSkyLUT.SetVector("SunColor", AtmosphereSettings.SunColor);
                    materialSkyLUT.SetVector("mie_eccentricity", AtmosphereSettings.mie_eccentricity);
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                }
            }
        }
    }
}