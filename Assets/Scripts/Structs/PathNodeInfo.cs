using Unity.Entities;

public struct PathNodeInfo
{
    public GridPosition position;
    public float g;
    public float h;

    public float f => g + h;

    public PathNodeInfo(GridPosition gridPosition)
    {
        position = gridPosition;
        g = 0f;
        h = 0f;
    }
}
