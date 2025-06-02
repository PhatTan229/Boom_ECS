using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public struct InTrigger : IBufferElementData
{
    public Entity value;
}

public struct Bomb : IComponentData, IEquatable<Bomb>
{
    public Entity entity;
    public readonly int lenght;
    public float lifeTime;
    public float currentLifeTime;

    public Bomb(Entity entity, float lifeTime, int lenght)
    {
        this.entity = entity;
        this.lifeTime = lifeTime;
        currentLifeTime = lifeTime;
        this.lenght = lenght;
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

    public void Explode(float3 position, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        var gridEntity = GridData.Instance.GetGridCoordination(position);
        var grid = entityManager.GetComponentData<Grid>(gridEntity);
        grid.travelable = true;
        ecb.SetComponent(gridEntity, grid);
        PoolData.GetEntity(new FixedString64Bytes("Flame"), position, ecb, entityManager);
        for (int i = 1; i < lenght; i++)
        {
            SpawnFire(position + new float3(i, 0, 0), ecb, entityManager);
            SpawnFire(position + new float3(-i, 0, 0), ecb, entityManager);
            SpawnFire(position + new float3(0, 0, i), ecb, entityManager);
            SpawnFire(position + new float3(0, 0, -i), ecb, entityManager);
        }
    }

    private void SpawnFire(float3 position, EntityCommandBuffer ecb, EntityManager entityManager)
    {
        if (!GridData.Instance.WorldToGrid(position, out var gridPos)) return;
        var grid = GridData.Instance.GetCellEntityAt(gridPos.Value);
        if (grid != null) PoolData.GetEntity(new FixedString64Bytes("Flame"), position, ecb, entityManager);
    }

    public bool Equals(Bomb other)
    {
        return entity == other.entity;
    }
}

public class BombAuthoring : MonoBehaviour
{
    public float lifeTime;
    public int lenght;

    class BombBaker : Baker<BombAuthoring>
    {
        public override void Bake(BombAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bomb(entity, authoring.lifeTime, authoring.lenght));
            AddBuffer<InTrigger>(entity);
        }
    }
}
