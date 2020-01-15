using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrObjectActivateRigidody : VRObject
{
    public Rigidbody rigidbody;
    public VoiceImageCanvasSync synchronizer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void interact()
    {
        base.interact();
        rigidbody.useGravity = true;
        if (synchronizer != null)
        {
            synchronizer.NextSync();
        }
    }
}
