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
        var textureSize = int2.zero;
        var enable = true;
        foreach (var (wall, entity) in SystemAPI.Query<RefRO<Wall>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            switch (wall.ValueRO.wallType)
            {
                case WallType.Wall:
                    textureSize = mapInfo.wallTextureSize;
                    break;
                case WallType.Destroyable:        
                    var rate = UnityEngine.Random.Range(0f, 1f);
                    enable = rate < 0.7f;
                    ecb.SetEnabled(entity, enable);
                    textureSize = mapInfo.destroyableTextureSize;
                    break;
            }

            var childs = state.EntityManager.GetBuffer<Child>(entity);
            foreach (var e in childs)
            {
                if(!enable)
                {
                    ecb.SetEnabled(e.Value, false);
                    continue;
                }
                if (state.EntityManager.HasComponent<SpriteIndex>(e.Value))
                {
                    state.EntityManager.SetComponentData(e.Value, new SpriteIndex() { Value = UnityEngine.Random.Range(0, textureSize.x * textureSize.y) });
                }
            }

        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();    
    }
}
