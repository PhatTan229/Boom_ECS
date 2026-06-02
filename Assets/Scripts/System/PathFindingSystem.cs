using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public struct PathRegisterInfo
{
    public Entity registerEntity;
    public Entity start;
    public Entity end;

    public PathRegisterInfo(Entity registerEntity, Entity start, Entity end)
    {
        this.registerEntity = registerEntity;
        this.start = start;
        this.end = end;
    }
}

public static class PathFindingHelper
{
    public static Queue<PathRegisterInfo> pathFindingQueue = new Queue<PathRegisterInfo>();
    public static Queue<Entity> clearPathQueue = new Queue<Entity>();
    
    public static void RegisterPathFinding(Entity registerEntity, Entity start, Entity end)
    {
        var info = new PathRegisterInfo(registerEntity, start, end);
        if (pathFindingQueue.Contains(info)) return;
        pathFindingQueue.Enqueue(info);
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
        [NativeDisableParallelForRestriction] public NativeParallelHashMap<Entity, PathNodeInfo> nodeInfo;
        public int mapSize;
        public Entity End;

        void Execute([ChunkIndexInQuery] int index, Entity SelfEntity, ref PathFinding pathFinding)
        {
            var position = transformLookup[SelfEntity].Position;
            var start = gridCoordination[SelfEntity].CurrentGrid;
            var path = new NativeList<Entity>(Allocator.Temp);
            var connections = new NativeHashMap<Entity, Entity>(mapSize, Allocator.Temp);
            AStar.FindPath(start, End, path, ref neibourLookup, ref gridLookup, ref connections, ref nodeInfo);
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
    private NativeParallelHashMap<Entity, NativeParallelHashMap<Entity, PathNodeInfo>> entityPathNodes;

    public void OnCreate(ref SystemState state)
    {
        pathBufferLookup = state.GetBufferLookup<Path>();
        transformLookup = state.GetComponentLookup<LocalTransform>(true);
        neighbourLookup = state.GetBufferLookup<GridNeighbour>(true);
        gridCoordination = state.GetComponentLookup<GridCoordination>();
        gridLookup = state.GetComponentLookup<Grid>();

        entityPathNodes = new NativeParallelHashMap<Entity, NativeParallelHashMap<Entity, PathNodeInfo>>(50, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        pathBufferLookup.Update(ref state);
        transformLookup.Update(ref state);
        neighbourLookup.Update(ref state);
        gridCoordination.Update(ref state);
        gridLookup.Update(ref state);

        CreatePathNodeForEntities(ref state);
        ExecuteClearPath(ref state);
        ExecutePathFinding(ref state);
    }

    private void CreatePathNodeForEntities(ref SystemState state)
    {
        foreach (var (pathFinding, entity) in SystemAPI.Query<RefRO<PathFinding>>().WithEntityAccess())
        {
            if (entityPathNodes.ContainsKey(entity)) continue;
            if (entityPathNodes.Count() == entityPathNodes.Capacity) entityPathNodes.Capacity *= 2;
            var cells = GridData.Instance.allCells;
            var arr = new NativeParallelHashMap<Entity, PathNodeInfo>(cells.Length, Allocator.Persistent);
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                var gridEntity = GridData.Instance.GetCellEntityAt(cell.gridPosition);
                arr.Add(gridEntity, new PathNodeInfo(cell.gridPosition));
            }
            entityPathNodes.Add(entity, arr);
        }
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

        var info = PathFindingHelper.pathFindingQueue.Peek();

        var endTransform = SystemAPI.GetComponentRO<LocalTransform>(info.end);
        var endGGrid = GridData.Instance.GetGridCoordination_Entity(endTransform.ValueRO.Position);
        var mapSize = state.EntityManager.GetSharedComponentManaged<MapSizeData>(info.end);
        //DebugUtils.Log($"Player Grid {Utils.EntityManager.GetComponentData<Grid>(playerGrid).gridPosition}");
        var nodeInfo = entityPathNodes[info.registerEntity];
        var job = new PathFindingJob()
        {
            End = endGGrid,
            PathBufferLookup = pathBufferLookup,
            transformLookup = transformLookup,
            neibourLookup = neighbourLookup,
            gridCoordination = gridCoordination,
            gridLookup = gridLookup,
            nodeInfo = nodeInfo,
            mapSize = mapSize.value
        };

        PathFindingHelper.pathFindingQueue.Dequeue();
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    public void OnDestroy(ref SystemState state)
    {
        foreach (var item in entityPathNodes)
        {
            item.Value.Dispose();
        }
        entityPathNodes.Dispose();
    }
}
