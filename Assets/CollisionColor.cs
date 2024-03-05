using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollisionColor : MonoBehaviour
{
 
    public Material normalColor;
    public Material collidedColor;

    // Did anything enter the collider?
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<Renderer>().material = collidedColor;
    }

}