using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerTrigger : IOnTrigger
{
    private ComponentLookup<Enemy> enemyLookup;
    private ComponentLookup<Killable> killableLookup;
    private ComponentLookup<StatData> statLookup;

    public PlayerTrigger(ComponentLookup<Enemy> enemyLookup, ComponentLookup<Killable> killableLookup, ComponentLookup<StatData> statLookup)
    {
        this.enemyLookup = enemyLookup;
        this.killableLookup = killableLookup;
        this.statLookup = statLookup;
    }

    public void OnEnter(ref SystemState state, PhysicEntityPair entityPair)
    {
        enemyLookup.Update(ref state);
        killableLookup.Update(ref state);
        statLookup.Update(ref state);

        if (enemyLookup.HasComponent(entityPair.EntityA) || enemyLookup.HasComponent(entityPair.EntityB))
        {
            var playerEntity = enemyLookup.HasComponent(entityPair.EntityA) ? entityPair.EntityB : entityPair.EntityA;
            if (!killableLookup.HasComponent(playerEntity)) return;
            var kill = killableLookup[playerEntity];
            var currentStat = statLookup.GetRefRW(playerEntity);
            kill.TakeDamge(currentStat, 1f);
        }
    }

    public void OnStay(ref SystemState state, PhysicEntityPair entityPair)
    {
        //Debug.Log("Stay");
    }

    public void OnExit(ref SystemState state, PhysicEntityPair entityPair)
    {
        //Debug.Log("Exit");
    }
}
