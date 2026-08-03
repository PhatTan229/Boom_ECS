//using Unity.Collections;
//using Unity.Entities;
//using Unity.Entities.Serialization;
//using Unity.Entities.UniversalDelegates;
//using Unity.Scenes;

//public static class SceneLoadHelper
//{
//    public static SceneInfo? sceneInfo;
//    public static void RequestSceneLoad(SceneInfo _sceneInfo)
//    {
//        sceneInfo = _sceneInfo;
//    }
//}


//[RequireMatchingQueriesForUpdate]
//public partial struct SceneLoaderSystem : ISystem
//{
//    public void OnUpdate(ref SystemState state)
//    {
//        if(!SceneLoadHelper.sceneInfo.HasValue) return;

//        var info = SceneLoadHelper.sceneInfo.Value;
//        var reference = new EntitySceneReference(SceneSystem.GetSceneGUID(ref state, info.path), 0);
//        SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, reference);

//        SceneLoadHelper.sceneInfo = null;
//    }
//}