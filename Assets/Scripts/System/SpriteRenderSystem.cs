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
        var toSet = new NativeList<Entity>(Allocator.Temp);

        foreach (var (_, entity) in SystemAPI.Query<RefRW<SpriteRenderInfo>>().WithEntityAccess())
        {
            toSet.Add(entity);
        }

        //var lookup = state.GetComponentLookup<SpriteRenderInfo>();
        foreach (var item in toSet)
        {
            var info = SystemAPI.GetComponentRW<SpriteRenderInfo>(item);
            SpriteRenderData.Instance.SetRenderInfo(info, item); 
        }
        toSet.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
