using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public partial struct GridSystem : ISystem ,ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (grid, entity) in SystemAPI.Query<RefRO<Grid>>().WithEntityAccess())
        {
            var buffer = ecb.SetBuffer<GridNeighbour>(entity);
            foreach (var offset in GridData.neighbourGridPosition)
            {
                var neighborEntity = GridData.Instance.GetCellEntityAt(grid.ValueRO.gridPosition + offset);
                if (neighborEntity != Entity.Null) buffer.Add(new GridNeighbour { value = neighborEntity });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
        GridData.Instance.Dispose();
    }
}
