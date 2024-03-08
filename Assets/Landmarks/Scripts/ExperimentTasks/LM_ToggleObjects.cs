﻿/*
    LM_ToggleObjects
       
    This is a modified version of the original LM_ToggleObjects script which only had the functionality to toggle active a list of objects provided to it in the inspector.

    This version does considerably more, and makes up the first portion of the main structure of the Viewpoint task.

    
    

    Copyright (C) 2019 Michael J. Starrett
    Modified by Matthew Watson

    Navigate by StarrLite (Powered by LandMarks)
    Human Spatial Cognition Laboratory
    Department of Psychology - University of Arizona   
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class LM_ToggleObjects : ExperimentTask
{
    [Header("Task-specific Properties")]
    public List<GameObject> objsToToggle;
    public bool setStateOn;
    public GameObject hudpanel;
    public GameObject preparedRooms;
    public float objectsAppear = 2;
    public float screenBlackout = 7;

    private List<string> moveItem = new List<string> { };
    private List<string> repeat = new List<string> { };
    private List<GameObject> seenObjects = new List<GameObject> { };
    private List<GameObject> hiddenObjects = new List<GameObject> { };
    private List<string> start = new List<string> { };
    public GameObject seenObject;
    private GameObject hiddenObject;
    private int taskCounter = new int();

    private float timer = 0;
    private bool timerSpawnReached = false;
    private bool timerDespawnItemsReached = false;
    public static bool participantReady = false;
    private bool timerRoomDespawnReached = false; 

    private GameObject spawnParent;
    public GameObject targetDisc;
    private GameObject discLocation;
    private GameObject disc;


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
            return;
        }
        // WRITE TASK STARTUP CODE HERE

        moveItem = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().moveItem;
        repeat = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().repeat;
        taskCounter = GameObject.Find("Counter").GetComponent<LM_DummyCounter>().counter;
        start = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().start;

        try
        {
            GameObject.Find("ALL_ROOMS").SetActive(false);
        }
        catch { }

        GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
        spawnParent = GetChildGameObject(currentRoom, "SpawnPoints");
        GameObject destination = GetChildGameObject(spawnParent, "PlayerSpawn" + start[taskCounter]);

        
        Vector3 sumVector = new Vector3(0f,0f, 0f);

        GameObject centerCalc = GetChildGameObject(currentRoom, "Objects " + repeat[taskCounter]);

        foreach (Transform child in centerCalc.transform)
        {
            sumVector += child.position;
        }

        Vector3 groupCenter = sumVector / centerCalc.transform.childCount;
        Debug.Log(groupCenter);


        hud.showOnlyHUD();


        timer = 0;
        timerSpawnReached = false;
        participantReady = false;

        seenObject = GetChildGameObject(currentRoom, "Objects " + repeat[taskCounter]);
        Debug.Log("Objects " + repeat[taskCounter]);
        Debug.Log(repeat[taskCounter]);
        seenObject.SetActive(false);
        currentRoom.SetActive(true); 

        // Orientation marker
        disc = Instantiate(targetDisc, destination.transform.position, Quaternion.identity);
        disc.transform.rotation = destination.transform.rotation;

        if (repeat[taskCounter] == "A")
        {
            hiddenObject = GetChildGameObject(currentRoom, "Objects " + "B");
            hiddenObject.SetActive(false);
        }
        else
        {
            hiddenObject = GetChildGameObject(currentRoom, "Objects " + "A");
            hiddenObject.SetActive(false);
        }


        foreach (var obj in objsToToggle)
        {
            obj.SetActive(setStateOn);
        }

    }


    public override bool updateTask()
    {
        if (skip)
        {
            //log.log("INFO    skip task    " + name,1 );
            return true;
        }

        if (Input.GetButtonDown("Return"))
        {
            hud.showEverything();
            participantReady = true;
            DestroyImmediate(disc);
        }

        if (vrEnabled)
        {
            if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.LeftHand) | vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.RightHand))
            {
                hud.showEverything();
                participantReady = true;
                DestroyImmediate(disc);
            }
        }


        if (participantReady == true)
        {
            timer += Time.deltaTime;
        }


        //Debug.Log(timer);

        // Objects to be viewed by the participant appear after 2 seconds pass of the participant standing in the empty room
        if (timerSpawnReached == false && timer >= objectsAppear)
        {
            Debug.Log("Objects now Appearing");
            Debug.Log(seenObject);
            seenObject.SetActive(true);
            timerSpawnReached = true;
        }


        // Objects will despawn after 7 seconds pass. (Might want to skip this part as instead the hud.blackout might do the trick instead.
        if (timerDespawnItemsReached == false && timer >= screenBlackout)
        {
            Debug.Log("Now moving on to BlackoutPath");
            //seenObject.SetActive(false);
            return true;
        }
        return false;

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

    IEnumerator Trial()
    {
        yield return new WaitForSecondsRealtime(2f);
        seenObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);
    }

}