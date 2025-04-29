using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;

public partial struct GridSystem : ISystem
{
    private bool init;
    public void OnUpdate(ref SystemState state)
    {
        Init(ref state);
    }

    private void Init(ref SystemState state)
    {
        if (init) return;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (grid, entity) in SystemAPI.Query<RefRO<Grid>>().WithEntityAccess())
        {
            var buffer = ecb.SetBuffer<GridNeighbour>(entity);
            foreach (var offset in GridUtils.neighbourGridPosition)
            {
                var neighborEntity = GridUtils.Instance.GetCellEntityAt(grid.ValueRO.gridPosition + offset);
                if (neighborEntity != Entity.Null) buffer.Add(new GridNeighbour { value = neighborEntity });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        init = true;
    }
}
