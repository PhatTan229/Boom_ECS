using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.Entities.Serialization;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct SceneLoader : IComponentData
{
    public EntitySceneReference SceneReference;
}

public class GameData
{
    private static List<SceneInfo> scenes;
    public static IReadOnlyList<SceneInfo> Scenes => scenes.AsReadOnly();

    public static void Initialize()
    {
        scenes = new List<SceneInfo>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            scenes.Add(new SceneInfo(i, scene.path, scene.name));
        }
    }
}

public struct SceneInfo
{
    public int index;
    public string path;
    public string name;

    public SceneInfo(int index, string path, string name)
    {
        this.index = index;
        this.path = path;
        this.name = name;
    }
}


public class Initializer : MonoBehaviour
{
    private void Awake()
    {
        //GameData.Initialize();
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        //SceneLoadHelper.RequestSceneLoad(GameData.Scenes[1]);
        SceneManager.LoadScene(1);
    }
}
