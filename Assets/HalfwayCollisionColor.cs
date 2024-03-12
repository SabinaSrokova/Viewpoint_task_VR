using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfwayCollisionColor : MonoBehaviour
{
    public Material normalColor;
    public Material collidedColor;
    public static bool half_reached = false;

    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Renderer>().material = collidedColor;
        half_reached = true;
    }

}
