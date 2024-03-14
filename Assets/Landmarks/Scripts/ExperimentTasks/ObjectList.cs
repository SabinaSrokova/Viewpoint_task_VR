/*
    Copyright (C) 2010  Jason Laczko

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
Notes by Matt for ObjectList edits:

This version of ObjectList is edited from the base landmarks version for the VR_Viewpoint task - it was attempted to do this as non-destructively as possible
An enum has been added which adds a drop-down menu to the inspector (spawnStatus & currentSpawnStatus). If "none" is selected, then the behavior of ObjectList should be identical to base Landmarks behavior (none should be the default).
spawn should be selected for the ListViewpointSpawns task object - transfer should be selected for the ListViewpointTransfers. Both should have PreparedRooms as the parent name. 
spawn will prepare a list of the first spawn points within each room. transfer will prepare a list of the end points for each room. This may or may not be the same as spawn, depending on the condition.
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class ObjectList : ExperimentTask {

    [Header("Task-specific Properties")]

    public string parentName = "";
	public GameObject parentObject;
	public int current = 0;
	public enum spawnStatus
	{
		none,
		spawn,
		transfer,
	};
	public spawnStatus currentSpawnStatus;
	
	public List<GameObject> objects;
	public EndListMode EndListBehavior; 
	public bool shuffle;

    private List<string> start = new List<string> { };
    private List<string> end = new List<string> { };


    public override void startTask () {

		//These lines of code load the relevant public lists from LM_PrepareRooms - a line like this is required in order to read data from another script.
        start = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().start;
        end = GameObject.Find("PrepareRooms").GetComponent<LM_PrepareRooms>().end;

		GameObject[] objs;

        if (objects.Count == 0)
        {
            if (parentObject == null & parentName == "") Debug.LogError("No objects found for objectlist.");

            // If parentObject is left blank and parentName is not, use parentName to get parentObject
            if (parentObject == null && parentName != "")
            {
                parentObject = GameObject.Find(parentName);
            }

            objs = new GameObject[parentObject.transform.childCount];

            Array.Sort(objs);

			//modifications

			for (int i = 0; i < parentObject.transform.childCount; i++)
			{
				//Depending on which option is selected in currentSpawnStatus, read the correct PlayerSpawn from SpawnPoints. Two calls to the helper function GetChildGameObject are required due to the nested nature of PreparedRooms
				if (currentSpawnStatus == spawnStatus.spawn)
				{
					GameObject spawn = GetChildGameObject(parentObject.transform.GetChild(i).gameObject, "SpawnPoints");
                    //Debug.Log("PlayerSpawn" + start[i]);
                    //Debug.Log(spawn);
                    objs[i] = GetChildGameObject(spawn, "PlayerSpawn" + start[i]);
					//Debug.Log(objs[i].ToString());
				}
				else if (currentSpawnStatus == spawnStatus.transfer)
				{
					GameObject transfer = GetChildGameObject(parentObject.transform.GetChild(i).gameObject, "SpawnPoints");
					objs[i] = GetChildGameObject(transfer, "PlayerSpawn" + end[i]);
				}
				else
				{
					objs[i] = parentObject.transform.GetChild(i).gameObject;
				}
			}
        }
		else 
		{
			objs = new GameObject[objects.Count];
			for (int i = 0; i < objects.Count; i++)
			{
				objs[i] = objects[i];
			}
		}
        
		// DEPRICATED
		// if (order ) {
		// 	// Deal with specific ordering
		// 	ObjectOrder ordered = order.GetComponent("ObjectOrder") as ObjectOrder;
		
		// 	if (ordered) {
		// 		Debug.Log("ordered");
		// 		Debug.Log(ordered.order.Count);
				
		// 		if (ordered.order.Count > 0) {
		// 			objs = ordered.order.ToArray();
		// 		}
		// 	}
		// }
			
		if ( shuffle ) {
			Experiment.Shuffle(objs);				
		}
		
		TASK_START();
	 
		foreach (GameObject obj in objs) {	             
        	objects.Add(obj);
			//log.log("TASK_ADD	" + name  + "\t" + this.GetType().Name + "\t" + obj.name  + "\t" + "null",1 );
		}
	}	
	
	public override void TASK_ADD(GameObject go, string txt) {
		objects.Add(go);
	}
	
	public override void TASK_START()
	{
		base.startTask();		
		if (!manager) Start();

		objects = new List<GameObject>();
	}
	
	public override bool updateTask () {
	    return true;
	}
	public override void endTask() {
		//current = 0;
		TASK_END();
	}
	
	public override void TASK_END() {
		base.endTask();
	}
	
	public GameObject currentObject() {
		if (current >= objects.Count) {
			return null;
		} else {
			return objects[current];
		}
	}

    public new void incrementCurrent() 
	{
		current++;
		
		if (current >= objects.Count && EndListBehavior == EndListMode.Loop) {
			current = 0;
		}
	}
    public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        var allKids = fromGameObject.GetComponentsInChildren<Transform>();
        var kid = allKids.FirstOrDefault(k => k.gameObject.name == withName);
        if (kid == null) return null;
        return kid.gameObject;
    }
}
