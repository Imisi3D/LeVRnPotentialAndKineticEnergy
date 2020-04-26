using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : CustomComponent
{
    public AudioSource audioSource;
    public AudioClip soundToPlay;

    private void OnEnable()
    {
        if (audioSource == null || soundToPlay == null) return;
        audioSource.PlayOneShot(soundToPlay);
    }
}
