using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HighlightOptions
{
    none = 0,
    correct = 1,
    wrong = 2
}

public enum HighlightType
{
    material,
    sprite
}

public class VRObject : MonoBehaviour
{
    public Pointer VRPointer;
    public bool bShouldHighlight = false;

    #region Highlight
    public HighlightType highlightType = HighlightType.material;
    public Material default_Material;
    public Material Correct_Material;
    public Material wrong_Material;
    public Sprite default_Sprite;
    public Sprite Correct_Sprite;
    public Sprite wrong_Sprite;
    #endregion

    // called when parent of this component is clicked on with pointer (either using touch pad or trigger).
    public virtual void interact()
    {

    }

    /*
     * called when parent of this component is clicked on with pointer (either using touch pad or trigger).
     * Can be used the same as interact() but with access to a pointer.
     */
    public virtual void interact(Pointer pointer)
    {

    }


    public void applyHighlight(HighlightOptions highlight)
    {
        currentHighlightOption = highlight;
        switch (highlight)
        {
            case HighlightOptions.none:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        meshRenderer.material = default_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        Image img = gameObject.GetComponent<Image>();
                        img.sprite = default_Sprite;

                    }
                    break;
                }
            case HighlightOptions.correct:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        meshRenderer.material = Correct_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        Image img = gameObject.GetComponent<Image>();
                        img.sprite = Correct_Sprite;

                    }
                    break;
                }
            case HighlightOptions.wrong:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        meshRenderer.material = wrong_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        Image img = gameObject.GetComponent<Image>();
                        img.sprite = wrong_Sprite;
                    }
                    break;
                }
        }
    }

    public HighlightOptions currentHighlightOptionB = HighlightOptions.none;
    public HighlightOptions currentHighlightOption = HighlightOptions.none;

    /*
     * The same as applyHighlight with an extra option. 
     * if blink is true then this object will toggle between none and given option, else it will behave as the basic applyHighlight
     * 
     */
    public void applyHighlight(HighlightOptions option, bool blink)
    {
        currentHighlightOption = option;
        if (option != HighlightOptions.none)
            currentHighlightOptionB = option;
        applyHighlight(option);
        if (blink)
        {
            StartCoroutine(HighlightAfterDelay(0.5f, option));
        }
    }

    private IEnumerator HighlightAfterDelay(float delay, HighlightOptions option = HighlightOptions.none)
    {
        applyHighlight(option, false);
        yield return new WaitForSeconds(delay);
        if (bShouldHighlight)
        {
            if (currentHighlightOption == HighlightOptions.none)
                StartCoroutine(HighlightAfterDelay(delay, currentHighlightOptionB));
            else
                StartCoroutine(HighlightAfterDelay(delay, HighlightOptions.none));
        }
    }
}
