using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

[BurstCompile]
public partial struct ControlSystem : ISystem
{
    partial struct ControlJob : IJobEntity
    {
        public float speed;
        public float3 direction;
        public bool pressBomb;

        void Execute([ChunkIndexInQuery] int index, RefRW<Controlable> control, RefRW<PhysicsVelocity> velocity)
        {
            control.ValueRW.ControlMovement(velocity, direction, speed);
            if (pressBomb) Debug.Log("BOM");
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, control, stat) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<Controlable>, RefRO<StatData>>())
        {
            var input = SystemAPI.GetSingletonRW<InputStorage>();

            var job = new ControlJob()
            {
                speed = stat.ValueRO.currentStat.speed,
                direction = input.ValueRO.direction,
                pressBomb = input.ValueRW.pressBomb
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            //control.ValueRW.ControlMovement(velocity, input.ValueRW.direction, stat.ValueRO.currentStat.speed);
        }
    }
}
