using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct ParticleMonitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (ps, data, transform, entity) in SystemAPI.Query<ParticleSystemRef, RefRW<ParticleData>, RefRW<LocalTransform>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            data.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            if (data.ValueRO.currentLifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
            }
            else
            {
                ecb.SetEnabled(entity, true);
                ps.particleSystem.Play(true);
                if(data.ValueRO.position.y != -99f) transform.ValueRW.Position = data.ValueRO.position;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}