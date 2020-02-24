using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Settings : MonoBehaviour
{
    private float startTime = 0;
    private void Start()
    {
        startTime = Time.time;
        //Time.timeScale = 1.5f;
    }

    private void Update()
    {
        // to make sure that settings are already set by other scripts used by oculus plugin.
        if (startTime == 0 || Time.time < startTime + 3f) return;

        if (QualitySettings.antiAliasing != 4 || XRSettings.eyeTextureResolutionScale != 1.25f)
        {
            QualitySettings.antiAliasing = 4;
            XRSettings.eyeTextureResolutionScale = 1.25f;
            enabled = false;
        }
    }
}
