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

public class Map : MonoBehaviour
{
    public ThemeData theme;

    class MapBaker : Baker<Map>
    {
        public override void Bake(Map authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var col = authoring.theme.tileMaterial.GetFloat("_Collum");
            var row = authoring.theme.tileMaterial.GetFloat("_Row");
            AddComponent(entity, new MapInfo((int)(row * col)));
        }
    }
}
