using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public struct MyPhysicCollider : IComponentData
{
    public BlobAssetReference<Unity.Physics.Collider> collider;
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public class PhysicColliderAuhoring : MonoBehaviour
{
    public Collider2D col;
    class PhysicColliderBaker : Baker<PhysicColliderAuhoring>
    {
        public override void Bake(PhysicColliderAuhoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            if(authoring.col == null) authoring.col = authoring.GetComponent<Collider2D>();
            var newCollider = authoring.col is CircleCollider2D ? authoring.col.ConvertToBlobCollider_Circle() : authoring.col.ConvertToBlobCollider_Box();
            AddBlobAsset(ref newCollider, out var hash);
            AddComponent(entity, new MyPhysicCollider { collider = newCollider });
        }
    }
}
