using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System.Diagnostics;
using UnityEngine;

[BurstCompile]
public partial struct EnityMove : ISystem
{
    [BurstCompile]
    public partial struct DragJob : IJobEntity
    {
        public float3 newPos;

        void Execute([ChunkIndexInQuery] int index, RefRO<Dragable> drag, ref LocalTransform transform)
        {
            drag.ValueRO.Drag(ref transform, newPos);
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var mousePos = MousePosition(ref state);
        MoveEntities(ref state, mousePos);
    }

    private float3 MousePosition(ref SystemState state)
    {
        var mousePos = SystemAPI.GetSingleton<MousePosition>();
        return mousePos.value;
    }

    private void MoveEntities(ref SystemState state, float3 newPos)
    {
        foreach (var drag in SystemAPI.Query<RefRO<Dragable>>())
        {
            var job = new DragJob();
            job.newPos = newPos;
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
