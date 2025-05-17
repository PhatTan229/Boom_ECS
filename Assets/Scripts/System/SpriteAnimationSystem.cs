using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[BurstCompile]
public partial struct SpriteAnimationSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    partial struct AnimaitonUpdateJob : IJobChunk
    {
        [NativeDisableParallelForRestriction] public ComponentTypeHandle<SpriteAnimationUpdate> updateHandler;
        [ReadOnly] public ComponentTypeHandle<SpriteAnimation> animtionHandler;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var update = chunk.GetNativeArray(ref updateHandler);
            var animation = chunk.GetNativeArray(ref animtionHandler);

            for (var i = 0; i < chunk.Count; i++) 
            {
                var element = update[i];
                element.Value = (float)animation[i].index;
                update[i] = element;
            }
        }
    }

    private EntityQuery query;
    private ComponentTypeHandle<SpriteAnimationUpdate> updateHandler;
    private ComponentTypeHandle<SpriteAnimation> animationHandler;


    public void OnStartRunning(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadWrite<SpriteAnimationUpdate>(), ComponentType.ReadOnly<SpriteAnimation>());
        updateHandler = state.GetComponentTypeHandle<SpriteAnimationUpdate>();
        animationHandler = state.GetComponentTypeHandle<SpriteAnimation>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        updateHandler.Update(ref state);
        animationHandler.Update(ref state);

        var job = new AnimaitonUpdateJob()
        {
            updateHandler = updateHandler,
            animtionHandler = animationHandler,
        };
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
