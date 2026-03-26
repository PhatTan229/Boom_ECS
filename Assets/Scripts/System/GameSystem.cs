using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

//class này chạy trên mainthread
//[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class GameSystem : SystemBase
{
    public static EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnStartRunning()
    {
        ecbSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

        var prefabRef = SystemAPI.QueryBuilder()
           .WithAll<PrefabReference>()
           .WithNone<PrefabLoadResult>().Build();
        EntityManager.AddComponent<RequestEntityPrefabLoaded>(prefabRef);

        PoolData.Init();
    }

    protected override void OnUpdate()
    {
        foreach (var info in SystemAPI.Query<PrefabReference>())
        {
            var prefabInfo = SystemAPI.GetComponentRO<EntityInfo>(info.value);
            PoolData.RegisterPrefab(prefabInfo.ValueRO);
        }
        ecbSystem.Update();
    }
}
