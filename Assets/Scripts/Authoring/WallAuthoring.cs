using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public enum WallType
{
    Wall, Destroyable
}

public struct Wall : IComponentData 
{
    public WallType wallType;
    public GridPosition gridPosition;

    public void OnWallDestroy(RefRW<Grid> grid)
    {
        grid.ValueRW.travelable = true;
    }
}


public class WallAuthoring : MonoBehaviour
{
    public WallType type;
    public GridPosition gridPosition;

    class WallAuthoringBaker : Baker<WallAuthoring>
    {
        public override void Bake(WallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Wall()
            {
                wallType = authoring.type,
                gridPosition = authoring.gridPosition
            });
        }
    }
}
