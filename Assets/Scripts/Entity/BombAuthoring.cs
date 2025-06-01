using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct InTrigger : IBufferElementData
{
    public Entity value;
}

public struct Bomb : IComponentData, IEquatable<Bomb>
{
    public Entity entity;
    public float lifeTime;
    public float currentLifeTime;

    public Bomb(Entity entity, float lifeTime)
    {
        this.entity = entity;
        this.lifeTime = lifeTime;
        currentLifeTime = lifeTime;
    }

    public void SetDefault(RefRW<PhysicsCollider> collider, DynamicBuffer<InTrigger> inTriggers)
    {
        inTriggers.Clear();
        collider.ValueRW.Value.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
    }

    public void SetStatic(RefRW<PhysicsCollider> collider)
    {
        currentLifeTime = lifeTime;
        collider.ValueRW.Value.Value.SetCollisionResponse(CollisionResponsePolicy.Collide);
    }

    public bool Equals(Bomb other)
    {
        return entity == other.entity;
    }
}

public class BombAuthoring : MonoBehaviour
{
    public float lifeTime;

    class BombBaker : Baker<BombAuthoring>
    {
        public override void Bake(BombAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bomb(entity, authoring.lifeTime));
            AddBuffer<InTrigger>(entity);
        }
    }
}
