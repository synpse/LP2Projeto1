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
    private List<string> lines;

    private const string appName = "MyIMDBSearcher";
    private const string fileTitleBasics = "title.basics.tsv.gz";
    private const string fileTitleRatings = "title.ratings.tsv.gz";

    private int x = 0;
    private int y = 100;

    private string directoryPath;

    private void Start()
    {
        
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {   
            if (x <= 0)
            {
                string fileTitleBasicsFull = Path.Combine(directoryPath, fileTitleBasics);
                DecompressAndRead(fileTitleBasicsFull);

                x = 1;
                y = 100;
            }
            else
            {
                x += 100;
                y += 100;

                string fileTitleBasicsFull = Path.Combine(directoryPath, fileTitleBasics);
                DecompressAndRead(fileTitleBasicsFull);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            x -= 100;
            y -= 100;

            if (x < 0)
            {
                x += 100;
                y += 100;
            }

            string fileTitleBasicsFull = Path.Combine(directoryPath, fileTitleBasics);
            DecompressAndRead(fileTitleBasicsFull);
        }
    }

    private void Initialize()
    {
        directoryPath = Path.Combine(
            Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), appName);
    }

    public void DecompressAndRead(string filePath)
    {
        GZipStream gzs = null;

        try
        {
            gzs = new GZipStream(
                File.OpenRead(filePath),
                CompressionMode.Decompress);
            
            textBox.text = "";
            //gzs.Position = x;
            lines = ReadLines(gzs).ToList();

            

            foreach (string line in lines)
            {
                textBox.text += line + "\n";
            }

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
        }
    }

    public IEnumerable<string> ReadLines(GZipStream gzs)
    {
        using (StreamReader reader = new StreamReader(gzs))
        {
            string line;
            int i = 0;

            while((line = reader.ReadLine()) != null && i <= y)
            {
                i++;

                line = line.Replace("\t", " ").TrimSpaces().FormatString(",");

                if (i >= x && i <= y)
                {
                    Debug.Log(line);
                    yield return line;
                }
            }         
        }
    }
}
