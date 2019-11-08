using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Settings : MonoBehaviour
{

    void Awake()
    {
        QualitySettings.antiAliasing = 8;
        XRSettings.eyeTextureResolutionScale = 2f;
        StartCoroutine(setSettings());
    }

    IEnumerator setSettings()
    {
        yield return new WaitForSeconds(2f);
        QualitySettings.antiAliasing = 8;
        XRSettings.eyeTextureResolutionScale = 2f;
    }

}
