using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct BombSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct SetStaticJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsCollider> colliderLookup;
        [ReadOnly] public BufferLookup<InTrigger> inTriggerLookup;
        public EntityCommandBuffer.ParallelWriter ecb;

        void Execute([ChunkIndexInQuery] int index, Bomb bomb)
        {
            var refCollider = colliderLookup.GetRefRW(bomb.entity);
            var bodyType = refCollider.ValueRO.Value.Value.GetCollisionResponse();
            if (bodyType == CollisionResponsePolicy.Collide) return;
            if (inTriggerLookup[bomb.entity].Length > 0) return;
            bomb.SetStatic(refCollider);
        }
    }

    partial struct CountDownJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsCollider> colliderLookup;
        [NativeDisableParallelForRestriction] public BufferLookup<InTrigger> inTriggerLookup;
        public EntityCommandBuffer ecb;
        public float deltaTime;

        void Execute([ChunkIndexInQuery] int index, Entity entity, ref Bomb bomb)
        {
            var refCollider = colliderLookup.GetRefRW(bomb.entity);
            var refBuffer = inTriggerLookup[bomb.entity];
            bomb.currentLifeTime -= deltaTime;
            if(bomb.currentLifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
                bomb.SetDefault(refCollider, refBuffer);
            }
        }
    }

    private ComponentLookup<PhysicsMass> massLookup;
    private ComponentLookup<PhysicsCollider> colliderLookup;
    private BufferLookup<InTrigger> inTriggerLookup;

    public void OnStartRunning(ref SystemState state)
    {
        massLookup = SystemAPI.GetComponentLookup<PhysicsMass>();
        colliderLookup = SystemAPI.GetComponentLookup<PhysicsCollider>();
        inTriggerLookup = SystemAPI.GetBufferLookup<InTrigger>();

        var playerLookup = SystemAPI.GetComponentLookup<Player>();
        var enemyLookup = SystemAPI.GetComponentLookup<Enemy>();
        OnTriggerContainer.Subscribe(new BombTrigger(playerLookup, enemyLookup));
    }

    public void OnUpdate(ref SystemState state) 
    {
        massLookup.Update(ref state);
        colliderLookup.Update(ref state);
        inTriggerLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (bomb, collider, triggers, entity) in SystemAPI.Query<RefRW<Bomb>, RefRW<PhysicsCollider>, DynamicBuffer<InTrigger>>().WithEntityAccess())
        {
            bomb.ValueRW.lifeTime -= SystemAPI.Time.DeltaTime;
            if(bomb.ValueRW.lifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
                bomb.ValueRO.SetDefault(collider, triggers);
                
            }
        }
        ecb.Playback(state.EntityManager);

        var job = new SetStaticJob()
        {
            ecb = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            inTriggerLookup = inTriggerLookup,
            colliderLookup = colliderLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
