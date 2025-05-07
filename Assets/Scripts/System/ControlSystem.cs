using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
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

        if(Input.GetKeyDown(KeyCode.H))
        {
            foreach(var (player, entity) in SystemAPI.Query<Player>().WithEntityAccess())
            {
                var child = SystemAPI.GetBuffer<Child>(entity);
                var renderer = Utils.EntityManager.GetComponentObject<SpriteRenderer>(child[0].Value);
                renderer.sprite = null;
            }
        }
    }
}
