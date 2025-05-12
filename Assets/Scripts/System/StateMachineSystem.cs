using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct StateMachineSystem : ISystem, ISystemStartStop
{
    private const int DEFAULT_CAPACITY = 256;
    private NativeHashMap<Entity, FixedString32Bytes> currentStatesMap;

    public void OnStartRunning(ref SystemState state)
    {
        currentStatesMap = new NativeHashMap<Entity, FixedString32Bytes>(DEFAULT_CAPACITY, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state) 
    {
        UpdateStateMachine(ref state);
    }

    public void OnStopRunning(ref SystemState state)
    {
        currentStatesMap.Dispose();
    }

    private void UpdateStateMachine(ref SystemState system)
    {
        foreach (var (animation, statesBuffer, entity) in SystemAPI.Query<RefRW<SpriteAnimation>, DynamicBuffer<AnimationStateBuffer>>().WithEntityAccess())
        {
            if (!system.EntityManager.HasComponent<StateMachine>(entity)) continue;
            var stateMachine = system.EntityManager.GetComponentObject<StateMachine>(entity);
            if(!currentStatesMap.ContainsKey(entity)) AddState(ref system, entity);
            var state = currentStatesMap[entity];
            if (state != animation.ValueRW.currentSate)
            {
                var exitState = stateMachine.stateMachines[state];
                var exitData = Utils.GetBufferElement(statesBuffer, x => x.state.name == state);
                exitState.OnStateExit(exitData.state);

                var enterState = stateMachine.stateMachines[animation.ValueRW.currentSate];
                var enterData = Utils.GetBufferElement(statesBuffer, x => x.state.name == animation.ValueRW.currentSate);
                enterState.OnStateEnter(exitData.state);
            }
            else
            {
                var currentState = stateMachine.stateMachines[animation.ValueRW.currentSate];
                var updateData = Utils.GetBufferElement(statesBuffer, x => x.state.name == animation.ValueRW.currentSate);
                currentState.OnStateUpdate(updateData.state);
            }
            currentStatesMap[entity] = animation.ValueRW.currentSate;
        }
    }

    private void AddState(ref SystemState system, Entity entity)
    {
        var state = SystemAPI.GetComponentRO<SpriteAnimation>(entity);
        IncreaseSize(ref system);
        currentStatesMap.Add(entity, state.ValueRO.currentSate);
    }

    private void IncreaseSize(ref SystemState state)
    {
        if (currentStatesMap.Count < currentStatesMap.Capacity) return;
        var tmp = new NativeHashMap<Entity, FixedString32Bytes>(currentStatesMap.Count, Allocator.Temp);
        foreach (var item in currentStatesMap)
        {
            tmp.Add(item.Key, item.Value);
        }
        currentStatesMap.Dispose();

        currentStatesMap = new NativeHashMap<Entity, FixedString32Bytes>(tmp.Count + DEFAULT_CAPACITY, Allocator.Persistent);
        foreach (var item in tmp)
        {
            currentStatesMap.Add(item.Key, item.Value);
        }
        tmp.Dispose();
    }
}
