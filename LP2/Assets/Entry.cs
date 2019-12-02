using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry
{
    public string ID { get; set; }
    public string[] Title { get; set; }

    public Entry(string ID, string[] Title)
    {
        this.ID = ID;
        this.Title = Title;
    }
}
