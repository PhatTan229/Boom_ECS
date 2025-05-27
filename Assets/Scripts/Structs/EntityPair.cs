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

    public Entity GetEntity<T>(ComponentLookup<T> lookup) where T : unmanaged, IComponentData
    {
        if (lookup.HasComponent(EntityA)) return EntityA;
        if (lookup.HasComponent(EntityB)) return EntityB;
        throw new Exception($"Can't find component {nameof(T)}");
    }

    public bool TryGetEntity<T>(ComponentLookup<T> lookup, out Entity entity) where T : unmanaged, IComponentData
    {
        entity = Entity.Null;
        if (lookup.HasComponent(EntityA))
        {
            entity = EntityA;
            return true;
        }
        if (lookup.HasComponent(EntityB))
        {
            entity = EntityB;
            return true;
        }
        return false;
    }

    public bool TryGetEntity<T>(ComponentLookup<T> lookup, out Entity entity, out Entity other ) where T : unmanaged, IComponentData
    {
        entity = Entity.Null;
        other = Entity.Null;
        if (lookup.HasComponent(EntityA))
        {
            entity = EntityA;
            other = EntityB;
            return true;
        }
        if (lookup.HasComponent(EntityB))
        {
            entity = EntityB;
            other = EntityA;
            return true;
        }
        return false;
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
