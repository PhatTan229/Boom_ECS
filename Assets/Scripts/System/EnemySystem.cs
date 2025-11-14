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

        foreach (var (detector, enemy, coord, pathfinding, path, stat, entity) in SystemAPI.Query<DynamicBuffer<DetectBuffer>, RefRW<Enemy>, RefRO<GridCoordination>, RefRW<PathFinding>, DynamicBuffer<Path>, RefRO<StatData>>().WithEntityAccess())
        {
            if (detector.Length <= 0 && (path.Length == 0 || coord.ValueRO.CurrentGrid == path[path.Length - 1].value))
            {
                Patrol(ref state, coord, pathfinding, entity);
                continue;
            }
            var target = Entity.Null;
            foreach (var item in detector)
            {
                if (!playerLookup.HasComponent(item.entity)) continue;
                if (stat.ValueRO.currentStat.HP > 0)
                {
                    target = item.entity;
                }
            }
            if(target != Entity.Null) PathFindingHelper.RegisterPathFinding(entity, target);
        }
    }

    private void Patrol(ref SystemState state, RefRO<GridCoordination> coord, RefRW<PathFinding> pathfinding, Entity entity)
    {
        var currentGrid = SystemAPI.GetComponentRO<Grid>(coord.ValueRO.CurrentGrid);
        var travelable = AStar.GetTravelableGrids(currentGrid.ValueRO.gridPosition);
        var destination = travelable[Random.Range(0, travelable.Length)];
        pathfinding.ValueRW.currentIndex = 0;
        PathFindingHelper.RegisterPathFinding(entity, destination);
        travelable.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
        detectPlayer.Dispose();
    }
}
