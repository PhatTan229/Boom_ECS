using System;
using System.Collections.Generic;
using Unity.Entities;

public static class OnTriggerContainer
{
    public static List<IOnTrigger> container = new List<IOnTrigger>();

    public static void Subscribe(IOnTrigger trigger)
    {
        if (!container.Contains(trigger)) container.Add(trigger);
    }

    public static void Trigger(ref SystemState state, OnTriggerData data)
    {
        foreach (var item in container)
        {
            foreach (var pair in data._enter)
            {
                item.OnEnter(ref state, pair);
            }

            foreach (var pair in data._current)
            {
                item.OnStay(ref state, pair);
            }

            foreach (var pair in data._previous)
            {
                if (!data._current.Contains(pair)) item.OnExit(ref state, pair);
            }
        }       
    }
}
