using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseManager : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    [SerializeField]
    private Text infoTextBox;

    [SerializeField]
    private Text idTextBox;

    [SerializeField]
    private Text typeTextBox;

    [SerializeField]
    private Text titleTextBox;

    [SerializeField]
    private Text yearsTextBox;

    [SerializeField]
    private Text adultTextBox;

    [SerializeField]
    private Text genreTextBox;

    [SerializeField]
    private Dropdown typesDropDown;

    [SerializeField]
    private Dropdown genresDropDown;

    private const string appName = "MyIMDBSearcher";
    private const string fileTitleBasics = "title.basics.tsv.gz";
    private const int numMaxEntriesOnScreen = 40;

    private int numEntriesOnScreen;
    private int page;
    private int pages;

    private ICollection<Entry> entries;
    private ISet<string> types;
    private ISet<string> genres;

    private Entry[] results;

    private void Start()
    {     
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (inputField.IsActive())
            {
                if (inputField.text != null &&
                    inputField.text != "")
                {
                    results = SelectEntries(inputField.text);
                    FilterByType();
                    FilterByGenre();
                    numEntriesOnScreen = 0;
                    page = 0;
                    pages = Mathf.CeilToInt(CountResults(results) / 40);
                    PrintResults(results);
                }
                else
                {
                    infoTextBox.text = "";

                    infoTextBox.text += $"\n\t\tNo results found.\n\n";
                }
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

    private void Initialize()
    {
        int numEntries = 0;

        types = new HashSet<string>();
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
        typesDropDown.options.Clear();

        typesDropDown.options.Add(new Dropdown.OptionData("All"));

        foreach (string type in types)
        {
            typesDropDown.options.Add(new Dropdown.OptionData(type));
        }

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

    private void PrintResults(Entry[] results)
    {
        infoTextBox.text = "";
        idTextBox.text = "\n\n";
        typeTextBox.text = "\n\n";
        titleTextBox.text = "\n\n";
        yearsTextBox.text = "\n\n";
        adultTextBox.text = "\n\n";
        genreTextBox.text = "\n\n";

        infoTextBox.text += $"\t{CountResults(results)} results found!";

        infoTextBox.text += $"\t-\tPage {page} of {pages}\n\n";

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

            if (entry.IsAdultOnly)
                adultTextBox.text +=
                    $"Adult Only";
            else
                adultTextBox.text +=
                    $"Everyone";

            adultTextBox.text += "\n";

            foreach (string genre in entry.Genres)
            {
                if (!firstGenre) 
                    genreTextBox.text += " / ";

                genreTextBox.text += $"{genre}";

                firstGenre = false;
            }

            if (entry.Genres.JoinToString() == "")
                genreTextBox.text += $"None";

            genreTextBox.text += "\n";
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

        // Add valid genres to our genres list
        AddType(entryType);

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

    private void AddGenres(ICollection<string> entryGenres)
    {
        foreach (string genre in entryGenres)
            genres.Add(genre);
    }

    private void AddType(string type)
    {
        types.Add(type);
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

                where entry
                .MainTitle
                .ToLower()
                .Contains(input.ToLower())

                select entry)
                .ToArray();
    }

    private int CountResults(Entry[] results)
    {
        return results.Count();
    }
}
