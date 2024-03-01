/*
    LM Dummy
       
    Attached object holds task components that need to be effectively ignored 
    by Tasklist but are required for the script. Thus the object this is 
    attached to can be detected by Tasklist (won't throw error), but does nothing 
    except start and end.   

    Copyright (C) 2019 Michael J. Starrett

    Navigate by StarrLite (Powered by LandMarks)
    Human Spatial Cognition Laboratory
    Department of Psychology - University of Arizona   
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LM_BlackoutPath : ExperimentTask
{
    [Header("Task-specific Properties")]
    public GameObject hudpanel;
    public GameObject preparedRooms;
    public GameObject targetDisc;
    public bool teleport = false;
    public float blackoutWalk = 7;
    public float viewingObjects = 14;
    public float delayBeforeContinuing = 17;

    private List<string> moveItem = new List<string> { };
    private List<string> repeat = new List<string> { };
    private List<string> condition = new List<string> { };
    private List<string> start = new List<string> { };
    private List<string> end = new List<string> { };
    private int taskCounter = new int();
    private GameObject seenObject;
    private GameObject spawnObject;
    private GameObject spawnParent;
    private GameObject walkTarget;
    private GameObject discLocation;
    private GameObject currentLocation;
    private GameObject disc;
    private float timer = 0;
    private bool timerSpawnReached = false;
    private bool timerDelay = false;
    private bool timerComplete = false;

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

        moveItem = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().moveItem;
        repeat = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().repeat;
        condition = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().condition;
        start = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().start;
        end = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().end;
        taskCounter = GameObject.Find("Counter").GetComponent<LM_DummyCounter>().counter;
        seenObject = GameObject.Find("ViewObjects").GetComponent<LM_ToggleObjects>().seenObject;

        GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;

        hud.showOnlyHUD();

        timer = 0;
        timerSpawnReached = false;
        timerDelay = false;
        timerComplete = false;

        // If the condition involves the item moving, this should teleport the item to it's new position after blacking out the screen (and technically before the spawning of the position marker for movement but this should be functionally instant for the participant)
        spawnParent = GetChildGameObject(currentRoom, "SpawnPoints");

        if (moveItem[taskCounter] == "yes")
        {
            if (repeat[taskCounter] == "A")
            {
                spawnObject = GetChildGameObject(spawnParent, "ObjectSpawnA");
                GameObject movingObject = seenObject.transform.GetChild(0).gameObject;
                movingObject.transform.position = spawnObject.transform.position;

            }
            else
            {
                spawnObject = GetChildGameObject(spawnParent, "ObjectSpawnB");
                GameObject movingObject = seenObject.transform.GetChild(0).gameObject;
                movingObject.transform.position = spawnObject.transform.position;
            }
        }

        if ((condition[taskCounter] == "walk" && start[taskCounter] == end[taskCounter]) || (condition[taskCounter] == "stay" && start[taskCounter] != end[taskCounter]))
        {
            teleport = true;
            Debug.Log("Participant will teleport to the second location");
        }

        if (condition[taskCounter] == "walk")
        {
            if (start[taskCounter] == "A")
            {
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnB");
                disc = Instantiate(targetDisc, walkTarget.transform.position, Quaternion.identity);

            }
            else
            {
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnA");
                disc = Instantiate(targetDisc, walkTarget.transform.position, Quaternion.identity);
            }
        }
        else
        {
            if (start[taskCounter] == "A")
            {
                currentLocation = GetChildGameObject(spawnParent, "PlayerSpawnA");
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnB");
                discLocation = Instantiate(walkTarget);

                //Calc the midpoint between the two vectors for the stay condition in order to place the marker inbetween
                //New method of calculation that should be more accurate + this should no longer be replacing PlayerSpawnB

                discLocation.transform.position = Vector3.Slerp(currentLocation.transform.position, walkTarget.transform.position, 0.5f);
                Debug.Log(discLocation.transform.position);

                disc = Instantiate(targetDisc, discLocation.transform.position, Quaternion.identity);

            }
            else
            {
                currentLocation = GetChildGameObject(spawnParent, "PlayerSpawnB");
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnA");
                discLocation = Instantiate(walkTarget);

                //Calc the midpoint between the two vectors for the stay condition in order to place the marker inbetween

  
                discLocation.transform.position = Vector3.Slerp(currentLocation.transform.position, walkTarget.transform.position, 0.5f);
                Debug.Log(discLocation.transform.position);

                disc = Instantiate(targetDisc, discLocation.transform.position, Quaternion.identity);
            }
        }

        // WRITE TASK STARTUP CODE HERE
    }


    public override bool updateTask()
    {
        if (skip)
        {
            //log.log("INFO    skip task    " + name,1 );
            return true;
        }

        timer += Time.deltaTime;
        //Debug.Log(timer);

        // Here the room is relit after the blackout walking stage is complete. If the participant is intended to teleport, they sill do so before the blackout ends.
        if (timerSpawnReached == false && timer >= blackoutWalk)
        {
            Debug.Log("blackoutWalk time reached");

            DestroyImmediate(disc);

            if (teleport == true)
            {
                GameObject player = avatar;
                GameObject destination = GetChildGameObject(spawnParent, "PlayerSpawn" + end[taskCounter]);

                Debug.Log("Player identified: " + player.gameObject.name);

                player.GetComponentInChildren<CharacterController>().enabled = false;
                Vector3 tempPos = player.transform.position;
                tempPos.x = destination.transform.position.x;
                tempPos.z = destination.transform.position.z;

                player.transform.position = tempPos;
                log.log("TASK_POSITION\t" + player.name + "\t" + this.GetType().Name + "\t" + player.transform.transform.position.ToString("f1"), 1);
                Debug.Log("Player now at: " + destination.name +
                        " (" + player.transform.position.x + ", " +
                        player.transform.position.z + ")");
                player.GetComponentInChildren<CharacterController>().enabled = true;
            }
            hud.showEverything();
            timerSpawnReached = true;
        }

        if (timerSpawnReached == true && timer >= viewingObjects && timerComplete == false)
        {
            Debug.Log("14 seconds elapsed: trial complete");
            seenObject.SetActive(false);
            //currentRoom.SetActive(false); ///////////////////////////////////////////////
            timerDelay = true;
            timerComplete = true;
            //return true;
        }

        if (timerDelay == true && timer >= delayBeforeContinuing)
        {
            Debug.Log("Delay over - teleporting to new room");
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

}