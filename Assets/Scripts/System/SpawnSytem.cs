using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SpawnSytem : ISystem
{
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<EntityPrefab>()
            .WithNone<PrefabLoadResult>().Build();
        state.EntityManager.AddComponent<RequestEntityPrefabLoaded>(query);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            var prefab = SystemAPI.GetSingletonRW<EntityPrefab>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entity = ecb.Instantiate(prefab.ValueRO.Value);
            ecb.SetComponent(entity, LocalTransform.FromPosition(float3.zero));
            ecb.RemoveComponent<RequestEntityPrefabLoaded>(entity);
            ecb.RemoveComponent<PrefabLoadResult>(entity);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
}}
