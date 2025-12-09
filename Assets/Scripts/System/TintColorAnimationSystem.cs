using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public static class TintColorHelper
{
    public static List<Entity> tintColorQueue = new List<Entity>();

    public static void RegisterTint(Entity entity)
    {
        if (tintColorQueue.Contains(entity)) return;
        tintColorQueue.Add(entity);
    }
}

public partial struct TintColorAnimationSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct TintColorJob : IJobChunk
    {
        [NativeDisableParallelForRestriction] public ComponentTypeHandle<TintColor> tintColorHandler;
        [ReadOnly] public ComponentTypeHandle<Tintable> tintableHandler;
        public float deltaTime;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var animation = chunk.GetNativeArray(ref tintColorHandler);
            var tint = chunk.GetNativeArray(ref tintableHandler);

            for (var i = 0; i < chunk.Count; i++)
            {
                var _animation = animation[i];
                if (_animation.Value <= 0f) continue;
                _animation.Value -= deltaTime * tint[i].fadeOutSpeed;
                _animation.Value = Mathf.Max(0f, _animation.Value);
                animation[i] = _animation;
            }
        }
    }

    private EntityQuery query;
    private ComponentTypeHandle<TintColor> tintColorHandler;
    private ComponentTypeHandle<Tintable> tintableHandler;

    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadWrite<TintColor>(), ComponentType.ReadOnly<Tintable>());
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        tintColorHandler = state.GetComponentTypeHandle<TintColor>();
        tintableHandler = state.GetComponentTypeHandle<Tintable>();
    }

    public void OnUpdate(ref SystemState state)
    {
        tintColorHandler.Update(ref state);
        tintableHandler.Update(ref state);

        for (int i = TintColorHelper.tintColorQueue.Count - 1; i >= 0; i--)
        {
            var item = TintColorHelper.tintColorQueue[i];
            ActivateTint(ref state, item);
            if (SystemAPI.HasBuffer<Child>(item))
            {
                var childs = SystemAPI.GetBuffer<Child>(item);
                foreach (var child in childs)
                {
                    ActivateTint(ref state, child.Value);
                }
            } 
            TintColorHelper.tintColorQueue.Remove(item);
        }

        var job = new TintColorJob()
        {
            tintColorHandler = tintColorHandler,
            tintableHandler = tintableHandler,
            deltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }

    private void ActivateTint(ref SystemState state, Entity item)
    {
        if (SystemAPI.HasComponent<TintColor>(item))
        {
            var tint = SystemAPI.GetComponentRW<TintColor>(item);
            tint.ValueRW.Value = 1f;
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
