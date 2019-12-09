using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    private Text entryPanelText;

    private const string appName = "MyIMDBSearcher";
    private const string fileTitleBasics = "title.basics.tsv.gz";
    private const int numMaxEntriesOnScreen = 11;

    private int numEntriesOnScreen;
    private int page;
    private int pages;

    private ICollection<Entry> entries;
    private ISet<string> types;
    private ISet<short?> startYears;
    private ISet<short?> endYears;
    private ISet<bool> adultsOnly;
    private ISet<string> genres;

    private Entry[] results;

    private Entry currentSelected;

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
                numEntriesOnScreen = 0;
                page = 0;
                pages = Mathf.CeilToInt(CountResults(results) / 
                    numMaxEntriesOnScreen);
                PrintResults(results);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (numEntriesOnScreen < results.Length - numMaxEntriesOnScreen)
            {
                numEntriesOnScreen += numMaxEntriesOnScreen;
                page++;
                PrintResults(results);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (numEntriesOnScreen > 0)
            {
                numEntriesOnScreen -= numMaxEntriesOnScreen;
                page--;
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
    }

    private void Initialize()
    {
        int numEntries = 0;

        types = new HashSet<string>();
        startYears = new HashSet<short?>();
        endYears = new HashSet<short?>();
        adultsOnly = new HashSet<bool>();
        genres = new HashSet<string>();

        string directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
            appName);

        string fileTitleBasicsFull = Path.Combine(
            directoryPath, 
            fileTitleBasics);

        Reader(fileTitleBasicsFull, line => numEntries++);

        entries = new List<Entry>(numEntries);

        Reader(fileTitleBasicsFull, LineToEntry);

        AssignDropdownValues();

        //inputField.onValueChanged.AddListener(
        //delegate {ValueChangeCheck();});
    }

    private void AssignDropdownValues()
    {
        // types
        typesDropDown.options.Clear();

        typesDropDown.options.Add(
            new Dropdown.OptionData("All"));

        foreach (string type in types)
        {
            typesDropDown.options.Add(
                new Dropdown.OptionData(type));
        }

        typesDropDown.value = 0;

        // adults only
        adultsOnlyDropDown.options.Clear();

        adultsOnlyDropDown.options.Add(
            new Dropdown.OptionData("All"));

        foreach (bool adultOnly in adultsOnly)
        {
            adultsOnlyDropDown.options.Add(
                new Dropdown.OptionData(adultOnly.ToString()));
        }

        // genres
        genresDropDown.options.Clear();

        genresDropDown.options.Add(new Dropdown.OptionData("All"));

        foreach (string genre in genres)
        {
            genresDropDown.options.Add(new Dropdown.OptionData(genre));
        }
    }

    private void Reader(string file, Action<string> lineAction)
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
        // Open entry info panel
        OpenPanel();

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

        bool firstGenre = true;

        entryPanelText.text = "";

        entryPanelText.text += 
            $"\t\t{currentSelected.MainTitle}";

        if (currentSelected.SecondaryTitle != currentSelected.MainTitle)
            entryPanelText.text +=
                $":\t{currentSelected.SecondaryTitle}";

        entryPanelText.text += "\n\n";

        entryPanelText.text +=
            $"\t\tID: {currentSelected.ID}";

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
            entryPanelText.text += $"None";

        entryPanelText.text += "\n\n";

        if (currentSelected.RuntimeMinutes != null)
            entryPanelText.text +=
                $"\t\tRuntime: {currentSelected.RuntimeMinutes} min";
        else
            entryPanelText.text +=
                $"\t\tRuntime: Unknown";
    }

    public void OpenPanel()
    {
        entryPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        entryPanel.SetActive(false);
    }

    private void PrintResults(Entry[] results)
    {
        infoTextBox.text = "";

        foreach (Text buttonText in buttonsText)
        {
            buttonText.text = "";
        }

        infoTextBox.text += $"\t{CountResults(results)} results found!";

        infoTextBox.text += $"\t-\tPage {page + 1} of {pages + 1}";

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

            /*
            idTextBox.text +=
                $"{entry.ID}\n";

            typeTextBox.text +=
                $"{entry.Type}\n";

            titleTextBox.text +=
                $"\t{entry.MainTitle}";

            if (entry.SecondaryTitle != entry.MainTitle)
                titleTextBox.text +=
                    $":\t{entry.SecondaryTitle}";

            titleTextBox.text += "\n";

            yearsTextBox.text += 
                $"{entry.StartYear?.ToString() ?? "Unknown Year"}";

            if (entry.EndYear != null)
                yearsTextBox.text +=
                    $" - {entry.EndYear?.ToString()}";

            yearsTextBox.text += "\n";

            adultTextBox.text += "\n";


            runtimeTextBox.text += "\n";
            */
        }
    }

    public void FilterByType()
    {
        try
        {
            if (typesDropDown.options[typesDropDown.value].text != "All")
            {
                results =
                    (from result in results

                     where result
                     .Type
                     .Contains(
                         typesDropDown
                         .options[typesDropDown.value]
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
            if (genresDropDown.options[genresDropDown.value].text != "All")
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
        }
        catch (Exception e)
        {
            Debug.LogError("FilterByGenre ERROR: " + e);
        }
    }

    public void OrderByType()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError("OrderByType ERROR: " + e);
        }
    }

    public void OrderByName()
    {
        try
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
        catch(Exception e)
        {
            Debug.LogError("OrderByName ERROR: " + e);
        }      
    }

    public void OrderByAdultOnly()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError("OrderByAdultOnly ERROR: " + e);
        }
    }

    public void OrderByStartYear()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError("OrderByStartYear ERROR: " + e);
        }
    }

    public void OrderByEndYear()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError("OrderByEndYear ERROR: " + e);
        }
    }

    public void OrderByGenre()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError("OrderByGenre ERROR: " + e);
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
            entryGenres);
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
                $"Tried to parse '{field}', but got exception '{e.Message}'"
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
                $"Tried to parse '{field}', but got exception '{e.Message}'"
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

    private void AddNewEntry(
        string entryID,
        string entryType,
        string entryMainTitle,
        string entrySecondaryTitle,
        bool entryIsAdult,
        short? entryStartYear,
        short? entryEndYear,
        short? entryRuntimeMinutes,
        IEnumerable<string> entryGenres)
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
            entryGenres.ToArray());

        entries.Add(entry);
    }

    private Entry[] SelectEntries(string input)
    {
        return (from entry in entries

                where (

                entry.MainTitle
                .ToLower().Contains(input.ToLower()) ||

                entry.SecondaryTitle
                .ToLower().Contains(input.ToLower()))

                select entry)
                .ToArray();
    }

    private int CountResults(Entry[] results)
    {
        return results.Count();
    }
}
