using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.Progress;

public partial struct EnemySystem : ISystem, ISystemStartStop
{
    private ComponentLookup<Player> playerLookup;
    private NativeList<Entity> detectPlayer;

    public void OnStartRunning(ref SystemState state)
    {
        playerLookup = state.GetComponentLookup<Player>();
        detectPlayer = new NativeList<Entity>(Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        playerLookup.Update(ref state);

        detectPlayer.Clear();

        foreach (var (detector, enemy, coord, pathfinding, path, entity) in SystemAPI.Query<DynamicBuffer<DetectBuffer>, RefRW<Enemy>, RefRO<GridCoordination>, RefRW<PathFinding>, DynamicBuffer<Path>>().WithEntityAccess())
        {
            if(detector.Length <= 0 && (path.Length == 0 || coord.ValueRO.CurrentGrid == path[path.Length - 1].value))
            {       
                var destination = GridData.Instance.RandomGrid();
                Debug.Log($"Change destination to : {state.EntityManager.GetComponentData<Grid>(destination).gridPosition}");
                pathfinding.ValueRW.currentIndex = 0;
                PathFindingHelper.RegisterPathFinding(entity, destination);
                continue;
            }
            foreach (var item in detector)
            {
                if (playerLookup.HasComponent(item.entity))
                {
                    PathFindingHelper.RegisterPathFinding(entity, item.entity);
                }
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        detectPlayer.Dispose();
    }
}
