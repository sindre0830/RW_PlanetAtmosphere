using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere.Patch
{
    [HarmonyPatch(typeof(WorldLayer_Glow))]
    internal static class WorldLayer_Glow_Patcher
    {
        private static MethodInfo WorldLayer_ClearSubMeshes = typeof(WorldLayer).GetMethod("ClearSubMeshes",BindingFlags.Instance | BindingFlags.NonPublic);

        private static AccessTools.FieldRef<WorldLayer,bool> WorldLayer_dirty = AccessTools.FieldRefAccess<WorldLayer,bool>("dirty");


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(WorldLayer_Glow),
            "Regenerate"
            )]
        private static bool PreWorldLayer_Glow_Regenerate(WorldLayer_Glow __instance, ref IEnumerable __result)
        {
            if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
            {
                __result = RegenerateOverider(__instance);
                return false;
            }
            return true;
        }

        private static IEnumerable RegenerateOverider(WorldLayer_Glow instance)
        {
            WorldLayer_dirty(instance) = false;
            WorldLayer_ClearSubMeshes.Invoke(instance,new object[]{MeshParts.All});
            yield break;
        }

    }
    public class WorldLayer_PlanetAtmosphere : WorldLayer
    {
        private GameObject sky = null;
        private MeshFilter meshFilter = null;
        private MeshRenderer meshRenderer = null;

        public override void Render()
        {
            if(subMeshes.Count > 0)
            {
                LayerSubMesh subMesh = subMeshes[0];
                if (subMesh.finalized)
                {
                    sky = sky ?? new GameObject("RW_PlanetAtmosphere");
                    meshFilter = meshFilter ?? sky.AddComponent<MeshFilter>();
                    meshRenderer = meshRenderer ?? sky.AddComponent<MeshRenderer>();
                    sky.layer = WorldCameraManager.WorldLayer;
                    meshFilter.mesh = subMesh.mesh;
                    meshRenderer.material = subMesh.material;
                }
            }
        }

        public override IEnumerable Regenerate()
        {
            foreach (object item in base.Regenerate())
            {
                yield return item;
            }
            if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
            {
                SphereGenerator.Generate(4, 1000f, Vector3.forward, 360f, out var outVerts, out var outIndices);
                LayerSubMesh subMesh = GetSubMesh(ShaderLoader.materialLUT);
                subMesh.verts.AddRange(outVerts);
                subMesh.tris.AddRange(outIndices);
                FinalizeMesh(MeshParts.All);
            }
        }

        ~WorldLayer_PlanetAtmosphere()
        {
            if(sky != null) GameObject.Destroy(sky);
        }
    }
}