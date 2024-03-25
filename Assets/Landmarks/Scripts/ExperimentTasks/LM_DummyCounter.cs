/*
    LM DummyCounter
       
    Helper script who only has the function of providing a counter accesible to other scripts within this trial. Arguable if it's necessary at all but here it is for now.

    Script "written" by Matthew Watson

    Copyright (C) 2019 Michael J. Starrett

    Navigate by StarrLite (Powered by LandMarks)
    Human Spatial Cognition Laboratory
    Department of Psychology - University of Arizona   
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LM_DummyCounter : ExperimentTask
{
    [Header("Task-specific Properties")]
    public GameObject dummyProperty;
    public int counter = 0;
    private bool first = true;

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

       if (first) {
            first = false;
            return;
       }
       counter++;
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

}