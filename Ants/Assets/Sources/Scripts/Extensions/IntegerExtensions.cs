using System;

public static class IntegerExtensions
{
    public static void For(this int number, Action<int> handler)
    {
        for (var i = 0; i < number; i++)
        {
            handler?.Invoke(i);
        }
    }

    public static void For(this int number, Action handler)
    {
        for (var i = 0; i < number; i++)
        {
            handler?.Invoke();
        }
    }

    public static bool ToBool(this int x)
    {
        return x >= 1;
    }
}