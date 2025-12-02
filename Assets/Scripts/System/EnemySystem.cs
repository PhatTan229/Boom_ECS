using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

public partial struct EnemySystem : ISystem, ISystemStartStop
{
    private BufferLookup<AnimationStateBuffer> stateLookup;
    private BufferLookup<Child> childLookup;
    private ComponentLookup<SpriteAnimation> animaitonLookup;
    private ComponentLookup<Player> playerLookup;
    private NativeList<Entity> detectPlayer;

    public void OnStartRunning(ref SystemState state)
    {
        stateLookup = SystemAPI.GetBufferLookup<AnimationStateBuffer>();
        childLookup = SystemAPI.GetBufferLookup<Child>();
        animaitonLookup = SystemAPI.GetComponentLookup<SpriteAnimation>();
        playerLookup = state.GetComponentLookup<Player>();
        detectPlayer = new NativeList<Entity>(Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        stateLookup.Update(ref state);
        childLookup.Update(ref state);
        animaitonLookup.Update(ref state);
        playerLookup.Update(ref state);

        detectPlayer.Clear();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (detector, enemy, coord, pathfinding, path, stat, velocity, entity) in SystemAPI.Query<DynamicBuffer<DetectBuffer>, RefRW<Enemy>, RefRO<GridCoordination>, RefRW<PathFinding>, DynamicBuffer<Path>, RefRO<StatData>, RefRO<PhysicsVelocity>>().WithEntityAccess())
        {
            if(stat.ValueRO.currentStat.HP <= 0)
            {
                ecb.SetEnabled(entity, false);
                continue;
            }

            UpdateAnimation(ref state, entity, path, coord);
            if (path.Length != 0 && coord.ValueRO.CurrentGrid != path[path.Length - 1].value)
            {
                if(IsPathDirty(ref state, path)) PathFindingHelper.RegisterClearPath(entity);
                continue;
            }

            var currentGrid = SystemAPI.GetComponentRO<Grid>(coord.ValueRO.CurrentGrid);
            var travelable = AStar.GetTravelableGrids(currentGrid.ValueRO.gridPosition);

            var target = FindTarget(ref state, detector, stat);
            if (target == Entity.Null)
            {
                Patrol(ref state, coord, pathfinding, entity);
                continue;
            }

            var gridEntity = SystemAPI.GetComponentRO<GridCoordination>(target).ValueRO.CurrentGrid;
            var targetGrid = SystemAPI.GetComponentRO<Grid>(gridEntity);
            if (travelable.Contains(GridData.Instance.GetCellEntityAt(targetGrid.ValueRO.gridPosition)))
            {
                pathfinding.ValueRW.currentIndex = 0;
                PathFindingHelper.RegisterPathFinding(entity, target);
            }
            else Patrol(ref state, coord, pathfinding, entity);
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private bool IsPathDirty(ref SystemState state, DynamicBuffer<Path> path)
    {
        foreach (var item in path)
        {
            var grid = state.EntityManager.GetComponentData<Grid>(item.value);
            if (!grid.travelable) return true;
        }
        return false;
    }

    private Entity FindTarget(ref SystemState state, DynamicBuffer<DetectBuffer> detector, RefRO<StatData> stat)
    {
        var target = Entity.Null;
        foreach (var item in detector)
        {
            if (!playerLookup.HasComponent(item.entity)) continue;
            if (stat.ValueRO.currentStat.HP > 0)
            {
                target = item.entity;
            }
        }

        return target;
    }

    private void UpdateAnimation(ref SystemState state, Entity entity, DynamicBuffer<Path> path, RefRO<GridCoordination> coord)
    {
        if (path.Length < 2) return;

        var currentGrid = SystemAPI.GetComponentRO<Grid>(coord.ValueRO.CurrentGrid).ValueRO.gridPosition;
        var index = 0;
        for (int i = 0; i < path.Length; i++)
        {
            var grid = SystemAPI.GetComponentRO<Grid>(path[i].value).ValueRO.gridPosition;
            if (currentGrid == grid)
            {
                index = i;
                if (i < path.Length - 1) index++;
                break;
            }
        }
        var nextGrid = SystemAPI.GetComponentRO<Grid>(path[index].value).ValueRO.gridPosition;

        var transform = SystemAPI.GetComponentRO<LocalTransform>(entity);
        var currentGridTransform = SystemAPI.GetComponentRO<LocalTransform>(coord.ValueRO.CurrentGrid);
        var dist = math.distance(transform.ValueRO.Position, currentGridTransform.ValueRO.Position);
        var canChangeState = dist < 0.1f;

        var currentState = Utils.FixString32_Emty;
        var childs = SystemAPI.GetBuffer<Child>(entity);

        foreach (var item in childs)
        {
            if (animaitonLookup.HasComponent(item.Value))
            {
                currentState = animaitonLookup[item.Value].CurrentSate;
            }
        }

        var newState = Utils.FixString32_Emty;
        if (nextGrid.y < currentGrid.y && canChangeState) newState = Utils.FixString32(nameof(Direction.Up));
        else if (nextGrid.y > currentGrid.y && canChangeState) newState = Utils.FixString32(nameof(Direction.Down));
        else if (nextGrid.x < currentGrid.x && canChangeState) newState = Utils.FixString32(nameof(Direction.Left));
        else if (nextGrid.x > currentGrid.x && canChangeState) newState = Utils.FixString32(nameof(Direction.Right));

        if (newState != Utils.FixString32_Emty && currentState != newState) currentState = newState;

        var job = new SetAnimationJob()
        {
            entity = entity,
            stateLookup = stateLookup,
            animaitonLookup = animaitonLookup,
            childLookup = childLookup,
            deltaTime = SystemAPI.Time.DeltaTime,
            stateName = currentState,
            //outputData = data

        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }

    private void Patrol(ref SystemState state, RefRO<GridCoordination> coord, RefRW<PathFinding> pathfinding, Entity entity)
    {
        var currentGrid = SystemAPI.GetComponentRO<Grid>(coord.ValueRO.CurrentGrid);
        var travelable = AStar.GetTravelableGrids(currentGrid.ValueRO.gridPosition);
        var destination = travelable[UnityEngine.Random.Range(0, travelable.Length)];
        pathfinding.ValueRW.currentIndex = 0;
        PathFindingHelper.RegisterPathFinding(entity, destination);
        travelable.Dispose();
    }

    public void OnStopRunning(ref SystemState state)
    {
        detectPlayer.Dispose();
    }
}
