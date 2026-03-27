using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct StatData : IComponentData
{
    public readonly StatValue baseStat;
    public StatValue currentStat;

    public StatData(StatValue value, string name)
    {
        value.name = Utils.FixString64(name);
        baseStat = value;
        currentStat = value;
    }
}

public class StatAuthoring : MonoBehaviour
{
    public StatValue stat;
    class StatAuthoringBaker : Baker<StatAuthoring>
    {
        public override void Bake(StatAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new StatData(authoring.stat, authoring.name));
        }
    }
}
