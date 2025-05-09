using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct GridCoordination : IComponentData
{
    public Entity CurrentGrid;
}

public class GridCoordiantionAuthoring : MonoBehaviour
{
    class GridCoordinationDataBaker : Baker<GridCoordiantionAuthoring>
    {
        public override void Bake(GridCoordiantionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<GridCoordination>(entity);
        }
    }
}
