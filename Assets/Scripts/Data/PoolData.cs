using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.Progress;

public sealed class PoolData
{
    public const int DEFAULT_CAPICITY = 10;

    public static NativeHashMap<FixedString64Bytes, NativeList<Entity>> allPools;

    public static void Init()
    {
        if (allPools.IsCreated) return;
        allPools = new NativeHashMap<FixedString64Bytes, NativeList<Entity>>(DEFAULT_CAPICITY, Allocator.Persistent);
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

        var newEntity = ecb.CreateEntity();
        ecb.SetComponent(newEntity, LocalTransform.FromPosition(position));
        pool.Add(newEntity);
        return newEntity;
    }
}
