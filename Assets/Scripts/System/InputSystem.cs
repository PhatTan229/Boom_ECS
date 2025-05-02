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

    public void UpdateInput()
    {
        if (Input.GetKey(KeyConfig.Move.up)) direction = new float3(0, 0, 1);
        else if (Input.GetKey(KeyConfig.Move.down)) direction = new float3(0, 0, -1);
        else if (Input.GetKey(KeyConfig.Move.left)) direction = new float3(-1, 0, 0);
        else if (Input.GetKey(KeyConfig.Move.right)) direction = new float3(1, 0, 0);
        else direction = float3.zero;
    }
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct InputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log("CreateSingleton");
        Utils.CreateSingleton<InputStorage>("InputStorage");
    }

    public void OnUpdate(ref SystemState state)
    {
        var storage = SystemAPI.GetSingletonRW<InputStorage>();
        storage.ValueRW.UpdateInput();
    }
}
