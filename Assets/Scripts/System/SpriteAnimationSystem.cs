using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct EntityAnimationData
{
    public NativeHashMap<FixedString32Bytes, AnimationState> states;
    public Dictionary<FixedString32Bytes, IStateMachine> stateMachine;

    public EntityAnimationData(IEnumerable<AnimationState> states)
    {
        this.states = new NativeHashMap<FixedString32Bytes, AnimationState>();
        stateMachine = new Dictionary<FixedString32Bytes, IStateMachine>();
        foreach (var state in states) 
        {
            this.states.Add(state.name, state);
         }
    }
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct SpriteAnimationSystem : ISystem, ISystemStartStop
{
    public void OnStartRunning(ref SystemState state)
    {
        Debug.Log("OnStartRunning");
        var player = SystemAPI.GetSingletonEntity<Player>();
        var child = SystemAPI.GetBuffer<Child>(player)[0].Value;
        var co = state.EntityManager.GetComponentTypes(child);
        foreach (var component in co) 
        {
            var type = component.GetManagedType();
            Debug.Log(type.Name);
            if (typeof(IStateMachine).IsAssignableFrom(type))
            {
                Debug.Log($"Component {type.Name} implements IStateMachine");
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
