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

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_GazeRaySample : ExperimentTask
            {
                public int participant_num;
                private static StreamWriter sw;
                private static FocusInfo focusInfo;
                private Ray ray;
                CultureInfo culture = new CultureInfo("en-us");
                private static VerboseData verboseData;

                // Debug vars
                GameObject hitObject;
                GameObject prevObj;
                Vector3 og_scale;
                public bool debug = true;
                //

                private new void Start()
                {

                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }

                    //
                    // CHANGE PATH FOR THE FILE YOU WANT TO SAVE!
                    //
                    string path = "C:\\Users\\psych-hscl-test\\Documents\\GitHub\\Viewpoint_task_VR\\Assets\\Eye_Tracking_Output\\";
                    string num = participant_num + "_eye_data.txt";
                    string location = path + num;

                    while (File.Exists(location))
                    {
                        participant_num++;
                        location = path + participant_num + "_eye_data.txt";
                    }
                    sw = new StreamWriter(location, false);
                    sw.WriteLine("Starting Experiment now at " + DateTime.Now.ToString(culture));
                    sw.WriteLine();


                }


                private void Update()
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
                    if (hitObject != null)
                    {

                        Vector3 user = Camera.main.transform.position + Camera.main.transform.rotation * ray.origin;
                        sw.WriteLine($"Time: {DateTime.Now.ToString(culture)}   |     Distance: {focusInfo.distance}    |   Participant Coords: {user}   |   " +
                            $"Item Coord: {focusInfo.point}   |  Item: {hitObject.name}  |   Pupil Size (Left, right): {verboseData.left.pupil_diameter_mm} {verboseData.right.pupil_diameter_mm}");

                        // DEBUGGING SEGMENT TO SHOW WHERE THE PARTICIPANT IS LOOKING AT AND CHECKING THAT EYE TRACKING WORKS. OBJECTS WILL RESCALE/RESIZE WHEN YOU LOOK AT IT
                        if (debug)
                        {
                            if (prevObj == null)
                            {
                                prevObj = hitObject;
                                og_scale = hitObject.transform.localScale;
                                hitObject.transform.localScale = og_scale * 8/9;
                            }
                            else if (prevObj != hitObject)
                            {
                                prevObj.transform.localScale = og_scale;
                                og_scale = hitObject.transform.localScale;
                                hitObject.transform.localScale = og_scale * 8/9;
                                prevObj = hitObject;
                            }
                        }
                    }
                }

                public void Release()
                {
                    sw.WriteLine();
                    sw.WriteLine("Ending experiment at " + DateTime.Now.ToString(culture));
                    sw.Close();
                }

            }
        }
    }
}
