using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;


public partial struct BombSystem : ISystem, ISystemStartStop
{
    partial struct SetStaticJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsCollider> colliderLookup;
        [ReadOnly] public BufferLookup<InTrigger> inTriggerLookup;
        public EntityCommandBuffer.ParallelWriter ecb;

        void Execute([ChunkIndexInQuery] int index, Entity entity, Bomb bomb)
        {
            var refCollider = colliderLookup.GetRefRW(entity);
            var bodyType = refCollider.ValueRO.Value.Value.GetCollisionResponse();
            if (bodyType == CollisionResponsePolicy.Collide) return;
            if (inTriggerLookup[entity].Length > 0) return;
            refCollider.ValueRW.Value.Value.SetCollisionResponse(CollisionResponsePolicy.Collide);
        }
    }

    private ComponentLookup<PhysicsMass> massLookup;
    private ComponentLookup<PhysicsCollider> colliderLookup;
    private BufferLookup<InTrigger> inTriggerLookup;

    public void OnStartRunning(ref SystemState state)
    {
        massLookup = SystemAPI.GetComponentLookup<PhysicsMass>();
        colliderLookup = SystemAPI.GetComponentLookup<PhysicsCollider>();
        inTriggerLookup = SystemAPI.GetBufferLookup<InTrigger>();

        var playerLookup = SystemAPI.GetComponentLookup<Player>();
        var enemyLookup = SystemAPI.GetComponentLookup<Enemy>();
        OnTriggerContainer.Subscribe(new BombTrigger(playerLookup, enemyLookup));
    }

    public void OnUpdate(ref SystemState state) 
    {
        massLookup.Update(ref state);
        colliderLookup.Update(ref state);
        inTriggerLookup.Update(ref state);

        var job = new SetStaticJob()
        {
            ecb = GameSystem.ecbSystem.CreateCommandBuffer().AsParallelWriter(),
            inTriggerLookup = inTriggerLookup,
            colliderLookup = colliderLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        GameSystem.ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
