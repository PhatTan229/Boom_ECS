using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public struct ExploseRange_Default : IExploseRange, IDisposable
{
    private (NativeList<Entity>, NativeList<Entity>) upCollection;
    private (NativeList<Entity>, NativeList<Entity>) downCollection;
    private (NativeList<Entity>, NativeList<Entity>) leftCollection;
    private (NativeList<Entity>, NativeList<Entity>) rightCollection;
    public BombHitData CheckRange(Entity entity, float3 position, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator)
    {
        var fireLength = length;
        var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
        var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        CheckDirection(entity, position, Direction.Up, ecb, entityManager, targetLayer, length, allocator, ref upCollection);
        CheckDirection(entity, position, Direction.Down, ecb, entityManager, targetLayer, length, allocator, ref downCollection);
        CheckDirection(entity, position, Direction.Left, ecb, entityManager, targetLayer, length, allocator, ref leftCollection);
        CheckDirection(entity, position, Direction.Right, ecb, entityManager, targetLayer, length, allocator, ref rightCollection);

        var killables = new NativeList<Entity>(allocator);
        var grids = new NativeList<Entity>(allocator);

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

    private void CheckDirection(Entity entity, float3 position, float3 direction, EntityCommandBuffer ecb, EntityManager entityManager, uint targetLayer, int length, Allocator allocator, ref (NativeList<Entity>, NativeList<Entity>) collection)
    {
        var fireLength = length;
        var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
        var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

        collection.Item1 = new NativeList<Entity>(allocator); // killabes
        collection.Item2 = new NativeList<Entity>(allocator); //grids

        PhysicsUtils.RaycastAll(position, position + (direction * length), targetLayer, collider.Value.Value.GetCollisionFilter().BelongsTo, ref hits);
        var hitEntity = Entity.Null;
        foreach (var item in hits)
        {
            if (item.Entity.Equals(entity)) continue;
            hitEntity = item.Entity;
            break;
        }

        if (!hitEntity.Equals(Entity.Null))
        {
            var wallTransform = entityManager.GetComponentData<LocalTransform>(hitEntity);
            fireLength = (int)math.distance(position, wallTransform.Position);
            if (entityManager.HasComponent<Killable>(hitEntity))
            {
                Debug.Log("Hit Killable");
                fireLength += 1;
                collection.Item1.Add(hitEntity);
            }
        }

        for (int i = 1; i < fireLength; i++)
        {
            var spawnPosition = position + (direction * i);
            if (!GridData.Instance.WorldToGrid(spawnPosition, out var gridPos)) break;
            collection.Item2.Add(GridData.Instance.GetCellEntityAt(gridPos.Value));
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
            AddComponentObject(entity, new ExplosionRange() { exploseRange = new ExploseRange_Default()});
        }
    }
}
