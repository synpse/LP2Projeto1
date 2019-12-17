using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to store all information
/// </summary>
public class Entry
{
    /// <summary>
    /// Stores an ID of one entrie
    /// </summary>
    public string ID { get; }

    /// <summary>
    /// Stores a Type of one entrie
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Stores a MainTitle of one entrie
    /// </summary>
    public string MainTitle { get; }

    /// <summary>
    /// Stores a SecondaryTitle of one entrie
    /// </summary>
    public string SecondaryTitle { get; }

    /// <summary>
    /// Stores if entrie is adult only or not
    /// </summary>
    public bool IsAdultOnly { get; }

    /// <summary>
    /// Stores the start Year of the entrie(can be null)
    /// </summary>
    public short? StartYear { get; }

    /// <summary>
    /// Stores end year of entrie(can be null)
    /// </summary>
    public short? EndYear { get; }

    /// <summary>
    /// Stores the amount of minutes entrie has(can be null)
    /// </summary>
    public short? RuntimeMinutes { get; }

    /// <summary>
    /// Stores all genres entrie can have
    /// </summary>
    public IEnumerable<string> Genres { get; }

    /// <summary>
    /// stores Rating of entrie(can be null)
    /// </summary>
    public float? Rating { get; }

    /// <summary>
    /// Stores ParentID of entrie
    /// </summary>
    public string ParentID { get; }

    /// <summary>
    /// Stores seasonNumber of entrie(can be null)
    /// </summary>
    public short? SeasonNumber { get; }

    /// <summary>
    /// Stores episode number of entrie(can be null)
    /// </summary>
    public short? EpisodeNumber { get; }

    /// <summary>
    /// Constructor of entrie
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
