using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BombAuthoring))]
public class BombAuthoringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var target = (BombAuthoring)this.target;
        if (target.TryGetComponent<ExpolsePartten_Base>(out var expolsePartten)) return;
        EditorGUILayout.HelpBox("Missing explose partten component.\nBomb cannot work properly without one.", MessageType.Warning);
        if (GUILayout.Button("Add Default"))
        {
            target.AddComponent<ExplosePartten_Default>();
        }
        if (GUILayout.Button("Create Explose Partten Script"))
        {
            CreateExplosePartten();
        }
    }

    public void CreateExplosePartten()
    {
        var target = (BombAuthoring)this.target;
        var rootPath = System.IO.Path.Combine(Application.dataPath, "Scripts", "ExplosePartten");

        var file = System.IO.Path.Combine(rootPath, $"ExplosePartten_{target.transform.root.name}.cs");
        if (!File.Exists(file))
        {
            var template = AssetDatabase.LoadAssetAtPath<TextAsset>(System.IO.Path.Combine("Assets", "Editor", "ExploseParttenTemplate.txt"));
            File.WriteAllText(file, string.Format(template.text, target.transform.root.name));
            AssetDatabase.Refresh();
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(file, 1);
        }
        var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(file.Substring(Application.dataPath.Length - "Assets".Length).Replace("\\", "/"));
        EditorGUIUtility.PingObject(scriptAsset);
        var className = $"ExplosePartten_{target.transform.root.name}";
    }
}