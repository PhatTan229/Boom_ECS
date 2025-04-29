using System;
using Unity.Entities;

public struct EntityPair : IEquatable<EntityPair>
{
    public Entity EntityA;
    public Entity EntityB;

    public EntityPair(Entity a, Entity b)
    {
        EntityA = a;
        EntityB = b;
    }

    public bool Equals(EntityPair other)
    {
        return (EntityA == other.EntityA && EntityB == other.EntityB) ||
               (EntityA == other.EntityB && EntityB == other.EntityA);
    }

    public override int GetHashCode()
    {
        return EntityA.Index ^ EntityB.Index;
    }
}
