/*
    LM PrepareRooms
       
    Script designed to prepare the order and conditions for the vr rooms. This script functionally has two parts

    First part: Reads a CSV file and loads all of the data contained inside into seperate public variables which are accesible from other scripts. 
    
    Second part: Prepares the PreparedRooms GameObject by instantiating copies of the room objects from ALL_ROOMS in order. This is the object list which will be stepped through by the task list

    Written by Matthew Watson

    Copyright (C) 2019 Michael J. Starrett

    Navigate by StarrLite (Powered by LandMarks)
    Human Spatial Cognition Laboratory
    Department of Psychology - University of Arizona   
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class LM_PrepareRooms : ExperimentTask
{
    [Header("Task-specific Properties")]
    public GameObject dummyProperty;
    public string csvFileName = "stimlist_00.csv";
    public GameObject roomParent;
    public GameObject spawnPoints;
    public GameObject transferPoints;

    [Header("Task Condition Settings")]
    public int blockValue = 20;
    public enum objectsMovedAssignment
    {
        left,
        right
    };
    public objectsMovedAssignment objectsMovedIs;

    public enum RoomShapeAssignment
    {
        CircularRooms,
        SquaredRooms
    };
    public RoomShapeAssignment roomShape;

    public enum RotationSettingAssignment
    {
        RotateTables,
        RotateRooms
    };
    public RotationSettingAssignment rotationSetting;

    public enum BlackoutAssignment
    {
        Yes,
        No
    };
    public BlackoutAssignment blackoutDuringDelay;


    // These are public for easy accesibility in other scripts. The counter will be used in order to access the right data as the trials are advanced through.
    public static List<string> trialDataList;
    public List<string> room = new List<string> { };
    public List<string> start = new List<string> { };
    public List<string> end = new List<string> { };
    public List<string> condition = new List<string> { };
    public List<string> moveItem = new List<string> { };
    public List<string> repeat = new List<string> { };
    public List<string> block = new List<string> { };
   // public List<string> hideRoom = new List<string> { };


    public override void startTask()
    {
        TASK_START();

        // LEAVE BLANK
    }


    public override void TASK_START()
    {
        if (!manager) Start();
        base.startTask();

        if (skip)
        {
            log.log("INFO    skip task    " + name, 1);
            return;
        }

        //This small loop will grab every single rigid body component in the project and enable the "Is Kinematic" toggle during runtime. This essentially turns of physics for each of these objects without removing their colliders which is important for eye tracking implementation.

        Rigidbody[] rb = Rigidbody.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[];
        foreach (Rigidbody body in rb)
        {
            body.isKinematic = true;
        }

        string filePath = csvFileName;
        if (File.Exists(filePath))
        {
            // Load trial data from the CSV file
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values[0] == "Room")
                    {
                    }
                    else
                    {

                        room.Add(values[0]);
                        start.Add(values[1]);
                        end.Add(values[2]);
                        condition.Add(values[3]);
                        moveItem.Add(values[4]);
                        repeat.Add(values[5]);
                        block.Add(values[6]);
                       // hideRoom.Add(values[7]);
                    }

                }
            }
        }
        else
        {
            Debug.LogError("CSV file not found: " + filePath);
        }


        //Fills out the PreparedRooms gameObject with the copies of rooms in order. The name bit is because Unity sticks (CLONE) at the end of the names of instantiated copies, so this is fixed before it inevitably causes problems.
        foreach (string iRoom in room)
        {
            if (iRoom != "Room")
            {
                GameObject searchRoom = Instantiate(GameObject.Find(iRoom));
                searchRoom.name = iRoom;
                if (searchRoom != null)
                {
                    searchRoom.transform.parent = roomParent.transform;
                    searchRoom.SetActive(false); //////// SS addition 2/28/2024
                }
            }

        }

        //Need to do prep for the Spawn points. The simplest way that this can work is a list of objects consisting only the correct spawn points.
        //This uses SpawnPoints and TransferPoints
        //Currently unused

        //for (int i=0; i< roomParent.transform.childCount; i++)
        //{
        //    GameObject spawn = GetChildGameObject(roomParent.transform.GetChild(i).gameObject, "SpawnPoints");
        //    Debug.Log("PlayerSpawn" + start[i]);
        //    Vector3 globalPosition = GetChildGameObject(spawn, "PlayerSpawn" + start[i]).transform.position;
        //    spawn = Instantiate(GetChildGameObject(spawn, "PlayerSpawn" + start[i]));
        //    spawn.name = spawn.name.Replace("(Clone)", "").Trim();

        //    GameObject transfer = GetChildGameObject(roomParent.transform.GetChild(i).gameObject, "SpawnPoints");
        //    Debug.Log("PlayerSpawn" + end[i]);
        //    Vector3 globalPosition2 = GetChildGameObject(transfer, "PlayerSpawn" + start[i]).transform.position;
        //    transfer = Instantiate(GetChildGameObject(transfer, "PlayerSpawn" + end[i]));
        //    transfer.name = transfer.name.Replace("(Clone)", "").Trim();

        //    spawn.transform.position = globalPosition;
        //    transfer.transform.position = globalPosition2;
        //    spawn.transform.parent = spawnPoints.transform;
        //    transfer.transform.parent = transferPoints.transform;
        //}


    }


    public override bool updateTask()
    {
        return true;

        // WRITE TASK UPDATE CODE HERE
    }


    public override void endTask()
    {
        TASK_END();

        // LEAVE BLANK
    }


    public override void TASK_END()
    {
        base.endTask();

        // WRITE TASK EXIT CODE HERE
    }

    public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        var allKids = fromGameObject.GetComponentsInChildren<Transform>();
        var kid = allKids.FirstOrDefault(k => k.gameObject.name == withName);
        if (kid == null) return null;
        return kid.gameObject;
    }

}