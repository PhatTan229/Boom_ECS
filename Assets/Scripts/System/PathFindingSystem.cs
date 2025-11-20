using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public static class PathFindingHelper
{
    public static Queue<(Entity, Entity)> pathFindingQueue = new Queue<(Entity, Entity)>();
    public static Queue<Entity> clearPathQueue = new Queue<Entity>();
    
    public static void RegisterPathFinding(Entity start, Entity end)
    {
        if (pathFindingQueue.Contains((start, end))) return;
        pathFindingQueue.Enqueue((start, end));
    }

    public static void RegisterClearPath(Entity entity)
    {
        if (clearPathQueue.Contains(entity)) return;
        clearPathQueue.Enqueue(entity);
    }
}

public partial struct PathFindingSystem : ISystem
{
    [BurstCompile]
    public partial struct PathFindingJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public BufferLookup<Path> PathBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [ReadOnly] public BufferLookup<GridNeighbour> neibourLookup;
        [ReadOnly] public ComponentLookup<GridCoordination> gridCoordination;
        [NativeDisableParallelForRestriction] public ComponentLookup<Grid> gridLookup;
        public int mapSize;
        public Entity End;

        void Execute([ChunkIndexInQuery] int index, Entity SelfEntity, ref PathFinding pathFinding)
        {
            var position = transformLookup[SelfEntity].Position;
            var start = gridCoordination[SelfEntity].CurrentGrid;
            var path = new NativeList<Entity>(Allocator.Temp);
            var connections = new NativeHashMap<Entity, Entity>(mapSize, Allocator.Temp);
            AStar.FindPath(start, End, path, ref neibourLookup, ref gridLookup, ref connections);
            if (PathBufferLookup.HasBuffer(SelfEntity))
            {
                var buffer = PathBufferLookup[SelfEntity];
                buffer.Clear();

                foreach (var c in path)
                {
                    buffer.Add(new Path { value = c });
                }
            }
            path.Dispose();
            connections.Dispose();
        }
    }

    private BufferLookup<Path> pathBufferLookup;
    private ComponentLookup<LocalTransform> transformLookup;
    private BufferLookup<GridNeighbour> neighbourLookup;
    private ComponentLookup<Grid> gridLookup;
    private ComponentLookup<GridCoordination> gridCoordination;
    public void OnCreate(ref SystemState state)
    {
        pathBufferLookup = state.GetBufferLookup<Path>();
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
        neighbourLookup = state.GetBufferLookup<GridNeighbour>(true);
        gridCoordination = state.GetComponentLookup<GridCoordination>();
        gridLookup = state.GetComponentLookup<Grid>();
    }

    public void OnUpdate(ref SystemState state)
    {
        pathBufferLookup.Update(ref state);
        transformLookup.Update(ref state);
        neighbourLookup.Update(ref state);
        gridCoordination.Update(ref state);
        gridLookup.Update(ref state);

        ExecuteClearPath(ref state);
        ExecutePathFinding(ref state);
    }

    private void ExecuteClearPath(ref SystemState state)
    {
        if (PathFindingHelper.clearPathQueue.Count <= 0) return;
        var entity = PathFindingHelper.clearPathQueue.Peek();
        var coord = gridCoordination[entity];
        var path = pathBufferLookup[entity];

        for (int i = path.Length - 1; i >= 0; i--)
        {
            if (coord.CurrentGrid == path[i].value) break;
            path.RemoveAt(i);
        }
        PathFindingHelper.clearPathQueue.Dequeue();
    }

    private void ExecutePathFinding(ref SystemState state)
    {
        if (PathFindingHelper.pathFindingQueue.Count <= 0) return;

        var (start, end) = PathFindingHelper.pathFindingQueue.Peek();

        var endTransform = SystemAPI.GetComponentRO<LocalTransform>(end);
        var endGGrid = GridData.Instance.GetGridCoordination_Entity(endTransform.ValueRO.Position);
        var mapSize = state.EntityManager.GetSharedComponentManaged<MapSizeData>(end);
        //DebugUtils.Log($"Player Grid {Utils.EntityManager.GetComponentData<Grid>(playerGrid).gridPosition}");

        var job = new PathFindingJob()
        {
            End = endGGrid,
            PathBufferLookup = pathBufferLookup,
            transformLookup = transformLookup,
            neibourLookup = neighbourLookup,
            gridCoordination = gridCoordination,
            gridLookup = gridLookup,
            mapSize = mapSize.value
        };

        PathFindingHelper.pathFindingQueue.Dequeue();
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}
