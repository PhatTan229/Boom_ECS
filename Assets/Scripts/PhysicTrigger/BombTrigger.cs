using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;
using static UnityEngine.EventSystems.EventTrigger;

public class BombTrigger : IOnTrigger
{
    private ComponentLookup<Player> playerLookup;
    private ComponentLookup<Enemy> enemyLookup;

    public BombTrigger(ComponentLookup<Player> playerLookup, ComponentLookup<Enemy> enemyLookup)
    {
        this.playerLookup = playerLookup;
        this.enemyLookup = enemyLookup;
    }

    public void OnEnter(ref SystemState state, PhysicEntityPair entityPair)
    {
        enemyLookup.Update(ref state);
        playerLookup.Update(ref state);

        if (playerLookup.HasComponent(entityPair.EntityA) || enemyLookup.HasComponent(entityPair.EntityA))
        {
            var buffer = state.EntityManager.GetBuffer<InTrigger>(entityPair.EntityB);
            var otherTrigger = new InTrigger() { value = entityPair.EntityA };
            if (!buffer.ContainsEx(otherTrigger))
            {
                buffer.Add(otherTrigger);
            }
        }
        else if(playerLookup.HasComponent(entityPair.EntityB) || enemyLookup.HasComponent(entityPair.EntityB))
        {
            var buffer = state.EntityManager.GetBuffer<InTrigger>(entityPair.EntityA);
            var otherTrigger = new InTrigger() { value = entityPair.EntityB };
            if (!buffer.ContainsEx(otherTrigger))
            {
                buffer.Add(otherTrigger);
            }
        }
    }

    public void OnStay(ref SystemState state, PhysicEntityPair entityPair)
    {
        Debug.Log(entityPair.EntityA);
        Debug.Log(entityPair.EntityB);
    }

    public void OnExit(ref SystemState state, PhysicEntityPair entityPair)
    {
        enemyLookup.Update(ref state);
        playerLookup.Update(ref state);

        if (playerLookup.HasComponent(entityPair.EntityA) || enemyLookup.HasComponent(entityPair.EntityA))
        {
            var buffer = state.EntityManager.GetBuffer<InTrigger>(entityPair.EntityB);
            var otherTrigger = new InTrigger() { value = entityPair.EntityA };
            if (buffer.ContainsEx(otherTrigger, out var index))
            {
                buffer.RemoveAt(index);
            }
        }
        else if (playerLookup.HasComponent(entityPair.EntityB) || enemyLookup.HasComponent(entityPair.EntityB))
        {
            var buffer = state.EntityManager.GetBuffer<InTrigger>(entityPair.EntityA);
            var otherTrigger = new InTrigger() { value = entityPair.EntityB };
            if (buffer.ContainsEx(otherTrigger, out var index))
            {
                buffer.RemoveAt(index);
            }
        }
    }
}
