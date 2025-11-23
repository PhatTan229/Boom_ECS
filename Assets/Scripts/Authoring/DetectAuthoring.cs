using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct DetectBuffer : IBufferElementData
{
    public Entity entity;
}

public struct DetectGridBuffer : IBufferElementData
{
    public GridPosition gridPosition;
}

public struct Detectablity : IComponentData
{
    public float radius;
    public PhysicsCategory targetLayer;
}

public class DetectAuthoring : MonoBehaviour
{
    public float radius;
    public GridPosition[] additions;
    public PhysicsCategory targetLayer;

    class DetectAuthoringBaker : Baker<DetectAuthoring>
    {
        public override void Bake(DetectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Detectablity()
            {
                radius = authoring.radius,
                targetLayer = authoring.targetLayer
            });
            AddBuffer<DetectBuffer>(entity);
            var buffer = AddBuffer<DetectGridBuffer>(entity);
            foreach (var item in authoring.additions)
            {
                buffer.Add(new DetectGridBuffer() { gridPosition = item });
            }
        }
    }
}
