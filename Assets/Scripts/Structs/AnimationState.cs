using System;
using Unity.Collections;
using Unity.Entities;

public struct AnimationState 
{
    public FixedString32Bytes name;
    public int startFrame;
    public int endFrame;
    public int currentFrame;

    public AnimationState(FixedString32Bytes name, int[] frame)
    {
        this.name = name;
        startFrame = frame[0];
        endFrame = frame[frame.Length - 1];
        currentFrame = 0;
    }
}
