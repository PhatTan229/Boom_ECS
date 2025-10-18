using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct MapSizeData : ISharedComponentData
{
    public int value;
}

public static class GridCooridnateCollecttion
{
    public static NativeHashMap<Grid, NativeList<Entity>> coordination;

    public static void Dispose()
    {
        if (coordination.IsCreated)
        {
            foreach (var key in coordination)
            {
                key.Value.Dispose();
            }
            coordination.Dispose();
            coordination = default;
        }
    }
}

public partial struct GridCoordinationUpdateSystem : ISystem, ISystemStartStop
{
    private NativeHashMap<Grid, NativeList<Entity>> _coordination;

    public void OnStartRunning(ref SystemState state)
    {
        if (_coordination.IsCreated)
        {
            // Dispose inner lists
            foreach (var key in GridData.Instance.allCells)
            {
                if (_coordination.TryGetValue(key, out var oldList))
                {
                    if (oldList.IsCreated) oldList.Dispose();
                }
            }
            _coordination.Dispose();
            _coordination = default;
        }

        _coordination = new NativeHashMap<Grid, NativeList<Entity>>(GridData.Instance.MapSize, Allocator.Persistent);
        foreach (var item in GridData.Instance.allCells)
        {
            _coordination.Add(item, new NativeList<Entity>(Allocator.Persistent));
        }

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (coord, entity) in SystemAPI.Query<GridCoordination>().WithEntityAccess())
        {
            ecb.AddSharedComponent(entity, new MapSizeData() { value = GridData.Instance.MapSize });
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void OnUpdate(ref SystemState state) 
    {
        foreach (var item in GridData.Instance.allCells)
        {
            if (_coordination.TryGetValue(item, out var list))
            {
                if (list.IsCreated)
                    list.Clear();
            }
        }

        foreach (var (coord, transform, entity) in SystemAPI.Query<RefRW<GridCoordination>, RefRO<LocalTransform>>().WithEntityAccess())
        {
            coord.ValueRW.CurrentGrid = GridData.Instance.GetGridCoordination_Entity(transform.ValueRO.Position);
            if (GridData.Instance.GetGridCoordination_Grid(transform.ValueRO.Position, out var grid))
            {
                _coordination[grid].Add(entity);
            }
        }

        GridCooridnateCollecttion.coordination = _coordination;
    }

    public void OnStopRunning(ref SystemState state)
    {
        if (_coordination.IsCreated)
        {
            foreach (var key in _coordination)
            {
                key.Value.Dispose();
            }
            _coordination.Dispose();
            _coordination = default;
        }

        GridCooridnateCollecttion.coordination = default;
    }
}
