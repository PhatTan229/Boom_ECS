using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct PathFindingSystem : ISystem
{
    public partial struct PathFindingJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public BufferLookup<Path> PathBufferLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [ReadOnly] public BufferLookup<GridNeighbour> neibourLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<Grid> gridLookup;
        public Entity End;

        void Execute([ChunkIndexInQuery] int index, Entity SelfEntity, ref PathFinding pathFinding)
        {
            var position = transformLookup[SelfEntity].Position;
            var start = GridUtils.Instance.GetGridCoordination(position);
            //Debug.Log($"Position {Position} Start At : {GridUtils.Instance.WorldToGrid(Position)}");
            var path = new NativeList<Entity>(Allocator.Temp);
            var connections = new NativeHashMap<Entity, Entity>(GridUtils.Instance.MapSize, Allocator.Temp);
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


    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var player = SystemAPI.GetSingletonEntity<Player>();
            var playerTranform = SystemAPI.GetComponentRO<LocalTransform>(player);
            var playerGrid = GridUtils.Instance.GetGridCoordination(playerTranform.ValueRO.Position);
            DebugUtils.Log($"Player Grid {Utils.EntityManager.GetComponentData<Grid>(playerGrid).gridPosition}");
            var job = new PathFindingJob()
            {
                End = playerGrid,
                PathBufferLookup = state.GetBufferLookup<Path>(),
                transformLookup = state.GetComponentLookup<LocalTransform>(),
                neibourLookup = state.GetBufferLookup<GridNeighbour>(),
                gridLookup = state.GetComponentLookup<Grid>(),
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
