using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Extensions
/// </summary>
public static class MyOtherExtensions
{
    /// <summary>
    /// Extension used to join an Ineumerable of strings
    /// </summary>
    /// <param name="collection"></param>
    /// <returns>Returns a string from an IEnumerable of strings</returns>
    public static string JoinToString(this IEnumerable<string> collection)
    {
        // Join IEnumerable of strings with nothing as a separator
        return string.Join(String.Empty, collection);
    }

    /// <summary>
    /// Extension that tries to Parse a string to short
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a nullable short</returns>
    public static short? TryParseThisShort(this string field)
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Auxiliary variable
            short aux;

            // Try Parse our field as a nullable short
            // If we can't parse it as a short make it null
            return short.TryParse(field, out aux)
                ? (short?)aux
                : null;
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}' short?, but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Extension that tries to Parse a string to bool
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a boolean</returns>
    public static bool TryParseThisBool(this string field)
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // If our field is 0 make our bool false
            if (field == "0")
                return false;
            // If our field is not 0 make our bool true
            else
                return true;
        }
        catch (Exception e)
        {
            // Catch and throw an exception
            throw new InvalidOperationException(
                $"Tried to parse '{field}', but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }
}
