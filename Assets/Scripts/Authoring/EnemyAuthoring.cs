using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Enemy : IComponentData
{
    public void TakeDamge(RefRW<StatData> stat, float damge)
    {
        stat.ValueRW.currentStat.HP -= damge;
        DebugUtils.Log($"Enemy take {damge} damage, {stat.ValueRW.currentStat.HP} remain");
        if (stat.ValueRW.currentStat.HP <= 0) Die();
    }

    public void Die()
    {
        DebugUtils.Log("ENEMY DIE");
    }

}

public class EnemyAuthoring : MonoBehaviour
{
    public StatValue stat;
    class EnemyAthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Enemy>(entity);
            AddComponent(entity, new StatData(authoring.stat));
        }
    }
}
