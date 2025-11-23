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
            var radius = Mathf.RoundToInt(detectablity.radius);
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    var gridPos = new GridPosition(x, y) + origin.gridPosition;
                    CheckGrid(detectablity.targetLayer, ref buffer, gridPos);
                }
            }
        }

        private void CheckGrid(PhysicsCategory targetLayer, ref DynamicBuffer<DetectBuffer> buffer, GridPosition gridPos)
        {
            if (!GridCooridnateCollecttion.coordination.TryGetValue(gridPos, out var list)) return;
            foreach (var item in list)
            {
                if (!colliderLookup.HasComponent(item)) continue;
                var collider = colliderLookup[item];
                var layerMask = collider.Value.Value.GetCollisionFilter().BelongsTo;
                if (!PhysicLayerUtils.HasLayer(layerMask, targetLayer)) continue;
                buffer.Add(new DetectBuffer() { entity = item });
            }
        }
    }

    private ComponentLookup<PhysicsCollider> colliderLookup;
    private ComponentLookup<GridCoordination> gridCoordLookup;
    private ComponentLookup<Grid> gridLookup;
    private BufferLookup<DetectBuffer> detectBufferLookup;

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        colliderLookup = state.GetComponentLookup<PhysicsCollider>();
        gridCoordLookup = state.GetComponentLookup<GridCoordination>();
        gridLookup = state.GetComponentLookup<Grid>();
        detectBufferLookup = state.GetBufferLookup<DetectBuffer>();
    }

    [BurstCompile]
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

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }
}
