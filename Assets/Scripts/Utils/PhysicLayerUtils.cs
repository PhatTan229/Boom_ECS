using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum PhysicsCategory : uint
{
    Player = 1 << 0,  // 1
    Enemy = 1 << 1,  // 2
    Bomb = 1 << 3,  // 8
    Wall = 1 << 4,  // 16
}

public class PhysicLayerUtils
{
    public static bool HasLayer(uint mask, PhysicsCategory category)
    {
        return (mask & (uint)category) != 0;
    }

    public static uint GetCollisonMask(int layer)
    {
        uint mask = 0;

        for (int i = 0; i < 32; i++)
        {
            if (Physics2D.GetIgnoreLayerCollision(layer, i)) continue;
            mask |= 1u << i;
        }

        return mask;
    }
}
