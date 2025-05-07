using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
public partial struct ControlSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, control, stat) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<Controlable>, RefRO<StatData>>())
        {
            var input = SystemAPI.GetSingletonRW<InputStorage>();
            control.ValueRW.ControlMovement(velocity, input.ValueRW.direction, stat.ValueRO.currentStat.speed);
        }
    }
}
