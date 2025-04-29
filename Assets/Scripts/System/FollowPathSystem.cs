using Unity.Entities;
using Unity.Transforms;

public partial struct FollowPathSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (pathFinding, path, transform) in SystemAPI.Query<RefRW<PathFinding>, DynamicBuffer<Path>, RefRW<LocalTransform>>())
        {
            if (pathFinding.ValueRO.currentIndex >= path.Length) continue;
            var targetEntity = path[pathFinding.ValueRO.currentIndex].value;
            var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
            pathFinding.ValueRW.FollowPath(transform, pathFinding, targetTransform, SystemAPI.Time.DeltaTime);
        }
    }
}
