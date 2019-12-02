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

    public static Dictionary<string, string[]> RemoveDuplicates(
        this Dictionary<string, string[]> dict)
    {
        HashSet<string[]> knownValues =
            new HashSet<string[]>();

        Dictionary<string, string[]> dbDictUniques =
            new Dictionary<string, string[]>();

        foreach (KeyValuePair<string, string[]> pair in dict)
        {
            if (knownValues.Add(pair.Value))
            {
                dbDictUniques.Add(pair.Key, pair.Value);
            }
        }

        return dbDictUniques;
    }
}
