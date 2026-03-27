using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct KillableMonitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (killable, stat, entity) in SystemAPI.Query<RefRO<Killable>, RefRO<StatData>>().WithEntityAccess())
        {
            if (stat.ValueRO.currentStat.HP <= 0)
            {
                ecb.SetEnabled(entity, false);
                continue;
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
           