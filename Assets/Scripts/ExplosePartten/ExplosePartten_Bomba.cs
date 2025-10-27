using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct ExploseRange_Bomba : IExploseRange, IDisposable
{
    public BombHitData CheckRange(Entity entity, float3 position, NativeHashMap<Grid, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator)
    {
        var killables = new NativeList<Entity>(allocator);
        var grids = new NativeList<Entity>(allocator);

        return new BombHitData()
        {
            hits = killables,
            grids = grids
        };
    }

    public void Dispose()
    {
       
    }
}

public class ExplosePartten_Bomba : ExpolsePartten_Base
{
    class ExplosePartten_Bomba_Baker : Baker<ExplosePartten_Bomba>
    {
        public override void Bake(ExplosePartten_Bomba authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ExplosionRange() { exploseRange = new ExploseRange_Bomba() });
        }
    }
}
