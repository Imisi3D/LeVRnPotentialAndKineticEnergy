using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : CustomComponent
{
    public Material InitialMaterial;
    public Material HighlightMaterial;

    void Start()
    {
        if (HighlightMaterial != null) { 
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = HighlightMaterial;
        }
        else if(InitialMaterial != null)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = InitialMaterial;
        }
        
    }
}
