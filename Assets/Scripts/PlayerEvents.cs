using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/**
 * Handler of player input events
 */
public class PlayerEvents : MonoBehaviour
{
    // anchor references.
    public GameObject LeftAnchor;
    public GameObject RightAnchor;
    public GameObject HeadAnchor;

    // game control canvas
    public GameObject exit_restart_canvas;
    // audio sources, will be paused and resumed according to interaction with exit_restart_canvas.
    public AudioSource[] audioSources;

    private Dictionary<OVRInput.Controller, GameObject> ControllerSets = null;
    private OVRInput.Controller inputSource = OVRInput.Controller.None;
    private OVRInput.Controller controller = OVRInput.Controller.None;

    public static UnityAction<bool> onHascontroller = null;
    public static UnityAction onTriggerUp = null;
    public static UnityAction onTriggerDown = null;
    public static UnityAction onTouchpadUp = null;
    public static UnityAction onTouchpadDown = null;
    public static UnityAction<OVRInput.Controller, GameObject> OnControllerSource = null;

    private bool hasController = false;
    private bool inputActive = true;

    public void Awake()
    {
        OVRManager.HMDMounted += PlayerFound;
        OVRManager.HMDUnmounted += PlayerLost;
        ControllerSets = CreateControllerSets();
    }

    private void OnDestroy()
    {
        OVRManager.HMDMounted -= PlayerFound;
        OVRManager.HMDUnmounted -= PlayerLost;
    }

    void Start()
    {

    }

    void Update()
    {
        if (!inputActive) return;
        hasController = checkForController(hasController);


        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        if (triggerValue >= 0.5)
        {
            print("trigger pressed");
        }

        checkInputSource();
        Input();
    }

    private bool checkForController(bool currentValue)
    {
        OVRInput.Controller controllerCheck = controller;

        if (OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote))
            controllerCheck = OVRInput.Controller.RTrackedRemote;

        if (OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
            controllerCheck = OVRInput.Controller.LTrackedRemote;

        if (!OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote) &&
         !OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote))
            controllerCheck = OVRInput.Controller.Touchpad;

        controller = UpdateSource(controllerCheck, controller);

        return true;
    }

    private void checkInputSource()
    {
        if (OVRInput.GetDown(OVRInput.Button.Any, OVRInput.Controller.LTrackedRemote))
        {
            Debug.Log("left input");
            Debug.Log("controller check: " + controller);
        }

        if (OVRInput.GetDown(OVRInput.Button.Any, OVRInput.Controller.RTrackedRemote))
        {
            Debug.Log("right input");
            Debug.Log("controller check: " + controller);
        }

        if (OVRInput.GetDown(OVRInput.Button.Any, OVRInput.Controller.Touchpad))
        {
            Debug.Log("head input");
            Debug.Log("controller check: " + controller);
        }

        inputSource = UpdateSource(OVRInput.GetActiveController(), inputSource);
    }

    private void PlayerFound()
    {
        inputActive = true;
    }
    private void PlayerLost()
    {
        inputActive = false;
    }

    private Dictionary<OVRInput.Controller, GameObject> CreateControllerSets()
    {
        Dictionary<OVRInput.Controller, GameObject> newSets = new Dictionary<OVRInput.Controller, GameObject>()
        {
            { OVRInput.Controller.LTrackedRemote, LeftAnchor },
            { OVRInput.Controller.RTrackedRemote, RightAnchor },
            { OVRInput.Controller.Touchpad, HeadAnchor }

        };
        return newSets;
    }

    public OVRInput.Controller UpdateSource(OVRInput.Controller check, OVRInput.Controller previous)
    {
        if (check == previous)
            return previous;
        GameObject controllerObject = null;
        ControllerSets.TryGetValue(check, out controllerObject);

        if (controllerObject == null)
            controllerObject = HeadAnchor;
        print(controllerObject.name);
        if (OnControllerSource != null)
            OnControllerSource(check, controllerObject);

        return check;
    }

    private void Input()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            if (onTriggerDown != null)
            {
                onTriggerDown();
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
        {
            if (onTriggerUp != null)
                onTriggerUp();
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad))
        {
            if (onTouchpadDown != null)
                onTouchpadDown();
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad))
        {
            if (onTouchpadUp != null)
                onTouchpadUp();
        }

        if (OVRInput.GetDown(OVRInput.Button.Back))
        {
            Canvas canvas = exit_restart_canvas.GetComponent<Canvas>();
            if (canvas.enabled)
            {
                canvas.enabled = false;
                Time.timeScale = 1;
                foreach (AudioSource audio in audioSources)
                    audio.UnPause();
            }
            else
            {
                canvas.enabled = true;
                Time.timeScale = 0;
                foreach (AudioSource audio in audioSources)
                    audio.Pause();
            }
        }
    }
}
