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
}

public interface IExploseRange
{
    public BombHitData CheckRange(Entity entity, float3 position, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator);
}

public abstract class ExpolsePartten_Base : MonoBehaviour
{
}
