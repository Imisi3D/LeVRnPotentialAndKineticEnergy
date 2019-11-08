using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * A component for an object that can be interacted with using VR pointer.
 * This component was created to make the interaction with fruits during the fruit saber possible.
 */
public class Interactible : MonoBehaviour
{
    /**
     * type of this interactible. 
     * like variable, algebraicexpression...
     */
    public string type = "";

    /**
     * The component that is responsible for both scripts Week1Class1Part1 and Week1Class1Part2.
     */
    public NumberInputInteraction interactor;


    /**
     * Hand anchor, used to attach toygun to player's hand.
     */
    public GameObject handAnchor;

    /**
     * Text Content of this interactible.
     */
    public string value;

    /**
     * Text component for this interactible.
     */
    public UnityEngine.UI.Text text;

    // only required for the toy gun
    public Material DefaultMaterial;
    // only required for the toy gun
    public Material HighlightMaterial;
    // only used with toygun. if material should be toggled or not.
    private bool MaterialToggle = false;

    // called when this object in pressed. An object is pressed when the pointer is on that object and the trigger or touch pad are pressed.
    public void Pressed()
    {
        if (gameObject.tag == "ToyGun")
        {
            if (interactor.audioSource.isPlaying) return;
            transform.parent = handAnchor.transform;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = new Vector3(0f, -90f, 0f);
            interactor.FruitGame_hasToyGun = true;
            transform.GetChild(0).gameObject.SetActive(false);
            interactor.FruitGame_instructionText.text = "Shoot at 4 fruits with Algebraic Expressions on them";
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            interactor.shouldHightlightToygun = false;
            if(interactor.FruitGame_CurrentStep == 1)
            {
                StartCoroutine(interactor.FruitGame_NextStep());
            }
            renderer.material = DefaultMaterial;
            return;
        }

        if (!interactor.FruitGame_hasToyGun) return;
        if (interactor.audioSource.isPlaying) return;

        if (interactor.FruitGame_DemandedType.Equals(type))
        {
            interactor.audioSource.PlayOneShot(interactor.gun_shot, 0.2f);
            interactor.FruitGame_currentProgress++;
            interactor.FruitGame_currentConsecutive++;
            interactor.PlaySoundForCurrentFruitGameProgress();
            gameObject.SetActive(false);
        }
        else
        {
            interactor.audioSource.PlayOneShot(interactor.error);
            interactor.FruitGame_currentConsecutive = 0;
            interactor.playSound(interactor.FruitGame_wrong);
            StartCoroutine(Shake());
            OVRInput.SetControllerVibration(1, 1, OVRInput.Controller.RTouch);
        }
    }

    // Shakes this object.
    public IEnumerator Shake()
    {
        Vector3 originalPosition = transform.position;
        float ShakeStartedAt = Time.time;
        while (Time.time - 0.50f < ShakeStartedAt)
        {
            transform.position = originalPosition + transform.right * Mathf.Sin(Time.time * 6000) * 0.035f;
            yield return new UnityEngine.WaitForSeconds(0.01f);
        }
        transform.position = originalPosition;
    }
}
