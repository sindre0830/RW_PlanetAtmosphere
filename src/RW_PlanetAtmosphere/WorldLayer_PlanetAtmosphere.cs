using RimWorld;
using RimWorld.Planet;
using System.Collections;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    
    public class WorldLayer_PlanetAtmosphere : WorldLayer
    {
        private Mesh mesh;
        private GameObject sky = null;
        private MeshFilter meshFilter = null;
        private MeshRenderer meshRenderer = null;
        private Light light = null;

        protected override Quaternion Rotation
        {
            get
            {
                if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
                {
                    Shader.SetGlobalVector("_WorldSpaceLightPos0",GenCelestial.CurSunPositionInWorldSpace());
                }
                return base.Rotation;
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
                if(mesh == null)
                {
                    SphereGenerator.Generate(4, 500f, Vector3.forward, 360f, out var outVerts, out var outIndices);
                    mesh = new Mesh
                    {
                        vertices = outVerts.ToArray(),
                        triangles = outIndices.ToArray()
                    };
                }
                
                sky = sky ?? new GameObject("RW_PlanetAtmosphere");
                meshFilter = meshFilter ?? sky.AddComponent<MeshFilter>();
                meshRenderer = meshRenderer ?? sky.AddComponent<MeshRenderer>();
                light = light ?? sky.AddComponent<Light>();
                sky.layer = WorldCameraManager.WorldLayer;
                meshFilter.mesh = mesh;
                meshRenderer.material = ShaderLoader.materialLUT;
                light.type = LightType.Directional;
            }
            else
            {
                SphereGenerator.Generate(4, 108.1f, Vector3.forward, 360f, out var outVerts, out var outIndices);
                LayerSubMesh subMesh = GetSubMesh(WorldMaterials.PlanetGlow);
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