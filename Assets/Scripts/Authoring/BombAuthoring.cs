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

    public void SetDefault(RefRW<PhysicsCollider> collider, DynamicBuffer<InTrigger> inTriggers)
    {
        inTriggers.Clear();
        collider.ValueRW.Value.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
    }

    public void SetStatic(RefRW<PhysicsCollider> collider)
    {
        collider.ValueRW.Value.Value.SetCollisionResponse(CollisionResponsePolicy.Collide);
    }

    public void Explode(float3 position, ExplosionRange range, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        var gridEntity = GridData.Instance.GetGridCoordination(position);
        var grid = entityManager.GetComponentData<Grid>(gridEntity);
        grid.travelable = true;
        ecb.SetComponent(gridEntity, grid);
        PoolData.GetEntity(new FixedString64Bytes("Flame"), position, ecb, entityManager);

        var hitData = range.exploseRange.CheckRange(entity, position, ecb, entityManager, (uint)targetLayer, length, Allocator.Temp);

        for (int i = 1; i < hitData.grids.Length; i++)
        {
            var spawnPosition = entityManager.GetComponentData<LocalTransform>(hitData.grids[i]).Position;
            if (!GridData.Instance.WorldToGrid(spawnPosition, out var gridPos)) return;
            var currentGrid = GridData.Instance.GetCellEntityAt(gridPos.Value);
            if (currentGrid != null) PoolData.GetEntity(new FixedString64Bytes("Flame"), spawnPosition, ecb, entityManager);
        }

        //SpawnFire(position, Direction.Up, ecb, entityManager);
        //SpawnFire(position, Direction.Down, ecb, entityManager);
        //SpawnFire(position, Direction.Left, ecb, entityManager);
        //SpawnFire(position, Direction.Right, ecb, entityManager);
    }

    //private void SpawnFire(float3 position, float3 direction, EntityCommandBuffer ecb, EntityManager entityManager)
    //{
    //    var fireLength = length;
    //    var collider = entityManager.GetComponentData<PhysicsCollider>(entity);
    //    var hits = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);
    //    PhysicsUtils.RaycastAll(position, position + (direction * length), (uint)targetLayer, collider.Value.Value.GetCollisionFilter().BelongsTo, ref hits);

    //    var hitEntity = Entity.Null;
    //    foreach (var item in hits)
    //    {
    //        if (item.Entity.Equals(entity)) continue;
    //        hitEntity = item.Entity;
    //        break;
    //    }
    //    if (!hitEntity.Equals(Entity.Null))
    //    {
    //        var wallTransform = entityManager.GetComponentData<LocalTransform>(hitEntity);
    //        fireLength = (int)math.distance(position, wallTransform.Position);
    //    }
    //    for (int i = 1; i < fireLength; i++)
    //    {
    //        var spawnPosition = position + (direction * i);
    //        if (!GridData.Instance.WorldToGrid(spawnPosition, out var gridPos)) return;
    //        var grid = GridData.Instance.GetCellEntityAt(gridPos.Value);
    //        if (grid != null) PoolData.GetEntity(new FixedString64Bytes("Flame"), spawnPosition, ecb, entityManager);
    //    }
    //}

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
