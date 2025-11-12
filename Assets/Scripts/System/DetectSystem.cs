using Mono.Cecil;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct DetectSystem : ISystem, ISystemStartStop
{
    partial struct DetectJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<PhysicsCollider> colliderLookup;
        [ReadOnly] public ComponentLookup<GridCoordination> gridCoordLookup;
        [ReadOnly] public ComponentLookup<Grid> gridLookup;
        [NativeDisableParallelForRestriction] public BufferLookup<DetectBuffer> bufferLookup;

        public void Execute([EntityIndexInQuery] int index, Entity entity, Detectablity detectablity)
        {
            var buffer = bufferLookup[entity];
            var origin = gridLookup[gridCoordLookup[entity].CurrentGrid];
            buffer.Clear();
            for (int i = 1; i <= Mathf.RoundToInt(detectablity.radius); i++)
            {
                foreach (var neighbour in GridData.neighbourGridPosition)
                {
                    var offset = neighbour * i;
                    var detectGrid = origin.gridPosition + offset;
                    if (!GridCooridnateCollecttion.coordination.TryGetValue(detectGrid, out var list)) continue;
                    foreach (var item in list)
                    {
                        if (!colliderLookup.HasComponent(item)) continue;
                        var collider = colliderLookup[item];
                        var layerMask = collider.Value.Value.GetCollisionFilter().BelongsTo;
                        if (!PhysicLayerUtils.HasLayer(layerMask, detectablity.targetLayer)) continue;
                        buffer.Add(new DetectBuffer() { entity = item });
                    }
                }
            }
        }
    }

    private ComponentLookup<PhysicsCollider> colliderLookup;
    private ComponentLookup<GridCoordination> gridCoordLookup;
    private ComponentLookup<Grid> gridLookup;
    private BufferLookup<DetectBuffer> detectBufferLookup;

    public void OnStartRunning(ref SystemState state)
    {
        colliderLookup = state.GetComponentLookup<PhysicsCollider>();
        gridCoordLookup = state.GetComponentLookup<GridCoordination>();
        gridLookup = state.GetComponentLookup<Grid>();
        detectBufferLookup = state.GetBufferLookup<DetectBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
        colliderLookup.Update(ref state);
        gridCoordLookup.Update(ref state);
        gridLookup.Update(ref state);
        detectBufferLookup.Update(ref state);

        var job = new DetectJob()
        {
            colliderLookup = colliderLookup,
            gridCoordLookup = gridCoordLookup,
            gridLookup = gridLookup,
            bufferLookup = detectBufferLookup
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
}
