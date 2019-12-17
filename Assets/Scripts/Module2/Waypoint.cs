using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
