using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Dragable : IComponentData
{
    public float3 newPos;
    public void Drag(ref LocalTransform transform, float3 newPos)
    {
        transform.Position = newPos;
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public class DragableAuthoring : MonoBehaviour
{
    class DragableAuthoringBaker : Baker<DragableAuthoring>
    {
        public override void Bake(DragableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Dragable());
        }
    }
}
