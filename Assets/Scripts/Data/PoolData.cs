using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct PoolEnity : IComponentData 
{
    public FixedString64Bytes name;
    public Entity entity;
}

public sealed class PoolData
{
    public const int DEFAULT_CAPICITY = 10;

    public static NativeHashMap<FixedString64Bytes, NativeList<Entity>> allPools;
    public static NativeHashMap<FixedString64Bytes, Entity> prefabs;
    public static void Init()
    {
        allPools = new NativeHashMap<FixedString64Bytes, NativeList<Entity>>(DEFAULT_CAPICITY, Allocator.Persistent);
        prefabs = new NativeHashMap<FixedString64Bytes, Entity>(DEFAULT_CAPICITY, Allocator.Persistent);
    }

    public static void RegisterPrefab(EntityInfo info)
    {
        prefabs.Add(info.Name, info.entity);
    }

    public static Entity GetEntity(FixedString64Bytes name, float3 position, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        if (!allPools.ContainsKey(name)) allPools.Add(name, new NativeList<Entity>(Allocator.Persistent));

        var pool = allPools[name];

        foreach (var item in pool)
        {
            if (entityManager.IsEnabled(item)) continue;
            Debug.Log("Has pool");
            ecb.SetEnabled(item, true);
            ecb.SetComponent(item, LocalTransform.FromPosition(position));
            return item;
        }
        var newEntity = ecb.Instantiate(prefabs[name]);
        ecb.SetEnabled(newEntity, true);
        ecb.AddComponent(newEntity, new PoolEnity() { name = name, entity = newEntity });
        ecb.SetComponent(newEntity, LocalTransform.FromPosition(position));
        return newEntity;
    }

    public static void ProcessPool(RefRO<PoolEnity> poolEntity)
    {
        var pool = allPools[poolEntity.ValueRO.name];
        if (!pool.Contains(poolEntity.ValueRO.entity)) pool.Add(poolEntity.ValueRO.entity);
    }

    public static void Dispose()
    {
        foreach (var item in allPools)
        {
            item.Value.Dispose();
        }
        allPools.Dispose();
        prefabs.Dispose();
    }
}
