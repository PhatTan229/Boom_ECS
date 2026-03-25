using UnityEditor;
using UnityEngine;

public class MapGenerateInfo : PopupWindowContent
{
    private int width;
    private int height;
    private string name;

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Create New Map Blueprint", EditorStyles.boldLabel);

        name = EditorGUILayout.TextField("Map Name", name);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (GUILayout.Button("Create")) MapBlueprintGeneratorTool.GenerateBlueprint(width, height, name);

        if (GUILayout.Button("Cancel")) editorWindow.Close();
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(400f, 100f);
    }

    public override void OnOpen()
    {
        width = 10;
        height = 10;
        name = "New Map Blueprint";
    }
}
