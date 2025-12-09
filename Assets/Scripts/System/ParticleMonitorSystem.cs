using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
public partial struct ParticleMonitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (ps, data, transform, entity) in SystemAPI.Query<ParticleSystemRef, RefRW<ParticleData>, RefRW<LocalTransform>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            if(state.EntityManager.IsEnabled(entity)) data.ValueRW.currentLifeTime -= SystemAPI.Time.DeltaTime;
            if (data.ValueRO.currentLifeTime <= 0)
            {
                ecb.SetEnabled(entity, false);
                data.ValueRW.shouldActive = false;
                data.ValueRW.currentLifeTime = data.ValueRO.lifeTime;
            }
            else if(data.ValueRO.currentLifeTime > 0 && data.ValueRO.shouldActive)
            {
                ecb.SetEnabled(entity, true);
                ps.particleSystem.Play(true);
                transform.ValueRW.Position = data.ValueRO.position;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}