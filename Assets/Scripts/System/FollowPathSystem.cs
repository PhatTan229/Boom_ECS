using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct FollowPathSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (pathFinding, path, transform, velocity, stat) in SystemAPI.Query<RefRW<PathFinding>, DynamicBuffer<Path>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRO<StatData>>())
        {
            if (pathFinding.ValueRO.currentIndex >= path.Length) continue;
            var targetEntity = path[pathFinding.ValueRO.currentIndex].value;
            var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
            pathFinding.ValueRW.FollowPath(velocity, transform, pathFinding, targetTransform, stat, SystemAPI.Time.DeltaTime);
        }
    }
}
