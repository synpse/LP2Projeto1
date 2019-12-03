using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry
{
    public string Title { get; }
    public short? Year { get; }
    public IEnumerable<string> Genres { get; }

    public Entry(string Title, short? Year, IEnumerable<string> Genres)
    {
        this.Title = Title;
        this.Year = Year;
        this.Genres = Genres;
    }
}
