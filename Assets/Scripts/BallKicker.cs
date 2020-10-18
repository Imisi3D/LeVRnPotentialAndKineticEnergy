using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class BallKicker : CustomComponent
{
    public Rigidbody rigidbody;
    public Vector3 kickDirection;
    public AudioSource audioSource;
    public AudioClip kickSound;

    void Start() { }

    void OnEnable()
    {
        rigidbody.AddForce(kickDirection);
        audioSource.PlayOneShot(kickSound);
    }
}
