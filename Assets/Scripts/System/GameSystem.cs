using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

//class này chạy trên mainthread
//[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class GameSystem : SystemBase
{
    public static EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnStartRunning()
    {
        ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

        var query = SystemAPI.QueryBuilder()
           .WithAll<PrefabReference>()
           .WithNone<PrefabLoadResult>().Build();
        EntityManager.AddComponent<RequestEntityPrefabLoaded>(query);
        PoolData.Init();
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
