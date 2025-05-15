using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct MapSizeData : ISharedComponentData
{
    public int value;
}

public partial struct GridCoordinationUpdateSystem : ISystem, ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
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
        foreach (var (coord, transform) in SystemAPI.Query<RefRW<GridCoordination>, RefRO<LocalTransform>>())
        {
            coord.ValueRW.CurrentGrid = GridData.Instance.GetGridCoordination(transform.ValueRO.Position);
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
