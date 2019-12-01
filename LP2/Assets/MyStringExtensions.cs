using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class MyStringExtensions
{
    public static String TrimSpaces(this String input)
    {
        StringBuilder newString = new StringBuilder();
        bool isWhitespace = false;

        for (int i = 0; i < input.Length; i++)
        {
            if (Char.IsWhiteSpace(input[i]))
            {
                if (isWhitespace)
                {
                    continue;
                }

                isWhitespace = true;
            }
            else
            {
                isWhitespace = false;
            }

            newString.Append(input[i]);
        }

        return newString.ToString();
    }

   public static string FormatString(this String input, string newString)
   {
        string output = 
            string.Join(newString, 
            input.Split(new char[] { ' ' }, 
            StringSplitOptions.RemoveEmptyEntries));

        return output;
   }
}
