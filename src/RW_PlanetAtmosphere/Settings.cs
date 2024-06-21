using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

namespace RW_PlanetAtmosphere
{
    internal class AtmosphereSettings : ModSettings
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
        public static Vector2 translucentLUTSize = new Vector2(16, 16);
        public static Vector3 SunColor = new Vector3(1,1,1);
        public static Vector3 mie_eccentricity = new Vector3(0.618f,0.618f,0.618f);
        public static Vector3 reayleighScatterFactor = new Vector3(0.47293f,1.22733f,2.09377f)/scale;
        public static Vector3 OZoneAbsorbFactor = new Vector3(0.21195f,0.20962f,0.01686f)/scale;
        public static Vector4 scatterLUTSize = new Vector4( 8, 2, 2, 1);


        private static Vector2 scrollPos = Vector2.zero;


        private const float scale = 99.85f/63.71393f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref exposure, "exposure", defaultValue: 4, forceSave: true);
            Scribe_Values.Look(ref ground_refract, "ground_refract", defaultValue: 1, forceSave: true);
            Scribe_Values.Look(ref ground_light, "ground_light", defaultValue: 0.01f, forceSave: true);
            Scribe_Values.Look(ref mie_amount, "mie_amount", defaultValue: 3.996f/scale, forceSave: true);
            Scribe_Values.Look(ref mie_absorb, "mie_absorb", defaultValue: 1.11f, forceSave: true);
            Scribe_Values.Look(ref H_Reayleigh, "H_Reayleigh", defaultValue: 0.08f*scale, forceSave: true);
            Scribe_Values.Look(ref H_Mie, "H_Mie", defaultValue: 0.02f*scale, forceSave: true);
            Scribe_Values.Look(ref H_OZone, "H_OZone", defaultValue: 0.25f*scale, forceSave: true);
            Scribe_Values.Look(ref D_OZone, "D_OZone", defaultValue: 0.15f*scale, forceSave: true);
            Scribe_Values.Look(ref translucentLUTSize, "translucentLUTSize", defaultValue: new Vector2(16, 16), forceSave: true);
            Scribe_Values.Look(ref SunColor, "SunColor", defaultValue: new Vector3(1, 1, 1), forceSave: true);
            Scribe_Values.Look(ref mie_eccentricity, "mie_eccentricity", defaultValue: new Vector3(0.618f,0.618f,0.618f), forceSave: true);
            Scribe_Values.Look(ref reayleighScatterFactor, "reayleighScatterFactor", defaultValue: new Vector3(0.47293f,1.22733f,2.09377f)/scale, forceSave: true);
            Scribe_Values.Look(ref OZoneAbsorbFactor, "OZoneAbsorbFactor", defaultValue: new Vector3(0.21195f,0.20962f,0.01686f)/scale, forceSave: true);
            Scribe_Values.Look(ref scatterLUTSize, "scatterLUTSize", defaultValue: new Vector4( 8, 2, 2, 1), forceSave: true);
        }

        public static void DoWindowContents(Rect inRect)
        {
            Widgets.Label(new Rect(0,0,inRect.width,48),"AtmosphereParms".Translate());
            Widgets.DrawLineHorizontal(0,47,inRect.width);
            Vector2 ScrollViewSize = new Vector2(inRect.width,512);
            if(ScrollViewSize.y > inRect.height-48) ScrollViewSize.x -= 36;
            Widgets.BeginScrollView(new Rect(0,48,inRect.width,inRect.height-48),ref scrollPos,new Rect(Vector2.zero, ScrollViewSize));

            float newValue;

            Widgets.Label(new Rect(0,0,ScrollViewSize.x*0.5f,32),"exposure".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,0,ScrollViewSize.x*0.5f,32),exposure.ToString()),out newValue);
            if(newValue != exposure) updated = false;
            exposure = newValue;


            Widgets.Label(new Rect(0,32,ScrollViewSize.x*0.5f,32),"ground_refract".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,32,ScrollViewSize.x*0.5f,32),ground_refract.ToString()),out newValue);
            if(newValue != ground_refract) updated = false;
            ground_refract = newValue;


            Widgets.Label(new Rect(0,64,ScrollViewSize.x*0.5f,32),"ground_light".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,64,ScrollViewSize.x*0.5f,32),ground_light.ToString()),out newValue);
            if(newValue != ground_light) updated = false;
            ground_light = newValue;


            Widgets.Label(new Rect(0,96,ScrollViewSize.x*0.5f,32),"mie_amount".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,96,ScrollViewSize.x*0.5f,32),mie_amount.ToString()),out newValue);
            if(newValue != mie_amount) updated = false;
            mie_amount = newValue;


            Widgets.Label(new Rect(0,128,ScrollViewSize.x*0.5f,32),"mie_absorb".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,128,ScrollViewSize.x*0.5f,32),mie_absorb.ToString()),out newValue);
            if(newValue != mie_absorb) updated = false;
            mie_absorb = newValue;


            Widgets.Label(new Rect(0,160,ScrollViewSize.x*0.5f,32),"H_Reayleigh".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,160,ScrollViewSize.x*0.5f,32),H_Reayleigh.ToString()),out newValue);
            if(newValue != H_Reayleigh) updated = false;
            H_Reayleigh = newValue;


            Widgets.Label(new Rect(0,192,ScrollViewSize.x*0.5f,32),"H_Mie".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,192,ScrollViewSize.x*0.5f,32),H_Mie.ToString()),out newValue);
            if(newValue != H_Mie) updated = false;
            H_Mie = newValue;


            Widgets.Label(new Rect(0,224,ScrollViewSize.x*0.5f,32),"H_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,224,ScrollViewSize.x*0.5f,32),H_OZone.ToString()),out newValue);
            if(newValue != H_OZone) updated = false;
            H_OZone = newValue;


            Widgets.Label(new Rect(0,256,ScrollViewSize.x*0.5f,32),"D_OZone".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,256,ScrollViewSize.x*0.5f,32),D_OZone.ToString()),out newValue);
            if(newValue != D_OZone) updated = false;
            D_OZone = newValue;


            Widgets.Label(new Rect(0,288,ScrollViewSize.x*0.5f,32),"translucentLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,288,ScrollViewSize.x*0.5f/2f,32),translucentLUTSize.x.ToString()),out newValue);
            if((int)newValue != translucentLUTSize.x) updated = false;
            translucentLUTSize.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*3f/2f,288,ScrollViewSize.x*0.5f/2f,32),translucentLUTSize.y.ToString()),out newValue);
            if((int)newValue != translucentLUTSize.y) updated = false;
            translucentLUTSize.y = (int)newValue;


            Widgets.Label(new Rect(0,320,ScrollViewSize.x*0.5f,32),"SunColor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.x.ToString()),out newValue);
            if(newValue != SunColor.x) updated = false;
            SunColor.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.y.ToString()),out newValue);
            if(newValue != SunColor.y) updated = false;
            SunColor.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,320,ScrollViewSize.x*0.5f/3f,32),SunColor.z.ToString()),out newValue);
            if(newValue != SunColor.z) updated = false;
            SunColor.z = newValue;


            Widgets.Label(new Rect(0,352,ScrollViewSize.x*0.5f,32),"mie_eccentricity".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.x.ToString()),out newValue);
            if(newValue != mie_eccentricity.x) updated = false;
            mie_eccentricity.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.y.ToString()),out newValue);
            if(newValue != mie_eccentricity.y) updated = false;
            mie_eccentricity.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,352,ScrollViewSize.x*0.5f/3f,32),mie_eccentricity.z.ToString()),out newValue);
            if(newValue != mie_eccentricity.z) updated = false;
            mie_eccentricity.z = newValue;


            Widgets.Label(new Rect(0,384,ScrollViewSize.x*0.5f,32),"reayleighScatterFactor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.x.ToString()),out newValue);
            if(newValue != reayleighScatterFactor.x) updated = false;
            reayleighScatterFactor.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.y.ToString()),out newValue);
            if(newValue != reayleighScatterFactor.y) updated = false;
            reayleighScatterFactor.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,384,ScrollViewSize.x*0.5f/3f,32),reayleighScatterFactor.z.ToString()),out newValue);
            if(newValue != reayleighScatterFactor.z) updated = false;
            reayleighScatterFactor.z = newValue;


            Widgets.Label(new Rect(0,416,ScrollViewSize.x*0.5f,32),"OZoneAbsorbFactor".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.x.ToString()),out newValue);
            if(newValue != OZoneAbsorbFactor.x) updated = false;
            OZoneAbsorbFactor.x = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*4f/3f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.y.ToString()),out newValue);
            if(newValue != OZoneAbsorbFactor.y) updated = false;
            OZoneAbsorbFactor.y = newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/3f,416,ScrollViewSize.x*0.5f/3f,32),OZoneAbsorbFactor.z.ToString()),out newValue);
            if(newValue != OZoneAbsorbFactor.z) updated = false;
            OZoneAbsorbFactor.z = newValue;


            Widgets.Label(new Rect(0,448,ScrollViewSize.x*0.5f,32),"scatterLUTSize".Translate());
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.x.ToString()),out newValue);
            if((int)newValue != scatterLUTSize.x) updated = false;
            scatterLUTSize.x = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*5f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.y.ToString()),out newValue);
            if((int)newValue != scatterLUTSize.y) updated = false;
            scatterLUTSize.y = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*6f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.z.ToString()),out newValue);
            if((int)newValue != scatterLUTSize.x) updated = false;
            scatterLUTSize.z = (int)newValue;
            float.TryParse(Widgets.TextField(new Rect(ScrollViewSize.x*0.5f*7f/4f,448,ScrollViewSize.x*0.5f/4f,32),scatterLUTSize.w.ToString()),out newValue);
            if((int)newValue != scatterLUTSize.y) updated = false;
            scatterLUTSize.w = (int)newValue;


            Widgets.DrawLineVertical(ScrollViewSize.x*0.5f,0,ScrollViewSize.y);
            Widgets.EndScrollView();

            ShaderLoader.materialLUT.SetFloat("exposure", exposure);
            ShaderLoader.materialLUT.SetFloat("ground_refract", ground_refract);
            ShaderLoader.materialLUT.SetFloat("ground_light", ground_light);
            ShaderLoader.materialLUT.SetVector("SunColor", SunColor);
            ShaderLoader.materialLUT.SetVector("mie_eccentricity", mie_eccentricity);

        }
    }

    public class AtmosphereMod : Mod
    {
        private static AtmosphereSettings settings;
        public AtmosphereMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AtmosphereSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            AtmosphereSettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Atmosphere".Translate();
        }
    }
}