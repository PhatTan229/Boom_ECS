using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Utils
{
    public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    public static FixedString32Bytes FixString32_Emty => new FixedString32Bytes();

    public static float2 ToFloat2(this Vector2 vector)
    {
        return new float2 (vector.x, vector.y);
    }

    public static float2 ToFloat2(this Vector3 vector)
    {
        return new float2(vector.x, vector.y);
    }


    public static float3 ToFloat3(this Vector2 vector) 
    {
        return new float3(vector.x, vector.y, 0);    
    }

    public static float3 ToFloat3(this Vector3 vector)
    {
        return new float3(vector.x, vector.y, vector.z);
    }

    public static BlobAssetReference<Unity.Physics.Collider> ConvertToBlobCollider_Box(this Collider2D collider2D)
    {
        var box = (BoxCollider2D)collider2D;
        var filter = new CollisionFilter()
        {
            BelongsTo = 1u << collider2D.gameObject.layer,
            CollidesWith = PhysicLayerUtils.GetCollisonMask(collider2D.gameObject.layer)
        };

        var collider = Unity.Physics.BoxCollider.Create(
            new Unity.Physics.BoxGeometry
            {
                Center = (Vector3)box.offset,
                Orientation = box.transform.rotation,
                Size = (Vector3)box.size,
                BevelRadius = 0
            }, filter
        );

        return collider;
    }

    public static BlobAssetReference<Unity.Physics.Collider> ConvertToBlobCollider_Circle(this Collider2D collider2D)
    {
        var circle = (CircleCollider2D)collider2D;
        var filter = new CollisionFilter()
        {
            BelongsTo = 1u << collider2D.gameObject.layer,
            CollidesWith = PhysicLayerUtils.GetCollisonMask(collider2D.gameObject.layer)
        };
        var collider = Unity.Physics.SphereCollider.Create(
            new Unity.Physics.SphereGeometry
            {
                Center = (Vector3)circle.offset,
                Radius = circle.radius
            },filter
        );
        return collider;
    }

    public static Entity CreateSingleton<T>(string name) where T : unmanaged, IComponentData
    {
        var query = EntityManager.CreateEntityQuery(typeof(T));
        if (query.IsEmpty)
        {
            var entity = EntityManager.CreateSingleton<T>(name);
            return entity;
        }
        else
        {
            return query.GetSingletonEntity();
        }
    }

    public static Entity CreateSingleton<T>(EntityManager entityManager, string name) where T : unmanaged, IComponentData
    {
        var query = entityManager.CreateEntityQuery(typeof(T));
        if (query.IsEmpty)
        {
            var entity = entityManager.CreateSingleton<T>(name);
            return entity;
        }
        else
        {
            return query.GetSingletonEntity();
        }
    }

    public static FixedString32Bytes FixString32(string str)
    {
        if (str.Length <= 32) return new FixedString32Bytes(str);
        var subStr = str.Substring(0, 32);
        return new FixedString32Bytes(subStr);
    }

    public static FixedString64Bytes FixString64(string str)
    {
        if (str.Length <= 64) return new FixedString64Bytes(str);
        var subStr = str.Substring(0, 64);
        return new FixedString64Bytes(subStr);
    }


    public static T GetComponentDataInChildren<T>(Entity entity, out Entity child) where T : unmanaged, IComponentData
    {
        return GetComponentDataInChildren<T>(entity, EntityManager, out child);
    }

    public static T GetComponentDataInChildren<T>(Entity entity, EntityManager entityManager, out Entity child) where T : unmanaged, IComponentData
    {
        child = Entity.Null;
        var children = entityManager.GetBuffer<Child>(entity);
        for (int i = 0; i < children.Length; i++)
        {
            if (entityManager.HasComponent<T>(children[i].Value))
            {
                child = children[i].Value;
                return entityManager.GetComponentData<T>(children[i].Value);
            }
        }
        return default;
    }

    public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup, out Entity child) where T : unmanaged, IComponentData
    {
        child = Entity.Null;
        var children = childLookup[entity];
        for (int i = 0; i < children.Length; i++)
        {
            if (lookup.HasComponent(children[i].Value))
            {
                child = children[i].Value;
                return lookup[children[i].Value];
            }
        }
        return default;
    }

    public static T GetComponentDataInChildren<T>(Entity entity, BufferLookup<Child> childLookup, ComponentLookup<T> lookup) where T : unmanaged, IComponentData
    {
        var children = childLookup[entity];
        for (int i = 0; i < children.Length; i++)
        {
            if (lookup.HasComponent(children[i].Value))
            {
                return lookup[children[i].Value];
            }
        }
        return default;
    }


    public static void SetComponentDataInChildren<T>(Entity entity, T value, out Entity child) where T : unmanaged, IComponentData
    {
        SetComponentDataInChildren<T>(entity, value, EntityManager, out child); 
    }

    public static void SetComponentDataInChildren<T>(Entity entity, T value, EntityManager entityManager, out Entity child) where T : unmanaged, IComponentData
    {
        child = Entity.Null;
        var children = entityManager.GetBuffer<Child>(entity);
        for (int i = 0; i < children.Length; i++)
        {
            if (entityManager.HasComponent<T>(children[i].Value))
            {
                child = children[i].Value;
                var component = entityManager.GetComponentData<T>(children[i].Value);
                entityManager.SetComponentData<T>(children[i].Value, value);
            }
        }
    }

    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, out Entity child) where T : unmanaged, IBufferElementData
    {
        return GetBufferInChildren<T>(entity, EntityManager, out child);
    }

    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, EntityManager entityManager, out Entity child) where T : unmanaged, IBufferElementData
    {
        child = Entity.Null;
        var children = entityManager.GetBuffer<Child>(entity);
        for (int i = 0; i < children.Length; i++)
        {
            if (entityManager.HasBuffer<T>(children[i].Value))
            {
                child = children[i].Value;
                return entityManager.GetBuffer<T>(children[i].Value);
            }
        }
        return default;
    }

    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, BufferLookup<Child> childLookup, BufferLookup<T> bufferLookup, out Entity child) where T : unmanaged, IBufferElementData
    {
        child = Entity.Null;
        var children = childLookup[entity];
        for (int i = 0; i < children.Length; i++)
        {
            if (bufferLookup.HasBuffer(children[i].Value))
            {
                child = children[i].Value;
                return bufferLookup[children[i].Value];
            }
        }
        return default;
    }

    public static DynamicBuffer<T> GetBufferInChildren<T>(Entity entity, BufferLookup<Child> childLookup, BufferLookup<T> bufferLookup) where T : unmanaged, IBufferElementData
    {
        var children = childLookup[entity];
        for (int i = 0; i < children.Length; i++)
        {
            if (bufferLookup.HasBuffer(children[i].Value))
            {
                return bufferLookup[children[i].Value];
            }
        }
        return default;
    }

    public static T GetBufferElement<T>(this DynamicBuffer<T> buffer, Func<T, bool> finder) where T : unmanaged, IBufferElementData
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (finder(buffer[i]))
            {
                return buffer[i];
            }
        }
        return default;
    }

    public static bool ContainsEx<T>(this DynamicBuffer<T> buffer, T element) where T : unmanaged, IBufferElementData
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Equals(element)) return true;
        }
        return false;
    }
    public static bool ContainsEx<T>(this DynamicBuffer<T> buffer, T element, out int index) where T : unmanaged, IBufferElementData
    {
        index = -1;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Equals(element))
            {
                index = i;
                return true;
            }
        }
        return false;
    }
}
