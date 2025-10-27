using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct ExploseRange_Default : IExploseRange, IDisposable
{
    private (NativeList<Entity>, NativeList<Entity>) upCollection;
    private (NativeList<Entity>, NativeList<Entity>) downCollection;
    private (NativeList<Entity>, NativeList<Entity>) leftCollection;
    private (NativeList<Entity>, NativeList<Entity>) rightCollection;

    public BombHitData CheckRange(Entity entity, float3 position, NativeHashMap<Grid, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator)
    {
        var fireLength = length;
        var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
        var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
        var killables = new NativeList<Entity>(allocator);
        var grids = new NativeList<Entity>(allocator);

        var origin = GridData.Instance.GetGridCoordination_Entity(position);
        grids.Add(origin);
        CheckDirection(entity, position, coordination, Direction.Up, ecb, entityManager, targetLayer, length, allocator, ref upCollection);
        CheckDirection(entity, position, coordination, Direction.Down, ecb, entityManager, targetLayer, length, allocator, ref downCollection);
        CheckDirection(entity, position, coordination, Direction.Left, ecb, entityManager, targetLayer, length, allocator, ref leftCollection);
        CheckDirection(entity, position, coordination, Direction.Right, ecb, entityManager, targetLayer, length, allocator, ref rightCollection);

        killables.AddRange(upCollection.Item1.AsArray());
        killables.AddRange(downCollection.Item1.AsArray());
        killables.AddRange(leftCollection.Item1.AsArray());
        killables.AddRange(rightCollection.Item1.AsArray());

        grids.AddRange(upCollection.Item2.AsArray());
        grids.AddRange(downCollection.Item2.AsArray());
        grids.AddRange(leftCollection.Item2.AsArray());
        grids.AddRange(rightCollection.Item2.AsArray());

        return new BombHitData()
        {
            hits = killables,
            grids = grids
        };
    }

    private void CheckDirection(Entity entity, float3 position, NativeHashMap<Grid, NativeList<Entity>> coordination, float3 direction, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator, ref (NativeList<Entity>, NativeList<Entity>) collection)
    {
        var fireLength = length;
        var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
        var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        collection.Item1 = new NativeList<Entity>(allocator); // killabes
        collection.Item2 = new NativeList<Entity>(allocator); //grids

        for (int i = 1; i < length - 1; i++)
        {
            var stop = false;
            var gridEntity = GridData.Instance.GetGridCoordination_Entity(position + (direction * i));
            var grid = entityManager.GetComponentData<Grid>(gridEntity);
            if (!grid.travelable)
            {
                foreach (var item in coordination[grid])
                {
                    if (entityManager.HasComponent<Bomb>(item) && item != entity)
                    {
                        collection.Item1.Add(item);
                    }
                }
                return;
            }
            collection.Item2.Add(gridEntity);
            foreach (var item in coordination[grid])
            {
                if (entityManager.HasComponent<Killable>(item))
                {
                    collection.Item1.Add(item);
                    stop = true;
                }
            }
            if (stop) return;
        }
    }


    public void Dispose()
    {
        upCollection.Item1.Dispose();
        downCollection.Item1.Dispose();
        leftCollection.Item1.Dispose();
        rightCollection.Item1.Dispose();

        upCollection.Item2.Dispose();
        downCollection.Item2.Dispose();
        leftCollection.Item2.Dispose();
        rightCollection.Item2.Dispose();
    }
}

public class ExplosePartten_Default : ExpolsePartten_Base
{
    class ExplosePartten_Default_Baker : Baker<ExplosePartten_Default>
    {
        public override void Bake(ExplosePartten_Default authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new ExplosionRange() { exploseRange = new ExploseRange_Default() });
        }
    }
}
