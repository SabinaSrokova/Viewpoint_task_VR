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
using UnityStandardAssets.Characters.FirstPerson;
using Valve.VR.InteractionSystem;

public class LM_BlackoutPath : ExperimentTask
{
    [Header("Task-specific Properties")]
    public GameObject hudpanel;
    public GameObject preparedRooms;
    public GameObject targetDisc; // To be used as the final destination target
    public GameObject halfwayDisc; // To be used during "stay" trials, halfway point with no plane
    public GameObject arrowPointer; // point left/right depending on the direction in which the participant should walk
    public GameObject blackoutFloor; // cylinder floor during blackout

    //public bool teleport = false;
    public bool rotateRoom = false;
    public bool reorientCamera = false;
    public bool showArrow = false;
    public float blackoutWalk = 7;
    public float arrowTime = 0.5f;
    public float viewingObjects = 14;
    public float delayBeforeContinuing = 17;

    private List<string> moveItem = new List<string> { };
    private List<string> repeat = new List<string> { };
    private List<string> condition = new List<string> { };
    private List<string> start = new List<string> { };
    private List<string> end = new List<string> { };
    private int taskCounter = new int();
    private LM_PrepareRooms.objectsMovedAssignment objectsMovedIs;
    private Vector3 tempHudPos;


    private GameObject seenObject;
    private GameObject spawnObject;
    private GameObject spawnParent;

    private GameObject walkTarget;
    private GameObject startLocation;
    private GameObject otherLocation;
    private GameObject endLocation;

    private GameObject discLocation;
    public GameObject disc;
    private GameObject disc_half;
    private GameObject arrow;
    private GameObject origFloor;
    private GameObject blackFloor;
    

    public float timer = 0;
    private bool timerSpawnReached = false;
    private bool timerDelay = false;
    private bool timerComplete = false;
    private bool responseMade = false;
    // Below is for the extra logging portion
    public Vector3 playerStartPos; // log the coordinate of player immediately at beginning of blackout and immediately before the blackout period ends.
    public Vector3 playerEndPos;
    public Quaternion playerStartRot;
    public Quaternion playerEndRot;


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
        objectsMovedIs = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().objectsMovedIs;

        GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;

        hud.showOnlyHUD();

        timer = 0;
        timerSpawnReached = false;
        timerDelay = false;
        timerComplete = false;
        responseMade = false;


        playerStartPos = Camera.main.transform.position;
        playerStartRot = Camera.main.transform.rotation;

        // Summon a circular floor during blackout
        origFloor = GetChildGameObject(currentRoom, "Floor 1");
        blackFloor = Instantiate(blackoutFloor, origFloor.transform.position, Quaternion.identity);

        // If the condition involves the item moving, this should teleport the item to it's new position after blacking out the screen (and technically before the spawning of the position marker for movement but this should be functionally instant for the participant)
        spawnParent = GetChildGameObject(currentRoom, "SpawnPoints");
        if (moveItem[taskCounter] == "yes")
        {
            if (repeat[taskCounter] == "A")
            {
                spawnObject = GetChildGameObject(spawnParent, "ObjectSpawnA");
            }
            else if (repeat[taskCounter] == "B")
            {
                spawnObject = GetChildGameObject(spawnParent, "ObjectSpawnB");
            }
            GameObject movingObject = seenObject.transform.GetChild(0).gameObject;

            // Move object, but keep Y coordinate of original position so that this silly thing doesn't spawn in the ground
            Vector3 tempLoc = movingObject.transform.position;
            tempLoc.x = spawnObject.transform.position.x;
            tempLoc.z = spawnObject.transform.position.z;
            movingObject.transform.position = tempLoc;
        }

        // Determine whether we need to teleport the participant in this trial
        if ((condition[taskCounter] == "walk" && start[taskCounter] == end[taskCounter]) || (condition[taskCounter] == "stay" && start[taskCounter] != end[taskCounter]))
        {
            //teleport = true;
            //Debug.Log("Participant will teleport to the second location");
            rotateRoom = true;
            Debug.Log("The room will rotate");
        }


        // If walk trial, place the disc in the non-starting location
        if (condition[taskCounter] == "walk")
        {
            // Define location of walk target
            if (start[taskCounter] == "A")
            {
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnB");
            }
            else if (start[taskCounter] == "B")
            {
                walkTarget = GetChildGameObject(spawnParent, "PlayerSpawnA");
            }

            // Summon target disc and set rotation according to the rotation of walk target (which is pre-set to point towards the middle of the room)
            disc = Instantiate(targetDisc, walkTarget.transform.position, Quaternion.identity);
            disc.transform.rotation = walkTarget.transform.rotation;

        }
        
        // If "stay", two discs should be placed (in between A and B and also in the "start" position) 
        else if (condition[taskCounter] == "stay")
        {
            // Define the Start and end locations
            if (start[taskCounter] == "A")
            {
                startLocation = GetChildGameObject(spawnParent, "PlayerSpawnA");
                otherLocation = GetChildGameObject(spawnParent, "PlayerSpawnB");
                discLocation = Instantiate(otherLocation);
            }
            else if (start[taskCounter] == "B")
            {
                startLocation = GetChildGameObject(spawnParent, "PlayerSpawnB");
                otherLocation = GetChildGameObject(spawnParent, "PlayerSpawnA");
                discLocation = Instantiate(otherLocation);
            }

            //Calc the midpoint between the two vectors for the stay condition in order to place the marker inbetween
            //New method of calculation that should be more accurate + this should no longer be replacing PlayerSpawnB
            discLocation.transform.position = Vector3.Slerp(startLocation.transform.position, otherLocation.transform.position, 0.5f);
            Debug.Log(discLocation.transform.position);

            // Move the halfway disc to the location between A and B
            disc_half = Instantiate(halfwayDisc, discLocation.transform.position, Quaternion.identity);

            // Place the target disc to the location of start
            disc = Instantiate(targetDisc, startLocation.transform.position, Quaternion.identity); // startLocation = start
            disc.transform.rotation = startLocation.transform.rotation;
        }


        // FOR LOGGING
        LM_TaskLog taskLog = GetComponent<ExperimentTask>().taskLog;
        taskLog.AddData("start", start[taskCounter]);
        taskLog.AddData("end", end[taskCounter]);
        taskLog.AddData("TarPos_x", disc.transform.position.x.ToString());
        taskLog.AddData("TarPos_z", disc.transform.position.z.ToString());
        taskLog.AddData("TarRot_x", disc.transform.rotation.x.ToString());
        taskLog.AddData("TarRot_z", disc.transform.rotation.z.ToString());
        taskLog.AddData("Start_SubPos_x", playerStartPos.x.ToString());
        taskLog.AddData("Start_SubPos_z", playerStartPos.z.ToString());
        taskLog.AddData("Start_SubRot_x", playerStartRot.x.ToString());
        taskLog.AddData("Start_SubRot_z", playerStartRot.z.ToString());




        // // In all cases, rotate the disc according to the end position, - NO LONGER RELEVANT SANS TELEPORTING
        // if (reorientCamera)
        // {
        //     if (condition[taskCounter] == "walk")
        //     {
        //         disc.transform.rotation = walkTarget.transform.rotation;
        //     } 
        //     else if (condition[taskCounter] == "stay")
        //     {
        //         disc.transform.rotation = startLocation.transform.rotation;
        //     }
        // } 
        // else
        // {
        //     endLocation = GetChildGameObject(spawnParent, "PlayerSpawn" + end[taskCounter]);
        //     disc.transform.rotation = endLocation.transform.rotation;
        // }


        // Summon an arrow in front of the player which points in the direction of which they should walk
        if (showArrow)
        {
            GameObject player = avatar;
            Vector3 playerPos = player.transform.position;
            Vector3 playerDir = player.transform.forward;
            Quaternion playerRot = player.transform.rotation;
            float dist = 1; // for now
            float height = 0.5f;

            Vector3 spawnPos = playerPos + playerDir * dist + Vector3.up * height;

            arrow = Instantiate(arrowPointer, spawnPos, playerRot);

            // If starting position is in A, point to the right
            if (start[taskCounter] == "A")
            {
                arrow.transform.rotation *= Quaternion.Euler(0, 180, 90); 
            }
            else if (start[taskCounter] == "B") // otherwise to the left
            {
               arrow.transform.rotation *= Quaternion.Euler(0, 0, 90); 
            }
        }
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

        // Destroy arrow after its time is up
        if (timer >= arrowTime)
        {
            DestroyImmediate(arrow);
        }

        // Here the room is relit after the blackout walking stage is complete. If the participant is intended to teleport, they sill do so before the blackout ends.
        // if (timerSpawnReached == false && timer >= blackoutWalk)
        // {
        //     Debug.Log("blackoutWalk time reached");

        //     DestroyImmediate(disc);
        //     DestroyImmediate(disc_half);
        //     DestroyImmediate(blackFloor);
        //     HalfwayCollisionColor.half_reached = false; /// For the halfway marker to turn back to false

        //     if (teleport == true)
        //     {
        //         GameObject player = avatar;
        //         GameObject destination = GetChildGameObject(spawnParent, "PlayerSpawn" + end[taskCounter]);

        //         Debug.Log("Player identified: " + player.gameObject.name);

        //         player.GetComponentInChildren<CharacterController>().enabled = false;

        //         Vector3 tempPos = player.transform.position;
        //         tempPos.x = destination.transform.position.x;
        //         tempPos.z = destination.transform.position.z;

        //         player.transform.position = tempPos;
        //         log.log("TASK_POSITION\t" + player.name + "\t" + this.GetType().Name + "\t" + player.transform.transform.position.ToString("f1"), 1);
        //         Debug.Log("Player now at: " + destination.name +
        //                 " (" + player.transform.position.x + ", " +
        //                 player.transform.position.z + ")");

        //         // If teleported, make sure the room is centered
        //         //if (reorientCamera)
        //         //{
        //          //   Vector3 tempRotate = player.transform.eulerAngles; ///////// taken out for immersive VR
        //         //    tempRotate.y = destination.transform.eulerAngles.y;
        //         //    player.transform.eulerAngles = tempRotate;
        //         //    avatar.GetComponent<FirstPersonController>().ResetMouselook();
        //        // }

        //         player.GetComponentInChildren<CharacterController>().enabled = true;

        //         teleport = false;
        //     }

        LM_TaskLog taskLog = GetComponent<ExperimentTask>().taskLog;
        //
        // Because teleport seems to not work in immersive VR -> rotate the room so that the participant shows up in "END"
        if (timerSpawnReached == false && timer >= blackoutWalk)
        {
            
            Debug.Log("blackoutWalk time reached");
            int color = (int)disc.GetComponent<Renderer>().material.color.r * 1000;
            int red = (int)new Color(1.000f, 0, 0, 1.000f).r * 1000;
            if ( color == red) taskLog.AddData("TargetReached", "False");
            else taskLog.AddData("TargetReached", "True");
            DestroyImmediate(disc);
            DestroyImmediate(disc_half);
            DestroyImmediate(blackFloor);
            HalfwayCollisionColor.half_reached = false;

            if (rotateRoom)
            {
                GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
                origFloor = GetChildGameObject(currentRoom, "Floor 1");
                Vector3 centerPoint = origFloor.transform.position; // The point around which the room rotates
                Transform currentRoomTransform = currentRoom.transform;

                // Find out the rotation axis (clockwise or counter-clockwise?)

                // If participant is being "teleported" from A to B, the rotation is clockwise (shifts to the left, because B is on the right of A)
                // If participants walked from A to B, and end is in A, they need "teleported" from B (walked position) to A. 
                // In this case, turn counter-clockwise. A to B is clockwise, as before.
                if (condition[taskCounter] == "stay")
                {
                     if (start[taskCounter] == "A" && end[taskCounter] == "B")
                     {
                        Vector3 rotationAxis = Vector3.up; // clockwise
                        currentRoomTransform.RotateAround(centerPoint, rotationAxis, 50f);
                     }
                     else if (start[taskCounter] == "B" && end[taskCounter] == "A")
                     {
                        Vector3 rotationAxis = -Vector3.up; // counter-clockwise
                        currentRoomTransform.RotateAround(centerPoint, rotationAxis, 50f);
                     }
                 }
                 else if (condition[taskCounter] == "walk")
                 {
                    if (start[taskCounter] == "A" && end[taskCounter] == "A")
                    {
                        Vector3 rotationAxis = -Vector3.up; // counter-clockwise
                        currentRoomTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    }
                    else if (start[taskCounter] == "B" && end[taskCounter] == "B")
                    {
                        Vector3 rotationAxis = Vector3.up; // clockwise
                        currentRoomTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    }
                 }

                rotateRoom = false;
            }

            // Response options on the screen
            if (objectsMovedIs == LM_PrepareRooms.objectsMovedAssignment.left)
            {
                hud.setLeftMessage("D");
                hud.setRightMessage("S");

                if (vrEnabled)
                {
                    hud.setLeftVRMessage("D");
                    hud.setRightVRMessage("S");
                    hud.setLeftVRScreenMessage("D");
                    hud.setRightVRScreenMessage("S");
                }
            }
            else if (objectsMovedIs == LM_PrepareRooms.objectsMovedAssignment.right)
            {
                hud.setLeftMessage("S");
                hud.setRightMessage("D");
            }
            if (vrEnabled)
            {
                hud.setLeftVRMessage("S");
                hud.setRightVRMessage("D");
                hud.setLeftVRScreenMessage("S");
                hud.setRightVRScreenMessage("D");
            }


            hud.leftButtonMessage.SetActive(true);
            hud.rightButtonMessage.SetActive(true);

            if (vrEnabled)
            {
                //hud.hudPanel.SetActive(true);
                //tempHudPos = hud.hudPanel.transform.position;
                //hud.hudPanel.transform.position = avatar.transform.position;
                //hud.leftVRMessage.SetActive(true);
                //hud.rightVRMessage.SetActive(true);
                hud.cameraScreen.SetActive(true);
                hud.leftVRMessageScreen.SetActive(true);
                hud.rightVRMessageScreen.SetActive(true);
            }


            timerSpawnReached = true;
            playerEndPos = Camera.main.transform.position;
            playerEndRot = Camera.main.transform.rotation;
            taskLog.AddData("End_SubPos_x", playerEndPos.x.ToString());
            taskLog.AddData("End_SubPos_z", playerEndPos.z.ToString());
            taskLog.AddData("End_SubRot_x", playerEndRot.x.ToString());
            taskLog.AddData("End_SubRot_z", playerEndRot.z.ToString());
            

            hud.showEverything();
        }
        string response = "No response";
        if (timerSpawnReached == true && responseMade == false)
        {
            if (vrEnabled)
            {
                if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.LeftHand))
                {
                    log.log("TASK_RESPONSE\t" + "Left Button Pressed\t" +"Response Time: " + timer, 1);
                    responseMade = true;
                    response = "Left";

                    taskLog.AddData("response", response);
                    taskLog.AddData("RT", timer.ToString());
                }
                else if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.RightHand))
                {
                    log.log("TASK_RESPONSE\t" + "Right Button Pressed\t" + "Response Time: " + timer, 1);
                    responseMade = true;
                    response = "Right";

                    taskLog.AddData("response", response);
                    taskLog.AddData("RT", timer.ToString());
                }
            }

            if (Input.GetButtonDown("Respond Left"))
            {
                log.log("TASK_RESPONSE\t" + "Left Button Pressed\t" + "Response Time: " + timer, 1);
                responseMade = true;
                response = "Left";

                taskLog.AddData("response", response);
                taskLog.AddData("RT", timer.ToString());
            }

            if (Input.GetButtonDown("Respond Right"))
            {
                log.log("TASK_RESPONSE\t" + "Right Button Pressed\t" + "Response Time: " + timer, 1);
                responseMade = true;
                response = "Right";

                taskLog.AddData("response", response);
                taskLog.AddData("RT", timer.ToString());
            }

        }

        if (timerSpawnReached == true && timer >= viewingObjects && timerComplete == false)
        {
            Debug.Log("14 seconds elapsed: trial complete");
            seenObject.SetActive(false);
            timerDelay = true;
            timerComplete = true;
            hud.leftButtonMessage.SetActive(false);
            hud.rightButtonMessage.SetActive(false);

            if (vrEnabled)
            {
                //hud.hudPanel.SetActive(false);
                //hud.leftVRMessage.SetActive(false);
                //hud.rightVRMessage.SetActive(false);
                hud.leftVRMessageScreen.SetActive(false);
                hud.rightVRMessageScreen.SetActive(false);
                //hud.cameraScreen.SetActive(false);
            }
        }

        if (timerDelay == true && timer >= delayBeforeContinuing)
        {
            Debug.Log("Delay over - teleporting to new room");
            GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
            currentRoom.SetActive(false); // deactivate the current room

            if (!responseMade)
            {
                taskLog.AddData("response", response);
                taskLog.AddData("RT", "N/A");
            }
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