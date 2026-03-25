using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct MapInfo : IComponentData
{
    public int tileCount;

    public MapInfo(int tileCount)
    {
        this.tileCount = tileCount;
    }
}

public class MapAuthoring : MonoBehaviour
{
    public ThemeData theme;
    public TextAsset mapBlueprint;

    class MapBaker : Baker<MapAuthoring>
    {
        public override void Bake(MapAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var col = authoring.theme.tileMaterial.GetFloat("_Collum");
            var row = authoring.theme.tileMaterial.GetFloat("_Row");
            AddComponent(entity, new MapInfo((int)(row * col)));
        }
    }
}
