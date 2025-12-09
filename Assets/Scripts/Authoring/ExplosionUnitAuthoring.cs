using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ExplosionUnit : IComponentData
{
    public readonly float lifeTime;
    public float currentLifeTime;
    public PhysicsCategory targetLayer;

    public ExplosionUnit(float lifeTime, PhysicsCategory category)
    {
        this.lifeTime = lifeTime;
        currentLifeTime = lifeTime;
        targetLayer = category;
    }

    public void ResetLifeTime()
    {
        currentLifeTime = lifeTime;
    }
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
            AddComponent(entity, new ExplosionUnit(authoring.lifeTime, authoring.targetLayer));
        }
    }
}
