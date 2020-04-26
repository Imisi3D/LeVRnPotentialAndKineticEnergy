using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSynAndPlayCW : VRObject
{
    public AudioClip audioClipCorrect;
    public AudioClip audioClipWrong;
    public AudioSource audioSource;
    public VoiceImageCanvasSync synchronizer;
    public static int Compt = 0;
    public bool test = false;
    public bool bCanInteract = true;
    public NextSynAndPlayCW[] otherOptions;

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
        
        if (Compt == 6)
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
        else if(Compt==7)
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
        
    }
    public override void interact()
    {
        if (!bCanInteract) return;
        base.interact();
        if (test)
        {
            Compt++;
            
        }
        synchronizer.NextSync();
        bCanInteract = false;
        foreach (NextSynAndPlayCW option in otherOptions)
            option.bCanInteract = false;
    }
}
