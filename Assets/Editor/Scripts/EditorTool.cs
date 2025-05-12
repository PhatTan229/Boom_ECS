using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTool : Editor
{
    [MenuItem("Tools/Generate Quad Mesh")]
    public static void CreateQuadMesh()
    {
        Mesh mesh = new Mesh();

        mesh.name = "Quad";

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
        };

        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };

        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        AssetDatabase.CreateAsset(mesh, "Assets/QuadMesh.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Quad mesh created!");
    }
}
