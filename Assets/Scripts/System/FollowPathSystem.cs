using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct FollowPathSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct FollowJob : IJobEntity
    {
        public Entity target;
        [ReadOnly] public LocalTransform targetTransform;
        public float deltaTime;

        void Execute([ChunkIndexInQuery] int index, RefRW<PathFinding> pathFinding, DynamicBuffer<Path> path, RefRW<LocalTransform> transform, RefRW<PhysicsVelocity> velocity, RefRO<StatData> stat)
        {
            pathFinding.ValueRW.FollowPath(velocity, transform, pathFinding, targetTransform, stat, deltaTime);
        }
    }

    private ComponentLookup<LocalTransform> transfomLookup;

    public void OnStartRunning(ref SystemState state)
    {
        transfomLookup = state.GetComponentLookup<LocalTransform>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        transfomLookup.Update(ref state);

        foreach (var (pathFinding, path, transform, velocity, stat) in SystemAPI.Query<RefRW<PathFinding>, DynamicBuffer<Path>, RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRO<StatData>>())
        {
            if (pathFinding.ValueRO.currentIndex >= path.Length) continue;
            var targetEntity = path[pathFinding.ValueRO.currentIndex].value;
            var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetEntity);
            var job = new FollowJob()
            {
                targetTransform = targetTransform,
                deltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);

            //pathFinding.ValueRW.FollowPath(velocity, transform, pathFinding, targetTransform, stat, SystemAPI.Time.DeltaTime);
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
