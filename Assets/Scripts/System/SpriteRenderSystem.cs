using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct SpriteRenderSystem : ISystem, ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
        var list = new NativeList<RefRW<SpriteRenderInfo>>(Allocator.Temp);
        foreach (var info in SystemAPI.Query<RefRW<SpriteRenderInfo>>())
        {
            list.Add(info);
        }
        SpriteRenderData.Instance = new SpriteRenderData(list);
        list.Dispose();
        var toSet = new NativeList<(RefRW<SpriteRenderInfo>, Entity)>(Allocator.Temp);

        foreach (var (info, entity) in SystemAPI.Query<RefRW<SpriteRenderInfo>>().WithEntityAccess())
        {
            toSet.Add((info, entity));
        }
        foreach (var pair in toSet)
        {
            SpriteRenderData.Instance.SetRenderInfo(pair.Item1, pair.Item2); 
        }
        toSet.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
