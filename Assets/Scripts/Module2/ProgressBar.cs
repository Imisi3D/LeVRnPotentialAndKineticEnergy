using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Slider slider;
    public float FillSpeed = 0.5f;
    public float targetProgress = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
    }
    void Start()
    {
        IncrementProgress(1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(slider.value < targetProgress)
        {
            slider.value += FillSpeed * Time.deltaTime;
        }
        else if(slider.value > targetProgress)
        {
            slider.value -= FillSpeed * Time.deltaTime;
        }
    }
    public void IncrementProgress(float newProgress)
    {
        if (slider.value < targetProgress)
        {
            targetProgress = slider.value + newProgress;
        }
        else
        {
            targetProgress = slider.value - newProgress;
        }
        
    }
}
