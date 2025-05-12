using System;
using Unity.Collections;
using Unity.Entities;

public struct AnimationData
{
    public FixedString32Bytes name;
    public readonly int rowIndex;
    public int currentFrame;
    public int fps;
    public bool isDefault;

    public AnimationData(FixedString32Bytes name, int rowIndex, int fps, bool isDefault)
    {
        this.name = name;
        this.rowIndex = rowIndex;
        currentFrame = 0;
        this.fps = fps;
        this.isDefault = isDefault;
    }
}
