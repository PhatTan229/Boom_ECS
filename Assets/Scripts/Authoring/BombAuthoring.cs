using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
internal class BombData
{
    public float lifeTime;
    public int lenght;
    public PhysicsCategory targetLayer;
}

public struct Bomb : IComponentData, IEquatable<Bomb>
{
    public Entity entity;
    public readonly int length;
    public float lifeTime;
    public float currentLifeTime;
    public PhysicsCategory targetLayer;

    internal Bomb(Entity entity, BombData data)
    {
        this.entity = entity;
        lifeTime = data.lifeTime;
        currentLifeTime = lifeTime;
        length = data.lenght;
        targetLayer = data.targetLayer;
    }

    public void ResetLifeTime()
    {
        currentLifeTime = lifeTime;
    }

    public void SetDefault(DynamicBuffer<InTrigger> inTriggers)
    {
        inTriggers.Clear();
    }

    public void Explode(float3 position, ExplosionRange range, NativeHashMap<Grid, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, ComponentLookup<Bomb> bombLookup, ref NativeList<float3> explosion)
    {
        ecb.SetEnabled(entity, false);
        var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
        var newColliderData = collider.Value.Value.Clone();
        newColliderData.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
        ecb.SetComponent(entity, new PhysicsCollider { Value = newColliderData });
        var trigger = entityManager.GetBuffer<InTrigger>(entity);
        trigger.Clear();
        ResetLifeTime();

        var gridEntity = GridData.Instance.GetGridCoordination_Entity(position);
        var grid = entityManager.GetComponentData<Grid>(gridEntity);
        grid.travelable = true;
        ecb.SetComponent(gridEntity, grid);

        explosion.Add(position);
        using (var hitData = range.CheckRange(entity, position, coordination, ecb, entityManager, (uint)targetLayer, length, Allocator.Temp))
        {
            for (int i = 0; i < hitData.grids.Length; i++)
            {
                var spawnPosition = entityManager.GetComponentData<LocalTransform>(hitData.grids[i]).Position;
                if (!GridData.Instance.WorldToGrid(spawnPosition, out var gridPos)) return;
                var currentGrid = GridData.Instance.GetCellEntityAt(gridPos.Value);
                if (currentGrid != null && !explosion.Contains(spawnPosition))
                {
                    explosion.Add(spawnPosition);
                }
            }

            for (int i = 0; i < hitData.hits.Length; i++)
            {
                if (bombLookup.HasComponent(hitData.hits[i]))
                {
                    var exploseRange = entityManager.GetComponentObject<ExplosionRange>(hitData.hits[i]);
                    var bombPosition = entityManager.GetComponentData<LocalTransform>(hitData.hits[i]).Position;
                    var exclude = Direction.None;
                    if (bombPosition.z == position.z)
                    {
                        if (bombPosition.x > position.x) exclude = Direction.Left;
                        else exclude = Direction.Right;
                    }
                    else if (bombPosition.x == position.x)
                    {
                        if (bombPosition.z > position.z) exclude = Direction.Down;
                        else exclude = Direction.Up;
                    }
                    exploseRange.excludeDirection = exclude;
                    var bom = entityManager.GetComponentData<Bomb>(hitData.hits[i]);
                    bom.currentLifeTime = 0f;
                    ecb.SetComponent(hitData.hits[i], bom);
                }
            }
        }
        range.excludeDirection = Direction.None;
    }

    public bool Equals(Bomb other)
    {
        return entity == other.entity;
    }
}

public class BombAuthoring : MonoBehaviour
{
    [SerializeField] internal BombData data;

    class BombBaker : Baker<BombAuthoring>
    {
        public override void Bake(BombAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bomb(entity, authoring.data));
        }
    }
}
