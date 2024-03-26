//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
/*
 * Modified SRanipal_GazeRaySample.cs by Melanie Gin
 * This is where all the eye tracking is. It uses Focus() to find any items with a collider that has been hit by its ray. Then that item is recorded depening on the tag.
 * Modify the path that you want to save your output to. And modify tags here if you tagged the items something different.
 * 
 */


using System.Runtime.InteropServices;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using ViveSR.anipal.Eye;
using Valve.VR;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_GazeRaySample : ExperimentTask
            {
                private static StreamWriter output;
                private static FocusInfo focusInfo;
                private Ray ray;
                CultureInfo culture = new CultureInfo("en-us");
                private static VerboseData verboseData;
                private string prev_room = "begin"; // this is to organize the logging

                // Debug vars
                GameObject hitObject;
                GameObject prevObj;
                Vector3 og_scale;
                public bool debug = true;
                //[SerializeField] private Material highlightMaterial;
                //[SerializeField] private Material defaultMaterial;
                List<String> ignore; // Objects to ignore during eye tracking debugging
                //

                private new void Start()
                {
                    //Thread.Sleep(10000); // Don't record eye tracking immediately. Waits until the participant is finished listening and reading instructions
                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }

                    //
                    // CHANGE PATH FOR THE FILE YOU WANT TO SAVE!
                    //
                    string path = Directory.GetCurrentDirectory() + "\\Output\\" + Config.Instance.subject + "_eye_data.csv";

                    output = new StreamWriter(path);
                    output.WriteLine("Time, Room, Distance, Subject Coord, Item Coord, Item, Pupil Size(left right)");

                    ignore = new List<String>
                    {
                        "Floor 1",
                        "w1",
                        "w2",
                        "w3",
                        "w4",
                        "Ceiling 1",
                        "Main_floor"
                    };
                }


                private void Update()
                {
                    
                    if (GameObject.Find("BlockInstruction").GetComponent<InstructionsTaskViewpoint>().start_eye_recording)
                    {
                        // Use built-in focus method to get object ("focus point data is calculated using a variety of factors, including the user's eye position, head position, and the scene geometry")
                        if (SRanipal_Eye.Focus(GazeIndex.COMBINE, out ray, out focusInfo, 100f)) { }
                        else if (SRanipal_Eye.Focus(GazeIndex.LEFT, out ray, out focusInfo, 100f)) { }
                        else if (SRanipal_Eye.Focus(GazeIndex.RIGHT, out ray, out focusInfo, 100f)) { }
                        else
                        {
                            return;
                        }

                        SRanipal_Eye.GetVerboseData(out verboseData);

                        // RECORD THE OBJECTS BEING LOOKED AT
                        // If we want more info on the hit we could try this method: https://gamedevbeginner.com/raycasts-in-unity-made-easy/#:~:text=Or%2C%20you%20could%20even%20use%20Raycast%20Hit%20to,first%20object%20that%20is%20hit%20by%20the%20Ray.
                        hitObject = focusInfo.collider.gameObject;
                        bool blackoutPath = GameObject.Find("BlackoutWalking").GetComponent<LM_BlackoutPath>().blackout;
                        if ( hitObject != null && !blackoutPath && !GameObject.Find("ViewObjects").GetComponent<LM_ToggleObjects>().blackout )
                        {

                            Vector3 user = Camera.main.transform.position + Camera.main.transform.rotation * ray.origin;
                            var room = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().room;
                            var count = GameObject.Find("Counter").GetComponent<LM_DummyCounter>().counter;
                            var current_room = room[count];
                            if (prev_room != current_room)
                            {
                                prev_room = current_room;
                                output.WriteLine();
                                output.WriteLine();
                            }
                            
                            output.WriteLine
                                (
                                $"{DateTime.Now.ToString(culture)}," +
                                $"{room[count]}, " +
                                $"{focusInfo.distance}, " +
                                $"{user.ToString().Replace(",", " ")}, " +
                                $"{focusInfo.point.ToString().Replace(",", " ")}, " +
                                $"{hitObject.name}, " +
                                $"{verboseData.left.pupil_diameter_mm}  {verboseData.right.pupil_diameter_mm}"
                                );
                            // DEBUGGING SEGMENT TO SHOW WHERE THE PARTICIPANT IS LOOKING AT AND CHECKING THAT EYE TRACKING WORKS. OBJECTS WILL RESCALE/RESIZE WHEN YOU LOOK AT IT
                            if (debug) Debugging();
                        }
                        else if (blackoutPath) // This is when the subject is in blackout path before they have to decide whether an object moved
                        {
                            output.WriteLine
                                (
                                $"{DateTime.Now.ToString(culture)}," +
                                $"{room[count]}, " +
                                $"BLACKOUT, " +
                                $", " +
                                $", " +
                                $", " +
                                $"{verboseData.left.pupil_diameter_mm}  {verboseData.right.pupil_diameter_mm}"
                                );
                        }
                    }
                }


                private void Debugging()
                {
                    // Commented code is for highlighting items but not all have renderers so it does not work on some. Saved for later
                    //var see = hitObject.transform;
                    //var seeRenderer = see.GetComponent<Renderer>();
                    if (!ignore.Contains(hitObject.name))
                    {
                        if (prevObj == null)
                        {
                            prevObj = hitObject;
                            og_scale = hitObject.transform.localScale;
                            hitObject.transform.localScale = og_scale * 8 / 7;
                            //defaultMaterial = seeRenderer.material;
                            //seeRenderer.material = highlightMaterial;
                        }
                        else if (prevObj != hitObject)
                        {
                            prevObj.transform.localScale = og_scale;
                            prevObj = hitObject;
                            og_scale = hitObject.transform.localScale;
                            hitObject.transform.localScale = og_scale * 8 / 7;

                            //prevObj.transform.GetComponent<Renderer>().material = defaultMaterial;
                            //prevObj = hitObject;
                            //defaultMaterial = seeRenderer.material;
                            //seeRenderer.material = highlightMaterial;


                        }
                    }
                    else
                    {
                        if (prevObj != null)
                        {
                            prevObj.transform.localScale = og_scale;
                            prevObj = null;
                        }
                    }
                
                }

                void OnApplicationQuit()
                {
                    Release();
                }

                public void Release()
                {
                    output.WriteLine();
                    output.WriteLine("Ending experiment at " + DateTime.Now.ToString(culture));
                    output.Close();
                }

            }
        }
    }
}
