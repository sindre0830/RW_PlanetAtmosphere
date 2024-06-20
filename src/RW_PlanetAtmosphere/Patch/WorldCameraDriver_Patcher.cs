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
    [HarmonyPatch(typeof(WorldCameraDriver))]
    internal static class WorldCameraDriver_Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(WorldCameraDriver),
            "get_MinAltitude"
            )]
        private static void PostWorldCameraDriver_get_MinAltitude(ref float __result)
        {
            if(ShaderLoader.isEnable)
            {
                __result = 100f + (SteamDeck.IsSteamDeck ? 5f : 8f);
            }
        }

    }
}