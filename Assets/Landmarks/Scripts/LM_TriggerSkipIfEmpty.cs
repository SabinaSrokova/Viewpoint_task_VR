using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LM_TriggerSkipIfEmpty : MonoBehaviour
{

    // Start is called before the first frame update
    void Awake()
    {
        ExperimentTask taskToSkip = gameObject.AddComponent<ExperimentTask>();
        if (transform.childCount == 0)
        {
            taskToSkip.skip = true;
        }
        else taskToSkip.skip = false;
    }

}
