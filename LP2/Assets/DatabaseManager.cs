using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private Dictionary<int ,Movie> plsWork = new Dictionary<int ,Movie>();

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

            /*List<string> kekusTestus = new List<string>();
            kekusTestus = File.ReadAllLines(path).ToList();*/         
              
            //plsWork.Add()

            List<string> kekusTestus = new List<string>();
            kekusTestus = File.ReadAllLines(path).ToList();

            foreach (string line in kekusTestus)
            {
                string[] values = line.Split(' ');
                Debug.Log(values);
            }

            /*foreach (string kek in kekusTestus)
            {
                Debug.Log(kek);
            }*/
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
