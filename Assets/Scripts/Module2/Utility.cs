using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utility : MonoBehaviour
{

    public void openSceneWithTransitionParam(string sceneName, string param)
    {
        TransitionManager.transitionParam = param;
        SceneManager.LoadScene(sceneName);
    }

    public static void AttachToObject(GameObject obj1, GameObject obj2)
    {
        obj1.transform.parent = obj2.transform;
        obj1.transform.localPosition = new Vector3(0, 0, 0);
        obj1.transform.localEulerAngles = new Vector3(0, 0, 0);
    }
}
