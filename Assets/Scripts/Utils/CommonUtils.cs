using System;
using System.Collections.Generic;
using System.Linq;

public static partial class Utils
{
    static class CommonUtils
    {
        public static int[] GetUniqueRandomNumbers(int min, int max, int n)
        {
            if (n <= 0 || n > (max - min + 1))
                throw new ArgumentException("Invalid n");

            HashSet<int> result = new HashSet<int>();
            Random rand = new Random();

            while (result.Count < n)
            {
                int value = rand.Next(min, max + 1);
                result.Add(value);
            }

            return result.ToArray();
        }
    }
}
