using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct MapGenerateSystem : ISystem, ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
        var mapInfo = SystemAPI.GetSingleton<MapInfo>();
        foreach (var item in SystemAPI.Query<RefRW<SpriteIndex>>())
        {
            item.ValueRW.Value = Random.Range(0, mapInfo.tileCount);
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
