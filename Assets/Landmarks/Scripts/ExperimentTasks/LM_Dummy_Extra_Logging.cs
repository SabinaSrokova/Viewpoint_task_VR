///*
//    LM Dummy
       
//    Attached object holds task components that need to be effectively ignored 
//    by Tasklist but are required for the script. Thus the object this is 
//    attached to can be detected by Tasklist (won't throw error), but does nothing 
//    except start and end.   

//    Copyright (C) 2019 Michael J. Starrett

//    Navigate by StarrLite (Powered by LandMarks)
//    Human Spatial Cognition Laboratory
//    Department of Psychology - University of Arizona   
//*/

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LM_Dummy_Extra_Logging : ExperimentTask
//{
//    [Header("Task-specific Properties")]
//    public GameObject dummyProperty;

//    public override void startTask()
//    {
//        TASK_START();

//        // LEAVE BLANK
//    }


//    public override void TASK_START()
//    {
//        if (!manager) Start();
//        base.startTask();

//        if (skip)
//        {
//            log.log("INFO    skip task    " + name, 1);
//            return;
//        }
//        var pathAccess = GetComponent<LM_BlackoutPath>();

//        taskLog.AddData("TargetPos_x", pathAccess.disc.transform.position.x.ToString());
//        taskLog.AddData("TargetPos_z", pathAccess.disc.transform.position.z.ToString());
//        taskLog.AddData("TargetRot_x", pathAccess.disc.transform.rotation.x.ToString());
//        taskLog.AddData("TargetRot_z", pathAccess.disc.transform.rotation.z.ToString());
//        taskLog.AddData("Start_SubPos_x", pathAccess.playerStartPos.x.ToString());
//        taskLog.AddData("Start_SubPos_z", pathAccess.playerStartPos.z.ToString());
//        taskLog.AddData("Start_SubRot_x", pathAccess.playerStartRot.x.ToString());
//        taskLog.AddData("Start_SubRot_z", pathAccess.playerStartRot.z.ToString());
//        taskLog.AddData("End_SubPos_x", pathAccess.playerEndPos.x.ToString());
//        taskLog.AddData("End_SubPos_z", pathAccess.playerEndPos.z.ToString());
//        taskLog.AddData("End_SubRot_x", pathAccess.playerEndRot.x.ToString());
//        taskLog.AddData("End_SubRot_z", pathAccess.playerEndRot.z.ToString());
//        taskLog.AddData("Resp_Time", pathAccess.timer.ToString());
//    }


//    public override bool updateTask()
//    {
//        return true;

//        // WRITE TASK UPDATE CODE HERE
//    }


//    public override void endTask()
//    {
//        TASK_END();

//        // LEAVE BLANK
//    }


//    public override void TASK_END()
//    {
//        base.endTask();

//        // WRITE TASK EXIT CODE HERE
//    }

//}