using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class ExplosionUnitHelper
{
    public static List<(NativeList<GridPosition>, NativeList<Entity>)> hitClusters = new List<(NativeList<GridPosition>, NativeList<Entity>)>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Clear()
    {
        for (int i = 0; i < hitClusters.Count; i++)
        {
            var (key, value) = hitClusters[i];
            if (key.IsCreated) key.Dispose();
            if (value.IsCreated) value.Dispose();
        }
        hitClusters.Clear();
    }

    public static void RegisterDealDamge(NativeList<float3> pos)
    {
        var key = new NativeList<GridPosition>(Allocator.Persistent);
        foreach (var item in pos)
        {
            GridData.Instance.GetGridCoordination_Grid(item, out var grid);
            key.Add(grid.gridPosition);
        }
        hitClusters.Add((key, new NativeList<Entity>(Allocator.Persistent)));
    }

    public static bool TryAddHit(GridPosition pos, Entity hitEntity, out int hitTime)
    {
        hitTime = 0;
        for (int i = 0; i < hitClusters.Count; i++)
        {
            if (!hitClusters[i].Item1.Contains(pos)) continue;
            if (!hitClusters[i].Item2.Contains(hitEntity))
            {
                hitTime++;
                hitClusters[i].Item2.Add(hitEntity);
            }
        }
        return hitTime > 0;
    }

    public static void RemoveCluster(GridPosition pos)
    {
        for (int i = hitClusters.Count - 1; i >= 0; i--)
        {
            var (key, value) = hitClusters[i];
            if(key.Contains(pos))
            {
                value.Dispose();
                key.Dispose();
                hitClusters.RemoveAt(i);
            }
        }
    }
}

public partial struct ExplosionUnitMonitorSystem : ISystem, ISystemStartStop
{
    private ComponentLookup<Killable> killableLookup;

    public void OnStartRunning(ref SystemState state)
    {
        killableLookup = state.GetComponentLookup<Killable>();
    }

    public void OnUpdate(ref SystemState state)
    {
        killableLookup.Update(ref state);

        foreach (var (unit, coord) in SystemAPI.Query<RefRW<ExplosionUnit>, RefRO<GridCoordination>>().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            var currentGrid = SystemAPI.GetComponent<Grid>(coord.ValueRO.CurrentGrid).gridPosition;
            if (unit.ValueRO.lifeTime <= 0)
            {
                ExplosionUnitHelper.RemoveCluster(currentGrid);
                continue;
            }
            unit.ValueRW.lifeTime -= SystemAPI.Time.DeltaTime;
            var entities = GridCooridnateCollecttion.coordination[currentGrid];
            foreach (var e in entities)
            {
                if (!killableLookup.EntityExists(e) || !killableLookup.HasComponent(e)) continue;
                if(ExplosionUnitHelper.TryAddHit(currentGrid, e, out var hitTime))
                {
                    if (!killableLookup.TryGetComponent(e, out var killable, out var exist)) continue;
                    var stat = SystemAPI.GetComponentRW<StatData>(e);
                    for (int i = 0; i < hitTime; i++)
                    {
                        killable.TakeDamge(stat, 1f);
                        if (stat.ValueRO.currentStat.HP > 0) TintColorHelper.RegisterTint(e);
                        else DissolveAnimationHelper.RegisterDissolve(e);
                    }
                }
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
