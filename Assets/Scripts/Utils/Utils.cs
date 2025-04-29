using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Utils
{
    public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

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
            CollidesWith = UintLayer.GetCollisonMask(collider2D.gameObject.layer)
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
            CollidesWith = UintLayer.GetCollisonMask(collider2D.gameObject.layer)
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

}
