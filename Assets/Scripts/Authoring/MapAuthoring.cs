using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TreeEditor;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpawnPointBuffer : IBufferElementData
{
    public GridPosition spawnPoint;
}

public struct MapInfo : IComponentData
{
    public int2 mapSize;
    public int2 wallTextureSize;
    public int2 destroyableTextureSize;

    public int TileCount => mapSize.x * mapSize.y;

    public MapInfo(ThemeData themeData)
    {
        var col = (int)themeData.tileMaterial.GetFloat("_Collum");
        var row = (int)themeData.tileMaterial.GetFloat("_Row");
        mapSize = new int2(col, row);

        wallTextureSize = int2.zero;
        destroyableTextureSize = int2.zero;
        foreach (var item in themeData.walls)
        {
            var material = item.prefab.GetComponentInChildren<SpriteRenderAuthoring>().material;
            var size = new int2((int)material.GetFloat("_Collum"), (int)material.GetFloat("_Row"));
            switch(item.key)
            {
                case "Wall":
                    wallTextureSize = size;
                    break;
                case "Destroyable":
                    destroyableTextureSize = size;
                    break;
                default:
                    break;
            }
        }
    }
}

public class MapAuthoring : MonoBehaviour
{
    public ThemeData theme;
    public TextAsset mapBlueprint;
    public List<GridPosition> spawnPoints;

    class MapBaker : Baker<MapAuthoring>
    {
        public override void Bake(MapAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var col = authoring.theme.tileMaterial.GetFloat("_Collum");
            var row = authoring.theme.tileMaterial.GetFloat("_Row");
            AddComponent(entity, new MapInfo(authoring.theme));
            var spawnPointBuffer = AddBuffer<SpawnPointBuffer>(entity);
            foreach (var item in authoring.spawnPoints)
            {
                spawnPointBuffer.Add(new SpawnPointBuffer() { spawnPoint = item });
            }
        }
    }
}
