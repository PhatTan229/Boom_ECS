using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;

public struct PrefabReference : IComponentData
{
    public Entity value;
}

public class PrefabAuthoring : MonoBehaviour
{
    public GameObject prefab;

    class PrefabAuthoringBaker : Baker<PrefabAuthoring>
    {
        public override void Bake(PrefabAuthoring authoring)
        {
            if (authoring.prefab == null) return;
            var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
            var prefabEntityReference = new EntityPrefabReference(authoring.prefab);
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabReference() { value = prefabEntity });
            AddComponent(entity, new RequestEntityPrefabLoaded() { Prefab = prefabEntityReference });
        }
    }
}
