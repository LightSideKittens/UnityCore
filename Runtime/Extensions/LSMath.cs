public static class LSMath
{
    public static int WrapIndex(int value, int min, int max)
    {
        if(value < min) return value;
        return (value - min) % (max - min) + min;
    }
}