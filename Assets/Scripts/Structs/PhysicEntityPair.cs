using System;
using Unity.Entities;
using Unity.Physics;

public struct PhysicEntityPair : IEquatable<PhysicEntityPair>
{
    public EntityPair pair;

    public Entity EntityA => pair.EntityA;
    public int BodyIndexA;
    public ColliderKey ColliderKeyA;

    public Entity EntityB => pair.EntityB;
    public int BodyIndexB;
    public ColliderKey ColliderKeyB;

    public PhysicEntityPair(TriggerEvent triggerEvent)
    {
        pair = new EntityPair(triggerEvent.EntityA, triggerEvent.EntityB);
        BodyIndexA = triggerEvent.BodyIndexA;
        BodyIndexB = triggerEvent.BodyIndexB;
        ColliderKeyA = triggerEvent.ColliderKeyA;
        ColliderKeyB = triggerEvent.ColliderKeyB;
    }

    public PhysicEntityPair(EntityPair pair, ColliderKeyPair colliderKeyPair, int bodyIndexA, int bodyIndexB)
    {
        this.pair = pair;
        BodyIndexA = bodyIndexA;
        BodyIndexB = bodyIndexB;
        ColliderKeyA = colliderKeyPair.ColliderKeyA;
        ColliderKeyB = colliderKeyPair.ColliderKeyB;
    }

    public PhysicEntityPair(EntityPair pair, ColliderKey colliderKeyA, ColliderKey colliderKeyB , int bodyIndexA, int bodyIndexB)
    {
        this.pair = pair;
        BodyIndexA = bodyIndexA;
        BodyIndexB = bodyIndexB;
        ColliderKeyA = colliderKeyA;
        ColliderKeyB = colliderKeyB;
    }

    public Entity GetEntity<T>(ComponentLookup<T> lookup) where T : unmanaged, IComponentData
    {
        return pair.GetEntity(lookup);
    }

    public bool TryGetEntity<T>(ComponentLookup<T> lookup, out Entity entity, out Entity other) where T : unmanaged, IComponentData
    {
        return pair.TryGetEntity(lookup, out entity, out other);
    }

    public bool Equals(PhysicEntityPair other) => pair.Equals(other.pair);

    public override int GetHashCode() => pair.GetHashCode();

}
