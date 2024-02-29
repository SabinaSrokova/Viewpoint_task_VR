using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TaskLoop : MonoBehaviour
{
    public string csvFileName = "your_csv_file.csv";

    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, csvFileName);

        if (File.Exists(filePath))
        {
            List<TrialData> trialDataList = LoadCSV(filePath);

            // Set the loaded trial data in DataManager
            DataManager.Instance.SetLoadedTrialData(trialDataList);

            // Log the loaded trial data to the console
            foreach (TrialData trialData in trialDataList)
            {
                Debug.Log($"Loaded trial data - Room: {trialData.Room}, Start: {trialData.Start}, End: {trialData.End}, Condition: {trialData.Condition}, MoveItem: {trialData.MoveItem}");
            }
        }
        else
        {
            Debug.LogError("CSV file not found: " + filePath);
        }
    }

    // Define a class to represent the data for each trial
    public class TrialData
    {
        public string Room;
        public float Start;
        public float End;
        public string Condition;
        public string MoveItem;
    }

    // Function to load CSV and return a list of TrialData
    List<TrialData> LoadCSV(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        // Assuming the first line contains column headers
        string[] headers = lines[0].Split(',');

        List<TrialData> data = new List<TrialData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            TrialData entry = new TrialData();

            entry.Room = values[0];
            entry.Start = float.Parse(values[1]);
            entry.End = float.Parse(values[2]);
            entry.Condition = values[3];
            entry.MoveItem = values[4];

            data.Add(entry);
        }

        return data;
    }
}