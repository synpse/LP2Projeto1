using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

/// <summary>
/// Main class of the app that searches and orders the database
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    /// <summary>
    /// InputField that is used to search for entries
    /// </summary>
    [SerializeField]
    private InputField inputField;

    /// <summary>
    /// TextBox that is used to display the info
    /// </summary>
    [SerializeField]
    private Text infoTextBox;

    /// <summary>
    /// Buttons that are used to display all entries and used to interact
    /// with them
    /// </summary>
    [SerializeField]
    private Text[] buttonsText;

    /// <summary>
    /// DropDown that is used to filter by type
    /// </summary>
    [SerializeField]
    private Dropdown typesDropDown;

    /// <summary>
    /// Textbox that is used to filter the minimum Start year
    /// </summary>
    [SerializeField]
    private Text minStartYears;

    /// <summary>
    /// Textbox that is used to filter the maximum start year
    /// </summary>
    [SerializeField]
    private Text maxStartYears;

    /// <summary>
    /// Textbox that is used to filter the minimum end year
    /// </summary>
    [SerializeField]
    private Text minEndYears;

    /// <summary>
    /// Textbox that is used to filter the maximum end year
    /// </summary>
    [SerializeField]
    private Text maxEndYears;

    /// <summary>
    /// Dropdown to filter for Adult Only or not
    /// </summary>
    [SerializeField]
    private Dropdown adultsOnlyDropDown;

    /// <summary>
    /// Dropdown to filter by genre
    /// </summary>
    [SerializeField]
    private Dropdown genresDropDown;

    /// <summary>
    /// Panel to show SubResults
    /// </summary>
    [SerializeField]
    private GameObject entryPanel;

    /// <summary>
    /// Toggle Multy Type search
    /// </summary>
    [SerializeField]
    private Toggle typesToggle;

    /// <summary>
    /// Toggles multy genre search
    /// </summary>
    [SerializeField]
    private Toggle genresToggle;

    /// <summary>
    /// Dropdown that will appear is typesToggle is active
    /// </summary>
    [SerializeField]
    private Dropdown typesDropDown2;

    /// <summary>
    /// Dropdown that will appear is genresToggle is active
    /// </summary>
    [SerializeField]
    private Dropdown genresDropDown2;

    /// <summary>
    /// Dropdown that is used to filter by Rating
    /// </summary>
    [SerializeField]
    private Dropdown ratingsDropDown;

    /// <summary>
    /// Button to show Scenes Info
    /// </summary>
    [SerializeField]
    private Button entryButton;

    /// <summary>
    /// Panel to show the episodes
    /// </summary>
    [SerializeField]
    private GameObject episodesPanel;

    /// <summary>
    /// Panel to show the seasons
    /// </summary>
    [SerializeField]
    private GameObject seasonsPanel;

    /// <summary>
    /// Text that shows the current season number
    /// </summary>
    [SerializeField]
    private Text seasonNumber;

    /// <summary>
    /// Buttons where will be shown the episodes name and is used to
    /// interact with them
    /// </summary>
    [SerializeField]
    private Button[] episodeButtons;

    /// <summary>
    /// Text to show detailed information of entries
    /// </summary>
    private Text entryPanelText;

    /// <summary>
    /// Name of the folder the files are on
    /// </summary>
    private const string appName = "MyIMDBSearcher";

    /// <summary>
    /// Basics file name
    /// </summary>
    private const string fileTitleBasics = "title.basics.tsv.gz";

    /// <summary>
    /// Ratings file name
    /// </summary>
    private const string fileTitleRatings = "title.ratings.tsv.gz";

    /// <summary>
    /// Episodes file name
    /// </summary>
    private const string fileTitleEpisodes = "title.episode.tsv.gz";

    /// <summary>
    /// Number of max entries that can be displayed on screen
    /// </summary>
    private const int numMaxEntriesOnScreen = 10;

    /// <summary>
    /// Number of entries that are currently on screen
    /// </summary>
    private int numEntriesOnScreen;

    /// <summary>
    /// Current page
    /// </summary>
    private int page;

    /// <summary>
    /// Max pages
    /// </summary>
    private int pages;

    /// <summary>
    /// Is entry a parent?
    /// </summary>
    private bool isParent;

    /// <summary>
    /// Number of seasons of a series
    /// </summary>
    private byte? seasonSeasons;

    /// <summary>
    /// Current season of series
    /// </summary>
    private byte? currentSeason;

    /// <summary>
    /// Temporary entries with rating
    /// </summary>
    private Dictionary<string, float?> tempRatingEntries;

    /// <summary>
    /// Temporary entries with episodes
    /// </summary>
    private Dictionary<string, string[]> tempEpisodes;

    /// <summary>
    /// Dictionary that stores all the entries
    /// </summary>
    private Dictionary<string, Entry> entries;

    /// <summary>
    /// ISet to hold types
    /// </summary>
    private ISet<string> types;

    /// <summary>
    /// ISet to hold start years
    /// </summary>
    private ISet<short?> startYears;

    /// <summary>
    /// ISet to hold end years
    /// </summary>
    private ISet<short?> endYears;

    /// <summary>
    /// ISet to hold adultsOnly
    /// </summary>
    private ISet<bool> adultsOnly;

    /// <summary>
    /// ISet to hold genres
    /// </summary>
    private ISet<string> genres;

    /// <summary>
    /// Array of entries that stores the results
    /// </summary>
    private Entry[] results;

    /// <summary>
    /// Current selected entry
    /// </summary>
    private Entry currentSelected;

    /// <summary>
    /// Previous selected entry
    /// </summary>
    private Entry previousSelected;

    /// <summary>
    /// Start method of the Database Manager
    /// </summary>
    private void Start()
    {
        // Get Components
        GetComponents();

        // Initialize our database manager
        Initialize();
    }

    /// <summary>
    /// Update method of the Database Manager
    /// </summary>
    private void Update()
    {
        // Wait for input in input field
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Continue only if input field text is not empty
            if (inputField.text != null &&
                inputField.text != "")
            {
                // Select entries to results array using text from input field
                results = SelectEntries(inputField.text);
                // Filter by Type
                FilterByType();
                // Filter by Start Year
                FilterByStartYear();
                // Filter by End Year
                FilterByEndYear();
                // Filter by Adult Only
                FilterByAdultOnly();
                // Filter by Genre
                FilterByGenre();
                // Filter by Rating
                FilterByRating();
                // Set entries on screen to 0
                numEntriesOnScreen = 0;
                // Set current page to 0
                page = 0;

                // Get the remainder of results size and max entries on screen
                float result = CountResults(results) % numMaxEntriesOnScreen;

                // If remainder is 0
                if (result == 0)
                    // Ceil division of results size and max entries on screen
                    pages = Mathf.CeilToInt(
                        CountResults(results) / numMaxEntriesOnScreen);
                // If remainder is not 0 
                else
                    // Add another page to results size and 
                    // max entries on screen
                    pages = Mathf.CeilToInt(
                        CountResults(results) / numMaxEntriesOnScreen) + 1;
                // Print the results
                PrintResults(results);
            }
        }
    }

    /// <summary>
    /// This method is used to GetComponents
    /// </summary>
    private void GetComponents()
    {
        // Temporary array to hold GameObjects with ResultButton tag
        GameObject[] tempObj = 
            GameObject.FindGameObjectsWithTag("ResultButton");

        // New array of Text from buttons
        buttonsText = new Text[tempObj.Length];
        // Iterate and add with GetComponent<T>()
        for (int i = 0; i < tempObj.Length; i++)
            buttonsText[i] = tempObj[i].GetComponent<Text>();

        // Get panel Text component
        entryPanelText = entryPanel.GetComponentInChildren<Text>();

        // Reuse temporary array to hold GameObjects with EpisodeButton tag
        tempObj =
            GameObject.FindGameObjectsWithTag("EpisodeButton");

        // New array of Button
        episodeButtons = new Button[tempObj.Length];
        // Iterate and add with GetComponent<T>()
        for (int i = 0; i < tempObj.Length; i++)
            episodeButtons[i] = tempObj[i].GetComponent<Button>();
    }

    /// <summary>
    /// This method is used to fetch the 3 databases
    /// </summary>
    private void Initialize()
    {
        // New HashSet<string> for types
        types = new HashSet<string>();
        // New HashSet<short?> for startYears
        startYears = new HashSet<short?>();
        // New HashSet<short?> for endYears
        endYears = new HashSet<short?>();
        // New HashSet<bool> for adultsOnly
        adultsOnly = new HashSet<bool>();
        // New HashSet<string> for genres
        genres = new HashSet<string>();

        // Hold directory path with persistant path for AppData/Local
        // and combine with local path to app folder
        string directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
            appName);

        // Hold basics file path
        // and combine with local path to folder
        string fileTitleBasicsFull = Path.Combine(
            directoryPath, 
            fileTitleBasics);

        // Hold ratings file path
        // and combine with local path to folder
        string fileTitleRatingsFull = Path.Combine(
            directoryPath,
            fileTitleRatings);

        // Hold episodes file path
        // and combine with local path to folder
        string fileTitleEpisodesFull = Path.Combine(
            directoryPath,
            fileTitleEpisodes);

        // Initialize temporary Dictionary
        tempRatingEntries = new Dictionary<string, float?>();

        // Initialize temporary Dictionary
        tempEpisodes = new Dictionary<string, string[]>();

        // Initialize entries Dictionary (main dictionary)
        entries = new Dictionary<string, Entry>();

        // Read ratings file
        Reader(fileTitleRatingsFull, LineToRating);

        // Read episodes file
        Reader(fileTitleEpisodesFull, LineToEpisode);

        // Read basics (main) file
        Reader(fileTitleBasicsFull, LineToEntry);

        // Assign unique values to dropdowns
        AssignDropdownValues();

        // Clean up what we can
        CleanUp();

        // Start with entry window disabled
        entryPanel.SetActive(false);

        //inputField.onValueChanged.AddListener(
        //delegate {ValueChangeCheck();});
    }

    /// <summary>
    /// This method is used to assign valued to the dropdowns menus
    /// </summary>
    private void AssignDropdownValues()
    {
        // Clear types dropdown
        typesDropDown.options.Clear();

        // Add types to types dropdown
        foreach (string type in types)
        {
            typesDropDown.options.Add(
                new Dropdown.OptionData(type));
        }

        // Refresh types dropdown
        typesDropDown.RefreshShownValue();

        // Clear types2 dropdown
        typesDropDown2.options.Clear();

        // Add types2 to types dropdown
        foreach (string type in types)
        {
            typesDropDown2.options.Add(
                new Dropdown.OptionData(type));
        }

        // Refresh types2 dropdown
        typesDropDown2.RefreshShownValue();

        // Clear adults only dropdown
        adultsOnlyDropDown.options.Clear();

        // Add adults only to adults only dropdown
        foreach (bool adultOnly in adultsOnly)
        {
            adultsOnlyDropDown.options.Add(
                new Dropdown.OptionData(adultOnly.ToString()));
        }

        // Refresh adults only dropdown
        adultsOnlyDropDown.RefreshShownValue();

        // Clear genres dropdown
        genresDropDown.options.Clear();

        // Add All option to genres dropdown
        genresDropDown.options.Add(new Dropdown.OptionData("All"));

        // Add genres to genres dropdown
        foreach (string genre in genres)
        {
            genresDropDown.options.Add(new Dropdown.OptionData(genre));
        }

        // Refresh genres dropdown
        genresDropDown.RefreshShownValue();

        // Clear genres2 dropdown
        genresDropDown2.options.Clear();

        // Add All option to genresw dropdown
        genresDropDown2.options.Add(new Dropdown.OptionData("All"));

        // Add genres to genres2 dropdown
        foreach (string genre in genres)
        {
            genresDropDown2.options.Add(new Dropdown.OptionData(genre));
        }

        // Refresh genres2 dropdown
        genresDropDown2.RefreshShownValue();

        // Clear ratings dropdown
        ratingsDropDown.options.Clear();

        // Add all options to ratings dropdown
        ratingsDropDown.options.Add(new Dropdown.OptionData("All"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("10"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("9+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("8+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("7+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("6+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("5+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("4+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("3+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("2+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("1+"));
        ratingsDropDown.options.Add(new Dropdown.OptionData("Unknown"));

        // Refresh ratings dropdown
        ratingsDropDown.RefreshShownValue();
    }

    /// <summary>
    /// Clears the temporary dictionaries and force calls garbage Collector
    /// </summary>
    private void CleanUp()
    {
        // Clear temporary rating entries
        tempRatingEntries.Clear();
        // Clear temporary rating entries
        tempEpisodes.Clear();
        // Force Garbage Collection
        GC.Collect();
    }

    /// <summary>
    /// Method used to Read the database files
    /// </summary>
    /// <param name="file"></param>
    /// <param name="lineAction"></param>
    private void Reader(
        string file, 
        Action<string> lineAction)
    {
        // Initialize FileStream as Null
        FileStream fs = null;
        // Initialize GZipStream as Null
        GZipStream gzs = null;
        // Initialize StreamReader as Null
        StreamReader sr = null;

        // Try in order to prevent bugs that break our application
        try
        {
            // Open and read our file with FileStream
            fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            // Decompress with GZipStream
            gzs = new GZipStream(fs, CompressionMode.Decompress);
            // Read with StreamReader
            sr = new StreamReader(gzs);

            // Declare line
            string line;

            // Read the first line (We don't want it)
            sr.ReadLine();

            // While there are lines to read continue and
            // assign read line to line
            while ((line = sr.ReadLine()) != null)
            {
                // Invoke lineAction with line we just read as parameter
                lineAction.Invoke(line);
            }
        }
        // Catch and throw an exception
        catch (FileNotFoundException e)
        {
            Debug.LogWarning($"FILE NOT FOUND! " +
                $"\n{e}");
        }
        // Catch and throw an exception
        catch (IOException e)
        {
            Debug.LogError(e);
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        // Finally close all streams
        finally
        {
            // Close FileStream
            if (fs != null)
            {
                fs.Close();
            }

            // Close GZipStream
            if (gzs != null)
            {
                gzs.Close();
            }

            // Close StreamReader
            if (sr != null)
            {
                sr.Close();
            }
        }
    }

    /// <summary>
    /// Method used to get info of a specific entry
    /// </summary>
    /// <param name="button"></param>
    public void GetEntryInfo(Button button)
    {
        // Get text from button child
        Text tempButtonText = button.GetComponentInChildren<Text>();

        // An index for our iterator
        int i = 0;

        // Iterator that finds entry corresponding to our button
        foreach (Text buttonText in buttonsText)
        {
            // If our iterated object text is the same as our button text
            if (buttonText.text == tempButtonText.text)
            {
                // Set our current selected entry to numMaxEntriesOnScreen
                // times the page we're currently at, plus the index
                currentSelected = results[numMaxEntriesOnScreen * page + i];
                // Break or iteration as we don't need continue
                break;
            }

            // Add one int to our index
            i++;
        }

        PrintEntryInPanel();
    }

    /// <summary>
    /// Method used to print values into the SubResults Panel
    /// </summary>
    private void PrintEntryInPanel()
    {
        if (currentSelected != null)
        {
            // Open entry info panel
            OpenPanel();

            // Declare and initialize as true
            bool firstGenre = true;

            // Set entry panel text to empty
            entryPanelText.text = "";

            // If the current selected type is a series
            if (currentSelected.Type == "tvSeries")
            {
                // Activate or deactivate entry button
                if (previousSelected != null)
                    if (!entryButton.gameObject.activeInHierarchy)
                        entryButton.gameObject.SetActive(true);
                else
                    if (entryButton.gameObject.activeInHierarchy)
                        entryButton.gameObject.SetActive(false);

                // Activate episodes panel
                if (!episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(true);

                // Activate seasons panel
                if (!seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(true);

                // Load episodes panel
                LoadEpisodesPanel();

                // Assign Main Title
                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Rating
                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Type
                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign start year
                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                // Assign End Year if it exists
                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign adult only
                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign all genres
                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                // Assign genre to none if it does not exist
                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";
            }
            // If current selected type is episode
            else if (currentSelected.Type == "tvEpisode")
            {
                // Activate entry button
                if (!entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(true);

                // Deactivate episodes panel
                if (episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(false);

                // Deactivate seasons panel
                if (seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(false);

                // Assign Main Title
                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign respective series Main Title
                entryPanelText.text +=
                    $"\t\tSeries: " +
                    $"{entries[currentSelected.ParentID].MainTitle}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign season number
                entryPanelText.text +=
                    $"\t\tSeason: {currentSelected.SeasonNumber}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign episode number
                entryPanelText.text +=
                    $"\t\tEpisode: {currentSelected.EpisodeNumber}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Rating
                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Type
                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Start Year
                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                // Assign End Year if not Null
                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign adult only
                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign genres
                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                // Assign genre to none if it does not exist
                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";
            }
            // Anything else
            else
            {
                // Deactivate entryButton
                if (entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(false);

                // Deactivate episodesPanel
                if (episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(false);

                // Deactivate seasonsPanel
                if (seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(false);

                // Assign Main Title
                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                // Assign Secondary Title if available
                if (currentSelected.SecondaryTitle !=
                    currentSelected.MainTitle)
                    entryPanelText.text +=
                        $":\t{currentSelected.SecondaryTitle}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign type
                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign start year
                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                // Assign end year if not null
                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Adult Only
                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Genres
                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                // Assign genre to none if it does not exist
                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign runtime minutes if not null
                if (currentSelected.RuntimeMinutes != null)
                    entryPanelText.text +=
                        $"\t\tRuntime: {currentSelected.RuntimeMinutes} min";
                else
                    entryPanelText.text +=
                        $"\t\tRuntime: Unknown";

                // Paragraph
                entryPanelText.text += "\n\n";

                // Assign Rating
                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";
            }
        }
    }

    /// <summary>
    /// Method used to go from an episode to is respective serie
    /// </summary>
    public void ToParent()
    {
        // If was parent
        if (isParent)
        {
            // If previousSelected is not null
            if (previousSelected != null)
            {
                // Current selected is now the previous selected
                currentSelected = previousSelected;
                // Print in panel
                PrintEntryInPanel();

                // Change entryButton text
                entryButton.GetComponentInChildren<Text>().text =
                    $"See Parent Series";

                // Not parent anymore
                isParent = false;
            }
            else
            {
                // Disable entryButton
                if (entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // Previous selected is now our current selected
            previousSelected = currentSelected;

            // If our current selected is not null
            if (currentSelected.ParentID != null)
                // Current selected is now the respective season
                currentSelected = entries[currentSelected.ParentID];

            // Print in panel
            PrintEntryInPanel();

            // Change entryButton text
            entryButton.GetComponentInChildren<Text>().text =
                $"Go Back";

            // Is now parent
            isParent = true;
        }
    }

    /// <summary>
    /// Method used to display the current season
    /// </summary>
    private void LoadEpisodesPanel()
    {
        // Load available seasons
        LoadSeasons();

        // Set current season to 1
        currentSeason = 1;

        // Set season text to currentSeason
        seasonNumber.text = $"Season {currentSeason}";

        // Load available episodes
        LoadEpisodes(currentSeason);
    }

    /// <summary>
    /// Method used to see the amount of seasons a show have
    /// </summary>
    private void LoadSeasons()
    {
        // Set seasonSeasons to null
        seasonSeasons = null;

        // Iterate over entry.Values
        foreach (Entry entry in entries.Values)
        {
            // If current entry parent id is the same
            // as currentSelected id
            if (entry.ParentID == currentSelected.ID)
            {
                // If seasonSeasons is not null or
                // current entry season number is bigger than
                // seasonSeasons current value
                if (seasonSeasons == null || 
                    entry.SeasonNumber > seasonSeasons)
                {
                    // Assign new value to seasonSeasons
                    seasonSeasons = entry.SeasonNumber;
                }
            }
        }
    }

    /// <summary>
    /// Method used to Load one episode of a specific entry
    /// </summary>
    /// <param name="buttonText"></param>
    public void LoadEpisode(Text buttonText)
    {
        // If text on buttonText is not empty
        if (buttonText.text != "")
        {
            // Iterate over entries.Values
            foreach (Entry entry in entries.Values)
            {
                // if main title of entry equal button text
                if (entry.MainTitle == buttonText.text)
                {
                    // previous selected is now the current selected
                    previousSelected = currentSelected;
                    Debug.Log(currentSelected.ToString());
                    // Current selected is now the entry
                    currentSelected = entry;
                    // Prints results in panel
                    PrintEntryInPanel();

                    // Changes button text
                    entryButton.GetComponentInChildren<Text>().text =
                        $"See Parent Series";

                    // Is now parent
                    isParent = true;

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Method the Loads all episodes of a specific entry
    /// </summary>
    /// <param name="season"></param>
    private void LoadEpisodes(short? season)
    {
        // Creates list with episodes
        List<Entry> episodes = new List<Entry>();
        // Create int i to iterate over entries
        int i = 0;

        // Clean episodes
        CleanEpisodes();

        //Iterates over entrie.value
        foreach (Entry entry in entries.Values)
        {
            // if entry.parentID equals currentSelected.ID and 
            // entry.seasonNumber equals season
            if (entry.ParentID == currentSelected.ID &&
                entry.SeasonNumber == season)
            {
                // Adds entry to List of episodes
                episodes.Add(entry);

                // Changes text in current episode button to current episode
                // MainTitle
                episodeButtons[i].GetComponentInChildren<Text>().text = 
                episodes[i].MainTitle;

                // Next Episode
                i++;
            }
        }
    }

    /// <summary>
    /// Clears all episode buttons names
    /// </summary>
    private void CleanEpisodes()
    {
        // Iterates over episodeButtons
        foreach (Button button in episodeButtons)
        {
            // Clears button name
            button.GetComponentInChildren<Text>().text = "";
        }
    }

    /// <summary>
    /// Method used to return to the previous season
    /// </summary>
    public void PreviousSeason()
    {
        // if current season is greter than 1
        if (currentSeason > 1)
        {
            // Current season decreases
            currentSeason--;

            // Displays current season number
            seasonNumber.text = $"Season {currentSeason}";
            // Loads episodes
            LoadEpisodes(currentSeason);
        }
    }

    /// <summary>
    /// Method used to advance to the next season
    /// </summary>
    public void NextSeason()
    {
        // if current season greater than seasonSeasons
        if (currentSeason < seasonSeasons)
        {
            // Increases current Season
            currentSeason++;

            // Displays current season
            seasonNumber.text = $"Season {currentSeason}";
            // Loads current season episodes
            LoadEpisodes(currentSeason);
        }
    }

    /// <summary>
    /// Method that Opens entryPanel(SubResults Panel)
    /// </summary>
    public void OpenPanel()
    {
        // Activates entry panel
        entryPanel.SetActive(true);
    }

    /// <summary>
    /// Method that closes entryPanel(SubResults Panel)
    /// </summary>
    public void ClosePanel()
    {
        // Deactivates entryPanel
        entryPanel.SetActive(false);

        // Changes entryButton text
        entryButton.GetComponentInChildren<Text>().text =
            $"See Parent Series";

        // previous selected is now nill
        previousSelected = null;
        // isParent is now false
        isParent = false;
    }

    /// <summary>
    /// Method used for when the user wants to go to the previous page
    /// </summary>
    public void PreviousPage()
    {
        //if number of entries on screen is greater than 0
        if (numEntriesOnScreen > 0)
        {
            // Subtracts numMaxEntiesOnScreen to numEntriesOnScreen
            numEntriesOnScreen -= numMaxEntriesOnScreen;
            // Decreases page
            page--;
            // Prints results
            PrintResults(results);
        }
    }

    /// <summary>
    /// Method that is used when user wants to advance for the next page
    /// </summary>
    public void NextPage()
    {
        //If results are different of null
        if (results != null)
        {
            // if numEntriesOnScreen is inferior to lengh of results minus
            // numMaxEntriesOnScreen
            if (numEntriesOnScreen < results.Length - numMaxEntriesOnScreen)
            {
                // numMaxEntriesOnScreen is added to numEntriesOnScreen
                numEntriesOnScreen += numMaxEntriesOnScreen;
                // Increases Page
                page++;
                // Prints results
                PrintResults(results);
            }
        }
    }

    /// <summary>
    /// Method used for when user wants to leave the program
    /// </summary>
    public void Quit()
    {
        // Quits application
        Application.Quit();
    }

    /// <summary>
    /// Method that enables the second type Dropdown menu
    /// </summary>
    public void EnableTypesDropdown()
    {
        // Activates or Deactivates Second type dropdown
        typesDropDown2.gameObject.SetActive(!typesDropDown2.IsActive());
    }

    /// <summary>
    /// Method that enables the second genre Dropdown menu
    /// </summary>
    public void EnableGenresDropdown()
    {
        // Activates or Deactivates Second genre dropdown
        genresDropDown2.gameObject.SetActive(!genresDropDown2.IsActive());
    }

    /// <summary>
    /// Method used to print the results
    /// </summary>
    /// <param name="results"></param>
    private void PrintResults(Entry[] results)
    {
        // Resets infoTextBox text
        infoTextBox.text = "";

        // Iterates over buttonsText
        foreach (Text buttonText in buttonsText)
        {
            // Resets button text
            buttonText.text = "";
        }

        // Displays amount of results
        infoTextBox.text += $"\t{CountResults(results)} results found!";

        // Displays number of pages
        infoTextBox.text += $"\t-\tPage {page + 1} of {pages}";

        // Creates int r
        int r = 0;

        // Shows next 10
        for (int i = numEntriesOnScreen;
        i < numEntriesOnScreen + numMaxEntriesOnScreen
            && i < results.Length;
        i++)
        {
            // Used to improve the way we display genres
            bool firstGenre = true;

            // Obter titulo atual
            Entry entry = results[i];

            // Displays on Button the title
            buttonsText[r].text +=
                $"{entry.MainTitle}";

            // If SecondaryTitle is different from MainTitle
            if (entry.SecondaryTitle != entry.MainTitle)
                buttonsText[r].text +=
                    $":\t{entry.SecondaryTitle}";

            // Separate result fields
            buttonsText[r].text += "\t\t|\t\t";

            // Assign start year
            buttonsText[r].text +=
                    $"{entry.StartYear?.ToString() ?? "Unknown Year"}";

            // Assign end year if not null
            if (entry.EndYear != null)
                buttonsText[r].text +=
                    $" - {entry.EndYear?.ToString()}";

            // Separate result fields
            buttonsText[r].text += "\t\t|\t\t";

            // Assign genres
            foreach (string genre in entry.Genres)
            {
                if (!firstGenre)
                    buttonsText[r].text += " / ";

                buttonsText[r].text += $"{genre}";

                firstGenre = false;
            }

            // Assign genre as none if there are not genres
            if (entry.Genres.JoinToString() == "")
                buttonsText[r].text += $"None";

            // Next r
            r++;
        }
    }

    /// <summary>
    /// Method that filters results by type
    /// </summary>
    public void FilterByType()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter results by type 1 and type 2
            results =
                (from result in results

                 where result
                 .Type
                 .Contains(
                     typesDropDown
                     .options[typesDropDown.value]
                     .text) ||
                 result
                 .Type
                 .Contains(
                     typesDropDown2
                     .options[typesDropDown2.value]
                     .text)

                 select result)
                .ToArray();
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByGenre ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that filters results by their start year
    /// </summary>
    public void FilterByStartYear()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter Results by start year range
            if (minStartYears.text == "" &&
                maxStartYears.text != "")
            {
                results =
                    (from result in results

                    where (
                    result.StartYear <=
                    (maxStartYears.text.TryParseThisShort()))

                    select result)
                    .ToArray();
            }
            else 
            if (maxStartYears.text == "" &&
                minStartYears.text != "")
            {
                results =
                    (from result in results

                    where (
                    result.StartYear >=
                    (minStartYears.text.TryParseThisShort()))

                    select result)
                    .ToArray();
            }
            else 
            if (minStartYears.text != "" &&
                maxStartYears.text != "")
            {
                results =
                    (from result in results

                    where (
                    (result.StartYear >=
                    (minStartYears.text.TryParseThisShort())) &&

                    (result.StartYear <=
                    (maxStartYears.text.TryParseThisShort())))

                    select result)
                    .ToArray();
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByStartYear ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that filters results by their end year
    /// </summary>
    public void FilterByEndYear()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter Results by end year range
            if (minEndYears.text == "" &&
                maxEndYears.text != "")
            {
                results =
                    (from result in results

                     where (
                     result.EndYear <=
                     (maxEndYears.text.TryParseThisShort()))

                     select result)
                    .ToArray();
            }
            else
            if (maxEndYears.text == "" &&
                minEndYears.text != "")
            {
                results =
                    (from result in results

                     where (
                     result.EndYear >=
                     (minEndYears.text.TryParseThisShort()))

                     select result)
                    .ToArray();
            }
            else
            if (minEndYears.text != "" && 
                maxEndYears.text != "")
            {
                results =
                    (from result in results

                     where (
                     (result.EndYear >=
                     (minEndYears.text.TryParseThisShort())) &&

                     (result.EndYear <=
                     (maxEndYears.text.TryParseThisShort())))

                     select result)
                    .ToArray();
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByEndYear ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that filters results by adult only
    /// </summary>
    public void FilterByAdultOnly()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter Results by adult only
            if (adultsOnlyDropDown.options
                [adultsOnlyDropDown.value].text != "All")
            {
                results =
                    (from result in results
                     
                     where result
                     .IsAdultOnly
                     .ToString()
                     .Contains(
                         adultsOnlyDropDown
                         .options[adultsOnlyDropDown.value]
                         .text)

                     select result)
                    .ToArray();
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByAdultsOnly ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to filter the results by genre
    /// </summary>
    public void FilterByGenre()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter Results by genre
            if (genresDropDown
                .options[genresDropDown.value].text != "All" &&
                genresDropDown2
                .options[genresDropDown2.value].text == "All")
            {
                results =
                    (from result in results

                    where result
                    .Genres
                    .JoinToString()
                    .Contains(
                        genresDropDown
                        .options[genresDropDown.value]
                        .text)

                    select result)
                    .ToArray();
            }
            else if (genresDropDown
                    .options[genresDropDown.value].text == "All" &&
                    genresDropDown2
                    .options[genresDropDown2.value].text != "All")
            {
                results =
                    (from result in results

                     where result
                     .Genres
                     .JoinToString()
                     .Contains(
                         genresDropDown2
                         .options[genresDropDown2.value]
                         .text)

                     select result)
                    .ToArray();
            }
            else if (genresDropDown
                    .options[genresDropDown.value].text != "All" &&
                    genresDropDown2
                    .options[genresDropDown2.value].text != "All")
            {
                results =
                    (from result in results

                     where result
                     .Genres
                     .JoinToString()
                     .Contains(
                         genresDropDown
                         .options[genresDropDown.value]
                         .text) ||
                      result
                     .Genres
                     .JoinToString()
                     .Contains(
                         genresDropDown2
                         .options[genresDropDown2.value]
                         .text)

                     select result)
                    .ToArray();
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByGenre ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to filter the results by Rating
    /// </summary>
    public void FilterByRating()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Filter Results by Rating
            if (ratingsDropDown.options
                [ratingsDropDown.value].text != "All")
            {
                if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "10")
                        results =
                            (from result in results

                             where result
                             .Rating == 10

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "9+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 9

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "8+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 8

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "7+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 7

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "6+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 6

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "5+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 5

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "4+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 4

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "3+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 3

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "2+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 2

                             select result)
                            .ToArray();

                else if (ratingsDropDown.options
                    [ratingsDropDown.value].text == "1+")
                        results =
                            (from result in results

                             where result
                             .Rating >= 1

                             select result)
                            .ToArray();
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("FilterByAdultsOnly ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that orders the results by type
    /// </summary>
    public void OrderByType()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by type if results are not null
            if (results != null)
            {
                results =
                    (from result in results
                     select result)
                    .OrderBy(result => result.Type)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByType ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that orders the results by name
    /// </summary>
    public void OrderByName()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by Name if results are not null
            if (results != null)
            {
                results =
                    (from result in results
                    select result)
                    .OrderBy(result => result.MainTitle)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }                
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByName ERROR: " + e);
        }      
    }

    /// <summary>
    /// Orders the results by if they are adult only or not
    /// </summary>
    public void OrderByAdultOnly()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by adultOnly if results are not null
            // Then reverse the results array
            if (results != null)
            {
                results =
                    (from result in results
                    select result)
                    .OrderBy(result => result.IsAdultOnly)
                    .Reverse()
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByAdultOnly ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to order the results by Start year
    /// </summary>
    public void OrderByStartYear()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by start year if results are not null
            // Then by MainTitle
            if (results != null)
            {
                results =
                    (from result in results
                     select result)
                    .OrderBy(result => result.StartYear)
                    .ThenBy(result => result.MainTitle)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByStartYear ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to order the results by end year
    /// </summary>
    public void OrderByEndYear()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by end year if results are not null
            // Then by MainTitle
            if (results != null)
            {
                results =
                    (from result in results
                     select result)
                    .OrderBy(result => result.EndYear)
                    .ThenBy(result => result.MainTitle)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByEndYear ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to order the results by genre
    /// </summary>
    public void OrderByGenre()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by genre if results are not null
            // Then by MainTitle
            if (results != null)
            {
                results =
                    (from result in results
                     select result)
                    .OrderBy(result => String.Join(
                        String.Empty, result.Genres.ToArray()))
                    .ThenBy(result => result.MainTitle)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }        
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByGenre ERROR: " + e);
        }
    }

    /// <summary>
    /// Method used to order the results by rating
    /// </summary>
    public void OrderByRating()
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Order by rating if results are not null
            // Then by MainTitle
            if (results != null)
            {
                results =
                    (from result in results
                     select result)
                    .OrderBy(result => result.Rating)
                    .ThenBy(result => result.MainTitle)
                    .ToArray();

                page = 0;
                numEntriesOnScreen = 0;

                PrintResults(results);
            }
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            Debug.LogError("OrderByRating ERROR: " + e);
        }
    }

    /// <summary>
    /// Method that returns entries that contain a desired input
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Returns an array of Entry</returns>
    private Entry[] SelectEntries(string input)
    {
        // Select entries which contain inputField text
        // in its Main Title or Secondary Title

        return (from entry in entries

                where (

                entry.Value.MainTitle
                .ToLower().Contains(input.ToLower()) ||

                entry.Value.SecondaryTitle
                .ToLower().Contains(input.ToLower()))

                select entry.Value)
                .ToArray();
    }

    /// <summary>
    /// Method called by reader in each line 
    /// to add new create all Entry fields, join temporary entries 
    /// and call AddNewEntry() method
    /// </summary>
    /// <param name="line"></param>
    private void LineToEntry(string line)
    {
        // Split to array of fields
        string[] fields = line.Split('\t');

        // ID
        string entryID = fields[0];

        // Type
        string entryType = fields[1];

        // Main Title
        string entryMainTitle = fields[2];

        // Secondary Title
        string entrySecondaryTitle = fields[3];

        // Is Adult
        bool entryIsAdult = TryParseBool(fields[4]);

        // Start Year
        short? entryStartYear = TryParseShort(fields[5]);

        // End Year
        short? entryEndYear = TryParseShort(fields[6]);

        // Runtime Minutes
        short? entryRuntimeMinutes = TryParseShort(fields[7]);

        // Genres
        // Split to array of strings
        string[] entryTitleGenres = fields[8].Split(',');
        // Make new ICollection (List) of genres
        ICollection<string> entryGenres = new List<string>();

        // Rating
        float? entryRating = null;

        // If temporary dictionary of rating entries
        // contains entryID (key)
        // Set entryRating
        if (tempRatingEntries.ContainsKey(entryID))
            entryRating = tempRatingEntries[entryID];

        // ParentID
        string entryParentID = null;

        // Season Number
        byte? entrySeasonNumber = null;

        // Episode Number
        byte? entryEpisodeNumber = null;

        // If temporary dictionary of episode entries
        // contains entryID (key)
        // Set fields
        if (tempEpisodes.ContainsKey(entryID))
        {
            // Set parent id
            entryParentID =
                tempEpisodes[entryID].GetValue(1).ToString();

            // Set season number
            entrySeasonNumber =
                TryParseByte(tempEpisodes[entryID].GetValue(2).ToString());

            // Set episode number
            entryEpisodeNumber =
                TryParseByte(tempEpisodes[entryID].GetValue(3).ToString());
        }

        // Add type to our types list
        AddType(entryType);

        // Add start year to our start years list
        AddStartYear(entryStartYear);

        // Add end year to our end years list
        AddEndYear(entryEndYear);

        // Add adult only to our adults only list
        AddAdultOnly(entryIsAdult);

        // Check for invalid genres
        CheckInvalidGenres(entryGenres, entryTitleGenres);

        // Add valid genres to our genres list
        AddGenres(entryGenres);

        // Add Entry
        AddNewEntry(
            entryID,
            entryType,
            entryMainTitle,
            entrySecondaryTitle,
            entryIsAdult,
            entryStartYear,
            entryEndYear,
            entryRuntimeMinutes,
            entryGenres,
            entryRating,
            entryParentID,
            entrySeasonNumber,
            entryEpisodeNumber);
    }

    /// <summary>
    /// Method used to add new entries
    /// </summary>
    /// <param name="entryID"></param>
    /// <param name="entryType"></param>
    /// <param name="entryMainTitle"></param>
    /// <param name="entrySecondaryTitle"></param>
    /// <param name="entryIsAdult"></param>
    /// <param name="entryStartYear"></param>
    /// <param name="entryEndYear"></param>
    /// <param name="entryRuntimeMinutes"></param>
    /// <param name="entryGenres"></param>
    /// <param name="entryRating"></param>
    /// <param name="entryParentID"></param>
    /// <param name="entrySeasonNumber"></param>
    /// <param name="entryEpisodeNumber"></param>
    private void AddNewEntry(
        string entryID,
        string entryType,
        string entryMainTitle,
        string entrySecondaryTitle,
        bool entryIsAdult,
        short? entryStartYear,
        short? entryEndYear,
        short? entryRuntimeMinutes,
        IEnumerable<string> entryGenres,
        float? entryRating,
        string entryParentID,
        byte? entrySeasonNumber,
        byte? entryEpisodeNumber)
    {
        // Make new entry with all passed parameters
        Entry entry = new Entry(
            entryID,
            entryType,
            entryMainTitle,
            entrySecondaryTitle,
            entryIsAdult,
            entryStartYear,
            entryEndYear,
            entryRuntimeMinutes,
            entryGenres.ToArray(),
            entryRating,
            entryParentID,
            entrySeasonNumber,
            entryEpisodeNumber);

        // Add entry to entries
        entries.Add(entryID, entry);
    }

    /// <summary>
    /// Method called by reader in each line 
    /// to add new temporary Rating entry
    /// </summary>
    /// <param name="line"></param>
    private void LineToRating(string line)
    {
        // Split line with tabs to array of fields
        string[] fields = line.Split('\t');

        // ID
        string entryID = fields[0];

        // Rating
        float? entryRating = TryParseFloat(fields[1]);

        // Add Temp Rating
        tempRatingEntries.Add(entryID, entryRating);
    }

    /// <summary>
    /// Method called by reader in each line 
    /// to add new temporary Episode entry
    /// </summary>
    /// <param name="line"></param>
    private void LineToEpisode(string line)
    {
        // Split line with tabs to array of fields
        string[] fields = line.Split('\t');

        // Add Temp Episode
        tempEpisodes.Add(fields[0], fields);
    }

    /// <summary>
    /// Method used try and parse a string to nullable short
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a nullable short</returns>
    private short? TryParseShort(string field)
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
                $"Tried to parse '{field}' as short?, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Method used try and parse a string to nullable byte
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a nullable byte</returns>
    private byte? TryParseByte(string field)
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Auxiliary variable
            byte aux;

            // Try Parse our field as a nullable byte
            // If we can't parse it as a byte make it null
            return byte.TryParse(field, out aux)
                ? (byte?)aux
                : null;
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}' as byte?, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Method used try and parse a string to nullable float
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a nullable float</returns>
    private float? TryParseFloat(string field)
    {
        // Try in order to prevent bugs that break our application
        try
        {
            // Auxiliary variable
            float aux;

            // Properly format
            NumberStyles style = 
                NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            CultureInfo culture = 
                CultureInfo.CreateSpecificCulture("en-US");

            // Try Parse our field as a nullable float
            // If we can't parse it as a float make it null
            return float.TryParse(field, style, culture, out aux)
                ? (float?)aux
                : null;
        }
        // Catch and throw an exception
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}' as float?, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Method used try and parse a string to boolean
    /// </summary>
    /// <param name="field"></param>
    /// <returns>Returns a boolean</returns>
    private bool TryParseBool(string field)
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
        // Catch and throw an exception
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}' as bool, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Method used to check for invalid genres on respective ICollection
    /// </summary>
    /// <param name="entryGenres"></param>
    /// <param name="titleGenres"></param>
    private void CheckInvalidGenres(
        ICollection<string> entryGenres, 
        string[] titleGenres)
    {
        // Check for valid genres
        // Iterate through genres in titleGenres
        foreach (string genre in titleGenres)
            // If genre is not null and genre is bigger than 0 and genre
            // is valid
            // Add genre to entryGenres
            if (genre != null && genre.Length > 0 && genre != @"\N")
                entryGenres.Add(genre);
    }

    /// <summary>
    /// Method used to add types to respective ISet
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private void AddType(string type)
    {
        // Add type to ISet
        types.Add(type);
    }

    /// <summary>
    /// Method used to add start years to respective ISet
    /// </summary>
    /// <param name="startYear"></param>
    /// <returns></returns>
    private void AddStartYear(short? startYear)
    {
        // Add StartYear to ISet
        startYears.Add(startYear);
    }

    /// <summary>
    /// Method used to add end years to respective ISet
    /// </summary>
    /// <param name="endYear"></param>
    /// <returns></returns>
    private void AddEndYear(short? endYear)
    {
        // Add EndYear to ISet
        endYears.Add(endYear);
    }

    /// <summary>
    /// Method used to add adultOnly to respective ISet
    /// </summary>
    /// <param name="adultOnly"></param>
    /// <returns></returns>
    private void AddAdultOnly(bool adultOnly)
    {
        // Add AdultOnly to ISet
        adultsOnly.Add(adultOnly);
    }

    /// <summary>
    /// Method used to add genres to respective ISet
    /// </summary>
    /// <param name="entryGenres"></param>
    /// <returns></returns>
    private void AddGenres(ICollection<string> entryGenres)
    {
        // Add all genres to Iset
        // Iterate over genres
        foreach (string genre in entryGenres)
            genres.Add(genre);
    }

    /// <summary>
    /// Method used to count all results
    /// </summary>
    /// <param name="results"></param>
    /// <returns>Returns an int with size of results array</returns>
    private int CountResults(Entry[] results)
    {
        // Count our results
        return results.Count();
    }
}
