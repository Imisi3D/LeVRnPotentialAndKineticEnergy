using UnityEngine;
using UnityEngine.SceneManagement;

public class PendingSceneLoader : MonoBehaviour
{
    private float startTime = 0f;
    private bool firstTime = true;
    private AsyncOperation op = null;
    public UnityEngine.UI.Slider slider;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time > startTime + 10f)
        {
            if (firstTime)
            {
                op = SceneManager.LoadSceneAsync(TransitionManager.pendingLoadingSceneName);
                firstTime = false;
            }
            if (slider != null)
            {
                if (op != null)
                {
                    slider.value = op.progress * slider.maxValue;
                }
                else
                {
                    slider.value = 0f;
                }
            }
        }
    }
}
