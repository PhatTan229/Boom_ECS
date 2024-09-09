using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct GridPosition
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class MyGrid : MonoBehaviour
{
    public GridPosition gridPosition;
    public bool travelable;
    [SerializeField] private TextMeshPro displayTxt;

#if UNITY_EDITOR
    private void Update()
    {
        displayTxt.text = $"x = {gridPosition.x}, y = {gridPosition.y}";
    }
#endif
}
