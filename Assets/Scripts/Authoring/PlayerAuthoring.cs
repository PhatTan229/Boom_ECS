using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
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


public struct Player : IComponentData
{
}

public class PlayerAuthoring : MonoBehaviour
{
    public string name = "";
    public StatValue stat;
    class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Player>(entity);
            AddComponent(entity, new StatData(authoring.stat, authoring.name));
        }
    }
}
