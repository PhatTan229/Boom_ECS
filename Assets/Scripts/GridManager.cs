using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Transform minPos;
    [SerializeField] private Transform maxPos;
    [SerializeField] private MyGrid gridPrefab;

    public void SpawnGrid()
    {
        var lenghtX = maxPos.position.x - minPos.position.x;
        var lengthY = maxPos.position.y - minPos.position.y;

        var currentPosX = minPos.position.x;
        var currentPosY = minPos.position.y;

        var gridPositionX = 0;
        var gridPositionY = 0;

        for (int y = 0; y < lengthY; y++)
        {
            currentPosX = minPos.position.x;
            gridPositionX = 0;
            for (int x = 0; x < lenghtX; x++)
            {
                var currentPos = new Vector2(currentPosX + 0.5f, currentPosY + 0.5f);
                var newGrid = Instantiate(gridPrefab);
                newGrid.gridPosition = new GridPosition(gridPositionX, gridPositionY);
                newGrid.transform.position = currentPos;
                newGrid.transform.SetParent(transform);
                gridPositionX++;
                currentPosX++;
            }
            gridPositionY++;
            currentPosY++;
        }
    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
