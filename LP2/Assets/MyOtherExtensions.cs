using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Extensions
/// </summary>
public static class MyOtherExtensions
{
    /// <summary>
    /// Extension used to check is an array of strings has some specific value
    /// </summary>
    /// <param name="values"></param>
    /// <param name="searchedValue"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Extension that is used to remove duplicates from a dictionary
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Extension used to join an Ineumerable of strings
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static string JoinToString(this IEnumerable<string> collection)
    {
        return string.Join(String.Empty, collection);
    }

    /// <summary>
    /// Extension that tries to Parse a string to short
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static short? TryParseThisShort(this string field)
    {
        try
        {
            short aux;

            return short.TryParse(field, out aux)
                ? (short?)aux
                : null;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}', but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Extension that tries to Parse a string to bool
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public static bool TryParseThisBool(this string field)
    {
        try
        {
            if (field == "0")
                return false;
            else
                return true;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}', but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }
}
