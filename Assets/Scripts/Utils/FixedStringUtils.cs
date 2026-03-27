using Unity.Collections;

public static partial class Utils
{
    static class FixedStringUtils
    {
        public static FixedString32Bytes FixString32(string str)
        {
            if (str.Length <= 32) return new FixedString32Bytes(str);
            var subStr = str.Substring(0, 32);
            return new FixedString32Bytes(subStr);
        }

        public static FixedString64Bytes FixString64(string str)
        {
            if (str.Length <= 64) return new FixedString64Bytes(str);
            var subStr = str.Substring(0, 64);
            return new FixedString64Bytes(subStr);
        }
    }
}
