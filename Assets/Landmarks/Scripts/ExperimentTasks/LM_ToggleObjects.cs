/*
    LM_ToggleObjects
       
    This is a modified version of the original LM_ToggleObjects script which only had the functionality to toggle active a list of objects provided to it in the inspector.

    This version does considerably more, and makes up the first portion of the main structure of the Viewpoint task.

    
    

    Copyright (C) 2019 Michael J. Starrett
    Modified by Matthew Watson & Sabina Srokova

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
using ViveSR.anipal.Eye;
using System.Drawing;
using Color = UnityEngine.Color;

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
    private List<string> end = new List<string> { };
    private List<string> condition = new List<string> { };
    public GameObject seenObject;
    private GameObject hiddenObject;
    private int taskCounter = new int();

    private float timer = 0;
    private bool timerSpawnReached = false;
    private bool timerDespawnItemsReached = false;
    public static bool participantReady = false;
    private bool timerRoomDespawnReached = false;
    public bool initETKDelay = false;

    private GameObject spawnParent;
    public GameObject targetDisc;
    public GameObject blackoutFloor; // cylinder floor during blackout
    private GameObject discLocation;
    private GameObject disc;
    private GameObject origFloor;
    private GameObject blackFloor;
    private GameObject currentRoom;
    private GameObject table;

    private LM_PrepareRooms.objectsMovedAssignment objectsMovedIs;
    private LM_PrepareRooms.RotationSettingAssignment rotationSetting;
    private LM_PrepareRooms.BlackoutAssignment blackoutDuringDelay; 
    private LM_PrepareRooms.HideRoomAssignment hideRoom; 
    private LM_PrepareRooms.DirectionHintAssignment dirHint; 
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
            return;
        }

        initETKDelay = true;

        // WRITE TASK STARTUP CODE HERE
        moveItem = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().moveItem;
        repeat = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().repeat;
        taskCounter = GameObject.Find("Counter").GetComponent<LM_DummyCounter>().counter;
        start = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().start;
        end = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().end;
        condition = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().condition;

        hideRoom = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().hideRoom;
        dirHint = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().dirHint;

        try
        {
            GameObject.Find("ALL_ROOMS").SetActive(false);
        }
        catch { }

        if (hideRoom == LM_PrepareRooms.HideRoomAssignment.Yes)
        {
            hud.showOnlyHUD(); 
        }


        currentRoom = preparedRooms.transform.GetChild(taskCounter).gameObject;
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
        if (vrEnabled)
        {
            hud.setStatusScreenMessage("Position youself on the marker, press any trigger to start the next trial");
            hud.cameraScreen.SetActive(true);
            hud.statusMessageScreen.SetActive(true);
        }
        else
        {
            hud.setStatusMessage("Position youself on the marker, press the Enter key to start the next trial");
            hud.statusMessage.SetActive(true);
        }

        timer = 0;
        timerSpawnReached = false;
        participantReady = false;

        seenObject = GetChildGameObject(currentRoom, "Objects " + repeat[taskCounter]);
        Debug.Log("Objects " + repeat[taskCounter]);
        Debug.Log(repeat[taskCounter]);
        seenObject.SetActive(false);

        // Activate the room but hide the table at the beginning (because table is set to HUD layer)
        currentRoom.SetActive(true); 

        table = GetChildGameObject(currentRoom, "Table");
        table.SetActive(false);


        // Draw a floor
       // origFloor = GetChildGameObject(sqr_walls, "Floor 1");
       // blackFloor = Instantiate(blackoutFloor, origFloor.transform.position, Quaternion.identity);

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

        if (participantReady == false)
        {


            // Allow the participant to proceed only if target disc is no longer red
            int color = (int)disc.GetComponent<Renderer>().material.color.r * 1000;
            int red = (int)new Color(1.000f, 0, 0, 1.000f).r * 1000;


            if (color != red)
            {
                if (Input.GetButtonDown("Return"))
                {

                    hud.statusMessage.SetActive(false);

                    if (hideRoom == LM_PrepareRooms.HideRoomAssignment.Yes)
                    {
                        hud.showOnlyHUD();
                    }
                    else
                    {
                        hud.showEverything();
                    }
                    participantReady = true;
                    DestroyImmediate(disc);
                    DestroyImmediate(blackFloor);

                    // Show table
                    table.SetActive(true);

                }
            }



            if (color != red)
            {
                if (vrEnabled)
                {
                    if (vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.LeftHand) | vrInput.TriggerButton.GetStateDown(Valve.VR.SteamVR_Input_Sources.RightHand))
                    {
                        hud.statusMessageScreen.SetActive(false);

                        hud.fadeScreen.SetActive(true);

                        if (hideRoom == LM_PrepareRooms.HideRoomAssignment.Yes)
                        {
                            hud.showOnlyHUD();
                        }
                        else
                        {
                            hud.showEverything();
                        }

                        hud.fadeScreen.GetComponent<PanelFader>().Fade();

                        participantReady = true;
                        DestroyImmediate(disc);
                        DestroyImmediate(blackFloor);

                        // Show table
                        table.SetActive(true);


                        GameObject.Find("Eye_Tracking").GetComponent<SRanipal_GazeRaySample>().blackout_eye_tracking = false;
                    }
                }
            }
        }
        

        if (participantReady == true)
        {
            timer += Time.deltaTime;
        }


        //Debug.Log(timer);
        if (timerSpawnReached == false && participantReady == true && dirHint == LM_PrepareRooms.DirectionHintAssignment.Yes)
        {

            if (vrEnabled)
            {
                if (condition[taskCounter] == "stay" && start[taskCounter] != end[taskCounter]) 
                {
                    if (start[taskCounter] == "A")
                    {
                        hud.setStatusScreenMessage("Table rotates <<<");
                    }
                    else if (start[taskCounter] == "B")
                    {
                        hud.setStatusScreenMessage("Table rotates >>>");
                    }
                } 
                else if (condition[taskCounter] == "walk" && start[taskCounter] == end[taskCounter])
                {
                    if (start[taskCounter] == "A")
                    {
                        hud.setStatusScreenMessage("Walking >>> \n\n Table rotates");
                    }
                    else if (start[taskCounter] == "B")
                    {
                        hud.setStatusScreenMessage("Walking <<< \n\n Table rotates");
                    }
                }
                else
                {
                    if (condition[taskCounter] == "stay")
                    {

                        hud.setStatusScreenMessage("Table will NOT rotate");
  
                    }
                    else if (condition[taskCounter] == "walk")
                    {
                        if (start[taskCounter] == "A")
                        {
                            hud.setStatusScreenMessage("Walking >>> \n\n Table will NOT rotate");
                        }
                        else if (start[taskCounter] == "B")
                        {
                            hud.setStatusScreenMessage("Walking <<< \n\n Table will NOT rotate");
                        }
                    }
                }

                hud.cameraScreen.SetActive(true);
                hud.statusMessageScreen.SetActive(true);

            }
 
            else
            {
                if (condition[taskCounter] == "stay" && start[taskCounter] != end[taskCounter])
                {
                    if (start[taskCounter] == "A")
                    {
                        hud.setStatusMessage("Table rotates <<<"); // "as if they viewed it from B, so table needs to rotate clockwise A<B
                    
                    }
                    else if (start[taskCounter] == "B")
                    {
                        hud.setStatusMessage("Table rotates >>>"); // A>B
                    }
                }
                else if (condition[taskCounter] == "walk" && start[taskCounter] == end[taskCounter])
                {
                    if (start[taskCounter] == "A")
                    {
                        hud.setStatusMessage("Walking >>> \n\n Table rotates"); // A to A means the table has to follow the PP as they walk counter-clockwise A>>>B

                    }
                    else if (start[taskCounter] == "B")
                    {
                        hud.setStatusMessage("Walking <<< \n\n Table rotates"); 
  
                    }
                }
                else
                {
                    if (condition[taskCounter] == "stay")
                    {

                        hud.setStatusMessage("Table will NOT rotate");

                    }
                    else if (condition[taskCounter] == "walk")
                    {
                        if (start[taskCounter] == "A")
                        {
                            hud.setStatusMessage("Walking >>> \n\n Table will NOT rotate");
                        }
                        else if (start[taskCounter] == "B")
                        {
                            hud.setStatusMessage("Walking <<< \n\n Table will NOT rotate");
                        }
                    }
                }
                hud.statusMessage.SetActive(true);
            }

            // Remove message after 2 sec
            if (timerSpawnReached == false && timer >= 4f)
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
        }

    
        // Objects to be viewed by the participant appear after 2 seconds pass of the participant standing in the empty room
        if (timerSpawnReached == false && timer >= objectsAppear)
        {
            Debug.Log("Objects now Appearing");
            initETKDelay = false;
            Debug.Log(seenObject);
            seenObject.SetActive(true);
            timerSpawnReached = true;
        }


        // Objects will despawn after 7 seconds pass. (Might want to skip this part as instead the hud.blackout might do the trick instead.
        if (timerDespawnItemsReached == false && timer >= screenBlackout)
        {
            Debug.Log("Now moving on to BlackoutPath");
            //seenObject.SetActive(false);

            if (vrEnabled)
            {
                hud.fadeScreen.SetActive(false);
                hud.fadeScreen.GetComponent<CanvasGroup>().alpha = 1.0f;
                //hud.cameraScreen.SetActive(false);
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

    IEnumerator Trial()
    {
        yield return new WaitForSecondsRealtime(2f);
        seenObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);
    }

}
