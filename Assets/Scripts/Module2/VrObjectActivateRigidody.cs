using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrObjectActivateRigidody : VRObject
{
    public static int count=0;
    public bool UseCount = true;
    public int number;
    public Rigidbody rigidbody;
    public VoiceImageCanvasSync synchronizer;
    public AudioClip interactionAudioClip;
    public AudioSource audioSource;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public override void interact()
    {
        base.interact();
        rigidbody.useGravity = true;
        if (audioSource != null && interactionAudioClip != null)
            audioSource.PlayOneShot(interactionAudioClip);
        if (UseCount)
        {
            count++;
            if (count == number)
            {
                if (synchronizer != null)
                {
                    synchronizer.NextSync();

                }
            }
        }
        else if (!UseCount)
        {
            if (synchronizer != null)
            {
                synchronizer.NextSync();
            }
        }
        
    }
}
