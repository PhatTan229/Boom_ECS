using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ExplosionUnit : IComponentData
{
    public float lifeTime;
    public PhysicsCategory targetLayer;
}

[RequireComponent(typeof(GridCoordiantionAuthoring))]
public class ExplosionUnitAuthoring : MonoBehaviour
{
    public float lifeTime;
    public PhysicsCategory targetLayer;

    class ExplosionAuthoringBaker : Baker<ExplosionUnitAuthoring>
    {
        public override void Bake(ExplosionUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ExplosionUnit()
            {
                lifeTime = authoring.lifeTime,
                targetLayer = authoring.targetLayer
            });
        }
    }
}
