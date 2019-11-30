using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private void Start()
    {
        GetFile();
    }

    private void GetFile()
    {
        StreamReader sr = null;

        try
        {
            string appDataFolder =
                Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

            string path = $@"{appDataFolder}\..\Local\MyIMDBSearcher\data.tsv";

            sr = File.OpenText(path);

            File.ReadAllLines(path).ToList().ForEach(Console.WriteLine);
        }
        catch (FileNotFoundException e)
        {
            Debug.LogWarning(e);
        }
        catch (IOException e)
        {
            Debug.LogWarning(e);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (sr != null)
            {
                sr.Close();
            }
        }
    }
}
