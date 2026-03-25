using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class MapBlueprintGeneratorTool
{
    public static void GenerateBlueprint(int width, int height, string name)
    {
        if (width == 0 && height == 0)
        {
            Debug.LogError("Width or Height must be greater than 0");
            return;
        }
        var content = string.Empty;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if((x == 0 || (x == 1 && y != 1) || x == width - 1 || (x == width - 2 && y != height - 2)) 
                    && (y == 0 || (y == 1 && x != 1) || y == height - 1 || (y == height - 2 && x != width - 2)))
                {
                    if (x == width - 2 && y == 1 ||
                        x == 1 && y == height - 2) content += "1";
                    else content += "2";
                }
                else if (x != 0 && x != width - 1 && y != 0 && y != height - 1) 
                {
                    if(y % 2 == 0)
                    {
                        if (x % 2 != 0) content += "1";
                        else content += "0";
                    }
                    else
                    {
                        if (x % 2 == 0) content += "1";
                        else content += "0";
                    }
                }
                else content += "0";
                if (x != width - 1) content += ",";
            }
            content += "\n";
        }
        var path = System.IO.Path.Combine(Application.dataPath, "Map Blueprint", $"{name}.txt");
        using (FileStream fs = File.Create(path))
        {
            byte[] info = new UTF8Encoding(true).GetBytes(content);
            fs.Write(info, 0, info.Length);
        }
        AssetDatabase.Refresh();
        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);
    }
}
