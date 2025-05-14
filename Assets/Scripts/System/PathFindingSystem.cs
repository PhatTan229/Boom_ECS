using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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

        if (Input.GetKeyDown(KeyCode.H))
        {
            var player = SystemAPI.GetSingletonEntity<Player>();
            var playerTransform = SystemAPI.GetComponentRO<LocalTransform>(player);
            var playerGrid = GridData.Instance.GetGridCoordination(playerTransform.ValueRO.Position);
            var mapSize = state.EntityManager.GetSharedComponentManaged<MapSizeData>(player);
            //DebugUtils.Log($"Player Grid {Utils.EntityManager.GetComponentData<Grid>(playerGrid).gridPosition}");

            var job = new PathFindingJob()
            {
                End = playerGrid,
                PathBufferLookup = pathBufferLookup,
                transformLookup = transformLookup,
                neibourLookup = neighbourLookup,
                gridCoordination = gridCoordination,
                gridLookup = gridLookup,
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
