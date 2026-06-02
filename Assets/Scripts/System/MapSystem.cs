using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public struct MapHelpRequest
{
    public GridPosition position;
    public Action<RefRW<Grid>> action;
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct MapSystem : ISystem, ISystemStartStop
{
    private NativeList<Entity> disabledWalls;

    public void OnStartRunning(ref SystemState state)
    {
        disabledWalls = new NativeList<Entity>(Allocator.Persistent);

        var mapEntity = SystemAPI.GetSingletonEntity<MapInfo>();
        var mapInfo = SystemAPI.GetComponentRW<MapInfo>(mapEntity);
        foreach (var item in SystemAPI.Query<RefRW<SpriteIndex>>())
        {
            item.ValueRW.Value = UnityEngine.Random.Range(0, mapInfo.ValueRW.TileCount);
        }

        SpawnWall(ref state, mapInfo.ValueRW);
    }

    private void SpawnWall(ref SystemState state, MapInfo mapInfo)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var textureSize = int2.zero;
        var enable = true;
        var wallIndex = UnityEngine.Random.Range(0, mapInfo.wallTextureSize.x * mapInfo.wallTextureSize.y);
        var destroyableIndexes = Utils.GetUniqueRandomNumbers(0, mapInfo.destroyableTextureSize.x * mapInfo.wallTextureSize.y, 2);
        SpriteIndex spriteIndex = new SpriteIndex();
        foreach (var (wall, entity) in SystemAPI.Query<RefRO<Wall>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            switch (wall.ValueRO.wallType)
            {
                case WallType.Wall:
                    textureSize = mapInfo.wallTextureSize;
                    spriteIndex = new SpriteIndex() { Value = wallIndex };
                    enable = true;
                    break;
                case WallType.Destroyable:        
                    var rate = UnityEngine.Random.Range(0f, 1f);
                    enable = rate < 0.7f;
                    ecb.SetEnabled(entity, enable);
                    textureSize = mapInfo.destroyableTextureSize;
                    spriteIndex = new SpriteIndex() { Value = destroyableIndexes[UnityEngine.Random.Range(0, destroyableIndexes.Length)] };
                    var gridEntity = GridData.Instance.GetCellEntityAt(wall.ValueRO.gridPosition);
                    var grid = SystemAPI.GetComponentRW<Grid>(gridEntity);
                    grid.ValueRW.travelable = !enable;
                    break;
            }

            Utils.SetComponentDataInChildren(entity, spriteIndex, state.EntityManager, (child) =>
            {
                if (!enable)
                {
                    ecb.SetEnabled(child, false);
                }
            });

        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();    
    }

    public void OnUpdate(ref SystemState state)
    {
        var wallBuffer = SystemAPI.GetSingletonBuffer<WallBuffer>();

        for (int i = wallBuffer.Length - 1; i >= 0; i--) 
        {
            var item = wallBuffer.ElementAt(i).wall;

            if(!state.EntityManager.IsEnabled(item))
            {
                if (disabledWalls.Contains(item)) continue;
                disabledWalls.Add(item);
                wallBuffer.RemoveAt(i);

                var wall = SystemAPI.GetComponentRO<Wall>(item);
                var gridEntity = GridData.Instance.GetCellEntityAt(wall.ValueRO.gridPosition);
                var grid = SystemAPI.GetComponentRW<Grid>(gridEntity);
                grid.ValueRW.travelable = true;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        disabledWalls.Dispose();
    }
}
