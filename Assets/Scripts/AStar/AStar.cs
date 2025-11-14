using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

public static class AStar
{
    public static void FindPath(Entity start, Entity end, NativeList<Entity> path)
    {
        var searching = new NativeList<Entity>(Allocator.Temp);
        var processed = new NativeHashSet<Entity>(128, Allocator.Temp);

        searching.Add(start);
        while (searching.Length > 0)
        {
            var current = searching[0];
            var currentGrid = Utils.EntityManager.GetComponentData<Grid>(searching[0]);
            foreach (var item in searching)
            {
                var grid = Utils.EntityManager.GetComponentData<Grid>(item);
                if (grid.f < currentGrid.f || grid.f == currentGrid.f && grid.h < currentGrid.h) current = item;
            }
            searching.RemoveAtSwapBack(searching.IndexOf(current));
            processed.Add(current);

            if (current.Equals(end))
            {
                var currentCell = end;
                while (!currentCell.Equals(start))
                {
                    var connectedGrid = GetConnection(currentCell);
                    //var connect = GridUtils.Instance.GetCellEntityAt(connectedGrid.gridPosition);
                    path.Add(connectedGrid);
                    currentCell = connectedGrid;
                }

                for (int i = 0, j = path.Length - 1; i < j; i++, j--)
                {
                    var temp = path[i];
                    path[i] = path[j];
                    path[j] = temp;
                }
                path.Add(end);
                //path.Reverse();
                searching.Dispose();
                processed.Dispose();
                var str = string.Empty;
                foreach (var item in path)
                {
                    var position = Utils.EntityManager.GetComponentData<Grid>(item).gridPosition;
                    str += position.ToString() + "\n";
                }
                DebugUtils.Log(str);
                return;
            }

            var neighbours = Utils.EntityManager.GetBuffer<GridNeighbour>(current);
            foreach (var item in neighbours)
            {
                /*.Where(x => x.travelable && !processed.Contains(x))*/
                var grid = Utils.EntityManager.GetComponentData<Grid>(item.value);

                if (!grid.travelable) continue;
                if (processed.Contains(item.value)) continue;

                var hasSearch = searching.Contains(item.value);
                var costToNextCell = currentGrid.g + currentGrid.GetDistance(grid);

                if (!hasSearch || costToNextCell < grid.g)
                {
                    grid.g = costToNextCell;
                    SetConnection(item.value, current);

                    if (!hasSearch)
                    {
                        grid.h = grid.GetDistance(Utils.EntityManager.GetComponentData<Grid>(end));
                        searching.Add(item.value);
                    }
                    Utils.EntityManager.SetComponentData(item.value, grid);
                }
            }
        }
        searching.Dispose();
        processed.Dispose();
    }

    public static void FindPath(Entity start, Entity end, NativeList<Entity> path, ref BufferLookup<GridNeighbour> neibourLookup, ref ComponentLookup<Grid> gridLookup, ref NativeHashMap<Entity, Entity> connections)
    {
        var searching = new NativeList<Entity>(Allocator.Temp);
        var processed = new NativeHashSet<Entity>(128, Allocator.Temp);

        searching.Add(start);
        while (searching.Length > 0)
        {
            var current = searching[0];
            var currentGrid = gridLookup[searching[0]];
            //var currentGrid = Utils.EntityManager.GetComponentData<Grid>(searching[0]);
            foreach (var item in searching)
            {
                var grid = gridLookup[item];
                if (grid.f < currentGrid.f || grid.f == currentGrid.f && grid.h < currentGrid.h) current = item;
            }
            searching.RemoveAtSwapBack(searching.IndexOf(current));
            processed.Add(current);

            if (current.Equals(end))
            {
                var currentCell = end;
                while (!currentCell.Equals(start))
                {
                    var connectedGrid = connections[currentCell];
                    path.Add(connectedGrid);
                    currentCell = connectedGrid;
                }

                for (int i = 0, j = path.Length - 1; i < j; i++, j--)
                {
                    var temp = path[i];
                    path[i] = path[j];
                    path[j] = temp;
                }
                path.Add(end);
                //path.Reverse();
                searching.Dispose();
                processed.Dispose();
                //var str = string.Empty;
                //foreach (var item in path)
                //{
                //    //var position = Utils.EntityManager.GetComponentData<Grid>(item).gridPosition;
                //    var position = gridLookup[item].gridPosition;
                //    str += position.ToString() + "\n";
                //}
                //DebugUtils.Log(str);
                return;
            }

            var neighbours = neibourLookup[current];
            //var neighbours = Utils.EntityManager.GetBuffer<GridNeighbour>(current);
            foreach (var item in neighbours)
            {
                /*.Where(x => x.travelable && !processed.Contains(x))*/
                var grid = gridLookup[item.value];
                //var grid = Utils.EntityManager.GetComponentData<Grid>(item.value);

                if (!grid.travelable) continue;
                if (processed.Contains(item.value)) continue;

                var hasSearch = searching.Contains(item.value);
                var costToNextCell = currentGrid.g + currentGrid.GetDistance(grid);

                if (!hasSearch || costToNextCell < grid.g)
                {
                    grid.g = costToNextCell;
                    //SetConnection(item.value, current);
                    connections[item.value] = current;

                    if (!hasSearch)
                    {
                        grid.h = grid.GetDistance(gridLookup[end]);
                        searching.Add(item.value);
                    }
                    gridLookup[item.value] = grid;
                }
            }
        }
        searching.Dispose();
        processed.Dispose();
    }

    public static NativeList<Entity> GetTravelableGrids(GridPosition position, Allocator allocator = Allocator.Temp)
    {
        var travelable = new NativeList<Entity>(allocator);
        CheckNeighbourGrids(position, travelable);
        return travelable;
    }

    private static void CheckNeighbourGrids(GridPosition position, NativeList<Entity> travelable)
    {
        var startGrid = GridData.Instance.GetCellAt(position).Value;

        foreach (var item in GridData.ajectionNeighbourGridPosition)
        {
            var neighbour = position + item;
            var grid = GridData.Instance.GetCellAt(neighbour);
            if (grid.HasValue && grid.Value.travelable)
            {
                var gridEntity = GridData.Instance.GetCellEntityAt(neighbour);
                if (!travelable.Contains(gridEntity))
                {
                    travelable.Add(gridEntity);
                    CheckNeighbourGrids(neighbour, travelable);
                }
            }
        }
    }

    private static Entity GetConnection(Entity currentEntity)
    {
        return Utils.EntityManager.GetComponentData<GridConnect>(currentEntity).value;
    }

    private static Entity GetConnection(Entity currentEntity, [ReadOnly] ComponentLookup<GridConnect> connectLookup)
    {
        return connectLookup[currentEntity].value;
    }

    private static void SetConnection(Entity child, Entity parent)
    {
        Utils.EntityManager.SetComponentData(child, new GridConnect { value = parent });
    }
}
