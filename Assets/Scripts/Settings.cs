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
    }

    private void Update()
    {
        if(Time.time < 10 + startTime)
        {
            if(QualitySettings.antiAliasing == 8)
            {
                QualitySettings.antiAliasing = 8;
                XRSettings.eyeTextureResolutionScale = 1.75f;

            }
        }
    }
}
