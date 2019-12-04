using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDraggableObjectTarget : VRObject
{
    /*
     * The type of object which this target supports
     * If this target supports more than one type, separate types which a ; (e.g: ui;model;formule)
     */
    public string requiredType;

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
                if (containsType(obj.type))
                {
                    pointer.attachedObject.transform.parent = gameObject.transform.GetChild(0).transform;
                    pointer.attachedObject.transform.localPosition = Vector3.zero;
                }
                else
                {
                    pointer.DisplayMessage("This is not the right place for that.", 2);
                }
            }
            else
            {

            }
        }
    }

    private bool containsType(string type)
    {
        string[] str = requiredType.Split(';');
        foreach (string s in str)
            if (s.Equals(type))
                return true;
        return false;
    }
}
