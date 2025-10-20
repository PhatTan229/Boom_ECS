using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

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

    [BurstCompile]
    partial struct SpawnExplodeJob : IJobEntity
    {
        [ReadOnly] public NativeList<float3> explodePosition;
        [ReadOnly] public NativeList<Entity> inactiveExplosion;
        [NativeDisableParallelForRestriction] public ComponentLookup<ParticleData> particleLookup;

        void Execute([ChunkIndexInQuery] int index)
        {
            if (index > explodePosition.Length - 1) return;
            var particle = particleLookup.GetRefRW(inactiveExplosion[index]);
            particle.ValueRW.ResetLifeTime(explodePosition[index]);
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
        var explosion = new NativeList<float3>(Allocator.TempJob);
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

        if (explosion.Length > 0)
        {
            var query = SystemAPI.QueryBuilder().WithAll<ParticleData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build().ToEntityArray(Allocator.TempJob);
            var inactiveExplosion = new NativeList<Entity>(Allocator.TempJob);
 
            foreach (var item in query)
            {
                if (!state.EntityManager.IsEnabled(item)) inactiveExplosion.Add(item);
            }

            if (inactiveExplosion.Length == 0 || inactiveExplosion.Length < explosion.Length)
            {
                var newEcb = new EntityCommandBuffer(Allocator.Temp);
                for (int i = explosion.Length - 1; i >= 0; i--)
                {
                    var position = explosion[i];
                    PoolData.GetEntity(new FixedString64Bytes("Flame"), position, newEcb, state.EntityManager);
                    explosion.RemoveAt(i);
                }
                newEcb.Playback(state.EntityManager);
                newEcb.Dispose();
            }
            else
            {
                var spawnExplodeJob = new SpawnExplodeJob()
                {
                    explodePosition = explosion,
                    inactiveExplosion = inactiveExplosion,
                    particleLookup = state.GetComponentLookup<ParticleData>()
                };

                state.Dependency = spawnExplodeJob.ScheduleParallel(state.Dependency);
                state.Dependency.Complete();
            }

            query.Dispose();
            inactiveExplosion.Dispose();
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


        var parallelSet = new NativeParallelHashSet<Entity>(activeBomb.Length, Allocator.TempJob);
        if (activeBomb.Length > 0)
        {
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

        state.Dependency.Complete();
        activeBomb.Dispose();
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
