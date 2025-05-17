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
    private ComponentLookup<LocalTransform> transformLookUp;

    public void OnStartRunning(ref SystemState state)
    {
        controlable = SystemAPI.GetSingletonEntity<Controlable>();
        controlLookup = state.GetComponentLookup<Controlable>();
        velocityLookup = state.GetComponentLookup<PhysicsVelocity>();
        statLookup = state.GetComponentLookup<StatData>();
        transformLookUp = state.GetComponentLookup<LocalTransform>();
    }

    public void OnUpdate(ref SystemState state)
    {
        controlLookup.Update(ref state);
        velocityLookup.Update(ref state);
        statLookup.Update(ref state);
        transformLookUp.Update(ref state);   

        var input = SystemAPI.GetSingletonRW<InputStorage>();

        if(input.ValueRO.pressBomb)
        {
            PutBomb(ref state);
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

    private void PutBomb(ref SystemState state)
    {
        var ecb = GameSystem.ecbSystem.CreateCommandBuffer();
        var transform = transformLookUp[controlable];
        var grid = GridData.Instance.GetGridCoordination(transform.Position);
        var gridPosition = transformLookUp[grid];
        PoolData.GetEntity(new FixedString64Bytes("Bomb"), gridPosition.Position, ecb, state.EntityManager);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
