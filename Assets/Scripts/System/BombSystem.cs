using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct ExplodeInfo : IDisposable
{
    public NativeList<float3> explosion;
    public float lifeTime;

    public ExplodeInfo(Allocator allocator, float lifeTime)
    {
        explosion = new NativeList<float3>(allocator);
        this.lifeTime = lifeTime;
    }

    public void Dispose()
    {
        explosion.Dispose();
    }
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct BombSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct BombPosProcessJob : IJobEntity
    {
        [ReadOnly] public NativeParallelHashSet<Entity> activeEntities;
        public EntityCommandBuffer.ParallelWriter ecb;
        [ReadOnly] public BufferLookup<InTrigger> inTriggerLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsCollider> colliderLookup;

        public void Execute([EntityIndexInQuery] int index, Bomb entity)
        {
            SetStatic(entity, index);
        }

        private void SetStatic(Bomb bomb, int index)
        {
            if (bomb.currentLifeTime > bomb.lifeTime - 2f) return;
            var refCollider = colliderLookup.GetRefRW(bomb.entity);
            var bodyType = refCollider.ValueRO.Value.Value.GetCollisionResponse();
            if (bodyType == CollisionResponsePolicy.Collide) return;
            if (inTriggerLookup[bomb.entity].Length > 0) return;
            var newColliderData = refCollider.ValueRW.Value.Value.Clone();
            newColliderData.Value.SetCollisionResponse(CollisionResponsePolicy.Collide);
            ecb.SetComponent(index, bomb.entity, new PhysicsCollider { Value = newColliderData });
        }
    }

    [BurstCompile]
    private partial struct SpawnExplodeJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<float3> explodePosition;
        [ReadOnly] public NativeList<Entity> inactiveExplosion;
        [NativeDisableParallelForRestriction] public ComponentLookup<ParticleData> particleLookup;

        void IJobParallelFor.Execute(int index)
        {
            if (index > explodePosition.Length - 1) return;
            var particle = particleLookup.GetRefRW(inactiveExplosion[index]);
            particle.ValueRW.ResetLifeTime(explodePosition[index]);
        }
    }

    private ComponentLookup<PhysicsMass> massLookup;
    private ComponentLookup<PhysicsCollider> colliderLookup;
    private ComponentLookup<Bomb> bombLookup;
    private ComponentLookup<Killable> killableLookup;
    private ComponentLookup<StatData> statLookup;
    private BufferLookup<InTrigger> inTriggerLookup;

    public void OnStartRunning(ref SystemState state)
    {
        massLookup = SystemAPI.GetComponentLookup<PhysicsMass>();
        colliderLookup = SystemAPI.GetComponentLookup<PhysicsCollider>();
        inTriggerLookup = SystemAPI.GetBufferLookup<InTrigger>();
        bombLookup = SystemAPI.GetComponentLookup<Bomb>();
        statLookup = SystemAPI.GetComponentLookup<StatData>();
        killableLookup = SystemAPI.GetComponentLookup<Killable>();

        var playerLookup = SystemAPI.GetComponentLookup<Player>();
        var enemyLookup = SystemAPI.GetComponentLookup<Enemy>();
        OnTriggerContainer.Subscribe(new BombTrigger(playerLookup, enemyLookup));
    }

    public void OnUpdate(ref SystemState state)
    {
        massLookup.Update(ref state);
        colliderLookup.Update(ref state);
        inTriggerLookup.Update(ref state);
        bombLookup.Update(ref state);
        statLookup.Update(ref state);
        killableLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var explosion = new NativeList<float3>(Allocator.TempJob);
        var chainedBomb = new NativeList<Entity>(Allocator.Temp);
        foreach (var (bomb, collider, triggers, transform, range, entity) in SystemAPI.Query<RefRW<Bomb>, RefRW<PhysicsCollider>, DynamicBuffer<InTrigger>, RefRO<LocalTransform>, ExplosionRange>().WithEntityAccess())
        {
            bomb.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            if (bomb.ValueRW.currentLifeTime <= 0)
            {
                chainedBomb.Add(entity);
                var position = transform.ValueRO.Position;
                bomb.ValueRW.Explode(position, range, GridCooridnateCollecttion.coordination, ecb, state.EntityManager, bombLookup, killableLookup, ref explosion, ref chainedBomb);
            }
        }

        chainedBomb.Dispose();

        JobHandle spawnJobHandle = new JobHandle();
        var spawn = false;

        var query = SystemAPI.QueryBuilder().WithAll<ParticleData>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build().ToEntityArray(Allocator.TempJob);
        var inactiveExplosion = new NativeList<Entity>(Allocator.TempJob);

        if (explosion.Length > 0)
        {
            //ExplosionUnitHelper.RegisterDealDamge(explosion);
            var damageBuffer = SystemAPI.GetSingletonBuffer<ExplosionUnitMonitorBuffer>();
            damageBuffer.Add(new ExplosionUnitMonitorBuffer() { hitClusters = ExplosionUnitHelper.CreateBufferElement(explosion) });
            foreach (var item in query)
            {
                if (!state.EntityManager.IsEnabled(item)) inactiveExplosion.Add(item);
            }

            if (inactiveExplosion.Length == 0 || inactiveExplosion.Length < explosion.Length)
            {
                for (int i = explosion.Length - 1; i >= 0; i--)
                {
                    var position = explosion[i];
                    PoolData.Instantiate(new FixedString64Bytes("Flame"), position, ecb, state.EntityManager);
                    explosion.RemoveAt(i);
                }
            }
            else
            {
                spawn = true;
                var spawnExplodeJob = new SpawnExplodeJob()
                {
                    explodePosition = explosion,
                    inactiveExplosion = inactiveExplosion,
                    particleLookup = state.GetComponentLookup<ParticleData>()
                };

                spawnJobHandle = spawnExplodeJob.ScheduleByRef(explosion.Length, 32, state.Dependency);
            }
        }

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
        }
        var posProcessJob = new BombPosProcessJob()
        {
            activeEntities = parallelSet,
            colliderLookup = colliderLookup,
            inTriggerLookup = inTriggerLookup,
            ecb = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
        };

        var posProcessHandle = posProcessJob.ScheduleParallel(state.Dependency);

        if (spawn) state.Dependency = JobHandle.CombineDependencies(spawnJobHandle, posProcessHandle);
        else state.Dependency = posProcessHandle;
        state.Dependency.Complete();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        state.Dependency = parallelSet.Dispose(state.Dependency);
        activeBomb.Dispose();
        query.Dispose();
        inactiveExplosion.Dispose();
        explosion.Dispose();

        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    private void DealDamge(ref SystemState state, NativeList<Entity> killables)
    {
        foreach (var item in killables)
        {
            var killable = killableLookup[item];
            var stat = statLookup.GetRefRW(item);
            killable.TakeDamge(stat, 1f);
            if (stat.ValueRO.currentStat.HP <= 0) DissolveAnimationHelper.RegisterDissolve(item);
            else TintColorHelper.RegisterTint(item);
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
