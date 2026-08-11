using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Serialization;
using UnityEngine;

public class StateMachineCollidable
{
    public int entityId;
    public Dictionary<int, IStateMachine> stateMachines;
}

public class StateMachineCollector
{
    public static StateMachineCollector Instance;

    public StateMachineCollector() 
    {
        stateMachineCollidable = new Dictionary<int, StateMachineCollidable> ();
    }

    public Dictionary<int, StateMachineCollidable> stateMachineCollidable;
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
public partial struct StateMachineSystem : ISystem, ISystemStartStop
{
    private const int DEFAULT_CAPACITY = 256;

    public void OnStartRunning(ref SystemState state)
    {
        var collector = new StateMachineCollector();

        foreach (var (stateMachineBuffer, info, entity) in SystemAPI.Query<DynamicBuffer<StateMachineBuffer>, RefRO<EntityInfo>>().WithEntityAccess())
        {
            var entityId = info.ValueRO.ID;
            var collectalbe = new StateMachineCollidable();
            foreach (var item in stateMachineBuffer)
            {
                var data = item.StateMachineData;
                collectalbe.stateMachines.Add(data.id, item.StateMachineData.stateMachineScript.Value.StateMachine);
            }
            collector.stateMachineCollidable.Add(entityId, collectalbe);
        }

        StateMachineCollector.Instance = collector;
    }

    public void OnUpdate(ref SystemState state)
    {
        UpdateStateMachine(ref state);
    }

    private void UpdateStateMachine(ref SystemState system)
    {
        var currentStatesMap = new NativeHashMap<Entity, int>(DEFAULT_CAPACITY, Allocator.Temp);
        foreach (var (animation, animationStatesBuffer, stateMachineBuffer, info, entity) in SystemAPI.Query<RefRW<SpriteAnimation>, DynamicBuffer<AnimationStateBuffer>, DynamicBuffer<StateMachineBuffer>, RefRO<EntityInfo>>().WithEntityAccess())
        {
            if (!currentStatesMap.ContainsKey(entity)) AddState(ref system, entity, ref currentStatesMap);
            var state = currentStatesMap[entity];
            var stateHash = Utils.FNV1aHash(animation.ValueRW.CurrentSate.Value);
            var states = StateMachineCollector.Instance.stateMachineCollidable[info.ValueRO.ID];
            if (state != stateHash)
            {
                var exitState = states.stateMachines[state];
                var exitData = animationStatesBuffer.GetBufferElement(x => Utils.FNV1aHash(x.state.name.Value) == state);
                exitState.OnStateExit(exitData.state);

                var enterState = states.stateMachines[stateHash];
                var enterData = animationStatesBuffer.GetBufferElement(x => x.state.name == animation.ValueRW.CurrentSate);
                enterState.OnStateEnter(exitData.state);
            }
            else
            {
                var currentState = states.stateMachines[stateHash];
                var updateData = animationStatesBuffer.GetBufferElement(x => x.state.name == animation.ValueRW.CurrentSate);
                currentState.OnStateUpdate(updateData.state);
            }
            //currentStatesMap[entity] = animation.ValueRW.CurrentSate;
            currentStatesMap[entity] = stateHash;
            currentStatesMap.Dispose();
        }
    }

    private void AddState(ref SystemState system, Entity entity, ref NativeHashMap<Entity, int> currentStatesMap)
    {
        var state = SystemAPI.GetComponentRO<SpriteAnimation>(entity);
        if (currentStatesMap.Count >= currentStatesMap.Capacity) IncreaseSize(ref system, ref currentStatesMap);
        currentStatesMap.Add(entity, Utils.FNV1aHash(state.ValueRO.CurrentSate.Value));
    }

    private void IncreaseSize(ref SystemState state, ref NativeHashMap<Entity, int> currentStatesMap)
    {
        var tmp = new NativeHashMap<Entity, int>(currentStatesMap.Count, Allocator.Temp);
        foreach (var item in currentStatesMap)
        {
            tmp.Add(item.Key, item.Value);
        }
        currentStatesMap.Dispose();

        currentStatesMap = new NativeHashMap<Entity, int>(tmp.Count + DEFAULT_CAPACITY, Allocator.Temp);
        foreach (var item in tmp)
        {
            currentStatesMap.Add(item.Key, item.Value);
        }
        tmp.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
        //currentStatesMap.Dispose();
        StateMachineCollector.Instance = null;
    }
}
