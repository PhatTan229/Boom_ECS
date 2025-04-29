using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Enemy : IComponentData, IKillable
{
    [field: SerializeField] public float MaxHp { get; set; }
    [field: SerializeField] public float Hp {  set; get; }

    public Enemy(float maxHp)
    {
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public void TakeDamge(float damge)
    {
        Hp -= damge;
        DebugUtils.Log($"Enemy take {damge} damage, {Hp} remain");
        if (Hp <= 0) Die();
    }

    public void Die()
    {
        DebugUtils.Log("ENEMY DIE");
    }

}

public class EnemyAuthoring : MonoBehaviour
{
    public float maxHp;
    class EnemyAthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Enemy(authoring.maxHp));
        }
    }
}
