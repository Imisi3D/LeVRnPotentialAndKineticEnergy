using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Used in simulation room to handle separate parts in scripts, so from another scene when it is told to go to the simulation room, going to the simulation room will play the part next to the old process.
 */
public class SimulationRoomScriptController : MonoBehaviour
{
    public VoiceImageCanvasSync[] classSynchronizer;
    public GameObject[] Assets;

    private void Awake()
    {
        Time.timeScale = 2;
        int RequiredAssets = 0;
        if (VoiceImageCanvasSync.SceneTransitionParam.Equals("class1"))
            RequiredAssets = 1;
        else if (VoiceImageCanvasSync.SceneTransitionParam.Equals("class2"))
            RequiredAssets = 2;
        else if (VoiceImageCanvasSync.SceneTransitionParam.Equals("class3"))
            RequiredAssets = 3;
        else if (VoiceImageCanvasSync.SceneTransitionParam.Equals("class4"))
            RequiredAssets = 4;
        else if (VoiceImageCanvasSync.SceneTransitionParam.Equals("class1Part2"))
            RequiredAssets = 5;
        SetRequiredAssets(RequiredAssets);
    }

    // Activates and deactivates game objects and components according to what is required.
    private void SetRequiredAssets(int requiredIndex)
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
        print(gameObject.name);
        StartCoroutine(EnableAfterDelay(classSynchronizer[requiredIndex], 1f));
    }

    private IEnumerator EnableAfterDelay(MonoBehaviour component, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (component != null)
            component.enabled = true;
    }

    public void StartClass(string name)
    {
        if (name.Equals("class1"))
        {
            VoiceImageCanvasSync.SceneTransitionParam = "class1";
            SceneManager.LoadScene("Market");
        }
        else if (name.Equals("class1Part2"))
        {
            SceneManager.LoadScene("Market");
            VoiceImageCanvasSync.SceneTransitionParam = "class1Part2";
        }
        else if (name.Equals("class2"))
        {
            VoiceImageCanvasSync.SceneTransitionParam = "class2";
            SceneManager.LoadScene("Street");
        }
        else if (name.Equals("class3"))
        {
            SetRequiredAssets(3);
            VoiceImageCanvasSync.SceneTransitionParam = "class3";
        }
        else if (name.Equals("class4"))
        {
            VoiceImageCanvasSync.SceneTransitionParam = "class4";
            SceneManager.LoadScene("Market");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            StartClass("class1");
        }
        if (Input.GetKeyDown("2"))
        {
            StartClass("class2");
        }
        if (Input.GetKeyDown("3"))
        {
            StartClass("class3");
        }
        if (Input.GetKeyDown("4"))
        {
            StartClass("class4");
        }
        if (Input.GetKeyDown("5"))
        {
            StartClass("class1Part2");
        }
    }
}
