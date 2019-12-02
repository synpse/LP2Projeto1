using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    private const string fileTitleRatings = "title.ratings.tsv.gz";

    private string directoryPath;

    private Dictionary<string, string[]> dbDict;

    private void Start()
    {     
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ValueChangeCheck();
        }
    }

    private void Initialize()
    {
        //inputField.onValueChanged.AddListener(
            //delegate {ValueChangeCheck();});

        directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), appName);

        string fileTitleBasicsFull =
            Path.Combine(directoryPath, fileTitleBasics);

        if (dbDict == null)
        {
            DecompressAndCopyToDictionary(fileTitleBasicsFull);
            //dbDict.Remove(dbDict.Keys.First());
        }

        GetAllValues();
    }

    private void ValueChangeCheck()
    {
        if (inputField.text == "" || inputField == null)
        {
            GetAllValues();
        }
        else if (inputField.text != null)
        {            
            textBox.text = "";

            int numberOfMatches = 0;

            foreach (string key in dbDict.Keys)
            {
                if (key.Contains(inputField.text))
                {
                    if (numberOfMatches < 100)
                    {
                        numberOfMatches++;
                        WriteLines(dbDict[key]);

                        break;
                    }
                }
            }

            /*
            foreach (KeyValuePair<string, string[]> entry in dbDict)
            {
                foreach (string value in entry.Value)
                {
                    if (value.Contains(inputField.text)
                            && numberOfMatches < 100)
                    {
                        numberOfMatches++;
                        WriteLines(entry.Value);

                        break;
                    }
                }
            }
            */
        }
    }

    private void WriteLines(string[] strings)
    {
        string line = null;

        foreach(string value in strings)
        {
            line += value + " - ";
        }

        textBox.text += line + "\n";
    }

    private void GetAllValues()
    {
        textBox.text = "";

        int i = 0;

        foreach (KeyValuePair<string, string[]> entry in dbDict)
        {
            if (i < 100)
            {
                foreach (string value in entry.Value)
                    textBox.text += value + " - ";

                textBox.text += "\n";
            }
            else
            {
                GC.Collect();
                break;
            }

            i++;
        }
        
    }

    private void DecompressAndCopyToDictionary(string filePath)
    {
        GZipStream gzs = null;
        StreamReader sr = null;

        try
        {
            gzs = new GZipStream(
                File.OpenRead(filePath),
                CompressionMode.Decompress);

            sr = new StreamReader(gzs);

            string[] separators = { "\t" };
            dbDict = ReadAndSplitToDictionary(sr, separators);
            dbDict.RemoveDuplicates();
        }
        catch (FileNotFoundException e)
        {
            Debug.LogWarning($"FILE NOT FOUND! " +
                $"Expected file location: {filePath}" +
                $"\nERROR: {e}");
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

    private Dictionary<string, string[]> ReadAndSplitToDictionary(
        StreamReader sr, string[] separators)
    {
        return sr.ReadAllLines()
                .Skip(1)
                .Select(line => line
                .Split(separators, StringSplitOptions.None))
                .ToDictionary(
                    items => items[1],
                    items => items
                    );
    }
}
