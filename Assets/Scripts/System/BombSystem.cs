using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
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
            if (bomb.lifeTime - bomb.currentLifeTime < 0.2f) return; 
            var refCollider = colliderLookup.GetRefRW(bomb.entity);
            var bodyType = refCollider.ValueRO.Value.Value.GetCollisionResponse();
            if (bodyType == CollisionResponsePolicy.Collide) return;
            if (inTriggerLookup[bomb.entity].Length > 0) return;
            bomb.SetStatic(refCollider);
        }
    }

    [BurstCompile]
    partial struct ResetBombJob : IJobEntity
    {
        [ReadOnly] public NativeParallelHashSet<Entity> activeEntities;
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsCollider> colliderLookup;
        [ReadOnly] public BufferLookup<InTrigger> inTriggerLookup;

        void Execute([ChunkIndexInQuery] int index, Bomb bomb)
        {
            if (activeEntities.Contains(bomb.entity)) return;
            var refCollider = colliderLookup.GetRefRW(bomb.entity);
            var trigger = inTriggerLookup[bomb.entity];
            bomb.SetDefault(refCollider, trigger);
            bomb.ResetLifeTime();
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

        foreach (var (bomb, collider, triggers, transform, entity) in SystemAPI.Query<RefRW<Bomb>, RefRW<PhysicsCollider>, DynamicBuffer<InTrigger>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            bomb.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            if(bomb.ValueRW.currentLifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
                bomb.ValueRO.SetDefault(collider, triggers);
                bomb.ValueRW.ResetLifeTime();
                PoolData.GetEntity(new FixedString64Bytes("Flame"), transform.ValueRO.Position, ecb, state.EntityManager);
            }
        }
        ecb.Playback(state.EntityManager);

        var setStaticjob = new SetStaticJob()
        {
            ecb = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            inTriggerLookup = inTriggerLookup,
            colliderLookup = colliderLookup
        };

        var activeBomb = new NativeList<Entity>(Allocator.Temp);
        foreach (var item in SystemAPI.Query<Bomb>())
        {
            activeBomb.Add(item.entity);
        }

        if(activeBomb.Length > 0)
        {
            var parallelSet = new NativeParallelHashSet<Entity>(activeBomb.Length, Allocator.TempJob);
            foreach (var item in activeBomb)
            {
                parallelSet.Add(item);
            }

            var resetJob = new ResetBombJob()
            {
                activeEntities = parallelSet,
                colliderLookup = colliderLookup,
                inTriggerLookup = inTriggerLookup
            };
            var resetJobHandle = resetJob.ScheduleParallel(state.Dependency);

            state.Dependency = setStaticjob.ScheduleParallel(resetJobHandle);
            state.Dependency = parallelSet.Dispose(state.Dependency);
        }
        else state.Dependency = setStaticjob.ScheduleParallel(state.Dependency);
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
        activeBomb.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
