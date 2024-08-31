namespace Src.Backend
{
    public static class Utils
    {
        public static bool IsOdd(uint value)
        {
            return (value & 1) == 0;
        }
    }
}