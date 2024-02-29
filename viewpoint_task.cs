/* 
Full task script - attempt 1

I wrote this because I hope it can provide some guidance on how I imagine the task should be set up. You're free to fix is however is necessary.

-----------------------
Things that need to be done which I did not even attempt to do:

- Logging/output
- User response - left vs right trigger correspond with "Same" vs "different" 
- can we have little "S" and "D" on each bottom corner of the visual field when responses are to-be-made? OA are likely to forget which button is which.
- The walking routine - described below
- Eyetracking

Also, I did not incorporate this with the firstpersoncontroller as defined in landmarks, not sure what that entails.

*/

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class runTask : ExperimentTask
{
    public string csvFileName = "C:/Users/sabinasrokova/Desktop/fmri_viewpoint_task/stimlist_00.csv";
    // change path above accordingly. Typically in my python task scripts, I have a GUI dialog window that shows up when the task is initialized and allows me to enter the number corresponding 
    // to a given stimlist (avoids unnecessarily editing a script). I have no idea how to do that in Unity. I'll let you play around with that if you're up for it :)   

    public GameObject roomParent; // This is where the entire ALL_ROOMS parent should be dropped in the inspector so that the necessary variables can be loaded.

    [HideInInspector] public GameObject start; // I don't remember what this is lol

    private List<TrialData> trialDataList; // List to store loaded trial data
    private int currentTrialIndex = 0; // Index to keep track of the current trial


    [Header("Timing Parameters")]
    public float initialDelay = 2f;
    public float viewSceneTime = 5f;
    public float blackoutTime = 7f;
    public float endDelay = 2f;


    public void Start()
    {
        // Load CSV data at the beginning
        LoadCSVData();

        // Loop through trials
        StartCoroutine(PerformTrials());
    }

    public void Update()
    {
        // I dunno if I need this if the trial loop is handled as a coroutine.
    }

    // ------------LOAD STIMLIST--------------------
    // Recover the CSV file with the stimlist 
    public void LoadCSVData()
    {
        // Construct the full file path
        string filePath = Path.Combine(Application.dataPath, csvFileName);

        // Check if the CSV file exists
        if (File.Exists(filePath))
        {
            // Load trial data from the CSV file
            trialDataList = LoadCSV(filePath);
        }
        else
        {
            Debug.LogError("CSV file not found: " + filePath);
        }
    }

    // Return a list of TrialData
    List<TrialData> LoadCSV(string filePath)
    {
        // Read all lines from the CSV file
        string[] lines = File.ReadAllLines(filePath); //Opens a text file, reads all lines of the file into a string array, and then closes the file.

        // Assuming the first line contains column headers
        string[] headers = lines[0].Split(',');

        // List to store trial data
        List<TrialData> data = new List<TrialData>();

        // Iterate through each line in the CSV file
        for (int i = 1; i < lines.Length; i++)
        {
            // Split the line into individual values separated by comma
            string[] values = lines[i].Split(',');

            // Create a new TrialData instance
            TrialData entry = new TrialData();

            // Populate TrialData properties from CSV values
            entry.Room = values[0]; // Specifies which "room" the participant spawns in
            entry.Start = values[1]; // There are two spawn points (A and B), specifies which one they start with
            entry.End = values[2]; // Ending spawn point
            entry.Condition = values[3]; // The condition determines whether the participant has to walk to "end" spawn vs teleport (or stay in same location if start == end)
            entry.MoveItem = values[4]; // Determines whether an item is moved to a new location or not
            entry.Repeat = values[5]; // Determines which set of objects to activate
            entry.Block = values[6]; // Block

            // Add the TrialData instance to the list
            data.Add(entry);
        }

        // Return the list of trial data
        return data;
    }

    // Define a class to represent the data for each trial
    [System.Serializable]
    public class TrialData
    {
        public string Room;
        public string Start;
        public string End;
        public string Condition;
        public string MoveItem;
        public string Repeat;
    }

    // ------------MOVING THE PLAYER--------------------
    // Question: I specified an empty game object for each spawn point in the room hierarchy. I set the rotation of that spawnpoint to "point" towards the center of the room... I think? 
    // It's the Y coordinate and it matches the orientation of my cameras that I was using for the fMRI study...Is that enough or do I need to specify rotation here?
    // Maybe this can be solved with LookAt ? see below
    public void movePlayer(string spawnPointLabel)
    {
        // Find the Room (here I assume that I set the parent of all rooms in the inspector)
        // I am assuming (from what I've read online) that C# can access the variable currentTrial because the function is called within the PerformTrials() routine, which is when currenTrial is defined.
        // However, from personal experience I know that not all languages can access variables that are not explicitly entered as input variables. Change functions to include currentTrial if that's the case.
        GameObject room = roomParent.transform.Find(currentTrial.Room).gameObject;

        if (room != null)
        {
            // Find the spawn point within the room
            Transform playerSpawn = room.transform.Find("SpawnPoints/PlayerSpawn" + spawnPointLabel); // Start or End points defined in the CSV file, labeled as "A" or "B"

            // Move player to the position of the spawn point
            playerTransform.position = playerSpawn.position;

            // See the question above... Do I need to orient view towards a gameobject? Position of "SpawnPoints" is the exact center of the room, so I'm borrowing its position 
            Transform center = room.transform.Find("SpawnPoints");
            playerTransform.LookAt(center);

            // Log the task position 
            Debug.Log($"Player moved to spawn point {spawnPointLabel}"); // It will be worth adding a thingy that logs time...?
        }
        else
        {
            Debug.LogWarning($"Room not found: {currentTrial.Room}");
        }
    }

    // ------------MANIPULATING THE OBJECTS--------------------
    // Moving objects
    public void moveObject(string objectSet) // ObjectSet is defined from CSV's "Repeat" to determine which object is being moved.
    {
        // Find the Room (if currentTrial is accessible, see note above)
        GameObject room = roomParent.transform.Find(currentTrial.Room).gameObject;

        // Find spawn for object
        string objectSpawnPath = string.Concat("SpawnPoints/ObjectSpawn", objectSet); // Determine whether I'm grabbing object from set A or B, according to column "Repeat"
        Transform objectSpawn = room.transform.Find(objectSpawnPath); // Define which spawn to use, if repeat == A, then this should return "ObjectSpawnA"

        // Determine which item is to be moved (always first in the hierarchy)
        Transform allObjects = room.transform.Find(string.Concat("Objects", objectSet)) // Find the parent
        GameObject moveObject = allObjects.GetChild(0).gameObject; // Find the object which we are to move, the first item on the child list

        // Set position of moveObject to objectSpawn
        moveObject.transform.position = objectSpawn.position;
    }

    // Activating/deactivating objects
    public void showObjects(bool setActive, string objectSet) // objectSet == Repeat, to determine which set should be shown (when reusing trials)
    {
        // Find the Room 
        GameObject room = roomParent.transform.Find(currentTrial.Room).gameObject;

        // At the beginning of each trial, I want to deactivate all objects
        if (objectSet == "all")
        {
            for (char set = 'A'; set <= 'B'; set++)
            {
                string currentObjects = "Objects " + set;
                Transform objectsParent = room.transform.Find(currentObjects);

                foreach (Transform child in objectsParent)
                {
                    child.gameObject.SetActive(setActive);
                }
            }
        }
        else // After that, I will show/hide objects according to Repeat = A or B
        {
            currentObjects = string.Concat("Objects ", objectSet);
            Transform objectsParent = room.transform.Find(currentObjects);
            // Iterate through each child and activate/deactivate them
            foreach (Transform child in objectsParent)
            {
                child.gameObject.SetActive(setActive);
            }
        }
    }

    // ------------WALKING ROUTINE--------------------
    // In some trials, participant is prompted to walk along a path from point A to point B 
    // help me out here pls
    // Spawn points occur along a 4.5m circle, and they are always separated by 50 degrees. 
    // Because their locations vary across trials, this chunk of code should draw a curved line from "start" to "end" spawn points.  (OR A STRAIGHT LINE?? LET'S DO THAT FOR NOW...)
    // The end point should have a big (red?) marker on the ground, with a small arrow coming out of the marker pointing towards the center of the room (so that Pp knows how to rotate their body)
    // The participant's task is to walk along the line to the red marker. 

    // Might create a game object that is positioned along the invisible circle as a "path" ? The game object gets reused and its position is the mid point between start and end.
    // Happy to discuss the best way to do this.

    public void showMsg(bool setActive, string text)
    {
        // do I need to create a canvas in the scene?
        // if setActive == false, ignore text input
    }
    public void showMarker(bool setActive)
    {
        /// Marker positioned in "End" as defined by CSV
    }
    public void showPath(bool setActive)
    {
        /// showPath() is invoked if condition == "walk", Connecting Start & End.
    }

    // ------------TRIAL LOOP--------------------
    // Trial coroutine. I set this up as an alternative to the update method because it seemed to make more sense, but I'm not sure if the way timing is handled is correct...?
    IEnumerator PerformTrials()
    {

        while (currentTrialIndex < trialDataList.Count)
        {
            TrialData currentTrial = trialDataList[currentTrialIndex];

            // FIRST PART OF THE TRIAL
            // Step 1: participant spawns in "Start" as defined by the CSV file. Objects are not there yet. Give a delay of 2 sec.
            showObjects(false, "all");
            movePlayer(currentTrial.Start);
            yield return new WaitForSeconds(initialDelay); // If this works the way I think it does, the execution of the routine is paused for 2 seconds. Pp should still be able to look around

            // Step 2: Objects show up and participant views them for 5 sec
            showObjects(true, currentTrial.Repeat); // Repeat determines whether I should show objects A or B. 
            yield return new WaitForSeconds(viewSceneTime);

            // SECOND PART OF THE TRIAL 
            // Step 1: Objects disappear for 7 seconds. 
            showObjects(false, currentTrial.Repeat);

            // Step 2 and 3: Create red marker in the position of "end" and show message
            showMarker(true); // to be defined

            if (currentTrial.Condition == "teleport" || currentTrial.Condition == "stay")
            {
                showMsg(true, "Stay"); // to be defined (Can the function specify that msg should show up for 1s or do we need to handle timing in the routine?)
            }
            else if (currentTrial.Condition == "walk")
            {
                showMsg(true, "Walk"); // to be defined
                showPath(true); // to be defined
            }

            yield return new WaitForSeconds(1); // see above
            showMsg(false);

            // here participant gets 6 more seconds to do whatever, we can move the object in the meantime

            // Step 4: Move object
            if (currentTrial.MoveItem == "yes")
            {
                moveObject(currentTrial.Repeat); // Move from Set A or Set B
                Debug.Log($"Trial {(currentTrialIndex + 1).ToString()} - object has moved. Participant should respond DIFFERENT");
            }
            else if (currentTrial.MoveItem == "no")
            {
                Debug.Log($"Trial {(currentTrialIndex + 1).ToString()} - object has NOT moved. Participant should respond SAME");
            }

            yield return new WaitForSeconds(6);

            // Hide stuff at the end of the 7 sec period
            showMarker(false);
            if (currentTrial.Condition == "walk")
            {
                showPath(false);
            }

            // THIRD PART OF THE TRIAL
            // Items show up again, participant must determine whether an item has moved (respond different) or not (respond same)
            showObjects(true, currentTrial.Repeat);

            // also show S and D on the sides of the screen if we can, to be defined

            trackResponse(); // This needs to be defined, and I didn't try because I'm not sure how the controllers work. I also think the function needs to specify the timing.
                             // trackResponse should be initialized from the moment objects show up on the screen again, and stopped after 5 seconds. I don't think the WaitForSeconds() approach would work here
                             // unless the trackResponse() method is explicitly disabled. Also, I need to be able to gather reaction times while ensuring that the timer is set to zero beginning of Part 3.

            yield return new WaitForSeconds(viewSceneTime);

            // FOURTH PART OF THE TRIAL
            // Items disappear, Pp is in empty room for 2 sec before they spawn in the next room.
            showObjects(false, currentTrial.Repeat);

            yield return new WaitForSeconds(endDelay);

            currentTrialIndex++;
        }

        Debug.Log("All trials completed!");
    }

