using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public struct ExplosionUnitMonitorBuffer : IBufferElementData
{
    public (NativeList<GridPosition>, NativeList<Entity>) hitClusters;
}

public static class ExplosionUnitHelper
{
    public static (NativeList<GridPosition>, NativeList<Entity>) CreateBufferElement(NativeList<float3> pos)
    {
        var key = new NativeList<GridPosition>(Allocator.Persistent);
        foreach (var item in pos)
        {
            GridData.Instance.GetGridCoordination_Grid(item, out var grid);
            key.Add(grid.gridPosition);
        }
        return (key, new NativeList<Entity>(Allocator.Persistent));
    }
}

public partial struct ExplosionUnitMonitorSystem : ISystem, ISystemStartStop
{
    private ComponentLookup<Killable> killableLookup;
    private ComponentLookup<PhysicsCollider> colliderLookup;

    public void OnStartRunning(ref SystemState state)
    {
        killableLookup = state.GetComponentLookup<Killable>();
        colliderLookup = state.GetComponentLookup<PhysicsCollider>();
        state.EntityManager.CreateSingletonBuffer<ExplosionUnitMonitorBuffer>(nameof(ExplosionUnitMonitorBuffer));
    }

    public void OnUpdate(ref SystemState state)
    {
        killableLookup.Update(ref state);
        colliderLookup.Update(ref state);

        var buffer = SystemAPI.GetSingletonBuffer<ExplosionUnitMonitorBuffer>();

        foreach (var (unit, coord, entity) in SystemAPI.Query<RefRW<ExplosionUnit>, RefRO<GridCoordination>>().WithEntityAccess())
        {
            var currentGrid = SystemAPI.GetComponent<Grid>(coord.ValueRO.CurrentGrid).gridPosition;

            unit.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            var entities = GridCooridnateCollecttion.coordination[currentGrid];
            foreach (var e in entities)
            {
                if (!killableLookup.EntityExists(e) || !killableLookup.HasComponent(e)) continue;
                if (TryAddHit(ref state, buffer, currentGrid, e, out var hitTime))
                {
                    if (!colliderLookup.TryGetComponent(e, out var collider, out var _)) continue;
                    if (!PhysicLayerUtils.HasLayer(collider.Value.Value.GetCollisionFilter().BelongsTo, unit.ValueRO.targetLayer)) continue;
                    if (!killableLookup.TryGetComponent(e, out var killable, out var _)) continue;
                    DealDamge(ref state, e, hitTime, killable);
                }
            }

            
        }

        foreach (var (unit, coord, entity) in SystemAPI.Query<RefRW<ExplosionUnit>, RefRO<GridCoordination>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            var currentGrid = SystemAPI.GetComponent<Grid>(coord.ValueRO.CurrentGrid).gridPosition;
            if (!state.EntityManager.IsEnabled(entity) && unit.ValueRO.currentLifeTime <= 0)
            {
                unit.ValueRW.ResetLifeTime();
                RemoveCluster(ref state, buffer, currentGrid);
            }
        }
    }

    private void DealDamge(ref SystemState state, Entity e, int hitTime, Killable killable)
    {
        var stat = SystemAPI.GetComponentRW<StatData>(e);
        for (int i = 0; i < hitTime; i++)
        {
            killable.TakeDamge(stat, 1f);
            if (stat.ValueRO.currentStat.HP > 0) TintColorHelper.RegisterTint(e);
            else DissolveAnimationHelper.RegisterDissolve(e);
        }
    }

    public bool TryAddHit(ref SystemState state, DynamicBuffer<ExplosionUnitMonitorBuffer> buffer, GridPosition pos, Entity hitEntity, out int hitTime)
    {
        hitTime = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (!buffer[i].hitClusters.Item1.Contains(pos)) continue;
            if (!buffer[i].hitClusters.Item2.Contains(hitEntity))
            {
                hitTime++;
                buffer[i].hitClusters.Item2.Add(hitEntity);
            }
        }
        return hitTime > 0;
    }

    public void RemoveCluster(ref SystemState state, DynamicBuffer<ExplosionUnitMonitorBuffer> buffer, GridPosition pos)
    {
        for (int i = buffer.Length - 1; i >= 0; i--)
        {
            var (key, value) = buffer[i].hitClusters;
            if (key.Contains(pos))
            {
                value.Dispose();
                key.Dispose();
                buffer.RemoveAt(i);
            }
        }
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
        var buffer = SystemAPI.GetSingletonBuffer<ExplosionUnitMonitorBuffer>();
        for (int i = buffer.Length - 1; i >= 0; i--)
        {
            var (key, value) = buffer[i].hitClusters;
            value.Dispose();
            key.Dispose();
            buffer.RemoveAt(i);
        }
    }
}
