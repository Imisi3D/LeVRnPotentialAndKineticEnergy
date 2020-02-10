using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTo : CustomComponent
{
    // sync manager
    public VoiceImageCanvasSync synchronizer;

    // location and rotation to teleport to.
    public Transform destination;

    // game object which should be teleported
    public GameObject Object;

    // if true then the moment this component is enabled, @object will be teleported to @destination
    public bool telepordOnEnabled = false;

    public void teleportToDestination()
    {
        Object.transform.position = destination.position;
        Object.transform.rotation = destination.rotation;
        gameObject.SetActive(false);
        if (synchronizer != null)
            synchronizer.NextSync();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            teleportToDestination();
        }
    }

    private void OnEnable()
    {
        if (telepordOnEnabled)
            teleportToDestination();
    }
}
