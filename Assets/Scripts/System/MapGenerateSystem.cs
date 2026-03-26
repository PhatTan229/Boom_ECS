using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct MapGenerateSystem : ISystem, ISystemStartStop
{
    struct WallTag : IComponentData { }

    public void OnStartRunning(ref SystemState state)
    {
        var mapInfo = SystemAPI.GetSingleton<MapInfo>();
        foreach (var item in SystemAPI.Query<RefRW<SpriteIndex>>())
        {
            item.ValueRW.Value = UnityEngine.Random.Range(0, mapInfo.TileCount);
        }

        SpawnWall(ref state, mapInfo);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    private void SpawnWall(ref SystemState state, MapInfo mapInfo)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach(var (grid, transform) in SystemAPI.Query<RefRO<Grid>, RefRO<LocalTransform>>())
        {
            switch(grid.ValueRO.gridType)
            {
                case GridType.Wall:
                    var newEntity = PoolData.GetEntity(new FixedString64Bytes("Wall") , transform.ValueRO.Position, ecb, state.EntityManager);
                    ecb.AddComponent(newEntity, new WallTag());
                    break;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        foreach (var (_, entity) in SystemAPI.Query<RefRO<WallTag>>().WithEntityAccess())
        {
            var childs = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
            foreach (var e in childs)
            {
                if (state.EntityManager.HasComponent<SpriteIndex>(e.Value))
                {
                    state.EntityManager.SetComponentData(e.Value, new SpriteIndex() { Value = UnityEngine.Random.Range(0, mapInfo.wallTextureSize.x * mapInfo.wallTextureSize.y) });
                }
            }
        }
    }
}
