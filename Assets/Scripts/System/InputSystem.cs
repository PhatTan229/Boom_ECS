using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static MoveKey;

public struct InputStorage : IComponentData
{
    public float3 direction;
    public bool pressBomb;

    public void UpdateInput()
    {
        if (Input.GetKey(KeyConfig.Move.up)) direction = Direction.Up;
        else if (Input.GetKey(KeyConfig.Move.down)) direction = Direction.Down;
        else if (Input.GetKey(KeyConfig.Move.left)) direction = Direction.Left;
        else if (Input.GetKey(KeyConfig.Move.right)) direction = Direction.Right;
        else direction = float3.zero;
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Utils.CreateSingleton<InputStorage>(state.EntityManager, "InputStorage");
    }

    public void OnUpdate(ref SystemState state)
    {
        var storage = SystemAPI.GetSingletonRW<InputStorage>();
        storage.ValueRW.UpdateInput();
        storage.ValueRW.pressBomb = Input.GetKeyDown(KeyConfig.BomButton);
    }
}
