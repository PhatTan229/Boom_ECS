using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
            ecb.SetEnabled(item, true);
            ecb.SetComponent(item, LocalTransform.FromPosition(position));
            return item;
        }

        var newEntity = ecb.Instantiate(prefabs[name]);
        ecb.SetEnabled(newEntity, true);
        ecb.SetComponent(newEntity, LocalTransform.FromPosition(position));
        pool.Add(newEntity);
        return newEntity;
    }
}
