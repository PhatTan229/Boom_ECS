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
    partial struct ControlJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecbParallel;
        [NativeDisableParallelForRestriction] public ComponentLookup<Controlable> controlLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsVelocity> velocityLookup;
        [ReadOnly] public ComponentLookup<StatData> statLookup;
        public Entity entity;
        public float3 direction;
        public bool pressBomb;

        void Execute([ChunkIndexInQuery] int index)
        {
            var control = controlLookup[entity];
            var velocity = velocityLookup.GetRefRW(entity);
            control.ControlMovement(velocity, direction, statLookup[entity].currentStat.speed);
            if (pressBomb) Debug.Log("BOM");
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

        var job = new ControlJob()
        {
            ecbParallel = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            entity = controlable,
            controlLookup = controlLookup,
            velocityLookup = velocityLookup,
            statLookup = statLookup,
            direction = input.ValueRO.direction,
            pressBomb = input.ValueRW.pressBomb
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
