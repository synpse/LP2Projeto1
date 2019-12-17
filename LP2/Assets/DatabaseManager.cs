using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class DatabaseManager : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private Text infoTextBox;

    [SerializeField]
    private Text[] buttonsText;

    [SerializeField]
    private Dropdown typesDropDown;

    [SerializeField]
    private Text minStartYears;

    [SerializeField]
    private Text maxStartYears;

    [SerializeField]
    private Text minEndYears;

    [SerializeField]
    private Text maxEndYears;

    [SerializeField]
    private Dropdown adultsOnlyDropDown;

    [SerializeField]
    private Dropdown genresDropDown;

    [SerializeField]
    private GameObject entryPanel;

    [SerializeField]
    private Toggle typesToggle;

    [SerializeField]
    private Toggle genresToggle;

    [SerializeField]
    private Dropdown typesDropDown2;

    [SerializeField]
    private Dropdown genresDropDown2;

    [SerializeField]
    private Dropdown ratingsDropDown;

    [SerializeField]
    private Button entryButton;

    [SerializeField]
    private GameObject episodesPanel;

    [SerializeField]
    private GameObject seasonsPanel;

    [SerializeField]
    private Text seasonNumber;

    [SerializeField]
    private Button[] episodeButtons;

    private Text entryPanelText;

    private const string appName = "MyIMDBSearcher";
    private const string fileTitleBasics = "title.basics.tsv.gz";
    private const string fileTitleRatings = "title.ratings.tsv.gz";
    private const string fileTitleEpisodes = "title.episode.tsv.gz";
    private const int numMaxEntriesOnScreen = 10;

    private int numEntriesOnScreen;
    private int page;
    private int pages;
    private bool isParent;
    private short? seasonSeasons;
    private short? currentSeason;

    private Dictionary<string, float?> tempRatingEntries;
    private Dictionary<string, string[]> tempEpisodes;
    private Dictionary<string, Entry> entries;
    private ISet<string> types;
    private ISet<short?> startYears;
    private ISet<short?> endYears;
    private ISet<bool> adultsOnly;
    private ISet<string> genres;
    private ISet<float?> ratings;

    private Entry[] results;
    private Entry currentSelected;
    private Entry previousSelected;

    private void Start()
    {
        GetComponents();
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (inputField.text != null &&
                inputField.text != "")
            {
                results = SelectEntries(inputField.text);
                FilterByType();
                FilterByStartYear();
                FilterByEndYear();
                FilterByAdultOnly();
                FilterByGenre();
                FilterByRating();
                numEntriesOnScreen = 0;
                page = 0;

                float result = CountResults(results) % numMaxEntriesOnScreen;

                if (result == 0)
                    pages = Mathf.CeilToInt(
                        CountResults(results) / numMaxEntriesOnScreen);
                else
                    pages = Mathf.CeilToInt(
                        CountResults(results) / numMaxEntriesOnScreen) + 1;

                PrintResults(results);
            }
        }
    }

    private void GetComponents()
    {
        GameObject[] tempObj = 
            GameObject.FindGameObjectsWithTag("ResultButton");

        buttonsText = new Text[tempObj.Length];
        for (int i = 0; i < tempObj.Length; i++)
            buttonsText[i] = tempObj[i].GetComponent<Text>();

        entryPanelText = entryPanel.GetComponentInChildren<Text>();

        tempObj =
            GameObject.FindGameObjectsWithTag("EpisodeButton");

        episodeButtons = new Button[tempObj.Length];
        for (int i = 0; i < tempObj.Length; i++)
            episodeButtons[i] = tempObj[i].GetComponent<Button>();
    }

    private void Initialize()
    {
        types = new HashSet<string>();
        startYears = new HashSet<short?>();
        endYears = new HashSet<short?>();
        adultsOnly = new HashSet<bool>();
        genres = new HashSet<string>();
        ratings = new HashSet<float?>();

        string directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
            appName);

        string fileTitleBasicsFull = Path.Combine(
            directoryPath, 
            fileTitleBasics);

        string fileTitleRatingsFull = Path.Combine(
            directoryPath,
            fileTitleRatings);

        string fileTitleEpisodesFull = Path.Combine(
            directoryPath,
            fileTitleEpisodes);

        tempRatingEntries = new Dictionary<string, float?>();

        tempEpisodes = new Dictionary<string, string[]>();

        entries = new Dictionary<string, Entry>();

        Reader(fileTitleRatingsFull, LineToRating);

        Reader(fileTitleEpisodesFull, LineToEpisode);

        Reader(fileTitleBasicsFull, LineToEntry);

        AssignDropdownValues();

        CleanUp();

        entryPanel.SetActive(false);

        //inputField.onValueChanged.AddListener(
        //delegate {ValueChangeCheck();});
    }

    private void AssignDropdownValues()
    {
        // types
        typesDropDown.options.Clear();

        foreach (string type in types)
        {
            typesDropDown.options.Add(
                new Dropdown.OptionData(type));
        }

        typesDropDown.RefreshShownValue();

        typesDropDown2.options.Clear();

        foreach (string type in types)
        {
            typesDropDown2.options.Add(
                new Dropdown.OptionData(type));
        }

        typesDropDown2.RefreshShownValue();

        // adults only
        adultsOnlyDropDown.options.Clear();

        foreach (bool adultOnly in adultsOnly)
        {
            adultsOnlyDropDown.options.Add(
                new Dropdown.OptionData(adultOnly.ToString()));
        }

        adultsOnlyDropDown.RefreshShownValue();

        // genres
        genresDropDown.options.Clear();

        genresDropDown.options.Add(new Dropdown.OptionData("All"));

        foreach (string genre in genres)
        {
            genresDropDown.options.Add(new Dropdown.OptionData(genre));
        }

        genresDropDown.RefreshShownValue();

        genresDropDown2.options.Clear();

        genresDropDown2.options.Add(new Dropdown.OptionData("All"));

        foreach (string genre in genres)
        {
            genresDropDown2.options.Add(new Dropdown.OptionData(genre));
        }

        genresDropDown2.RefreshShownValue();

        // ratings

        ratingsDropDown.options.Clear();

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

        ratingsDropDown.RefreshShownValue();
    }

    private void CleanUp()
    {
        tempRatingEntries.Clear();
        tempEpisodes.Clear();
        GC.Collect();
    }

    private void Reader(
        string file, 
        Action<string> lineAction)
    {
        FileStream fs = null;
        GZipStream gzs = null;
        StreamReader sr = null;

        try
        {
            fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            gzs = new GZipStream(fs, CompressionMode.Decompress);
            sr = new StreamReader(gzs);

            string line;

            sr.ReadLine();

            while ((line = sr.ReadLine()) != null)
            {
                lineAction.Invoke(line);
            }
        }
        catch (FileNotFoundException e)
        {
            Debug.LogWarning($"FILE NOT FOUND! " +
                $"\n{e}");
        }
        catch (IOException e)
        {
            Debug.LogError(e);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }

            if (gzs != null)
            {
                gzs.Close();
            }

            if (sr != null)
            {
                sr.Close();
            }
        }
    }

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

    private void PrintEntryInPanel()
    {
        if (currentSelected != null)
        {
            // Open entry info panel
            OpenPanel();

            bool firstGenre = true;

            entryPanelText.text = "";

            if (currentSelected.Type == "tvSeries")
            {
                if (previousSelected != null)
                    if (!entryButton.gameObject.activeInHierarchy)
                        entryButton.gameObject.SetActive(true);
                else
                    if (entryButton.gameObject.activeInHierarchy)
                        entryButton.gameObject.SetActive(false);

                if (!episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(true);

                if (!seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(true);

                LoadEpisodesPanel();

                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                entryPanelText.text += "\n\n";

                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                entryPanelText.text += "\n\n";

                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";
            }
            else if (currentSelected.Type == "tvEpisode")
            {
                if (!entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(true);

                if (episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(false);

                if (seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(false);

                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tSeries: " +
                    $"{entries[currentSelected.ParentID].MainTitle}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tSeason: {currentSelected.SeasonNumber}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tEpisode: {currentSelected.EpisodeNumber}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                entryPanelText.text += "\n\n";

                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                entryPanelText.text += "\n\n";

                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";
            }
            else
            {
                if (entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(false);

                if (episodesPanel.gameObject.activeInHierarchy)
                    episodesPanel.gameObject.SetActive(false);

                if (seasonsPanel.gameObject.activeInHierarchy)
                    seasonsPanel.gameObject.SetActive(false);

                entryPanelText.text +=
                    $"\t\t{currentSelected.MainTitle}";

                if (currentSelected.SecondaryTitle !=
                    currentSelected.MainTitle)
                    entryPanelText.text +=
                        $":\t{currentSelected.SecondaryTitle}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tEntry Type: {currentSelected.Type}";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                    $"\t\tYear: " +
                    $"{currentSelected.StartYear?.ToString() ?? "Unknown Year"}";

                if (currentSelected.EndYear != null)
                    entryPanelText.text +=
                        $" - {currentSelected.EndYear?.ToString()}";

                entryPanelText.text += "\n\n";

                if (currentSelected.IsAdultOnly)
                    entryPanelText.text +=
                        $"\t\tAudience: Adult Only";
                else
                    entryPanelText.text +=
                        $"\t\tAudience: Everyone";

                entryPanelText.text += "\n\n";

                foreach (string genre in currentSelected.Genres)
                {
                    if (!firstGenre)
                        entryPanelText.text += " / ";
                    else
                        entryPanelText.text += "\t\tGenres: ";

                    entryPanelText.text += $"{genre}";

                    firstGenre = false;
                }

                if (currentSelected.Genres.JoinToString() == "")
                    entryPanelText.text += $"\t\tNone";

                entryPanelText.text += "\n\n";

                if (currentSelected.RuntimeMinutes != null)
                    entryPanelText.text +=
                        $"\t\tRuntime: {currentSelected.RuntimeMinutes} min";
                else
                    entryPanelText.text +=
                        $"\t\tRuntime: Unknown";

                entryPanelText.text += "\n\n";

                entryPanelText.text +=
                        $"\t\tRating: {currentSelected.Rating}";
            }
        }
    }

    public void ToParent()
    {
        if (isParent)
        {
            if (previousSelected != null)
            {
                currentSelected = previousSelected;
                PrintEntryInPanel();

                entryButton.GetComponentInChildren<Text>().text =
                    $"See Parent Series";

                isParent = false;
            }
            else
            {
                if (entryButton.gameObject.activeInHierarchy)
                    entryButton.gameObject.SetActive(false);
            }
        }
        else
        {

            previousSelected = currentSelected;

            if (currentSelected.ParentID != null)
                currentSelected = entries[currentSelected.ParentID];

            PrintEntryInPanel();

            entryButton.GetComponentInChildren<Text>().text =
                $"Go Back";

            isParent = true;
        }
    }

    private void LoadEpisodesPanel()
    {
        LoadSeasons();

        currentSeason = 1;

        seasonNumber.text = $"Season {currentSeason}";

        LoadEpisodes(currentSeason);
    }

    private void LoadSeasons()
    {
        seasonSeasons = null;

        foreach (Entry entry in entries.Values)
        {
            if (entry.ParentID == currentSelected.ID)
            {
                if (seasonSeasons == null || 
                    entry.SeasonNumber > seasonSeasons)
                {
                    seasonSeasons = entry.SeasonNumber;
                }
            }
        }
    }

    public void LoadEpisode(Text buttonText)
    {
        if (buttonText.text != "")
        {
            foreach (Entry entry in entries.Values)
            {
                if (entry.MainTitle == buttonText.text)
                {
                    previousSelected = currentSelected;
                    Debug.Log(currentSelected.ToString());
                    currentSelected = entry;
                    PrintEntryInPanel();

                    entryButton.GetComponentInChildren<Text>().text =
                        $"See Parent Series";

                    isParent = true;

                    break;
                }
            }
        }
    }

    private void LoadEpisodes(short? season)
    {
        List<Entry> episodes = new List<Entry>();
        int i = 0;

        CleanEpisodes();

        foreach (Entry entry in entries.Values)
        {
            if (entry.ParentID == currentSelected.ID &&
                entry.SeasonNumber == season)
            {
                episodes.Add(entry);

                episodeButtons[i].GetComponentInChildren<Text>().text = 
                episodes[i].MainTitle;

                i++;
            }
        }
    }

    private void CleanEpisodes()
    {
        foreach (Button button in episodeButtons)
        {
            button.GetComponentInChildren<Text>().text = "";
        }
    }

    public void PreviousSeason()
    {
        if (currentSeason > 1)
        {
            currentSeason--;
            seasonNumber.text = $"Season {currentSeason}";
            LoadEpisodes(currentSeason);
        }
    }

    public void NextSeason()
    {
        if (currentSeason < seasonSeasons)
        {
            currentSeason++;
            seasonNumber.text = $"Season {currentSeason}";
            LoadEpisodes(currentSeason);
        }
    }

    public void OpenPanel()
    {
        entryPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        entryPanel.SetActive(false);

        entryButton.GetComponentInChildren<Text>().text =
            $"See Parent Series";

        previousSelected = null;

        isParent = false;
    }

    public void PreviousPage()
    {
        if (numEntriesOnScreen > 0)
        {
            numEntriesOnScreen -= numMaxEntriesOnScreen;
            page--;
            PrintResults(results);
        }
    }

    public void NextPage()
    {
        if (results != null)
        {
            if (numEntriesOnScreen < results.Length - numMaxEntriesOnScreen)
            {
                numEntriesOnScreen += numMaxEntriesOnScreen;
                page++;
                PrintResults(results);
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void EnableTypesDropdown()
    {
        typesDropDown2.gameObject.SetActive(!typesDropDown2.IsActive());
    }

    public void EnableGenresDropdown()
    {
        genresDropDown2.gameObject.SetActive(!genresDropDown2.IsActive());
    }

    private void PrintResults(Entry[] results)
    {
        infoTextBox.text = "";

        foreach (Text buttonText in buttonsText)
        {
            buttonText.text = "";
        }

        infoTextBox.text += $"\t{CountResults(results)} results found!";

        infoTextBox.text += $"\t-\tPage {page + 1} of {pages}";

        int r = 0;

        // Mostrar próximos 10
        for (int i = numEntriesOnScreen;
        i < numEntriesOnScreen + numMaxEntriesOnScreen
            && i < results.Length;
        i++)
        {
            // Usar para melhorar a forma como mostramos os géneros
            bool firstGenre = true;

            // Obter titulo atual
            Entry entry = results[i];

            buttonsText[r].text +=
                $"{entry.MainTitle}";

            if (entry.SecondaryTitle != entry.MainTitle)
                buttonsText[r].text +=
                    $":\t{entry.SecondaryTitle}";

            buttonsText[r].text += "\t\t|\t\t";

            buttonsText[r].text +=
                    $"{entry.StartYear?.ToString() ?? "Unknown Year"}";

            if (entry.EndYear != null)
                buttonsText[r].text +=
                    $" - {entry.EndYear?.ToString()}";

            buttonsText[r].text += "\t\t|\t\t";

            foreach (string genre in entry.Genres)
            {
                if (!firstGenre)
                    buttonsText[r].text += " / ";

                buttonsText[r].text += $"{genre}";

                firstGenre = false;
            }

            if (entry.Genres.JoinToString() == "")
                buttonsText[r].text += $"None";

            r++;
        }
    }

    public void FilterByType()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByGenre ERROR: " + e);
        }
    }

    public void FilterByStartYear()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByStartYear ERROR: " + e);
        }
    }

    public void FilterByEndYear()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByEndYear ERROR: " + e);
        }
    }

    public void FilterByAdultOnly()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByAdultsOnly ERROR: " + e);
        }
    }

    public void FilterByGenre()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByGenre ERROR: " + e);
        }
    }

    public void FilterByRating()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("FilterByAdultsOnly ERROR: " + e);
        }
    }

    public void OrderByType()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByType ERROR: " + e);
        }
    }

    public void OrderByName()
    {
        try
        {
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
        catch(Exception e)
        {
            Debug.LogError("OrderByName ERROR: " + e);
        }      
    }

    public void OrderByAdultOnly()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByAdultOnly ERROR: " + e);
        }
    }

    public void OrderByStartYear()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByStartYear ERROR: " + e);
        }
    }

    public void OrderByEndYear()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByEndYear ERROR: " + e);
        }
    }

    public void OrderByGenre()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByGenre ERROR: " + e);
        }
    }

    public void OrderByRating()
    {
        try
        {
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
        catch (Exception e)
        {
            Debug.LogError("OrderByRating ERROR: " + e);
        }
    }

    private void LineToEntry(string line)
    {
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
        string[] entryTitleGenres = fields[8].Split(',');
        ICollection<string> entryGenres = new List<string>();

        // Rating
        float? entryRating = null;

        if (tempRatingEntries.ContainsKey(entryID))
            entryRating = tempRatingEntries[entryID];

        // ParentID
        string entryParentID = null;

        // Season Number
        short? entrySeasonNumber = null;

        // Episode Number
        short? entryEpisodeNumber = null;

        if (tempEpisodes.ContainsKey(entryID))
        {
            entryParentID =
                tempEpisodes[entryID].GetValue(1).ToString();

            entrySeasonNumber =
                TryParseShort(tempEpisodes[entryID].GetValue(2).ToString());

            entryEpisodeNumber =
                TryParseShort(tempEpisodes[entryID].GetValue(3).ToString());
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

    private void LineToRating(string line)
    {
        string[] fields = line.Split('\t');

        // ID
        string entryID = fields[0];

        // Rating
        float? entryRating = TryParseFloat(fields[1]);

        AddRating(entryRating);

        tempRatingEntries.Add(entryID, entryRating);
    }

    private void LineToEpisode(string line)
    {
        string[] fields = line.Split('\t');

        tempEpisodes.Add(fields[0], fields);
    }

    public short? TryParseShort(string field)
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
                $"Tried to parse '{field}' as short?, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    public float? TryParseFloat(string field)
    {
        try
        {
            float aux;
            NumberStyles style = 
                NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
            CultureInfo culture = 
                CultureInfo.CreateSpecificCulture("en-US");

            return float.TryParse(field, style, culture, out aux)
                ? (float?)aux
                : null;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{field}' as float?, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    public bool TryParseBool(string field)
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
                $"Tried to parse '{field}' as bool, " +
                $"but got exception '{e.Message}'"
                + $" with this stack trace: {e.StackTrace}");
        }
    }

    private void CheckInvalidGenres(
        ICollection<string> entryGenres, 
        string[] titleGenres)
    {
        foreach (string genre in titleGenres)
            if (genre != null && genre.Length > 0 && genre != @"\N")
                entryGenres.Add(genre);
    }

    private void AddType(string type)
    {
        types.Add(type);
    }

    private void AddStartYear(short? startYear)
    {
        startYears.Add(startYear);
    }

    private void AddEndYear(short? endYear)
    {
        endYears.Add(endYear);
    }

    private void AddAdultOnly(bool adultOnly)
    {
        adultsOnly.Add(adultOnly);
    }

    private void AddGenres(ICollection<string> entryGenres)
    {
        foreach (string genre in entryGenres)
            genres.Add(genre);
    }
    private void AddRating(float? rating)
    {
        ratings.Add(rating);
    }


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
        short? entrySeasonNumber,
        short? entryEpisodeNumber)
    {
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

        entries.Add(entryID, entry);
    }

    private Entry[] SelectEntries(string input)
    {
        return (from entry in entries

                where (

                entry.Value.MainTitle
                .ToLower().Contains(input.ToLower()) ||

                entry.Value.SecondaryTitle
                .ToLower().Contains(input.ToLower()))

                select entry.Value)
                .ToArray();
    }

    private int CountResults(Entry[] results)
    {
        return results.Count();
    }
}
