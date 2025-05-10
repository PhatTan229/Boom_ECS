using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public partial struct PlayerSystem : ISystem, ISystemStartStop
{
    private Entity player;

    public void OnStartRunning(ref SystemState state)
    {
        player = SystemAPI.GetSingletonEntity<Player>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var input = SystemAPI.GetSingletonRW<InputStorage>();
        if (math.all(input.ValueRO.direction == float3.zero)) return;

        var animation = Utils.GetComponentDataInChildren<SpriteAnimation>(player, state.EntityManager);
        var allState = Utils.GetBufferInChildren<AnimationStateBuffer>(player, state.EntityManager);

        var stateName = Utils.FixString32_Emty;
        if (math.all(input.ValueRO.direction == InputStorage.Up)) stateName = Utils.FixString32(nameof(InputStorage.Up));
        else if (math.all(input.ValueRO.direction == InputStorage.Down)) stateName = Utils.FixString32(nameof(InputStorage.Down));
        else if (math.all(input.ValueRO.direction == InputStorage.Left)) stateName = Utils.FixString32(nameof(InputStorage.Left));
        else stateName = Utils.FixString32(nameof(InputStorage.Right));

        AnimationData data = allState[0].state;
        for (int i = 0; i < allState.Length; i++)
        {
            if (allState[i].state.name == stateName)
            {
                data = allState[i].state;
                break;
            }
        }
        animation.UpdateAnimation(ref data, SystemAPI.Time.DeltaTime);
        for (int i = 0; i < allState.Length; i++)
        {
            if (allState[i].state.name == stateName)
            {
                var bufferElement = allState[i]; 
                bufferElement.state = data;          
                allState[i] = bufferElement;              
                break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
