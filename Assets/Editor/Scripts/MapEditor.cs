using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[CustomEditor(typeof(MapAuthoring))]
public class MapEditor : Editor
{
    private MapAuthoring map;
    private ThemeData themeData;
    private TextAsset blueprint;
    private MapGenerateInfo infoPopup;
    private void OnEnable()
    {
        map = (MapAuthoring)target;
        themeData = (ThemeData)serializedObject.FindProperty("theme").objectReferenceValue;
        blueprint = (TextAsset)serializedObject.FindProperty("mapBlueprint").objectReferenceValue;
        infoPopup = new MapGenerateInfo();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        if (GUILayout.Button("Setup Map")) SetupMap();
        if (GUILayout.Button("New Map Blueprint")) NewMapBlueprint();
    }

    private void SetupMap()
    {
        for (int i = map.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(map.transform.GetChild(i).gameObject);
        }
        map.spawnPoints.Clear();

        if(map.GetComponentInChildren<GridSpawner>(true) == null)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(System.IO.Path.Combine("Assets", "Prefabs", "Grids.prefab"));
            PrefabUtility.InstantiatePrefab(obj, map.transform);
        }

        var bytes = DecomposeBlueprint(blueprint, out int width, out int height);

        var spawner = map.GetComponentInChildren<GridSpawner>(true);
        spawner.transform.localPosition = Vector3.zero;
        spawner.ClearGrid();
        spawner.SpawnGrid3D(width, height);

        var grids = target.GetComponentsInChildren<GridAuthoring>();
        var wallParent = new GameObject("WallParent");
        wallParent.transform.SetParent(map.transform);
        wallParent.transform.localPosition = Vector3.zero;

        foreach (var g in grids)
        {
            var num = bytes[g.position.x - 1, g.position.y - 1];
            g.gridType = (GridType)num;

            var r = g.GetComponentInChildren<SpriteRenderer>();
            r.enabled = false;
            if (!r.TryGetComponent<SpriteRenderAuthoring>(out _)) r.AddComponent<SpriteRenderAuthoring>();
            var authoring = r.GetComponent<SpriteRenderAuthoring>();
            authoring.material = themeData.tileMaterial;
            authoring.mesh = themeData.mesh;
            authoring.transform.eulerAngles = new Vector3(90, 0, 0);
            SpawnWall(g, wallParent.transform);
        }
    }

    private void NewMapBlueprint()
    {
        PopupWindow.Show(new Rect(), infoPopup);
    }

    private byte[,] DecomposeBlueprint(TextAsset blueprint, out int width, out int height)
    {
        var content = blueprint.text;
        var row = content.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
        var col = row[0].Split(',');
        width = col.Length;
        height = row.Length;
        var bytes = new byte[width, height];
        for (int y = 0; y < row.Length; y++)
        {
            var thisRow = row[y].Replace(",", string.Empty);
            for (int x = 0; x < thisRow.Length; x++)
            {
                var c = thisRow[x];
                if (byte.TryParse($"{c}", out var num))
                    bytes[x, y] = num;
            }
        }
        return bytes;
    }

    private void SpawnWall(GridAuthoring grid, Transform parent)
    {
        switch (grid.gridType)
        {
            case GridType.Wall:
                var wallPrefab = themeData.walls.FirstOrDefault(x => x.key == "Wall");
                var wall = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab.prefab, parent);
                wall.transform.position = grid.transform.position;
                break;
            case GridType.Travelable:
                var destroyablePrefab = themeData.walls.FirstOrDefault(x => x.key == "Destroyable");
                var destroyable = (GameObject)PrefabUtility.InstantiatePrefab(destroyablePrefab.prefab, parent);
                destroyable.transform.position = grid.transform.position;
                break;
            case GridType.SpawnPoint:
                map.spawnPoints.Add(grid.position);
                break;
        }
    }
}
