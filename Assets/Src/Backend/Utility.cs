using System;

namespace Src.Backend
{
    public static class Utility
    {
        public static byte[] Concatenate(byte[] array1, byte[] array2)
        {
            var result = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, result, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, result, array1.Length, array2.Length);
            return result;
        }
        
    }
}