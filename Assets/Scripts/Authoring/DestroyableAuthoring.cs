using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DestroyableAuthoring : MonoBehaviour
{
    public StatValue stat;

    class DestroyableAuthoringBaker : Baker<DestroyableAuthoring>
    {
        public override void Bake(DestroyableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new StatData(authoring.stat, ""));
        }
    }
}
