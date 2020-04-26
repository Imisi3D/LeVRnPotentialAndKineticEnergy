using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCar : MonoBehaviour
{
    public float speed = 3f;
    public bool bIsMoving = false;
    public Transform startPoint;
    public Transform endPoint;
    public GameObject[] wheels;

    void Update()
    {
        if (bIsMoving)
        {
            Vector3 pos = gameObject.transform.position;
            float offset = Time.deltaTime * speed * Mathf.Sign(endPoint.position.x - startPoint.position.x);
            gameObject.transform.position = new Vector3(
                pos.x + offset,
                pos.y,
                pos.z
                );
            float wheelRotationOffset = (offset / 6.28f) * 360 * Mathf.Sign(offset);
            foreach(GameObject wheel in wheels)
            {
                wheel.transform.Rotate(Vector3.right, wheelRotationOffset);
            }
            if (Mathf.Abs(transform.position.x - endPoint.position.x) < 1)
            {
                transform.position = new Vector3(startPoint.position.x, transform.position.y, transform.position.z);
            }
        }
    }
}
