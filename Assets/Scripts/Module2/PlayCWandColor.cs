﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayCWandColor : VRObject
{
    public static int cpt=0 ;
    public AudioClip audioClipCorrect;
    public AudioClip audioClipWrong;
    public AudioSource audioSource;
    public VoiceImageCanvasSync synchronizer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator ExampleCoroutine(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        if (synchronizer != null)
        {
            synchronizer.NextSync();
        }
    }
    public override void interact()
    {
        base.interact();

        if (audioClipCorrect != null)
        {
            audioSource.clip = audioClipCorrect;
            audioSource.Play();
            image.color = Correct_Color;
            cpt = 0;
            StartCoroutine(ExampleCoroutine(audioSource));

        }
        else if (audioClipWrong != null)
        {
            audioSource.clip = audioClipWrong;
            image.color = wrong_Color;
            audioSource.Play();
            cpt++;
            if (cpt == 2)
            {
                cpt = 0;
                StartCoroutine(ExampleCoroutine(audioSource));

            }
        }

    }
}
