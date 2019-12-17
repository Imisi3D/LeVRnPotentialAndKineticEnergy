using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAlongPoints : CustomComponent
{
    public GameObject[] destinationPoints;
    public int currentDestinationIndex = 0;
    public float speed = 5;
    public float rotationSpeed = 30f;

    void Update()
    {
        GameObject targetObject = destinationPoints[currentDestinationIndex];
        Vector3 relativePos = targetObject.transform.position - gameObject.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(relativePos, transform.up);
        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        gameObject.transform.position += transform.forward * speed * Time.deltaTime;
        Vector3 distance = gameObject.transform.position - targetObject.transform.position;
        float dist = distance.magnitude;
        if (dist < 0.5)
        {
            if (currentDestinationIndex < destinationPoints.Length - 1)
                currentDestinationIndex++;
            else
                enabled = false;
        }
    }
}
