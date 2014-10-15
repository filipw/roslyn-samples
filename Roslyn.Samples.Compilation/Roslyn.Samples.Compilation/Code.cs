using System;

public class Foo
{
    public static void Bar()
    {
        var items = new int[4] { 1, 2, 3, 4 };
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}