using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UIElements;

public class GridSpawner : MonoBehaviour
{
    [SerializeField] private int MAX_ROW = 8;
    [SerializeField] private int MAX_COL = 5;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Transform parent;


#if UNITY_EDITOR
    private static GridAuthoring[] allGrid;

    public static void Init()
    {
        allGrid = FindObjectsOfType<GridAuthoring>();
    }

    public static void DebugPath(NativeHashSet<Grid> path)
    {
        if (allGrid == null || allGrid.Length == 0) allGrid = FindObjectsOfType<GridAuthoring>();
        for (int i = 0; i < allGrid.Length; i++) 
        {
            foreach (var item in path)
            {
                if (allGrid[i].position == item.gridPosition)
                {
                    allGrid[i].GetComponentInChildren<SpriteRenderer>().color = Color.red;
                }
            }
        }
    }

    public static void DebugPath(GridPosition position)
    {
        if(allGrid == null || allGrid.Length == 0) allGrid = FindObjectsOfType<GridAuthoring>();
        foreach (var item in allGrid)
        {
            if(item.position == position) item.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }
    }

    public static void ResetGrid()
    {
        var allGrid = FindObjectsOfType<GridAuthoring>();
        foreach (var item in allGrid)
        {
            item.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
    }
#endif

    public void SpawnGrid()
    {
        var pos = new Vector2((int)(-MAX_COL / 2f), (MAX_ROW / 2f) - 0.5f);
        var gridPos = new GridPosition(1, 1);
        for (int x = 0; x < MAX_COL; x++)
        {
            for (int y = 0; y < MAX_ROW; y++)
            {
                var gameObj = new GameObject("Grid", typeof(EntityInfoAuthoring));
                gameObj.transform.position = pos;
                gameObj.transform.rotation = Quaternion.identity;
                var cell = gameObj.AddComponent<GridAuthoring>();
                cell.position = gridPos;
                gridPos.y += 1;
                pos.y -= 1f;
                cell.travelable = true;
                if (sprite == null) continue;
                var renderer = gameObj.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            gridPos.x += 1;
            gridPos.y = 1;
            pos.x += 1f;
            pos.y = (MAX_ROW / 2f) - 0.5f;
        }
        if (parent == null) return;
        var cells = FindObjectsOfType<GridAuthoring>();
        parent.position = Vector3.zero;
        foreach (var item in cells)
        {
            item.transform.SetParent(parent);
        }
    }

    public void SpawnGrid3D()
    {
        var pos = new Vector3((int)(-MAX_COL / 2f), 0, (MAX_ROW / 2f) - 0.5f);
        var gridPos = new GridPosition(1, 1);
        for (int x = 0; x < MAX_COL; x++)
        {
            for (int y = 0; y < MAX_ROW; y++)
            {
                var gameObj = new GameObject("Grid", typeof(EntityInfoAuthoring));
                gameObj.transform.position = pos;
                gameObj.transform.rotation = Quaternion.identity;
                var cell = gameObj.AddComponent<GridAuthoring>();
                cell.position = gridPos;
                gridPos.y += 1;
                pos.y = 0;
                pos.z -= 1f;
                cell.travelable = true;
                if (sprite == null) continue;
                var child = new GameObject("Sprite", typeof (SpriteRenderer));
                child.transform.SetParent(gameObj.transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.eulerAngles = new Vector3(-90f, 0, 0);
                var renderer = child.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
            gridPos.x += 1;
            gridPos.y = 1;
            pos.x += 1f;
            pos.y = 0;
            pos.z = (MAX_ROW / 2f) - 0.5f;
        }
        if (parent == null) return;
        var cells = FindObjectsOfType<GridAuthoring>();
        parent.position = Vector3.zero;
        foreach (var item in cells)
        {
            item.transform.SetParent(parent);
        }
    }

    [ContextMenu("Test")]
    public void Test()
    {
        var category = (PhysicsCategory[])Enum.GetValues(typeof(PhysicsCategory));

        foreach (var item in category)
        {
            Debug.Log($"Name {item.ToString()} : {(uint)item}");
        }
    }
}
