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
                StartCoroutine(ResumeSynchronizerAfterDelay(audioSource));
            }
        }
        else if(Compt==9)
        {
            
            if (audioClipCorrect != null)
            {
                audioSource.clip = audioClipCorrect;
                Compt = 0;
                audioSource.Play();
                StartCoroutine(ResumeSynchronizerAfterDelay(audioSource));
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
