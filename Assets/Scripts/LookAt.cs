using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        if(target != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - target.transform.position);
        }        
    }
}
