using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Pointer : MonoBehaviour
{
    // distance that this pointer does ray casts on.
    public float distance = 10.0f;
    // line renderer to show current pointing action.
    public LineRenderer lineRenderer = null;
    // layer used when ray casting
    public LayerMask everythingMask = 0;
    // layer used when ray casting
    public LayerMask interactibleMask = 0;
    // origin of pointer
    private Transform currentOrigin = null;
    // called when pointer is updating location and rotation.
    public UnityAction<Vector3, GameObject> OnPointerUpdate = null;
    // object that this pointer is pointing on (fruit, toygun...)
    private GameObject currentObject = null;

    private void Awake()
    {
        PlayerEvents.OnControllerSource += UpdateOrigin;
        PlayerEvents.onTriggerDown += ProcessTriggerDown;
        PlayerEvents.onTouchpadDown += ProcessTouchpadDown;

    }

    void Start()
    {

    }

    void Update()
    {
        if (QualitySettings.antiAliasing != 8)
        {
            QualitySettings.antiAliasing = 8;
            UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.8f;
        }

        Vector3 hitPoint = UpdateLine();
        currentObject = UpdatePointerStatus();
        if (OnPointerUpdate != null)
            OnPointerUpdate(hitPoint, currentObject);

        if (Input.GetKeyDown("space"))
        {
            if (!currentObject)
            {
                print("no object is interactible");
                return;
            }
            Interactible interactible = currentObject.GetComponent<Interactible>();
            if (interactible)
            {
                interactible.Pressed();
                print("interactible pressed");
            }
        }
    }

    public Vector3 UpdateLine()
    {
        RaycastHit hit = CreateRaycast(everythingMask);
        SetLinecolor();
        if (currentOrigin == null) return Vector3.zero;
        Vector3 endPosition = currentOrigin.position + currentOrigin.forward * distance;
        if (hit.collider != null)
            endPosition = hit.point;
        lineRenderer.SetPosition(0, currentOrigin.position);
        lineRenderer.SetPosition(1, endPosition);
        return endPosition;
    }

    private RaycastHit CreateRaycast(int layer)
    {
        RaycastHit hit;
        if (currentOrigin != null)
        {
            Ray ray = new Ray(currentOrigin.position, currentOrigin.forward);
            Physics.Raycast(ray, out hit, distance, layer);
            return hit;
        }
        else
            print("controller not set yet");
        return new RaycastHit();
    }

    private void SetLinecolor()
    {
        if (!lineRenderer) return;
        Color endColor = Color.white;
        endColor.a = 0.0f;
        lineRenderer.endColor = endColor;
        Color startColor = Color.white;
        startColor.a = 1.0f;
        lineRenderer.startColor = startColor;
    }

    private void OnDestroy()
    {
        PlayerEvents.OnControllerSource -= UpdateOrigin;
        PlayerEvents.onTriggerDown -= ProcessTriggerDown;
        PlayerEvents.onTouchpadDown -= ProcessTouchpadDown;
    }



    private void UpdateOrigin(OVRInput.Controller controller, GameObject controllerObject)
    {
        currentOrigin = controllerObject.transform;
        if (controller == OVRInput.Controller.Touchpad)
        {
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.enabled = true;
        }
    }

    private GameObject UpdatePointerStatus()
    {
        RaycastHit hit = CreateRaycast(interactibleMask);
        if (hit.collider)
            return hit.collider.gameObject;
        return null;
    }

    private void ProcessTriggerDown()
    {
        if (!currentObject) return;
        Interactible interactible = currentObject.GetComponent<Interactible>();
        interactible.Pressed();

    }


    private void ProcessTouchpadDown()
    {
        if (!currentObject)
            return;
        Interactible interactible = currentObject.GetComponent<Interactible>();
        interactible.Pressed();
    }
}
