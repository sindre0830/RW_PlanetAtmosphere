using RimWorld;
using RimWorld.Planet;
using System.Collections;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    
    public class WorldLayer_PlanetAtmosphere : WorldLayer
    {

        // protected override Quaternion Rotation
        // {
        //     get
        //     {
        //         if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
        //         {
        //             Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
        //         }
        //         return base.Rotation;
        //     }
        // }

        public override IEnumerable Regenerate()
        {
            foreach (object item in base.Regenerate())
            {
                yield return item;
            }
            if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
            {
                ShaderLoader.mesh.vertices = new Vector3[]
                {
                    new Vector3(-500,-500,-500),
                    new Vector3( 500,-500,-500),
                    new Vector3(-500, 500,-500),
                    new Vector3( 500, 500,-500),
                    new Vector3(-500,-500, 500),
                    new Vector3( 500,-500, 500),
                    new Vector3(-500, 500, 500),
                    new Vector3( 500, 500, 500)
                };
                ShaderLoader.mesh.triangles = new int[]
                {
                    0,2,1,3,1,2,
                    5,7,4,6,4,7,
                    4,6,0,2,0,6,
                    1,3,5,7,5,3,
                    2,6,3,7,3,6,
                    4,0,5,1,5,0
                };
                ShaderLoader.mesh.RecalculateBounds();
                ShaderLoader.mesh.RecalculateNormals();
                ShaderLoader.mesh.RecalculateTangents();
            }
        }
    }
}