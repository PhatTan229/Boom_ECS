using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using System;

public struct GridNeighbour : IBufferElementData
{
    public Entity value;
}

public struct GridConnect : IComponentData
{
    public Entity value;
}

public struct Grid : IComponentData, IEquatable<Grid>
{
    public GridPosition gridPosition;
    public bool travelable;
    public float g;
    public float h;
    public float f => g + h;

    public Grid(GridPosition position, bool travelable)
    {
        this.travelable = travelable;
        gridPosition = position;
        g = 0;
        h = 0;
    }
    
    public int GetDistance(Grid neighbour)
    {
        var dist = new GridPosition(Mathf.Abs(gridPosition.x - neighbour.gridPosition.x), Mathf.Abs(gridPosition.y - neighbour.gridPosition.y));

        var lowest = Mathf.Min(dist.x, dist.y);
        var highest = Mathf.Max(dist.x, dist.y);

        var horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    public bool Equals(Grid other)
    {
        return gridPosition == other.gridPosition;
    }

    public override int GetHashCode()
    {
        return gridPosition.GetHashCode();
    }
}

public class GridAuthoring : MonoBehaviour
{
    public GridPosition position;
    public bool travelable;

    class GridAuthoringBaker : Baker<GridAuthoring>
    {
        public override void Bake(GridAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var grid = new Grid(authoring.position, authoring.travelable);
            AddComponent(entity, new Grid(authoring.position, authoring.travelable));
            AddComponent(entity, new GridConnect() { value = Entity.Null });
            AddBuffer<GridNeighbour>(entity);
        }
    }
}
