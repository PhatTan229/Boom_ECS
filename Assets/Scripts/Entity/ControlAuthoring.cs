using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct Controlable : IComponentData
{
    public void ControlMovement(RefRW<PhysicsVelocity> velocity, float3 direction, float speed)
    {
        velocity.ValueRW.Linear = math.normalize(direction) * speed;
    }
}

public class ControlAuthoring : MonoBehaviour
{
    class ControlAuthoringBaker : Baker<ControlAuthoring>
    {
        public override void Bake(ControlAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Controlable>(entity);
        }
    }
}
