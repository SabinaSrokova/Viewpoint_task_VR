using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollisionColor : MonoBehaviour
{
 
    public Material normalColor;
    public Material collidedColor;
    private List<string> condition = new List<string> { };
    private int taskCounter = new int();
    
    // Did anything enter the collider?
    private void OnTriggerEnter(Collider other)
    {
        taskCounter = GameObject.Find("Counter").GetComponent<LM_DummyCounter>().counter;
        condition = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().condition;


        if (LM_ToggleObjects.participantReady)
        {
            if (condition[taskCounter] == "walk")
            {
                GetComponent<Renderer>().material = collidedColor;
            } 
            else if (condition[taskCounter] == "stay") 
            {
                if (HalfwayCollisionColor.half_reached) // only if half point has been reached. This is turned back to false at the end of LM_BlackoutPath
                {
                    GetComponent<Renderer>().material = collidedColor;
                }
            }

            //log.log("MARKER_REACHED\t" + "Final target point");
        }
        else
        {
            GetComponent<Renderer>().material = collidedColor; // turns green at the beginning of trial, assuming things went right and Pp is standing on it.
        }
    }

}