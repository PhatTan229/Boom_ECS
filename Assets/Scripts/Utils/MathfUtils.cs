using Unity.Mathematics;
using UnityEngine;

public static partial class Utils
{
    static class MathfUtils
    {
        public static float2 ToFloat2(Vector2 vector)
        {
            return new float2(vector.x, vector.y);
        }

        public static float2 ToFloat2(Vector3 vector)
        {
            return new float2(vector.x, vector.y);
        }


        public static float3 ToFloat3(Vector2 vector)
        {
            return new float3(vector.x, vector.y, 0);
        }

        public static float3 ToFloat3(Vector3 vector)
        {
            return new float3(vector.x, vector.y, vector.z);
        }

        public static bool IsEqual(float3 origin, float3 other)
        {
            return origin.x == other.x && origin.y == other.y && origin.z == other.z;
        }
    }
}
