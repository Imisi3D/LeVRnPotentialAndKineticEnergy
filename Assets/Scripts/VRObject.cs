using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HighlightOptions
{
    none = 0,
    correct = 1,
    wrong = 2,
    hover = 3
}

public enum HighlightType
{
    material,
    sprite,
    color
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
    public Material hover_Material;
    public Image image;
    public Sprite default_Sprite;
    public Sprite Correct_Sprite;
    public Sprite wrong_Sprite;
    public Sprite hover_Sprite;
    public Color default_Color;
    public Color Correct_Color;
    public Color wrong_Color;
    public Color hover_Color;
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


    public void applyHighlight(HighlightOptions highlight, bool force = false)
    {
        if ((currentHighlightOption == HighlightOptions.correct || currentHighlightOption == HighlightOptions.wrong) && !force) return;
        currentHighlightOption = highlight;
        switch (highlight)
        {
            case HighlightOptions.none:
                {

                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        if (default_Material != null)
                            meshRenderer.material = default_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (default_Sprite != null)
                            image.sprite = default_Sprite;

                    }
                    else if (highlightType == HighlightType.color)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (default_Color != null)
                            image.color = default_Color;
                    }
                    break;
                }
            case HighlightOptions.hover:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        if (hover_Material != null)
                            meshRenderer.material = hover_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (hover_Sprite != null)
                            image.sprite = hover_Sprite;

                    }
                    else if (highlightType == HighlightType.color)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (hover_Color != null)
                            image.color = hover_Color;
                    }
                    break;
                }
            case HighlightOptions.correct:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        if (Correct_Material != null)
                            meshRenderer.material = Correct_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (Correct_Sprite != null)
                            image.sprite = Correct_Sprite;
                    }
                    else if (highlightType == HighlightType.color)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (Correct_Color != null)
                            image.color = Correct_Color;
                    }
                    break;
                }
            case HighlightOptions.wrong:
                {
                    if (highlightType == HighlightType.material)
                    {
                        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                        if (wrong_Material != null)
                            meshRenderer.material = wrong_Material;
                    }
                    else if (highlightType == HighlightType.sprite)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (wrong_Sprite != null)
                            image.sprite = wrong_Sprite;
                    }
                    else if (highlightType == HighlightType.color)
                    {
                        //Image img = gameObject.GetComponent<Image>();
                        if (wrong_Color != null)
                            image.color = wrong_Color;
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
    public void applyHighlight(HighlightOptions option, bool blink, bool force)
    {
        currentHighlightOption = option;
        if (option != HighlightOptions.none)
            currentHighlightOptionB = option;
        applyHighlight(option, force);
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
