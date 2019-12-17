using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public GameObject[] Assets;
    public VoiceImageCanvasSync[] voiceSynchronizers;
    public static string transitionParam = "";

    private void Awake()
    {
        int RequiredAssets = 0;
        if (transitionParam.Equals("scene1"))
            RequiredAssets = 0;
        else if (transitionParam.Equals("scene2"))
            RequiredAssets = 1;
        else if (transitionParam.Equals("scene3"))
            RequiredAssets = 2;
        else if (transitionParam.Equals("scene4"))
            RequiredAssets = 3;
        else if (transitionParam.Equals("scene5"))
            RequiredAssets = 4;
        else if (transitionParam.Equals("scene6"))
            RequiredAssets = 5;
        else if (transitionParam.Equals("scene7"))
            RequiredAssets = 6;
        else if (transitionParam.Equals("scene8"))
            RequiredAssets = 7;
        else if (transitionParam.Equals("scene9"))
            RequiredAssets = 8;
        else if (transitionParam.Equals("scene10"))
            RequiredAssets = 9;
        else if (transitionParam.Equals("scene11"))
            RequiredAssets = 10;
        else if (transitionParam.Equals("scene12"))
            RequiredAssets = 11;
        //RequiredAssets = 12;
        SetRequiredAssets(RequiredAssets);
    }

    // Activates and deactivates game objects and components according to what is required.
    public void SetRequiredAssets(int requiredIndex)
    {
        int i = 0;
        foreach (GameObject asset in Assets)
        {
            if (asset == null) { i++; continue; }
            if (i == requiredIndex)
                asset.SetActive(true);
            else
                asset.SetActive(false);
            i++;
        }
        StartCoroutine(EnableAfterDelay(voiceSynchronizers[requiredIndex], 1f));
    }

    private IEnumerator EnableAfterDelay(MonoBehaviour component, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (component != null)
            component.enabled = true;
    }

    float lastKeyPressedTime = 0f;
    private void Update()
    {
        if (Input.GetKeyDown("1") && lastKeyPressedTime + 0.5f < Time.time)
        {
            lastKeyPressedTime = Time.time;
            SetRequiredAssets(1);
        }
    }
}
