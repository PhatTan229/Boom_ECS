using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class GridData : IDisposable
{
    private static GridData instance;

    private Dictionary<GridPosition, Entity> cellDic = new Dictionary<GridPosition, Entity>();
    private Dictionary<float3, GridPosition> posToGrid = new Dictionary<float3, GridPosition>();
    public NativeArray<Grid> allCells;
    public NativeArray<Entity> cellEntities;
    public readonly int MaxRow;
    public readonly int MaxCol;
    public static readonly GridPosition[] ajectionNeighbourGridPosition = new GridPosition[]
    {
        new GridPosition(0, 1),
        new GridPosition(1, 0),
        new GridPosition(0, -1),
        new GridPosition(-1, 0),
    };

    public static readonly GridPosition[] neighbourGridPosition = new GridPosition[]
    {
        new GridPosition(-1, -1),
        new GridPosition(0, 1),
        new GridPosition(1, 0),
        new GridPosition(1,-1),
        new GridPosition(0, -1),
        new GridPosition(-1, 0),
        new GridPosition(-1,1),
        new GridPosition (1,1),
    };

    public int MapSize => cellEntities.Count();

    public static GridData Instance
    {
        get
        {
            if (instance == null) instance = new GridData();
            return instance;
        }
    }

    private GridData()
    {
        var query = Utils.EntityManager.CreateEntityQuery(typeof(Grid));
        allCells = query.ToComponentDataArray<Grid>(Allocator.Persistent);
        cellEntities = query.ToEntityArray(Allocator.Persistent);
        for (int i = 0; i < allCells.Length; i++)
        {
            var grid = Utils.EntityManager.GetComponentData<Grid>(cellEntities[i]);
            if (grid.gridPosition.x > MaxRow) MaxRow = grid.gridPosition.x;
            if (grid.gridPosition.y > MaxCol) MaxCol = grid.gridPosition.y;
            cellDic.Add(grid.gridPosition, cellEntities[i]);
            var pos = Utils.EntityManager.GetComponentData<LocalTransform>(cellEntities[i]);
            posToGrid.Add(pos.Position, grid.gridPosition);
        }
    }

    public Grid? GetCellAt(GridPosition gridPosition)
    {
        if (!cellDic.TryGetValue(gridPosition, out var entity)) return null;
        return Utils.EntityManager.GetComponentData<Grid>(entity);
    }

    public Entity GetCellEntityAt(GridPosition gridPosition)
    {
        if (!cellDic.TryGetValue(gridPosition, out var entity)) return Entity.Null;
        return entity;
    }

    public Entity GetGridCoordination_Entity(float3 position)
    {
        var nearestDistance = float.MaxValue;
        var nearestPos = float3.zero;

        foreach (var item in posToGrid)
        {
            var distance = math.distance(position, item.Key);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPos = item.Key;
            }
        }

        var gridPos = WorldToGrid(nearestPos);
        return GetCellEntityAt(gridPos);
    }

    public bool GetGridCoordination_Grid(float3 position, out Grid grid)
    {
        var nearestDistance = float.MaxValue;
        var nearestPos = float3.zero;

        foreach (var item in posToGrid)
        {
            var distance = math.distance(position, item.Key);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPos = item.Key;
            }
        }

        var gridPos = WorldToGrid(nearestPos);
        var tmp = GetCellAt(gridPos);
        grid = tmp.Value;
        return tmp.HasValue;
    }


    public GridPosition WorldToGrid(float3 position)
    {
        return posToGrid[position];
    }

    public bool WorldToGrid(float3 position, out GridPosition? gridPosition)
    {
        if (posToGrid.ContainsKey(position)) gridPosition = posToGrid[position];
        else gridPosition = null;
        return posToGrid.ContainsKey(position);
    }

    public Entity RandomGrid()
    {
        return cellEntities[UnityEngine.Random.Range(0, cellEntities.Length)];
    }

    public void Dispose()
    {
        if (allCells.IsCreated) allCells.Dispose();
        if (cellEntities.IsCreated) cellEntities.Dispose();
        instance = null;
    }
}
