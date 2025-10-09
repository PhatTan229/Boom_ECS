using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct EntityInfo : IComponentData
{
    public FixedString64Bytes ID;
    public FixedString64Bytes Name;
    public int layer;
    public Entity entity;
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public class EntityInfoAuthoring : MonoBehaviour
{
    class EntityInfoAuthoringBaker : Baker<EntityInfoAuthoring>
    {
        public override void Bake(EntityInfoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntityInfo()
            {
                ID = new FixedString64Bytes(authoring.gameObject.GetInstanceID().ToString()),
                Name = new FixedString64Bytes(authoring.gameObject.name),
                entity = entity
            });
        }
    }
}
