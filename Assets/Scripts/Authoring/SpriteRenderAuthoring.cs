using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[MaterialProperty("_Index")]
public struct SpriteIndex : IComponentData
{
    public float Value;
}

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
    public int index;

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
            AddComponent(entity, new SpriteIndex() { Value = authoring.index });
        }
    }
}
