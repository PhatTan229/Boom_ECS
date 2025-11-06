using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct BombHitData : IDisposable
{
    public NativeList<Entity> grids;
    public NativeList<Entity> hits;

    public void Dispose()
    {
        grids.Dispose();
        hits.Dispose();
    }
}

public class ExplosionRange : IComponentData
{
    public IExploseRange exploseRange;

    public BombHitData CheckRange(Entity entity, float3 position, NativeHashMap<GridPosition, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator)
    {
        return exploseRange.CheckRange(entity, position, coordination, ecb, entityManager, targetLayer, length, allocator);
    }
}

public interface IExploseRange
{
    BombHitData CheckRange(Entity entity, float3 position, NativeHashMap<GridPosition, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator);
}

public abstract class ExpolsePartten_Base : MonoBehaviour
{
}
