using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public struct SpriteRenderInfo : IComponentData
{
    public UnityObjectRef<Material> material;
    public UnityObjectRef<Mesh> mesh;
    public int materialIndex;
    public int meshIndex;
}

public class SpriteRenderAuthoring : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    class SpriteRenderAuthoringBaker : Baker<SpriteRenderAuthoring>
    {
        public override void Bake(SpriteRenderAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpriteRenderInfo()
            {
                material = authoring.material,
                mesh = authoring.mesh,
            });
            //AddComponent(entity, new MaterialMeshInfo { Material = 0, Mesh = 0 });
            //AddComponent(entity, new RenderBounds { Value = authoring.mesh.bounds.ToAABB() });
        }
    }
}
