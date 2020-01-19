using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDraggableObjectTarget : VRObject
{
    /*
     * The type of object which this target supports
     * If this target supports more than one type, separate types which a ; (e.g: ui;model;formule)
     */
    public AudioClip audioClipCorrect;
    public AudioClip audioClipWrong;
    public AudioSource audioSource;
    public string requiredType;
    public MultiStepInteraction interactionManager;
    public int remainingTrials = 0;
    public bool bIsAnswerHolder = false;

    public void react(Pointer pointer)
    {
        if (pointer.attachedObject != null)
        {
            VRDraggableObject obj = pointer.attachedObject.GetComponent<VRDraggableObject>();
            if (obj != null)
            {
                if (containsType(obj.type))
                {
                    applyHighlight(HighlightOptions.correct);
                }
                else
                {
                    applyHighlight(HighlightOptions.wrong);
                }
            }
            else
            {
                applyHighlight(HighlightOptions.none);
            }
        }
    }

    public override void interact(Pointer pointer)
    {
        base.interact(pointer);
        if (pointer.attachedObject != null)
        {
            VRDraggableObject obj = pointer.attachedObject.GetComponent<VRDraggableObject>();
            if (obj != null)
            {
                interactionManager.Answer(obj, this);
                if (containsType(obj.type))
                {
                    pointer.Drop(gameObject);
                    if (obj.defaultHolder != gameObject)
                    {
                        obj.applyHighlight(HighlightOptions.correct, true);
                        if (audioClipCorrect != null)
                        {
                            audioSource.clip = audioClipCorrect;
                            audioSource.Play();
                        }
                    }
                }
                else
                {
                    pointer.Drop(obj.defaultHolder);
                    obj.applyHighlight(HighlightOptions.wrong);
                    if (audioClipCorrect != null)
                    {
                        audioSource.clip = audioClipWrong;
                        audioSource.Play();
                    }
                    //pointer.DisplayMessage("This is not the right place for that.", 2);
                }
            }
            else
            {

            }
        }
    }

    public bool containsType(string type)
    {
        string[] str = requiredType.Split(';');
        foreach (string s in str)
            if (s.Equals(type))
                return true;
        return false;
    }
}
