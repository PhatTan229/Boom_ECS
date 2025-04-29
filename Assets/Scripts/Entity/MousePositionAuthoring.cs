using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public struct MousePosition : IComponentData
{
    public float3 value;
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public class MousePositionAuthoring : MonoBehaviour
{
    class PositionBaker : Baker<MousePositionAuthoring>
    {
        public override void Bake(MousePositionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MousePosition());
        }
    }
}


