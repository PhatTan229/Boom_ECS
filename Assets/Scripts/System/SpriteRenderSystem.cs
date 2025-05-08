using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct SpriteRenderSystem : ISystem
{
    private void Init(ref SystemState state)
    {
        if (SpriteRenderData.Instance != null) return;
        Debug.Log("OnStartRunning");
        var list = new NativeList<RefRW<SpriteRenderInfo>>(Allocator.Temp);
        foreach (var info in SystemAPI.Query<RefRW<SpriteRenderInfo>>())
        {
            list.Add(info);
        }
        SpriteRenderData.Instance = new SpriteRenderData(list);
        list.Dispose();
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (info, entity) in SystemAPI.Query<RefRW<SpriteRenderInfo>>().WithEntityAccess())
        {
            Debug.Log("AA");
            SpriteRenderData.Instance.SetRenderInfo(info, entity, ecb);
        }
        ecb.Playback(Utils.EntityManager);
        ecb.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        Init(ref state);
    }
}
