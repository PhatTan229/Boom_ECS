using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
public partial struct ControlSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var kvp in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<Controlable>>()) { }
    }
}
