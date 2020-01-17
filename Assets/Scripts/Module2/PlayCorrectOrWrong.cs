using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCorrectOrWrong : VRObject
{
    public AudioClip audioClip;
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
    
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            
            audioSource.Play();
            StartCoroutine(ExampleCoroutine(audioSource));
        }
        
    }
}
