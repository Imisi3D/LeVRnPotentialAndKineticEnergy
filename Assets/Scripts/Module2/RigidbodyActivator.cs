using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyActivator : CustomComponent
{

    public Rigidbody rigidbody;
    public float activationDelay = 1f;

    void Start()
    {
        StartCoroutine(CallAfterDelay(() => { rigidbody.useGravity = true; }, activationDelay));
    }


    private IEnumerator CallAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
