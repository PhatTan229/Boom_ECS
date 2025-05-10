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

    public StatData(StatValue value)
    {
        baseStat = value;
        currentStat = value;
    }
}


public struct Player : IComponentData, IKillable
{
    public void TakeDamge(RefRW<StatData> stat, float damge)
    {
        stat.ValueRW.currentStat.HP -= damge;
        Debug.Log($"Player take {damge} damage, {stat.ValueRW.currentStat.HP} remain");
        if (stat.ValueRW.currentStat.HP <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("PLAYER DIE");
    }
}

public class PlayerAuthoring : MonoBehaviour
{
    public StatValue stat;
    class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Player>(entity);
            AddComponent(entity, new StatData(authoring.stat));
        }
    }
}
