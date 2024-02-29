using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LoadTrialInfo : MonoBehaviour
{
    public string csvFileName = "C:\Users\sabinasrokova\Desktop\fmri_viewpoint_task\stimlist_00.csv";

    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, csvFileName);

        if (File.Exists(filePath))
        {
            List<TrialData> trialDataList = LoadCSV(filePath);

            // Now you can use the loaded data for your trials
            foreach (TrialData trialData in trialDataList)
            {
                // Access variables for each trial
                string room = trialData.Room;
                float start = trialData.Start;
                float end = trialData.End;
                string condition = trialData.Condition;
                string moveItem = trialData.MoveItem;

                // Perform your task logic with these variables
                Debug.Log($"Room: {room}, Start: {start}, End: {end}, Condition: {condition}, MoveItem: {moveItem}");
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