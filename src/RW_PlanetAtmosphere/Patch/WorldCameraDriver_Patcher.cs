using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace RW_PlanetAtmosphere.Patch
{
    // [HarmonyPatch(typeof(WorldCameraDriver))]
    // internal static class WorldCameraDriver_Patcher
    // {
    //     [HarmonyPostfix]
    //     [HarmonyPatch(
    //         typeof(WorldCameraDriver),
    //         "Update"
    //         )]
    //     private static void PostWorldCameraDriver_Update(ref WorldCameraDriver __instance)
    //     {
    //         if(ShaderLoader.isEnable)
    //         {
    //             float maxh = ShaderLoader.materialSkyLUT.GetFloat("maxh");
    //             WorldCameraManager.WorldCamera.farClipPlane = __instance.altitude + maxh + 1.0f;
    //             WorldCameraManager.WorldCamera.nearClipPlane = __instance.altitude - maxh - 1.0f;
    //         }
    //     }

    // }
}