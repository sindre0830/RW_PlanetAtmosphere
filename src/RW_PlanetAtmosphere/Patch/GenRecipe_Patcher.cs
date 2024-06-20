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
        private static MethodInfo WorldLayer_GetSubMesh = typeof(WorldLayer).GetMethod("GetSubMesh",BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo WorldLayer_FinalizeMesh = typeof(WorldLayer).GetMethod("FinalizeMesh",BindingFlags.Instance | BindingFlags.NonPublic);

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
            SphereGenerator.Generate(4, 1000f, Vector3.forward, 360f, out var outVerts, out var outIndices);
            LayerSubMesh subMesh = (LayerSubMesh)WorldLayer_GetSubMesh.Invoke(instance,new object[]{ShaderLoader.materialLUT});
            subMesh.verts.AddRange(outVerts);
            subMesh.tris.AddRange(outIndices);
            WorldLayer_FinalizeMesh.Invoke(instance,new object[]{MeshParts.All});
            yield break;
        }

    }
}