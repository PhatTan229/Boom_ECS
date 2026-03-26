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
        var content = GenerateContent(width, height);

        var path = System.IO.Path.Combine(Application.dataPath, "Map Blueprint", $"{name}.txt");
        using (FileStream fs = File.Create(path))
        {
            byte[] info = new UTF8Encoding(true).GetBytes(content);
            fs.Write(info, 0, info.Length);
        }
        AssetDatabase.Refresh();
        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);
    }

    private static string  GenerateContent(int width, int height)
    {
        StringBuilder sb = new StringBuilder();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int value;

                // Chỉ 4 góc = 2
                bool isCorner =
                    (row == 0 && col == 0) ||
                    (row == 0 && col == 1) ||
                    (row == 1 && col == 0) ||
                    (row == 1 && col == width - 1) ||
                    (row == 0 && col == width - 1) ||
                    (row == 0 && col == width - 2) ||
                    (row == height - 1 && col == 0) ||
                    (row == height - 1 && col == 1) ||
                    (row == height - 2 && col == width - 1) ||
                    (row == height - 2 && col == 0) ||
                    (row == height - 1 && col == width - 1) ||
                    (row == height - 1 && col == width - 2);

                if (isCorner)
                {
                    value = 2;
                }
                else
                {
                    // Pattern bên trong
                    if (row % 2 == 1)
                    {
                        value = (col % 2 == 1) ? 1 : 0;
                    }
                    else
                    {
                        value = 0;
                    }
                }

                sb.Append(value);

                if (col < width - 1)
                    sb.Append(",");
            }

            if (row < height - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string ReplaceAt(string input, int index, char newChar)
    {
        if (string.IsNullOrEmpty(input) || index < 0 || index >= input.Length)
        {
            throw new ArgumentException("Invalid input or index");
        }

        char[] chars = input.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }

}
