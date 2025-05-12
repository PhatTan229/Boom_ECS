using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct PlayerSystem : ISystem, ISystemStartStop
{
    private Entity player;

    public void OnStartRunning(ref SystemState state)
    {
        player = SystemAPI.GetSingletonEntity<Player>();

        var animation = Utils.GetComponentDataInChildren<SpriteAnimation>(player, state.EntityManager, out var child);
        var allState = state.EntityManager.GetBuffer<AnimationStateBuffer>(child);
        var data = allState.GetBufferElement(x => x.state.isDefault);
        var stateName = data.state.name;
        animation.UpdateAnimation(ref data.state, SystemAPI.Time.DeltaTime);
        var newBuffer = data;
        newBuffer.state = data.state;
        data = newBuffer;
        state.EntityManager.SetComponentData(child, animation);
    }

    public void OnUpdate(ref SystemState state)
    {
        var input = SystemAPI.GetSingletonRW<InputStorage>();
        if (math.all(input.ValueRO.direction == float3.zero)) return;

        var animation = Utils.GetComponentDataInChildren<SpriteAnimation>(player, state.EntityManager, out var child);
        var allState = state.EntityManager.GetBuffer<AnimationStateBuffer>(child);

        var stateName = animation.currentSate;
        if (math.all(input.ValueRO.direction == InputStorage.Up)) stateName = Utils.FixString32(nameof(InputStorage.Up));
        else if (math.all(input.ValueRO.direction == InputStorage.Down)) stateName = Utils.FixString32(nameof(InputStorage.Down));
        else if (math.all(input.ValueRO.direction == InputStorage.Left)) stateName = Utils.FixString32(nameof(InputStorage.Left));
        else stateName = Utils.FixString32(nameof(InputStorage.Right));

        var stateBuffer = allState.GetBufferElement(x => x.state.name == stateName);
        var data = stateBuffer.state;
        animation.UpdateAnimation(ref data, SystemAPI.Time.DeltaTime);

        var newBuffer = stateBuffer;
        newBuffer.state = data;
        stateBuffer = newBuffer;

        state.EntityManager.SetComponentData(child, animation);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
