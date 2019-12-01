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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string fileTitleBasicsFull = 
                Path.Combine(directoryPath, fileTitleBasics);

            if (dbDict == null)
                DecompressAndCopyToDictionary(fileTitleBasicsFull);

            GetAllValues();
        }
    }

    private void Initialize()
    {
        inputField.onValueChanged.AddListener(delegate {ValueChangeCheck();});

        directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), appName);
    }

    private void ValueChangeCheck()
    {
        if (inputField.text != null)
        {
            textBox.text = "";

            int i = 0;

            foreach (KeyValuePair<string, string[]> entry in dbDict)
            {
                if (i > 0)
                {
                    if (i < 100)
                    {
                        if (entry.Value.HasValue(inputField.text))
                            textBox.text += entry.Value + "\n";
                    }
                    else
                    {
                        GC.Collect();
                        break;
                    }
                }

                i++;
            }
        }
        else
        {
            GetAllValues();
        }
    }

    private void GetAllValues()
    {
        textBox.text = "";

        int i = 0;

        foreach (KeyValuePair<string, string[]> entry in dbDict)
        {
            if (i > 0)
            {
                if (i < 100)
                {
                    foreach (string value in entry.Value)
                        textBox.text += value;

                    textBox.text += "\n";
                }
                else
                {
                    GC.Collect();
                    break;
                }
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

            dbDict = ReadAndFormatToDictionary(sr, "\t");
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

    private Dictionary<string, string[]> ReadAndFormatToDictionary(
        StreamReader sr, string stringToFormat)
    {
        return sr.ReadAllLines()
                .Select(line => line.Replace(stringToFormat, ",")
                .Split(','))
                .ToDictionary(
                    items => items[0],
                    items => items
                    );
    }
}
