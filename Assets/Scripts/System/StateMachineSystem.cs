using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
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

    private void UpdateStateMachine(ref SystemState system)
    {
        foreach (var (animation, statesBuffer, stateMachine, entity) in SystemAPI.Query<RefRW<SpriteAnimation>, DynamicBuffer<AnimationStateBuffer>, StateMachine>().WithEntityAccess())
        {
            if (!currentStatesMap.ContainsKey(entity)) AddState(ref system, entity);
            var state = currentStatesMap[entity];
            if (state != animation.ValueRW.CurrentSate)
            {
                var exitState = stateMachine.stateMachines[state];
                var exitData = statesBuffer.GetBufferElement(x => x.state.name == state);
                exitState.OnStateExit(exitData.state);

                var enterState = stateMachine.stateMachines[animation.ValueRW.CurrentSate];
                var enterData = statesBuffer.GetBufferElement(x => x.state.name == animation.ValueRW.CurrentSate);
                enterState.OnStateEnter(exitData.state);
            }
            else
            {
                var currentState = stateMachine.stateMachines[animation.ValueRW.CurrentSate];
                var updateData = statesBuffer.GetBufferElement(x => x.state.name == animation.ValueRW.CurrentSate);
                currentState.OnStateUpdate(updateData.state);
            }
            currentStatesMap[entity] = animation.ValueRW.CurrentSate;
        }
    }

    private void AddState(ref SystemState system, Entity entity)
    {
        var state = SystemAPI.GetComponentRO<SpriteAnimation>(entity);
        if (currentStatesMap.Count >= currentStatesMap.Capacity) IncreaseSize(ref system);
        currentStatesMap.Add(entity, state.ValueRO.CurrentSate);
    }

    private void IncreaseSize(ref SystemState state)
    {
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

    public void OnStopRunning(ref SystemState state)
    {
        currentStatesMap.Dispose();
    }
}
