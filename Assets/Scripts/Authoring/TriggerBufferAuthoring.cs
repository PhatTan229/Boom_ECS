using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

public struct InTrigger : IBufferElementData
{
    public Entity value;
}

[RequireComponent(typeof(PhysicsShapeAuthoring))]
[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class TriggerBufferAuthoring : MonoBehaviour
{
    class TriggerBufferAuthoringBaker : Baker<TriggerBufferAuthoring>
    {
        public override void Bake(TriggerBufferAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<InTrigger>(entity);
        }
    }
}
