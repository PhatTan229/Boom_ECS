using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "ThemeData")]
public class ThemeData : ScriptableObject
{
    public Material tileMaterial;
    public Mesh mesh;

    public PrefabData[] walls;
}
