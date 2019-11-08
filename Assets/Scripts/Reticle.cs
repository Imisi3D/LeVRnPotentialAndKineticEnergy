using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 *  Reticle which is showing when pointing with controller
 */
public class Reticle : MonoBehaviour
{
    // Pointer component this reticle is attached to.
    public Pointer pointer;
    // SpriterRenderer to show a sprite according to object type.
    public SpriteRenderer spriteRenderer;
    // Sprite when no object is detected.
    public Sprite openSprite;
    // Sprite when an object is detected.
    public Sprite closeSprite;
    // Camera that this reticle should always face.
    private Camera m_Camera = null;

    private void Awake()
    {
        pointer.OnPointerUpdate += UpdateSprite;
        m_Camera = Camera.main;
    }

    private void OnDestroy()
    {
        pointer.OnPointerUpdate -= UpdateSprite;
    }

    void Update()
    {
        transform.LookAt(m_Camera.gameObject.transform);
    }

    private void UpdateSprite(Vector3 point, GameObject hitObject)
    {
        transform.position = point;
        if (hitObject)
        {
            spriteRenderer.sprite = closeSprite;
        }
        else
        {
            spriteRenderer.sprite = openSprite;
        }
    }
}
