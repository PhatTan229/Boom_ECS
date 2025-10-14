using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public static class PhysicsUtils
{
    internal struct RaycastHitFractionComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b)
        {
            if (a.Fraction < b.Fraction) return -1;
            if (a.Fraction > b.Fraction) return 1;
            return 0;
        }
    }

    /// <summary>
    /// Set up Entity Query to get PhysicsWorldSingleton
    /// If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
    /// </summary>
    public static Entity Raycast(float3 RayFrom, float3 RayTo, out RaycastHit hit)
    {
        return Raycast(RayFrom, RayTo, ~0u, ~0u, out hit);
    }

    public static void RaycastAll(float3 RayFrom, float3 RayTo, ref NativeList<RaycastHit> hits)
    {
        RaycastAll(RayFrom, RayTo, ~0u, ~0u, ref hits);
    }

    public static void RaycastAll(float3 rayFrom, float3 rayTo, uint collideWith, uint belongTo, ref NativeList<RaycastHit> hits)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();
        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var physicWorldSingleton = singletonQuery.GetSingleton<PhysicsWorldSingleton>();

        RaycastAll(physicWorldSingleton, rayFrom, rayTo, collideWith, belongTo, ref hits);

        singletonQuery.Dispose();
    }

    public static Entity Raycast(float3 RayFrom, float3 RayTo, uint collideWith, uint belongTo, out RaycastHit hit)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        RaycastInput input = new RaycastInput()
        {
            Start = RayFrom,
            End = RayTo,
            Filter = new CollisionFilter()
            {
                BelongsTo = belongTo,
                CollidesWith = collideWith,
                GroupIndex = 0
            }
        };

        hit = new RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity Raycast(PhysicsWorldSingleton physicsWorldSingleton, float3 RayFrom, float3 RayTo, out RaycastHit hit)
    {
        return Raycast(physicsWorldSingleton, RayFrom, RayTo, ~0u, ~0u, out hit);
    }

    public static Entity Raycast(PhysicsWorldSingleton physicsWorldSingleton, float3 RayFrom, float3 RayTo, uint collideWith, uint belongTo, out RaycastHit hit)
    {
        var collisionWorld = physicsWorldSingleton.CollisionWorld;

        RaycastInput input = new RaycastInput()
        {
            Start = RayFrom,
            End = RayTo,
            Filter = new CollisionFilter()
            {
                BelongsTo = belongTo,
                CollidesWith = collideWith,
                GroupIndex = 0
            }
        };

        hit = new RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static void RaycastAll(PhysicsWorldSingleton physicsWorldSingleton, float3 rayFrom, float3 rayTo, uint collideWith, uint belongTo, ref NativeList<RaycastHit> hits)
    {
        var collisionWorld = physicsWorldSingleton.CollisionWorld;

        var input = new RaycastInput
        {
            Start = rayFrom,
            End = rayTo,
            Filter = new CollisionFilter
            {
                BelongsTo = belongTo,
                CollidesWith = collideWith,
                GroupIndex = 0
            }
        };

        collisionWorld.CastRay(input, ref hits);
        hits.Sort(new RaycastHitFractionComparer());
    }
}
