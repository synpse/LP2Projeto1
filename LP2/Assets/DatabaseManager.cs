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
    private Text textBox;

    [SerializeField]
    private InputField inputField;

    private const string appName = "MyIMDBSearcher";
    private const string fileTitleBasics = "title.basics.tsv.gz";
    private const int numMaxEntriesOnScreen = 40;

    private int numEntriesOnScreen;

    private ICollection<Entry> entries;

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
            if(inputField.text != null &&
                inputField.text != "")
            {
                results = SelectEntries(inputField.text);
                numEntriesOnScreen = 0;
                PrintResults(results);
            }
            else
            {
                textBox.text = "";

                textBox.text += $"\n\t\tNo results found.\n\n";
            }
            
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (numEntriesOnScreen < results.Length)
            {
                numEntriesOnScreen += numMaxEntriesOnScreen;
                PrintResults(results);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (numEntriesOnScreen > 0)
            {
                numEntriesOnScreen -= numMaxEntriesOnScreen;
                PrintResults(results);
            }
        }
    }

    private void Initialize()
    {
        int numEntries = 0;

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

        Reader(fileTitleBasicsFull, LineToTitle);

        PrintAllGenres();

        //inputField.onValueChanged.AddListener(
        //delegate {ValueChangeCheck();});
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

            //dbDict.RemoveDuplicates();
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
        textBox.text = "";

        textBox.text += $"\n\t\t{CountResults(results)} results found!\n\n";

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

            textBox.text +=
                "\t\t* " +
                $"\"{entry.Title}\" " +
                $"({entry.Year?.ToString() ?? "Unknown Year"}): ";

            foreach (string genre in entry.Genres)
            {
                if (!firstGenre) 
                    textBox.text += "/ ";

                textBox.text += $"{genre} ";
                firstGenre = false;
            }
            textBox.text += "\n";
        }
    }

    public void OrderByName()
    {
        try
        {
            results = 
            (from result in results
             select result)
            .OrderBy(result => result.Title)
            .ToArray();

            numEntriesOnScreen = 0;

            PrintResults(results);                   
        }
        catch(Exception e)
        {
            Debug.LogError("OrderByName ERROR: " + e);
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
                .ThenBy(result => result.Title)
                .ToArray();

            numEntriesOnScreen = 0;

            PrintResults(results);           
        }
        catch (Exception e)
        {
            Debug.LogError("OrderByGenre ERROR: " + e);
        }
    }

    public void OrderByYear()
    {
        try
        {
            results =
                (from result in results
                select result)
                .OrderBy(result => result.Year)
                .ThenBy(result => result.Title)
                .ToArray();

            numEntriesOnScreen = 0;

            PrintResults(results);
        }
        catch (Exception e)
        {
            Debug.LogError("OrderByYear ERROR: " + e);
        }
        
    }

    private void LineToTitle(string line)
    {
        string[] fields = line.Split('\t');
        string[] titleGenres = fields[8].Split(',');
        ICollection<string> entryGenres = new List<string>();
        short? year = TryParse(fields[5]);

        CheckInvalidGenres(entryGenres, titleGenres);
        AddGenres(entryGenres);
        AddNewEntry(fields, year, entryGenres);
    }

    public short? TryParse(string field)
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

    private void AddNewEntry(
        string[] fields, 
        short? year, 
        ICollection<string> entryGenres)
    {
        Entry entry = new Entry(fields[2], year, entryGenres.ToArray());

        /*if (entry.Title.StartsWith("-"))
        {
            Debug.Log(entry.Title);
            //Does Nothing
        }
        else
        {
            entries.Add(entry);
        }*/

        entries.Add(entry);
    }

    private void PrintAllGenres()
    {
        foreach (string genre in genres.OrderBy(g => g))
            Debug.Log($"{genre}");
    }    

    private Entry[] SelectEntries(string input)
    {
        return (from entry in entries
                where entry.Title.ToLower().Contains(input.ToLower())
                select entry)
                .ToArray();
    }

    private int CountResults(Entry[] results)
    {
        return results.Count();
    }
}
