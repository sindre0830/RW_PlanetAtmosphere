using RimWorld.Planet;
using System.Collections;
using UnityEngine;
using Verse;

namespace RW_PlanetAtmosphere
{
    
    public class WorldLayer_PlanetAtmosphere : WorldLayer
    {
        private GameObject sky = null;
        private MeshFilter meshFilter = null;
        private MeshRenderer meshRenderer = null;

        public override void Render()
        {
            if(ShaderLoader.materialLUT != null && (ShaderLoader.materialLUT.shader?.isSupported ?? false))
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
            else base.Render();
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