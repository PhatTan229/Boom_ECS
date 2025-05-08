using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

//class này chạy trên mainthread
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class GameSystem : SystemBase
{
    private void InitializeRender()
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
            SpriteRenderData.Instance.SetRenderInfo(info, entity);
        }
        ecb.Playback(Utils.EntityManager);
        ecb.Dispose();
    }

    protected override void OnUpdate()
    {
        var mousePosition = Input.mousePosition;

        foreach (var mousePos in SystemAPI.Query<RefRW<MousePosition>>())
        {
            var pos = Camera.main.ScreenToWorldPoint(mousePosition);
            pos.y = 0;
            mousePos.ValueRW.value = pos;
        }
    }
}
