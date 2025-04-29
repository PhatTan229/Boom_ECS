using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridAuthoring))]

public class GridEditor : Editor
{
    //private void OnEnable()
    //{
    //    Selection.selectionChanged = () =>
    //    {
    //        var activeObj = Selection.activeGameObject;
    //        if (activeObj != null && activeObj.TryGetComponent<GridAuthoring>(out var grid))
    //        {
    //            grid.travelable = !grid.travelable;
    //        }
    //    };
    //}

    private void OnSceneGUI()
    {
        var target = (GridAuthoring)this.target;
        if(target.travelable) target.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        else target.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
    }
}
