using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movie : MonoBehaviour
{
    string titleID;
    string averageRating;
    string title;

    public Movie(string titleID, string averageRating, string title)
    {
        this.titleID = titleID;
        this.averageRating = averageRating;
        this.title = title;
    }
}
