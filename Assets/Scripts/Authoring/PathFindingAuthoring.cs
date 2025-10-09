using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
public struct Path : IBufferElementData
{
    public Entity value;
}

public struct PathFinding : IComponentData
{
    public int currentIndex;

    public void FollowPath(RefRW<LocalTransform> transform, RefRW<PathFinding> follow, LocalTransform targetTransform, RefRO<StatData> stat, float deltaTime)
    {
        float3 targetPos = targetTransform.Position;
        float3 currentPos = transform.ValueRO.Position;
        float3 dir = math.normalize(targetPos - currentPos);
        float dist = math.distance(currentPos, targetPos);

        float moveStep = stat.ValueRO.currentStat.speed * deltaTime;

        if (dist < moveStep)
        {
            transform.ValueRW.Position = targetPos;
            follow.ValueRW.currentIndex++;
        }
        else
        {
            transform.ValueRW.Position += dir * moveStep;
        }
    }

    public void FollowPath(RefRW<PhysicsVelocity> velocity, RefRW<LocalTransform> transform, RefRW<PathFinding> follow, LocalTransform targetTransform, RefRO<StatData> stat, float deltaTime)
    {
        float3 targetPos = targetTransform.Position;
        float3 currentPos = transform.ValueRO.Position;
        float3 dir = math.normalize(targetPos - currentPos);
        float dist = math.distance(currentPos, targetPos);

        float moveStep = stat.ValueRO.currentStat.speed * deltaTime;

        if (dist < moveStep)
        {
            velocity.ValueRW.Linear = float3.zero;
            follow.ValueRW.currentIndex++;
        }
        else
        {
            velocity.ValueRW.Linear = math.normalize(dir * moveStep) * stat.ValueRO.currentStat.speed;
        }
    }
}

public class PathFindingAuthoring : MonoBehaviour
{
    class PathFindingAuhoringBakeer : Baker<PathFindingAuthoring>
    {
        public override void Bake(PathFindingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PathFinding());
            AddBuffer<Path>(entity);
        }
    }
}
