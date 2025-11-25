using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[Serializable]
public struct PathFindingStat
{
    public double interval;
    public double lastFindTime;
}

public struct Enemy : IComponentData
{
    public PathFindingStat pathFindingStat;
}

public class EnemyAuthoring : MonoBehaviour
{
    public string name = "";
    public StatValue stat;
    public PathFindingStat pathFindingStat;

    class EnemyAthoringBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Enemy() { pathFindingStat = authoring.pathFindingStat});
            AddComponent(entity, new StatData(authoring.stat, authoring.name));
        }
    }
}
