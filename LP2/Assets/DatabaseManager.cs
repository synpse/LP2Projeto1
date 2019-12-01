using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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

    private string directoryPath;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
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

            for (int i = 0; i < 100; i++)
            {
                line = reader.ReadLine();

                if (i > 5)
                {
                    yield return line;
                }
            }

            /*
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
            */
        }
    }
}
