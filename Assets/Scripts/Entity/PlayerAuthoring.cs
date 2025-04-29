using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData, IKillable
{
    [field: SerializeField] public float MaxHp { get; set; }
    [field: SerializeField] public float Hp { get; set; }

    public Player(float maxHp)
    {
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public void TakeDamge(float damge)
    {
        Hp -= damge;
        Debug.Log($"Player take {damge} damage, {Hp} remain");
        if (Hp <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("PLAYER DIE");
    }
}

public class PlayerAuthoring : MonoBehaviour
{
    public float maxHp;
    class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Player(authoring.maxHp));
        }
    }
}
