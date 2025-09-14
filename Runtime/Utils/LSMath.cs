using System.Collections.Generic;

public static class LSMath
{
    public static int WrapIndex(int value, int min, int max)
    {
        if(value < min) return value >= max ? max-1 : value;
        return (value - min) % (max - min) + min;
    }

    public static T GetWrapped<T>(this IList<T> list, int value, int min)
    {
        return list[WrapIndex(value, min, list.Count)];
    }
}