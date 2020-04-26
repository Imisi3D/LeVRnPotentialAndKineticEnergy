using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyActivator : CustomComponent
{

    public Rigidbody rb;
    public float activationDelay = 1f;
    public AudioSource audioSource;
    public AudioClip soundOnActivation;

    void Start()
    {
        StartCoroutine(CallAfterDelay(() => { 
            rb.useGravity = true;
            if (audioSource != null && soundOnActivation != null)
                audioSource.PlayOneShot(soundOnActivation);
        }, activationDelay));
    }


    private IEnumerator CallAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
