using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoScene : MonoBehaviour
{

    public TransitionManager transitionManager;
    public string[] scenesNames;

    public void goToScene(int sceneIndex)
    {
        if (transitionManager.Assets.Length > sceneIndex && transitionManager.Assets[sceneIndex] != null)
        {
            transitionManager.SetRequiredAssets(sceneIndex);
        }
        else
        {
            string param = "scene" + (sceneIndex + 1).ToString();
            TransitionManager.transitionParam = param;
            SceneManager.LoadScene(scenesNames[sceneIndex]);
            VoiceImageCanvasSync.SceneTransitionParam = param;
        }
    }
}
