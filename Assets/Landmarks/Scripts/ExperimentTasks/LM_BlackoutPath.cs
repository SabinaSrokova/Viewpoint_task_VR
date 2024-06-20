﻿/*
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

    public bool rotateRoom = false;
    public bool rotateTable = false;
    public bool rotateNothing = false;
    public bool reorientCamera = false;
    public bool triggerPressed = false;
    public bool proceedTrial = false;
    public float blackoutWalk = 7;
    public float arrowTime = 0.5f;
    public float viewingObjects = 5;
    public float delayBeforeContinuing = 3;

    private List<string> moveItem = new List<string> { };
    private List<string> repeat = new List<string> { };
    private List<string> condition = new List<string> { };
    private List<string> start = new List<string> { };
    private List<string> end = new List<string> { };
    //private List<string> hideRoom = new List<string> { };
    private int taskCounter = new int();
    private List<string> block = new List<string> { };
    
    private Vector3 tempHudPos;


    private GameObject seenObject;
    private GameObject spawnObject;
    private GameObject spawnParent;

    private GameObject walkTarget;
    private GameObject startLocation;
    private GameObject otherLocation;
    private GameObject endLocation;

    private GameObject discLocation;
    private GameObject disc;
    private GameObject disc_half;
    private GameObject arrow;
    private GameObject origFloor;
    private GameObject blackFloor;

    public float timer = 0;
    public float timer2 = 0;
    public float RT_timer = 0;
    private bool delayReached = false;
    private bool timerDelay = false;
    private bool timerComplete = false;
    private bool responseMade = false;

    // Below is for the extra logging portion
    public Vector3 playerStartPos; // log the coordinate of player immediately at beginning of blackout and immediately before the blackout period ends.
    public Vector3 playerEndPos;
    public Quaternion playerStartRot;
    public Quaternion playerEndRot;
    public bool blackout = false; // this var is for eye tracking log
    public bool endETKDelay = false; // for eyetracking


    private LM_PrepareRooms.objectsMovedAssignment objectsMovedIs; 
    private LM_PrepareRooms.RotationSettingAssignment rotationSetting;
    private LM_PrepareRooms.BlackoutAssignment blackoutDuringDelay;
    private LM_PrepareRooms.HideRoomAssignment hideRoom; 
    private LM_PrepareRooms.SelfPacedSettingAssignment selfPacedSetting; 


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
        block = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().block;
        seenObject = GameObject.Find("ViewObjects").GetComponent<LM_ToggleObjects>().seenObject;

        objectsMovedIs = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().objectsMovedIs;
        blackoutDuringDelay = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().blackoutDuringDelay;
        hideRoom = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().hideRoom;
        selfPacedSetting = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().selfPacedSetting;

        GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
        GameObject movingObject = seenObject.transform.GetChild(0).gameObject;

        timer = 0;
        timer2 = 0;
        RT_timer = 0;
        delayReached = false;
        proceedTrial = false;
        timerDelay = false;
        timerComplete = false;
        responseMade = false;
        triggerPressed = false;
        blackout = true; // eye tracking will record that blackout is happening

        // Record the subject's starting spot when delay
        playerStartPos = Camera.main.transform.position;
        playerStartRot = Camera.main.transform.rotation;


        if (hideRoom == LM_PrepareRooms.HideRoomAssignment.Yes)
        {
            hud.showOnlyHUD(); 
            // Deactivate all objects
            seenObject.SetActive(false);
        } 
        else
        {
            // If the condition is blackout, then show only HUD & any relevant components. Otherwise, deactivate objects
            if (blackoutDuringDelay == LM_PrepareRooms.BlackoutAssignment.Yes)
            {
                hud.showOnlyHUD(); 
            
                // Summon a circular floor during blackout
                origFloor = GetChildGameObject(currentRoom, "Floor 1"); /////////////////////////////////////////////////////////////////
                blackFloor = Instantiate(blackoutFloor, origFloor.transform.position, Quaternion.identity);
            }
            else if (blackoutDuringDelay == LM_PrepareRooms.BlackoutAssignment.No)
            {
                hud.showEverything();

                // Deactivate all objects
                seenObject.SetActive(false);
            }
        }

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
            //GameObject movingObject = seenObject.transform.GetChild(0).gameObject;

            // Move object, but keep Y coordinate of original position so that this silly thing doesn't spawn in the ground
            Vector3 tempLoc = movingObject.transform.position;
            tempLoc.x = spawnObject.transform.position.x;
            tempLoc.z = spawnObject.transform.position.z;
            movingObject.transform.position = tempLoc;
        }

        // Determine whether the table or the room needs to rotate
        if ((condition[taskCounter] == "walk" && start[taskCounter] == end[taskCounter]) || (condition[taskCounter] == "stay" && start[taskCounter] != end[taskCounter]))
        {
            if (rotationSetting == LM_PrepareRooms.RotationSettingAssignment.RotateRooms)
            {
                rotateRoom = true;
                Debug.Log("The room will rotate");
            }
            else if (rotationSetting == LM_PrepareRooms.RotationSettingAssignment.RotateTables)
            {
                rotateTable = true;
                Debug.Log("The table will rotate");
            }
            
        }
        else
        {
            Debug.Log("AAA");
            rotateNothing = true;
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

        // Stimulus specific information
        taskLog.AddData("Timestamp", DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt"));
        taskLog.AddData("subID", Config.Instance.subject);
        taskLog.AddData("trial", taskCounter.ToString());
        taskLog.AddData("Room", currentRoom.name);
        taskLog.AddData("Condition", condition[taskCounter]);
        taskLog.AddData("moveItem", moveItem[taskCounter]);
        taskLog.AddData("Repeat", repeat[taskCounter]);
        taskLog.AddData("Block", block[taskCounter]);
        taskLog.AddData("ObjectMovedIs", objectsMovedIs.ToString()); // counterbalance
        taskLog.AddData("start", start[taskCounter]);
        taskLog.AddData("end", end[taskCounter]);

        // which object movd?
        if (moveItem[taskCounter] == "yes") {taskLog.AddData("TargetObject", movingObject.name); }
        else if (moveItem[taskCounter] == "no") {taskLog.AddData("TargetObject", "None"); }

        // Position information
        taskLog.AddData("TarPos_x", disc.transform.position.x.ToString());
        taskLog.AddData("TarPos_z", disc.transform.position.z.ToString());
        taskLog.AddData("Start_SubPos_x", playerStartPos.x.ToString());
        taskLog.AddData("Start_SubPos_z", playerStartPos.z.ToString());


       // --------------------- Walking direction text at the beginning of delay ---------------------
        if (vrEnabled)
        {

            if (start[taskCounter] == "A")
            {
                hud.setStatusScreenMessage(">>>");
                hud.cameraScreen.SetActive(true);
                hud.statusMessageScreen.SetActive(true);
            }
            else if (start[taskCounter] == "B")
            {
                hud.setStatusScreenMessage("<<<");
                hud.cameraScreen.SetActive(true);
                hud.statusMessageScreen.SetActive(true);
            }
        }
        else
        {
            if (start[taskCounter] == "A")
            {
                hud.setStatusMessage(">>>");
                hud.statusMessage.SetActive(true);
            }
            else if (start[taskCounter] == "B")
            {
                hud.setStatusMessage("<<<");
                hud.statusMessage.SetActive(true);
            }
        }

    }


    public override bool updateTask()
    {
        if (skip)
        {
            return true;
        }



        timer += Time.deltaTime;
        LM_TaskLog taskLog = GetComponent<ExperimentTask>().taskLog;


        // --------------------- Walking direction arrow - destroy after arrowTime is up ---------------------
        if (timer >= arrowTime)
        {
            if (vrEnabled)
            {
                hud.statusMessageScreen.SetActive(false);
            }
            else
            {
                hud.statusMessage.SetActive(false);
            }
        }


        // --------------------- Handle self-paced condition ---------------------
        // Here, the participant won't be able to continue until the targets are green
        // 6/12/2024 changelog - even if the participant indicates that they're ready, the objects will
        // not show up until blackoutWalk time has passed. If blackoutWalk had passed and they press trigger, objects will show up immediately.

        if (proceedTrial == false && selfPacedSetting == LM_PrepareRooms.SelfPacedSettingAssignment.Yes)
        {

            // Allow the participant to proceed only if target disc is no longer red
            int color = (int)disc.GetComponent<Renderer>().material.color.r * 1000;
            int red = (int)new Color(1.000f, 0, 0, 1.000f).r * 1000;
            
            if (color != red) 
            {
                if (vrEnabled)
                {
                    if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.RightHand)) // 6/20/2024 - changed to right hand only
                    {
                        triggerPressed = true;
                        Debug.Log("Participant reached the marker and pressed trigger to continue");
                    }
                } 
                else 
                {
                    if (Input.GetButtonDown("Return"))
                    {
                        triggerPressed = true;
                        Debug.Log("Participant reached the marker and pressed enter to continue");
                    }
                }
            }

            if (triggerPressed == true && timer >= blackoutWalk)
            {
                proceedTrial = true;
            }

        }

        // When self-paced option is disabled
        else if (proceedTrial == false && selfPacedSetting == LM_PrepareRooms.SelfPacedSettingAssignment.No)
        {
            // Set proceed trial only after the timer is reached
            if (timer >= blackoutWalk) 
            {
                proceedTrial = true;
            }
        }


        // --------------------- Delay ended, respawn objects and response cues ---------------------

        if (proceedTrial && (rotateRoom || rotateTable || rotateNothing))
        {
            taskLog.AddData("Delay_time", timer.ToString());
            Debug.Log("End of delay.");

            seenObject.SetActive(true);

            // Find out if the marker was reached
            int color = (int)disc.GetComponent<Renderer>().material.color.r * 1000;
            int red = (int)new Color(1.000f, 0, 0, 1.000f).r * 1000;
            if (color == red) taskLog.AddData("TargetReached", "False");
            else taskLog.AddData("TargetReached", "True");

            DestroyImmediate(disc);
            DestroyImmediate(disc_half);
            DestroyImmediate(blackFloor);
            HalfwayCollisionColor.half_reached = false;


            // --------------------- Handle Room/table rotation  ---------------------
            // Because teleport seems to not work in immersive VR -> rotate the room so that the participant shows up in "END"
            // Code-wise it's after the objects are re-activated, but from participants POV they see the objects after they've rotated

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
            else if (rotateTable) {

                GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
                origFloor = GetChildGameObject(currentRoom, "Floor 1");
                Vector3 centerPoint = origFloor.transform.position; // The point around which the Table and the contents rotate

                // Objects to rotate
                seenObject = GameObject.Find("ViewObjects").GetComponent<LM_ToggleObjects>().seenObject;
                Transform seenObjectTransform = seenObject.transform;

                // Table
                GameObject table = GetChildGameObject(currentRoom, "Table");
                Transform tableTransform = table.transform;

                // Object spawns
                spawnParent = GetChildGameObject(currentRoom, "SpawnPoints");
                GameObject spawnA = GetChildGameObject(currentRoom, "ObjectSpawnA");
                GameObject spawnB = GetChildGameObject(currentRoom, "ObjectSpawnB");
                Transform spawnATransform = spawnA.transform;
                Transform spawnBTransform = spawnB.transform;


                if ((condition[taskCounter] == "stay" && start[taskCounter] == "A" && end[taskCounter] == "B") || (condition[taskCounter] == "walk" && start[taskCounter] == "B" && end[taskCounter] == "B"))
                {
                    //clockwise
                    Vector3 rotationAxis = Vector3.up;
                    seenObjectTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    tableTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    spawnATransform.RotateAround(centerPoint, rotationAxis, 50f);
                    spawnBTransform.RotateAround(centerPoint, rotationAxis, 50f);
                }
                else if ((condition[taskCounter] == "stay" && start[taskCounter] == "B" && end[taskCounter] == "A") || (condition[taskCounter] == "walk" && start[taskCounter] == "A" && end[taskCounter] == "A"))
                {
                    //counter-clockwise
                    Vector3 rotationAxis = -Vector3.up;
                    seenObjectTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    tableTransform.RotateAround(centerPoint, rotationAxis, 50f);
                    spawnATransform.RotateAround(centerPoint, rotationAxis, 50f);
                    spawnBTransform.RotateAround(centerPoint, rotationAxis, 50f);
                }

                rotateTable = false;
            }
            else
            {
                rotateNothing = false;
            }

            // --------------------- Response cues  ---------------------
            if (objectsMovedIs == LM_PrepareRooms.objectsMovedAssignment.left)
            {
               
                if (vrEnabled)
                {
                    hud.setLeftVRMessage("D");
                    hud.setRightVRMessage("S");
                    hud.setLeftVRScreenMessage("D");
                    hud.setRightVRScreenMessage("S");
                }
                else
                { 
                    hud.setLeftMessage("D");
                    hud.setRightMessage("S");
                }
            }
            else if (objectsMovedIs == LM_PrepareRooms.objectsMovedAssignment.right)
            {
                
                if (vrEnabled)
                {
                    hud.setLeftVRMessage("S");
                    hud.setRightVRMessage("D");
                    hud.setLeftVRScreenMessage("S");
                    hud.setRightVRScreenMessage("D");
                }
                else
                {
                    hud.setLeftMessage("S");
                    hud.setRightMessage("D");
                }
            }


            hud.leftButtonMessage.SetActive(true);
            hud.rightButtonMessage.SetActive(true);

            if (vrEnabled)
            {

                hud.cameraScreen.SetActive(true);
                hud.leftVRMessageScreen.SetActive(true);
                hud.rightVRMessageScreen.SetActive(true);
            }


            playerEndPos = Camera.main.transform.position;
            playerEndRot = Camera.main.transform.rotation;
            taskLog.AddData("End_SubPos_x", playerEndPos.x.ToString());
            taskLog.AddData("End_SubPos_z", playerEndPos.z.ToString());

            blackout = false; // blackout ends so eye recording will resume recording names

            if (hideRoom == LM_PrepareRooms.HideRoomAssignment.Yes)
            {
                hud.showOnlyHUD(); 
            } else
            {
               hud.showEverything(); 
            }

            delayReached = true;
            
        }

        // --------------------- 2nd phase timer ---------------------
        if (delayReached == true) 
        {
            timer2 += Time.deltaTime;
        }


        // --------------------- Handle responses ---------------------
        string response = "No response";
        if (delayReached == true && responseMade == false)
        {
            RT_timer += Time.deltaTime;
        }

        if (delayReached == true && responseMade == false && timer2 >= 0.5f) // Allows responses only after 500ms (too fast otherwise) and accounts for getStateDown issues if self-paced.
        {

            if (vrEnabled)
            {
                if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.LeftHand))
                {
                    log.log("TASK_RESPONSE\t" + "Left Button Pressed\t" + "Response Time: " + RT_timer, 1);
                    responseMade = true;
                    response = "Left";

                    taskLog.AddData("response", response);
                    taskLog.AddData("RT", RT_timer.ToString());
                }
                else if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.RightHand))
                {
                    log.log("TASK_RESPONSE\t" + "Right Button Pressed\t" + "Response Time: " + RT_timer, 1);
                    responseMade = true;
                    response = "Right";

                    taskLog.AddData("response", response);
                    taskLog.AddData("RT", RT_timer.ToString());
                }
            }

            if (Input.GetButtonDown("Respond Left"))
            {
                log.log("TASK_RESPONSE\t" + "Left Button Pressed\t" + "Response Time: " + RT_timer, 1);
                responseMade = true;
                response = "Left";

                taskLog.AddData("response", response);
                taskLog.AddData("RT", RT_timer.ToString());
            }

            if (Input.GetButtonDown("Respond Right"))
            {
                log.log("TASK_RESPONSE\t" + "Right Button Pressed\t" + "Response Time: " + RT_timer, 1);
                responseMade = true;
                response = "Right";

                taskLog.AddData("response", response);
                taskLog.AddData("RT", RT_timer.ToString());
            }

        }

        // --------------------- Finish-up this trial ---------------------
        // leave the room on "delaybeforecontinuing" where no objects are visible. Then move to the next trial.
        if (delayReached == true && timer2 >= viewingObjects && timerComplete == false)
        {
            Debug.Log("Trial complete");
            seenObject.SetActive(false);
            timerDelay = true; // Delay timer for expt handling
            timerComplete = true; 
            endETKDelay = true; // this one is eyetracking specific
            hud.leftButtonMessage.SetActive(false);
            hud.rightButtonMessage.SetActive(false);

            if (vrEnabled)
            {

                hud.leftVRMessageScreen.SetActive(false);
                hud.rightVRMessageScreen.SetActive(false);
            }
        }

        if (timerDelay == true && timer2 >= viewingObjects + delayBeforeContinuing)
        {
            Debug.Log("Delay over - teleporting to new room");
            GameObject currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
            
            // Deactivate the room
            currentRoom.SetActive(false);

            endETKDelay = false;

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