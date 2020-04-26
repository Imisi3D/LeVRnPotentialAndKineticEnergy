using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Plays a given sound when the parent object hits the ground. Sound can be played for the first time only and can be played anytime the object hits the ground.
 */
public class PlaySoundOnHitGround : CustomComponent
{
    // distance to the ground. Used to know at what distance the object should be considered on ground. 
    public float distToGround = 0.25f;
    // audio source used to play the on hit audio.
    public AudioSource audioSource;
    // sound to play when parent object hits the ground.
    public AudioClip audioClip;
    /* defines if the sound should be only played once or should be played each time the object is on the ground.
     * set to true if the object will fall to the ground and won't move again.
     */
    public bool shouldPlayOneTimeOnly = true;
    // used to prevent looping audio when object stays on ground.
    private bool isGrounded = false;

    // returns true if the object is at a distance less than distToGround, false if not.
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    void Update()
    {
        if (IsGrounded())
        {
            if (!isGrounded)
            {
                if (audioSource != null && audioClip != null)
                {
                    audioSource.PlayOneShot(audioClip);
                    isGrounded = true;
                    if (shouldPlayOneTimeOnly)
                        enabled = false;
                }
            }
        }
        else
        {
            isGrounded = false;
        }
    }
}
