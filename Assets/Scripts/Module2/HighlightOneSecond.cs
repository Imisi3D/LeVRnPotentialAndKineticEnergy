using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightOneSecond : CustomComponent
{
    public Material InitialMaterial;
    public Material HighlightMaterial;

    IEnumerator ResumeSynchronizerAfterDelay()
    {
        yield return new WaitForSeconds(1);
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = InitialMaterial;
    }

    void Start()
    {
        if ((HighlightMaterial != null) && (InitialMaterial != null)){
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = HighlightMaterial;
            StartCoroutine(ResumeSynchronizerAfterDelay());
        }
        
    }
}
