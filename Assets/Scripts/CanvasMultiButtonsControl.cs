using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CanvasMultiButtonsControl : MonoBehaviour
{
    [System.Serializable]
    public struct Step_Action
    {
        public int step;
        [SerializeField]
        public UnityEvent action;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown("e"))
    //    {
    //        print("correct");
    //        availableTries = 2;
    //        currentStep++;
    //        gameObject.GetComponent<Canvas>().enabled = false;
    //        currentCorrect = "btn_14";
    //        synchronizer.NextSync();
    //        foreach (Step_Action step in actions)
    //        {
    //            if (step.step == currentStep)
    //            {
    //                step.action?.Invoke();
    //                break;
    //            }
    //        }
    //    }
    //    if (Input.GetKeyDown("w"))
    //    {
    //        print("got wrong answer");
    //        availableTries--;
    //        if (availableTries == 1)
    //            audioSource.PlayOneShot(wrong_tryAgain);
    //        else if (availableTries == 0)
    //        {
    //            availableTries = 2;
    //            audioSource.PlayOneShot(wrong);
    //            synchronizer.Start();
    //            gameObject.GetComponent<Canvas>().enabled = false;
    //        }
    //    }
    //}

    public AudioSource audioSource;
    public AudioClip wrong;
    public AudioClip wrong_tryAgain;
    public AudioClip correct;
    public VoiceImageCanvasSync synchronizer; 
    private int currentStep = 0;
    public string currentCorrect = "btn_01";
    public Step_Action[] actions;
    private int availableTries = 2;

    public void reactionFromButtons()
    {
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject.name == currentCorrect)
        {
            availableTries = 2;
            currentStep++;
            gameObject.GetComponent<Canvas>().enabled = false;
            audioSource.PlayOneShot(correct);
            BoxCollider col = gameObject.GetComponent<BoxCollider>();
            if (col != null) col.enabled = false;
            currentCorrect = "btn_14";
            synchronizer.NextSync();
            foreach (Step_Action step in actions)
            {
                if (step.step == currentStep)
                {
                    step.action?.Invoke();
                    break;
                }
            }
        }
        else
        {
            availableTries--;
            if (availableTries == 1)
                audioSource.PlayOneShot(wrong_tryAgain);
            else if (availableTries == 0)
            {
                availableTries = 2;
                audioSource.PlayOneShot(wrong);
                synchronizer.NextSync();
                currentStep++;
                currentCorrect = "btn_14";
                gameObject.GetComponent<Canvas>().enabled = false;
                BoxCollider col = gameObject.GetComponent<BoxCollider>();
                if (col != null) col.enabled = false;
                foreach (Step_Action step in actions)
                {
                    if (step.step == currentStep)
                    {
                        step.action?.Invoke();
                        break;
                    }
                }
            }
        }
    }

    public void setNextCorrect(string name)
    {
        currentCorrect = name;
    }

}
