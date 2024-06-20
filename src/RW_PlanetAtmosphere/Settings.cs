using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

namespace RW_PlanetAtmosphere
{
    internal class Settings : ModSettings
    {
        public static bool updated = false;
        public static float exposure = 4;
        public static float ground_refract = 1;
        public static float ground_light = 0.01f;
        public static float mie_amount = 3.996f/scale;
        public static float mie_absorb = 1.11f;
        public static float H_Reayleigh = 0.08f*scale;
        public static float H_Mie = 0.02f*scale;
        public static float H_OZone = 0.25f*scale;
        public static float D_OZone = 0.15f*scale;
        public static Vector3 SunColor = Vector3.one;
        public static Vector3 mie_eccentricity = new Vector3(0.618f,0.618f,0.618f);
        public static Vector3 reayleighScatterFactor = new Vector3(0.47293f,1.22733f,2.09377f)/scale;
        public static Vector3 OZoneAbsorbFactor = new Vector3(0.21195f,0.20962f,0.01686f)/scale;
        public static Vector2 translucentLUTSize = new Vector2(16, 16);
        public static Vector4 scatterLUTSize = new Vector4( 8, 2, 2, 1);


        private const float scale = 99.85f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ground_refract, "ground_refract", defaultValue: 1, forceSave: true);
            Scribe_Values.Look(ref exposure, "exposure", defaultValue: 4, forceSave: true);
            Scribe_Values.Look(ref ground_light, "ground_light", defaultValue: 0.01f, forceSave: true);
            Scribe_Values.Look(ref mie_amount, "mie_amount", defaultValue: 3.996f/scale, forceSave: true);
            Scribe_Values.Look(ref mie_absorb, "mie_absorb", defaultValue: 1.11f, forceSave: true);
            Scribe_Values.Look(ref H_Reayleigh, "H_Reayleigh", defaultValue: 0.08f*scale, forceSave: true);
            Scribe_Values.Look(ref H_Mie, "H_Mie", defaultValue: 0.02f*scale, forceSave: true);
            Scribe_Values.Look(ref H_OZone, "H_OZone", defaultValue: 0.25f*scale, forceSave: true);
            Scribe_Values.Look(ref D_OZone, "D_OZone", defaultValue: 0.15f*scale, forceSave: true);
            Scribe_Values.Look(ref SunColor, "SunColor", defaultValue: Vector3.one, forceSave: true);
            Scribe_Values.Look(ref mie_eccentricity, "mie_eccentricity", defaultValue: new Vector3(0.618f,0.618f,0.618f), forceSave: true);
            Scribe_Values.Look(ref reayleighScatterFactor, "reayleighScatterFactor", defaultValue: new Vector3(0.47293f,1.22733f,2.09377f)/scale, forceSave: true);
            Scribe_Values.Look(ref OZoneAbsorbFactor, "OZoneAbsorbFactor", defaultValue: new Vector3(0.21195f,0.20962f,0.01686f)/scale, forceSave: true);
            Scribe_Values.Look(ref translucentLUTSize, "translucentLUTSize", defaultValue: new Vector2Int(16, 16), forceSave: true);
            Scribe_Values.Look(ref scatterLUTSize, "scatterLUTSize", defaultValue: new Vector4( 8, 2, 2, 1), forceSave: true);
        }

        public static void DoWindowContents(Rect inRect)
        {
            
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            
            listing_Standard.Label("AtmosphereParam".Translate());
            listing_Standard.Gap();
            // listing_Standard.te("exposure".Translate(), ref exposure);
            listing_Standard.Gap();


            listing_Standard.End();
            ShaderLoader.materialLUT.SetFloat("exposure", exposure);
            ShaderLoader.materialLUT.SetFloat("ground_refract", ground_refract);
            ShaderLoader.materialLUT.SetFloat("ground_light", ground_light);
            ShaderLoader.materialLUT.SetVector("SunColor", SunColor);
            ShaderLoader.materialLUT.SetVector("mie_eccentricity", mie_eccentricity);

        }
    }
}