using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
public struct EntityPrefab : IComponentData
{
    public Entity Value;
}

public class GetPrefabAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public class GetPrefabBaker : Baker<GetPrefabAuthoring>
    {
        public override void Bake(GetPrefabAuthoring authoring)
        {
            // Register the Prefab in the Baker
            var entityPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);
            // Add the Entity reference to a component for instantiation later
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntityPrefab() { Value = entityPrefab });
        }
    }
}
