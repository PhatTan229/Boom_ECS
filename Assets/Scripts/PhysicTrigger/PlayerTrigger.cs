using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerTrigger : IOnTrigger
{
    public void OnEnter(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Enter");

        //if (SystemAPI.HasComponent<Enemy>(entityPair.EntityA) || SystemAPI.HasComponent<Enemy>(entityPair.EntityB))
        //{
        //    var playerEntity = SystemAPI.HasComponent<Player>(entityPair.EntityA) ? entityPair.EntityA : entityPair.EntityB;

        //    //var refPlayer = SystemAPI.GetComponentRW<Player>(playerEntity);
        //    //var currentStat = SystemAPI.GetComponentRW<StatData>(playerEntity);
        //    //refPlayer.ValueRW.TakeDamge(currentStat.ValueRW.currentStat, 1f);
        //}
    }

    public void OnStay(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Stay");
    }

    public void OnExit(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log("Exit");
    }
}
