using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CanvasControllerForClass2 : MonoBehaviour
{

    [System.Serializable]
    public class customEvent : UnityEvent<string> { }

    [System.Serializable]
    public class specificUpdate
    {
        public GameObject trigger;
        [SerializeField]
        public customEvent action;
        public string newText;
    }

    public specificUpdate[] updates;

    public Color defaultColor;
    public Color correct;
    public Color wrong;

    public AudioClip audio_correct;
    public AudioClip audio_wrong;

    public VoiceImageCanvasSync synchronizer;
    public AudioSource audioSource;

    private int remainingCorrect = 3;
    public void selectAndKeepColor(bool isCorrect)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (DEBUG_button != null)
            obj = DEBUG_button;
        Button btn = obj.GetComponent<Button>();

        if (isCorrect)
        {
            remainingCorrect--;
            obj.GetComponent<Image>().color = correct;
            audioSource.PlayOneShot(audio_correct);
            if (remainingCorrect == 0)
            {
                remainingCorrect = 1;
                Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
                //Image[] btnImages = new Image[buttons.Length];
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
                }
                foreach (specificUpdate update in updates)
                {
                    if (obj == update.trigger)
                    {
                        update.action?.Invoke(update.newText);
                    }
                }
                obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
                BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
                col.enabled = false;
                StartCoroutine(ResumeProgressAfterDelay(1f));
            }
        }
        else
        {
            obj.GetComponent<Image>().color = wrong;
            //btn.image.color = wrong;
            audioSource.PlayOneShot(audio_wrong);
        }
    }

    public Text text_01;
    public void UpdateText(string newText)
    {
        text_01.text = newText;
    }

    private IEnumerator ResumeProgressAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        synchronizer.NextSync();
    }

    [System.Serializable]
    public class CanvasUpdater
    {
        public int updateIndex;
        public Text text;
        public string newInstruction;
        public string newbuttonsTexts;
        public string newCorrectAnswer;
        public Text[] buttonsTexts;
    }

    public CanvasUpdater[] canvasUpdatersHolder;

    public void PerformCanvasUpdates(int index)
    {
        foreach (CanvasUpdater cu in canvasUpdatersHolder)
        {
            if (index == cu.updateIndex)
            {
                UpdateCanvas(cu);
                break;
            }
        }
    }

    public void UpdateCanvas(CanvasUpdater canvasUpdater)
    {
        if (canvasUpdater.text != null)
            canvasUpdater.text.text = canvasUpdater.newInstruction;
        awaitedAnswer = canvasUpdater.newCorrectAnswer;
        string[] buttonsTexts = canvasUpdater.newbuttonsTexts.Split(';');
        for (int i = 0; i < buttonsTexts.Length; i++)
        {
            if (i < canvasUpdater.buttonsTexts.Length)
                canvasUpdater.buttonsTexts[i].text = buttonsTexts[i];
        }
    }

    private string awaitedAnswer = "3";
    public void Answer(AudioClip clip = null)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (DEBUG_button != null)
            obj = DEBUG_button;

        Text clickedButtonText = obj.GetComponentInChildren<Text>();
        if (clickedButtonText.text.Equals(awaitedAnswer))
        {
            if (clip != null)
                audioSource.PlayOneShot(clip);
            else
                audioSource.PlayOneShot(audio_correct);
            obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
            col.enabled = false;
            obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;
            Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
            }
            StartCoroutine(ResumeProgressAfterDelay(1f));
        }
        else
        {
            obj.GetComponent<Image>().color = wrong;
            if (clip != null)
                audioSource.PlayOneShot(clip);
            else
                audioSource.PlayOneShot(audio_wrong);
        }
    }

    public void AnswerAndResumeProgress(AudioClip clip = null)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (DEBUG_button != null)
            obj = DEBUG_button;
        Text clickedButtonText = obj.GetComponentInChildren<Text>();
        AudioClip playedClip;
        if (clickedButtonText.text.Equals(awaitedAnswer))
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                playedClip = clip;
            }
            else
            {
                audioSource.PlayOneShot(audio_correct);
                playedClip = audio_correct;
            }
            obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
            col.enabled = false;
            obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;
            Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
            }
            StartCoroutine(ResumeProgressAfterDelay(playedClip.length));
        }
        else
        {
            obj.GetComponent<Image>().color = wrong;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                playedClip = clip;
            }
            else
            {
                audioSource.PlayOneShot(audio_wrong);
                playedClip = audio_wrong;
            }
            obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
            col.enabled = false;
            obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;
            Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
            }
            StartCoroutine(ResumeProgressAfterDelay(playedClip.length));
        }
    }

    public void AnswerAndUpdateIndex(AudioClip clip = null)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (DEBUG_button != null)
            obj = DEBUG_button;
        Text clickedButtonText = obj.GetComponentInChildren<Text>();
        AudioClip playedClip;
        if (clickedButtonText.text.Equals(awaitedAnswer))
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                playedClip = clip;
            }
            else
            {
                audioSource.PlayOneShot(audio_correct);
                playedClip = audio_correct;
            }
            obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
            col.enabled = false;
            obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;
            Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
            }
            StartCoroutine(ResumeProgressAfterDelay(playedClip.length));
        }
        else
        {
            obj.GetComponent<Image>().color = wrong;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                playedClip = clip;
            }
            else
            {
                audioSource.PlayOneShot(audio_wrong);
                playedClip = audio_wrong;
            }
            obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = obj.transform.parent.gameObject.GetComponent<BoxCollider>();
            col.enabled = false;
            obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;
            Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.GetComponent<Image>().color = defaultColor;
            }
            print("current audio index is " + synchronizer.currentAudioIndex);
            print("trying to jump to " + synchronizer.currentVoiceTimingData.GoToIndex);
            if (synchronizer.currentVoiceTimingData.GoToIndex >= 0)
                synchronizer.currentAudioIndex = synchronizer.currentVoiceTimingData.GoToIndex - 1;
            StartCoroutine(ResumeProgressAfterDelay(playedClip.length));
        }
    }

    public GameObject DEBUG_button;
    private void Update()
    {
        if (Input.GetKeyDown("y"))
        {
            AnswerAndUpdateIndex();
        }
    }
}
