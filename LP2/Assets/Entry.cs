using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to store all information of an Entry / Title
/// </summary>
public class Entry
{
    /// <summary>
    /// Stores an ID of one entry
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// Stores a Type of one entry
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Stores a MainTitle of one entry
    /// </summary>
    public string MainTitle { get; }

    /// <summary>
    /// Stores a SecondaryTitle of one entry
    /// </summary>
    public string SecondaryTitle { get; }

    /// <summary>
    /// Stores if entrie is adult only or not
    /// </summary>
    public bool IsAdultOnly { get; }

    /// <summary>
    /// Stores the start Year of the entry(can be null)
    /// </summary>
    public short? StartYear { get; }

    /// <summary>
    /// Stores end year of entry(can be null)
    /// </summary>
    public short? EndYear { get; }

    /// <summary>
    /// Stores the amount of minutes entry has(can be null)
    /// </summary>
    public short? RuntimeMinutes { get; }

    /// <summary>
    /// Stores all genres entry can have
    /// </summary>
    public IEnumerable<string> Genres { get; }

    /// <summary>
    /// stores Rating of entry(can be null)
    /// </summary>
    public float? Rating { get; }

    /// <summary>
    /// Stores ParentID of entry
    /// </summary>
    public string ParentID { get; }

    /// <summary>
    /// Stores seasonNumber of entry(can be null)
    /// </summary>
    public byte? SeasonNumber { get; }

    /// <summary>
    /// Stores episode number of entry(can be null)
    /// </summary>
    public byte? EpisodeNumber { get; }

    /// <summary>
    /// Constructor of Entry
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="Type"></param>
    /// <param name="MainTitle"></param>
    /// <param name="SecondaryTitle"></param>
    /// <param name="IsAdultOnly"></param>
    /// <param name="StartYear"></param>
    /// <param name="EndYear"></param>
    /// <param name="RuntimeMinutes"></param>
    /// <param name="Genres"></param>
    /// <param name="Rating"></param>
    /// <param name="ParentID"></param>
    /// <param name="SeasonNumber"></param>
    /// <param name="EpisodeNumber"></param>
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
        byte? SeasonNumber,
        byte? EpisodeNumber)
    {
        // Run Entry Contructor
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
