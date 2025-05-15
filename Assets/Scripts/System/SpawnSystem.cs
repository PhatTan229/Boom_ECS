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
public partial struct SpawnSystem : ISystem
{
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Spawm");
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (prefab, entity) in
                     SystemAPI.Query<RefRO<PrefabLoadResult>>().WithEntityAccess())
            {
                var instance = ecb.Instantiate(prefab.ValueRO.PrefabRoot);
                ecb.RemoveComponent<RequestEntityPrefabLoaded>(entity);
                ecb.RemoveComponent<PrefabLoadResult>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
