using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoveKey
{
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;

    public MoveKey()
    {
        up = KeyCode.UpArrow;
        down = KeyCode.DownArrow;
        left = KeyCode.LeftArrow;
        right = KeyCode.RightArrow;
    }

    public static class KeyConfig
    { 
        public static MoveKey Move {  get; private set; } = new MoveKey();
        public static KeyCode BomButton { get; private set; } = KeyCode.Space;
    }
}
