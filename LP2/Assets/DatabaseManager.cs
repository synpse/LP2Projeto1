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

    private Dictionary<string, string[]> dbDict;

    private Entry[] results;

    private void Start()
    {     
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(inputField.text != null)
            {
                results = SelectEntries(inputField.text);
                PrintResults(results);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            numEntriesOnScreen += numMaxEntriesOnScreen;
            PrintResults(results);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            numEntriesOnScreen -= numMaxEntriesOnScreen;
            PrintResults(results);
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

        results = SelectEntries(inputField.text);

        PrintResults(results);

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
            Debug.LogException(e);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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

        textBox.text += $"\n\t\t{CountResults(results)} results found!\n";

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
                //$"({entry.Year?.ToString() ?? "unknown year"}): ";
                $"({entry.Year}): ";

            foreach (string genre in entry.Genres)
            {
                if (!firstGenre) textBox.text += "/ ";
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
            Entry[] results;

            results = (
                from entry in entries
                select entry)
                .OrderBy(entry => entry.Title)
                .ToArray();

            PrintResults(results);         
            
        }catch(Exception e)
        {
            Debug.Log("OrderByName ERROR - " + e);
        }
        
    }

    public void OrderByGenre()
    {
        try
        {
            Entry[] results;

            results = (
                from entry in entries
                select entry)
                .OrderBy(entry => entry.Genres)
                .ToArray();

            PrintResults(results);
            
        }
        catch (Exception e)
        {
            Debug.Log("OrderByGenre ERROR - " + e);
        }

        Debug.Log("Is Not Working");
    }

    public void OrderByYear()
    {
        try
        {
            Entry[] results;

            results = (
                from entry in entries
                select entry)
                .OrderBy(entry => entry.Year.ToString())
                .ToArray();

            PrintResults(results);

        }
        catch (Exception e)
        {
            Debug.Log("OrderByYear ERROR - " + e);
        }
        
    }

    private void LineToTitle(string line)
    {
        string[] fields = line.Split('\t');
        string[] titleGenres = fields[8].Split(',');
        ICollection<string> entryGenres = new List<string>();
        short? year = TryParse(fields);

        CheckInvalidGenres(entryGenres, titleGenres);
        AddGenres(entryGenres);
        AddNewEntry(fields, year, entryGenres);
    }

    public short? TryParse(string[] fields)
    {
        try
        {
            short aux;

            return short.TryParse(fields[5], out aux)
                ? (short?)aux
                : null;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Tried to parse '{fields[5]}', but got exception '{e.Message}'"
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
