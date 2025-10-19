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

    public void Explode(float3 position, ExplosionRange range, NativeHashMap<Grid, NativeList<Entity>> coordination, EntityCommandBuffer ecb, EntityManager entityManager, ref NativeList<float3> explosion)
    {
        var gridEntity = GridData.Instance.GetGridCoordination_Entity(position);
        var grid = entityManager.GetComponentData<Grid>(gridEntity);
        grid.travelable = true;
        ecb.SetComponent(gridEntity, grid);

        explosion.Add(position);

        using (var hitData = range.exploseRange.CheckRange(entity, position, coordination, ecb, entityManager, (uint)targetLayer, length, Allocator.Temp))
        {
            for (int i = 1; i < hitData.grids.Length; i++)
            {
                var spawnPosition = entityManager.GetComponentData<LocalTransform>(hitData.grids[i]).Position;
                if (!GridData.Instance.WorldToGrid(spawnPosition, out var gridPos)) return;
                var currentGrid = GridData.Instance.GetCellEntityAt(gridPos.Value);
                if (currentGrid != null)
                {
                    explosion.Add(spawnPosition);
                }
            }
        }
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
