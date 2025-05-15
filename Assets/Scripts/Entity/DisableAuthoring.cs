using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DisableAuthoring : MonoBehaviour
{
    class DisableBaker : Baker<DisableAuthoring>
    {
        public override void Bake(DisableAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Disabled>(entity);
        }
    }
}
