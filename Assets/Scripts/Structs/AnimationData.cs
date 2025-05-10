using System;
using Unity.Collections;
using Unity.Entities;

public struct AnimationData
{
    public FixedString32Bytes name;
    public readonly int rowIndex;
    public int currentFrame;

    public AnimationData(FixedString32Bytes name, int rowIndex)
    {
        this.name = name;
        this.rowIndex = rowIndex;
        currentFrame = 0;
    }
}
