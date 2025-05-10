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
    public static readonly float3 Right = new float3(1, 0, 0);
    public static readonly float3 Left = new float3(-1, 0, 0);
    public static readonly float3 Up = new float3(0, 0, 1);
    public static readonly float3 Down = new float3(0, 0, -1);

    public float3 direction;

    public void UpdateInput()
    {
        if (Input.GetKey(KeyConfig.Move.up)) direction = Up;
        else if (Input.GetKey(KeyConfig.Move.down)) direction = Down;
        else if (Input.GetKey(KeyConfig.Move.left)) direction = Left;
        else if (Input.GetKey(KeyConfig.Move.right)) direction = Right;
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
    }
}
