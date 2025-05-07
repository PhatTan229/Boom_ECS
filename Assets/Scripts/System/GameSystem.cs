using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static MoveKey;


//class này chạy trên mainthread
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class GameSystem : SystemBase
{
    protected override void OnCreate()
    {
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
