using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct PlayerSystem : IOnTrigger
{
    private void OnTrigger(ref SystemState state)
    {
        var data = state.EntityManager.GetComponentDataRW<OnTriggerData>(state.SystemHandle);

        foreach (var entity in data.ValueRO._enter)
        {
            OnEnter(ref state, entity);
        }

        foreach (var entity in data.ValueRO._current)
        {
            OnStay(ref state, entity);
        }

        foreach (var item in data.ValueRO._previous)
        {
            if (!data.ValueRO._current.Contains(item)) OnExit(ref state, item);
        }
    }

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
