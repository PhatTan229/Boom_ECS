using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapAuthoring))]
public class MapEditor : Editor
{
    private ThemeData themeData;
    private TextAsset blueprint;
    private MapGenerateInfo infoPopup;
    private void OnEnable()
    {
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
        var grids = target.GetComponentsInChildren<GridAuthoring>();
        var bytes = DecomposeBlueprint(blueprint);
        foreach (var g in grids)
        {
            var num = bytes[g.position.x - 1, g.position.y - 1];
            g.gridType = (GridType)num;

            var r = g.GetComponentInChildren<SpriteRenderer>();
            r.enabled = false;
            if (!r.TryGetComponent<SpriteRenderAuthoring>(out _)) g.AddComponent<SpriteRenderAuthoring>();
            var authoring = r.GetComponent<SpriteRenderAuthoring>();
            authoring.material = themeData.tileMaterial;
            authoring.mesh = themeData.mesh;
            authoring.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }

    private void NewMapBlueprint()
    {
        PopupWindow.Show(new Rect(), infoPopup);
    }

    private byte[,] DecomposeBlueprint(TextAsset blueprint)
    {
        var content = blueprint.text;
        var row = content.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
        var col = row[0].Split(',');
        var bytes = new byte[col.Length, row.Length];
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
}
