using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtils
{
    public static void Log(object message)
    {
#if !UNITY_EDITOR
        return;
#endif
        Debug.Log(message);
    }

    public static void LogWarning(object message)
    {
#if !UNITY_EDITOR
        return;
#endif
        Debug.LogWarning(message);
    }

    public static void LogExpection(Exception exception)
    {
#if !UNITY_EDITOR
        return;
#endif
        Debug.LogException(exception);
    }
}
