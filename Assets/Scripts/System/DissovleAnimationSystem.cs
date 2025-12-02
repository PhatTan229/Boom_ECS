using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public static class DissolveAnimationHelper
{
    public static List<Entity> dissolveEntity = new List<Entity>();

    public static void RegisterDissolve(Entity entity)
    {
        if (dissolveEntity.Contains(entity)) return;
        dissolveEntity.Add(entity);
    }
}

public partial struct DissovleAnimationSystem : ISystem, ISystemStartStop
{
    partial struct DissovleJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeList<Entity> dissovleEntities;
        [NativeDisableParallelForRestriction] public ComponentLookup<DissovleFade> dissovleLookup;
        [ReadOnly] public ComponentLookup<DissovleModifier> modifierLookup;
        public float detaTime;

        public void Execute(int index)
        {
            var item = dissovleEntities[index];
            ActivateDissolve(item);
        }

        private void ActivateDissolve(Entity entity)
        {
            if (!dissovleLookup.HasComponent(entity) || !modifierLookup.HasComponent(entity)) return;
            var modifier = modifierLookup.GetRefRO(entity);
            var dissovle = dissovleLookup.GetRefRW(entity);
            var value = dissovle.ValueRO.Value + detaTime * modifier.ValueRO.dissovleSpeed;
            dissovle.ValueRW.Value = value;
        }
    }

    private ComponentLookup<DissovleFade> dissovleLookup;
    private ComponentLookup<DissovleModifier> modifierLookup;
    private BufferLookup<Child> childLookup;

    public void OnStartRunning(ref SystemState state)
    {
        dissovleLookup = state.GetComponentLookup<DissovleFade>();
        modifierLookup = state.GetComponentLookup<DissovleModifier>();
        childLookup = state.GetBufferLookup<Child>();
    }

    public void OnUpdate(ref SystemState state)
    {
        dissovleLookup.Update(ref state);
        modifierLookup.Update(ref state);
        childLookup.Update(ref state);

        var dissovleEntities = new NativeList<Entity>(Allocator.TempJob);

        foreach (var item in DissolveAnimationHelper.dissolveEntity)
        {
            dissovleEntities.Add(item);
            if (!childLookup.HasBuffer(item)) continue;
            foreach (var child in childLookup[item])
            {
                dissovleEntities.Add(child.Value);
            }
        }

        foreach (var item in dissovleEntities)
        {
            if (!DissolveAnimationHelper.dissolveEntity.Contains(item)) DissolveAnimationHelper.RegisterDissolve(item);
        }

        var job = new DissovleJob()
        {
            dissovleEntities = dissovleEntities,
            dissovleLookup = dissovleLookup,
            modifierLookup = modifierLookup,
            detaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleByRef(dissovleEntities.Length, 128, state.Dependency);
        state.Dependency.Complete();

        for (int i = DissolveAnimationHelper.dissolveEntity.Count - 1; i >= 0; i--)
        {
            var e = DissolveAnimationHelper.dissolveEntity[i];
            if (!dissovleLookup.HasComponent(e))
            {
                DissolveAnimationHelper.dissolveEntity.Remove(e);
                continue;
            }
            var fade = dissovleLookup.GetRefRO(e);
            if (fade.ValueRO.Value >= 1f) DissolveAnimationHelper.dissolveEntity.Remove(e);
        }

        dissovleEntities.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
       
    }
}
