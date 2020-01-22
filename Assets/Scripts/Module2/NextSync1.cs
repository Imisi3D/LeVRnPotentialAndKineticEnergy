using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSync1 : VRObject
{
    public AudioClip audioClipCorrect;
    public AudioClip audioClipWrong;
    public AudioSource audioSource;
    public VoiceImageCanvasSync synchronizer;
    public static int Compt = 0;
    public bool test = false;

    IEnumerator ExampleCoroutine(AudioSource audioSource)
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        if (synchronizer != null)
        {
            synchronizer.NextSync();
        }
    }

    void Start()
    {
        Compt += 2;
        if (Compt == 8)
        {

            if (audioClipWrong != null)
            {
                audioSource.clip = audioClipWrong;

                audioSource.Play();
                StartCoroutine(ExampleCoroutine(audioSource));
            }
        }
        else if (Compt == 9)
        {

            if (audioClipCorrect != null)
            {
                audioSource.clip = audioClipCorrect;

                audioSource.Play();
                StartCoroutine(ExampleCoroutine(audioSource));
            }
        }

    }
    public override void interact()
    {
        base.interact();
        if (test)
        {
            Compt++;
        }
        synchronizer.NextSync();
    }
}
