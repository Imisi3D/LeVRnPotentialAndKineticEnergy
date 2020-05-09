using UnityEngine;
using UnityEngine.UI;

public class SliderMover : MonoBehaviour
{
    public Slider slider;
    public float speed = 5;
    private bool fill = true;

    void Start()
    {
        slider.value = 0f;
        slider.maxValue = 100f;
        slider.minValue = 0f;
    }

    void Update()
    {
        if (fill)
        {
            slider.value += Time.deltaTime * speed;
            if (slider.value >= slider.maxValue)
                fill = false;
        }
        else
        {
            slider.value -= Time.deltaTime * speed;
            if (slider.value <= slider.minValue)
                fill = true;
        }
    }
}
