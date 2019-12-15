using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry
{
    public string ID { get; }
    public string Type { get; }
    public string MainTitle { get; }
    public string SecondaryTitle { get; }
    public bool IsAdultOnly { get; }
    public short? StartYear { get; }
    public short? EndYear { get; }
    public short? RuntimeMinutes { get; }
    public IEnumerable<string> Genres { get; }
    public float? Rating { get; }
    public string ParentID { get; }
    public short? SeasonNumber { get; }
    public short? EpisodeNumber { get; }

    public Entry(
        string ID,
        string Type, 
        string MainTitle,
        string SecondaryTitle,
        bool IsAdultOnly,
        short? StartYear,
        short? EndYear,
        short? RuntimeMinutes,
        IEnumerable<string> Genres,
        float? Rating,
        string ParentID,
        short? SeasonNumber,
        short? EpisodeNumber)
    {
        this.ID = ID;
        this.Type = Type;
        this.MainTitle = MainTitle;
        this.SecondaryTitle = SecondaryTitle;
        this.IsAdultOnly = IsAdultOnly;
        this.StartYear = StartYear;
        this.EndYear = EndYear;
        this.RuntimeMinutes = RuntimeMinutes;
        this.Genres = Genres;
        this.Rating = Rating;
        this.ParentID = ParentID;
        this.SeasonNumber = SeasonNumber;
        this.EpisodeNumber = EpisodeNumber;
    }
}
