using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct PoolProcessSystem : ISystem, ISystemStartStop
{
    partial struct ProcessJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<PoolEnity> poolEntityLookup;

        void Execute([ChunkIndexInQuery] int index, Entity entity)
        {
            if (!poolEntityLookup.HasComponent(entity)) return;
            var poolEntity = poolEntityLookup.GetRefRO(entity);
            PoolData.ProcessPool(poolEntity);
        }
    }

    private ComponentLookup<PoolEnity> poolEntityLookup;

    public void OnStartRunning(ref SystemState state)
    {
        poolEntityLookup = state.GetComponentLookup<PoolEnity>();
    }

    public void OnUpdate(ref SystemState state) 
    {
        poolEntityLookup.Update(ref state);

        var job = new ProcessJob()
        {
            poolEntityLookup = poolEntityLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
        PoolData.Dispose();
    }
}
