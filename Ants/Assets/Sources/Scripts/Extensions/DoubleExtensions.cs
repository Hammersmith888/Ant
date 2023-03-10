using System;


public static class DoubleExtensions
{
    public static int ToInt32(this double value)
    {
        return Convert.ToInt32(value);
    }

    public static float ToSingle(this double value)
    {
        return Convert.ToSingle(value);
    }
}