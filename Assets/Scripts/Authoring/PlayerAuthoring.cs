using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct Player : IComponentData
{
}

[RequireComponent(typeof(StatAuthoring))]
public class PlayerAuthoring : MonoBehaviour
{
    public string name = "";
    class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Player>(entity);
            var statAuth = GetComponent<StatAuthoring>();
            var stat = statAuth.stat;
            stat.name = Utils.FixString64(authoring.name);
            statAuth.stat = stat;
        }
    }
}
