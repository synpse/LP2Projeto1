using System.Collections.Generic;
using System.IO;

public static class MyOtherExtensions
{
    public static IEnumerable<string> ReadAllLines(this StreamReader reader)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static bool HasValue(this string[] values, string searchedValue)
    {
        foreach (string value in values)
        {
            if (value.Contains(searchedValue))
                return true;
            else
                return false;
        }

        return false;
    }
}
