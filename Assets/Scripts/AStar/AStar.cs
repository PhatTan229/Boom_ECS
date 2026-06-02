using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Mono.Cecil;

public static class AStar
{
    public static void FindPath(Entity start, Entity end, NativeList<Entity> path, ref BufferLookup<GridNeighbour> neibourLookup, ref ComponentLookup<Grid> gridLookup, ref NativeHashMap<Entity, Entity> connections, ref NativeParallelHashMap<Entity, PathNodeInfo> nodeInfos)
    {
        var searching = new NativeList<Entity>(Allocator.Temp);
        var processed = new NativeHashSet<Entity>(128, Allocator.Temp);

        searching.Add(start);
        while (searching.Length > 0)
        {
            var current = searching[0];
            var currentGrid = gridLookup[searching[0]];
            var currentNodeInfo = nodeInfos[current];
            //var currentGrid = Utils.EntityManager.GetComponentData<Grid>(searching[0]);
            foreach (var item in searching)
            {
                var grid = gridLookup[item];
                var gridNodeInfo = nodeInfos[item];
                if (gridNodeInfo.f < currentNodeInfo.f || gridNodeInfo.f == currentNodeInfo.f && gridNodeInfo.h < currentNodeInfo.h) current = item;
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
                var gridNodeInfo = nodeInfos[item.value];
                //var grid = Utils.EntityManager.GetComponentData<Grid>(item.value);

                if (!grid.travelable) continue;
                if (processed.Contains(item.value)) continue;

                var hasSearch = searching.Contains(item.value);
                var costToNextCell = currentNodeInfo.g + currentGrid.GetDistance(grid);

                if (!hasSearch || costToNextCell < gridNodeInfo.g)
                {
                    gridNodeInfo.g = costToNextCell;
                    //SetConnection(item.value, current);
                    connections[item.value] = current;

                    if (!hasSearch)
                    {
                        gridNodeInfo.h = grid.GetDistance(gridLookup[end]);
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
        var result = new NativeList<Entity>(allocator);
        var visited = new NativeHashSet<Entity>(100, allocator);
        var queue = new NativeQueue<GridPosition>(allocator);

        queue.Enqueue(position);

        while (queue.TryDequeue(out var pos))
        {
            var cellOpt = GridData.Instance.GetCellAt(pos);
            if (!cellOpt.HasValue) continue;

            var cell = cellOpt.Value;
            if (!cell.travelable) continue;

            var entity = GridData.Instance.GetCellEntityAt(pos);

            if (!visited.Add(entity))
                continue;

            result.Add(entity);

            foreach (var offset in GridData.ajectionNeighbourGridPosition)
            {
                var next = pos + offset;
                queue.Enqueue(next);
            }
        }

        visited.Dispose();
        queue.Dispose();

        return result;
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
