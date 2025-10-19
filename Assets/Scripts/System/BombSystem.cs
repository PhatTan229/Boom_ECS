using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
        var explosion = new NativeList<float3>(Allocator.Temp);
        foreach (var (bomb, collider, triggers, transform, range, entity) in SystemAPI.Query<RefRW<Bomb>, RefRW<PhysicsCollider>, DynamicBuffer<InTrigger>, RefRO<LocalTransform>, ExplosionRange>().WithEntityAccess())
        {
            bomb.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            if (bomb.ValueRW.currentLifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
                bomb.ValueRO.SetDefault(collider, triggers);
                bomb.ValueRW.ResetLifeTime();
                var position = transform.ValueRO.Position;
                bomb.ValueRW.Explode(position, range, GridCooridnateCollecttion.coordination, ecb, state.EntityManager, ref explosion);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();


        if(explosion.Length > 0)
        {
            var query = SystemAPI.QueryBuilder().WithAll<ParticleData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build().ToEntityArray(Allocator.Temp);

            Debug.Log($"Query Lenght: {query.Length}");

            if(query.Length == 0 || query.Length < explosion.Length)
            {
                var newEcb = new EntityCommandBuffer(Allocator.Temp);
                for (int i = explosion.Length - 1; i >= 0; i--)
                {
                    PoolData.GetEntity(new FixedString64Bytes("Flame"), explosion[i], newEcb, state.EntityManager);
                }
                newEcb.Playback(state.EntityManager);
                newEcb.Dispose();
            }
            else
            {
                var i = explosion.Length - 1;
                foreach (var item in query)
                {
                    if (explosion.Length <= 0) break;
                    var particle = SystemAPI.GetComponentRW<ParticleData>(item);
                    if (particle.ValueRO.currentLifeTime > 0) continue;

                    var transform = SystemAPI.GetComponentRW<LocalTransform>(item);

                    particle.ValueRW.ResetLifeTime(explosion[i]);
                    explosion.RemoveAt(i);
                    i--;
                }
            }   
        }



        explosion.Dispose();

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

        if (activeBomb.Length > 0)
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
