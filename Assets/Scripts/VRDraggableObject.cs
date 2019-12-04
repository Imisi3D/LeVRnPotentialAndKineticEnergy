using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDraggableObject : VRObject
{
    public string type;


    void Start()
    {
        //applyHighlight(HighlightOptions.correct, true);
    }

    void Update()
    {

    }

    public override void interact(Pointer pointer)
    {
        pointer.Drag(gameObject);
        print(gameObject.name);
    }


}
