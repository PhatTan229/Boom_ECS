using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEditor.Rendering;
using UnityEngine;

[MaterialProperty("_Index")]
public struct TileIndex : IComponentData
{
    public float Value;
}

public class TileAuthoring : MonoBehaviour
{
    public int index;

    class TileAuthoringBaker : Baker<TileAuthoring>
    {
        public override void Bake(TileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TileIndex() { Value = authoring.index });
        }
    }
}
