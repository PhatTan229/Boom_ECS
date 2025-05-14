using System;
using Unity.Collections;
using Unity.Entities;

public struct AnimationData
{
    public FixedString32Bytes name;
    public readonly int startFrame;
    public readonly int endFrame;
    public int currentFrame;
    public int fps;
    public bool isDefault;

    public AnimationData(FixedString32Bytes name, int startFrame, int endFrame, int fps, bool isDefault)
    {
        this.name = name;
        this.startFrame = startFrame;
        this.endFrame = endFrame;
        currentFrame = startFrame;
        this.fps = fps;
        this.isDefault = isDefault;
    }
}
