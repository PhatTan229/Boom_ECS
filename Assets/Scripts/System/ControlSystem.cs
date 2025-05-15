using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
public partial struct ControlSystem : ISystem, ISystemStartStop
{
    partial struct MoveControlJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbParallel;
        [NativeDisableParallelForRestriction] public ComponentLookup<Controlable> controlLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsVelocity> velocityLookup;
        [ReadOnly] public ComponentLookup<StatData> statLookup;
        public Entity entity;
        public float3 direction;

        void Execute([ChunkIndexInQuery] int index)
        {
            var control = controlLookup[entity];
            var velocity = velocityLookup.GetRefRW(entity);
            control.ControlMovement(velocity, direction, statLookup[entity].currentStat.speed);
        }
    }

    private Entity controlable;
    private ComponentLookup<Controlable> controlLookup;
    private ComponentLookup<PhysicsVelocity> velocityLookup;
    private ComponentLookup<StatData> statLookup;

    public void OnStartRunning(ref SystemState state)
    {
        controlable = SystemAPI.GetSingletonEntity<Controlable>();
        controlLookup = state.GetComponentLookup<Controlable>();
        velocityLookup = state.GetComponentLookup<PhysicsVelocity>();
        statLookup = state.GetComponentLookup<StatData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        controlLookup.Update(ref state);
        velocityLookup.Update(ref state);
        statLookup.Update(ref state);   

        var input = SystemAPI.GetSingletonRW<InputStorage>();

        if(input.ValueRO.pressBomb)
        {
            var ecb = GameSystem.ecbSystem.CreateCommandBuffer();
            PoolData.GetEntity(new FixedString64Bytes("Bomb"), float3.zero, ecb, state.EntityManager);
        }

        var job = new MoveControlJob()
        {
            ecbParallel = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            entity = controlable,
            controlLookup = controlLookup,
            velocityLookup = velocityLookup,
            statLookup = statLookup,
            direction = input.ValueRO.direction,
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
