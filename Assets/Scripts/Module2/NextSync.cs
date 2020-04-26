using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSync : VRObject
{
    public AudioClip audioClipCorrect;
    public AudioClip audioClipWrong;
    public AudioSource audioSource;
    public VoiceImageCanvasSync synchronizer;
    public static int Compt = 0;
    public bool test = false;
    public bool bCanInteract = true;
    public NextSync[] otherOptions;


    IEnumerator ResumeSynchronizerAfterDelay(AudioSource audioSource)
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
                Compt = 0;
                audioSource.Play();
                synchronizer.explain();
                StartCoroutine(ResumeSynchronizerAfterDelay(audioSource));
            }
        }
        else if (Compt == 9)
        {

            if (audioClipCorrect != null)
            {
                audioSource.clip = audioClipCorrect;
                Compt = 0;
                audioSource.Play();
                synchronizer.explain();
                StartCoroutine(ResumeSynchronizerAfterDelay(audioSource));
            }
        }
        else
        {
            print("WTF count is " + Compt);
        }

    }
    public override void interact()
    {
        if (!bCanInteract) return;
        base.interact();
        print("interacting with " + gameObject.name);
        if (test)
        {
            Compt++;
            print("compt incremented");

        }
        synchronizer.NextSync();
        bCanInteract = false;
        foreach (NextSync option in otherOptions)
            option.bCanInteract = false;
        enabled = false;
    }

    private void OnEnable()
    {
        bCanInteract = true;
    }
}
