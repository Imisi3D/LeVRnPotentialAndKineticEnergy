using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectMovement : CustomComponent
{
    // Transform of object on which movement will be applied.
    public Transform objectToMove;

    // Transform of target location.
    public Transform targetLocation;

    // Movement speed.
    public float movementSpeed = 1f;

    // Distance threshold at which destination is considered as reached.
    public float threshold = 0.2f;

    void Start()
    {

    }

    void Update()
    {
        Vector3 offset = targetLocation.position - objectToMove.position;
        if (offset.magnitude < threshold)
            enabled = false;
        objectToMove.position += offset.normalized * movementSpeed * Time.deltaTime;
    }
}
