using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UintLayer
{
    public static readonly uint[] Layer = new uint[]
    {
        1u << 0,
        1u << 1,
        1u << 2,
        1u << 3,
        1u << 4,
        1u << 5,
        1u << 6,
        1u << 7,
        1u << 8,
        1u << 9,
        1u << 10,
        1u << 11,
        1u << 12,
        1u << 13,
        1u << 14,
        1u << 15,
        1u << 16,
        1u << 17,
        1u << 18,
        1u << 19,
        1u << 20,
        1u << 21,
        1u << 22,
        1u << 23,
        1u << 24,
        1u << 25,
        1u << 26,
        1u << 27,
        1u << 28,
        1u << 29,
        1u << 30,
        1u << 31,
    };

    public const uint AllLayers = ~0u;
    public const uint NoLayer = 0u;


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
