using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/**
 * A component that can be attached to a canvas with button interaction. 
 * Really helps when there are more than one interaction with that canvas and a correct answer is required.
 */
public class MultiStepCanvasInput : MonoBehaviour
{
    /**
     * A button data for an interaction.
     */
    [System.Serializable]
    public class ButtonTextData
    {
        /**
         * A string displayed for a button.
         */
        public string text;

        /**
         * Text Component for the button.
         */
        public Text component;
    }

    public void Start()
    {
        //UpdateCanvasContent();
    }

    /**
     * Data for a single step for an interaction with this canvas.
     */
    [System.Serializable]
    public class StepData
    {
        // Used when there is an instruction that is displayed in the canvas (like a question).
        public ButtonTextData instruction;
        // All buttons in this canvas.
        public ButtonTextData[] buttonsTexts;
        // The correct answer for this step.
        public string correctAnswer;

        public bool isAutoSolved = false;
        public Button correctAnswerButton;


        // if true, then in this step the player must answer correctly. Canvas won't disappear when the answer is wrong unless the maximum number of tries is reached.
        public bool mustAnswerCorrectly = true;
        // Audio for saying wrong. If set, this will be used instead of the component's wrong audio.
        public AudioClip forWrong;
        // Audio for saying correct. If set, this will be used instead of the component's correct audio.
        public AudioClip forCorrect;
        // The maximum number of tries, when this is reached the value of mustAnswerCorrectly is ignored.
        public int maximumNumberOfTries = 1;
        // if this audio is set then when all attempts are failed, this audio will be played.
        public AudioClip toPlayAfterAttemptsDone;
        // holder for the sprite which will be set.
        public Image imageComponent;
        // updates image component's sprite when correctly answered
        public Sprite sprite;
        // if true, then this canvas will disappear.
        public bool ShouldStopAtThis = false;
        //  if true the component specified as synchronizer will resume.
        public bool shouldResumeVoice = false;
        // if an object is specified, it will be activated after this step.
        public GameObject NextToActivate;
        // if a sprite is specified, it will be set in imageRef component after this step.
        public Sprite ImageOnActivation;

    }
    // DEBUG: this is only for testing and should always be none when building the APK.
    public GameObject DEBUG_clickedButton;

    // Array of all steps that this canvas will go threw
    public StepData[] steps;

    #region colors
    /**
     * Default color for a button.
     */
    public Color color_Default;
    /**
     * Old color for a button (when there are more than one step that this canvas will go threw without stopping then this color will be set for the button that was correct in the last question).
     */
    public Color color_Old;
    /**
     * Correct color for a button.
     */
    public Color color_Correct;
    /**
     * Wrong color for a button.
     */
    public Color color_Wrong;
    #endregion

    #region audio
    // Audio source that all audios will be played on.
    public AudioSource audioSource;
    // Default audio to be said when the answer is correct.
    public AudioClip audio_correct;
    // Default audio to be said when the answer is wrong.
    public AudioClip audio_wrong;
    #endregion

    // reference to the synchronizer. Used to resume playing after a step.
    public VoiceImageCanvasSync synchronizer;

    // index of current step
    public int currentIndex = 0;
    // The correct answer awaited by this component, this will be compared to the child text component of the clicked button
    public string awaitedAnswer;
    // How many trials are remaining (no modification in the component will affect the logic, it is set from code and exposed as DEBUG).
    public int possibleTries = 1;
    // How many answers were answered correctly.
    public int correctAnswersCount = 0;
    // What was answered (no modification in the component will affect the logic, it is set from code and exposed as DEBUG).
    public string answeredOptions = ";;";

    /**
     * This method is called when a button is clicked. 
     * @clip : audio clip to play for this specific button.
     */
    public void Answer(AudioClip clip = null)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        // DEBUG_clickedButton is used only in editor to test without building to Oculus.
        if (DEBUG_clickedButton != null)
            obj = DEBUG_clickedButton;
        Text clickedButtonText = obj.GetComponentInChildren<Text>();
        AudioClip playedClip = null;
        bool isCorrect = false;
        print(answeredOptions);
        
        if (clickedButtonText.text.Equals(awaitedAnswer) || (awaitedAnswer.Contains(";" + clickedButtonText.text + ";") && !answeredOptions.Contains(";" + clickedButtonText.text + ";")))
        {
            // if a correct button is clicked
            isCorrect = true;
            obj.GetComponent<Image>().color = color_Correct;
            correctAnswersCount++;
            answeredOptions = answeredOptions.Insert(answeredOptions.Length - 1, ";" + clickedButtonText.text);
            // referencing played clip for later
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                playedClip = clip;
            }
            else if (steps[currentIndex].forCorrect != null)
            {
                audioSource.PlayOneShot(steps[currentIndex].forCorrect);
                playedClip = steps[currentIndex].forCorrect;
            }
            else
            {
                audioSource.PlayOneShot(audio_correct);
                playedClip = audio_correct;
            }
        }
        else if (!awaitedAnswer.Contains(";" + clickedButtonText.text + ";")) // if a wrong button was clicked
        {
            possibleTries--;
            obj.GetComponent<Image>().color = color_Wrong;
            // referencing played clip for later
            if (possibleTries > 0 || steps[currentIndex].toPlayAfterAttemptsDone == null)
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                    playedClip = clip;
                }
                else if (steps[currentIndex].forWrong != null)
                {
                    audioSource.PlayOneShot(steps[currentIndex].forWrong);
                    playedClip = steps[currentIndex].forWrong;
                }
                else
                {
                    audioSource.PlayOneShot(audio_wrong);
                    playedClip = audio_wrong;
                }
        }
        synchronizer.explain();
        StartCoroutine(idleAfterDelay(playedClip.length));
        // if was answered correctly and answered all required answers or no need for a correct answer or out of tries
        if ((isCorrect && steps[currentIndex].correctAnswer.Split(';').Length - 3 <= correctAnswersCount) || !steps[currentIndex].mustAnswerCorrectly || possibleTries == 0)
        {
            if (possibleTries == 0 && !isCorrect && steps[currentIndex].toPlayAfterAttemptsDone != null)
            {
                audioSource.PlayOneShot(steps[currentIndex].toPlayAfterAttemptsDone);
                playedClip = steps[currentIndex].toPlayAfterAttemptsDone;
            }
            // if a sprite and an image component are set, update image with sprite
            if (steps[currentIndex].imageComponent != null && steps[currentIndex].sprite != null)
            {
                // enabling component and parent object before
                steps[currentIndex].imageComponent.enabled = true;
                steps[currentIndex].imageComponent.gameObject.SetActive(true);
                // updating sprite
                steps[currentIndex].imageComponent.sprite = steps[currentIndex].sprite;
            }
            // activating game object 
            GameObject toActivate = steps[currentIndex].NextToActivate;
            if (toActivate != null)
            {
                // if has canvas comp, enable it
                Canvas canvas = toActivate.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = true;
                    Collider col = toActivate.GetComponent<Collider>();
                    if (col != null)
                        col.enabled = true;
                    MeshRenderer meshRenderer = toActivate.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.enabled = true;

                }

                // if has MultiStepCanvasInput comp, enable it
                MultiStepCanvasInput comp = toActivate.GetComponent<MultiStepCanvasInput>();
                if (comp != null) comp.enabled = true;

                ControllerSelection.OVRRaycaster raycaster = toActivate.GetComponent<ControllerSelection.OVRRaycaster>();
                if (raycaster != null) raycaster.enabled = true;

                if (steps[currentIndex].ImageOnActivation != null)
                    steps[currentIndex].imageComponent.sprite = steps[currentIndex].ImageOnActivation;
            }

            // increment current index
            currentIndex++;
            // if no other data exists to update, disable canvas, disable OVRRaycaster and reset buttons' colors
            print(gameObject.name + ":current index: " + currentIndex);
            if (currentIndex >= steps.Length || steps[currentIndex - 1].ShouldStopAtThis)
            {
                Button[] buttons = obj.transform.parent.GetComponentsInChildren<Button>();
                if (steps[currentIndex - 1].isAutoSolved)
                {
                    StartCoroutine(CallAfterDelay(() =>
                    {
                        obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
                        obj.transform.parent.gameObject.GetComponent<BoxCollider>().enabled = false;
                        for (int i = 0; i < buttons.Length; i++)
                            buttons[i].gameObject.GetComponent<Image>().color = color_Default;
                    }, 1f));
                }
                else
                {
                    obj.transform.parent.gameObject.GetComponent<Canvas>().enabled = false;
                    obj.transform.parent.gameObject.GetComponent<BoxCollider>().enabled = false;
                    MeshRenderer meshRenderer = obj.transform.parent.gameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.enabled = false;
                    for (int i = 0; i < buttons.Length; i++)
                        buttons[i].gameObject.GetComponent<Image>().color = color_Default;
                }
                shouldDisableComp = true;
                obj.transform.parent.gameObject.GetComponent<ControllerSelection.OVRRaycaster>().enabled = false;

                print(gameObject.name + ":trying to resume for index " + (currentIndex - 1) + "(" + steps[currentIndex - 1].shouldResumeVoice + ")");
                if (steps[currentIndex - 1].shouldResumeVoice)
                {
                    print(gameObject.name + ":should resume voice");
                    // if should go to an index after this interaction
                    if (synchronizer.currentVoiceTimingData.GoToIndex >= 0)
                        synchronizer.currentAudioIndex = synchronizer.currentVoiceTimingData.GoToIndex - 1;
                    if (playedClip != null)
                        StartCoroutine(ResumeProgressAfterDelay(playedClip.length + 0.5f));
                    else
                        StartCoroutine(ResumeProgressAfterDelay(0.5f));
                }
                else enabled = false;
            }
            else
            {
                StartCoroutine(CallAfterDelay(UpdateCanvasContent, 2f));
                if (steps[currentIndex - 1].shouldResumeVoice)
                {
                    if (playedClip != null)
                        StartCoroutine(ResumeProgressAfterDelay(playedClip.length + 0.5f));
                    else
                        StartCoroutine(ResumeProgressAfterDelay(0.5f));
                }
            }
        }
    }

    private IEnumerator idleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        synchronizer.idle();
    }

    private bool shouldDisableComp = false;

    private IEnumerator ResumeProgressAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        synchronizer.NextSync();
        enabled = false;
    }

    private IEnumerator CallAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }


    // Applies the specified step data to canvas components.
    private void UpdateCanvasContent()
    {
        print(gameObject.name + ":trying to update canvas content");
        if (currentIndex >= steps.Length) return;
        print(gameObject.name + ":updating canvas content");
        StepData step = steps[currentIndex];
        answeredOptions = ";;";
        possibleTries = step.maximumNumberOfTries;
        // updating instruction if a component exists
        if (step.instruction.component != null)
            step.instruction.component.text = step.instruction.text;

        // updating text for all referenced button in current step
        foreach (ButtonTextData data in step.buttonsTexts)
        {
            if (data.component != null)
            {
                data.component.text = data.text;
            }
        }
        awaitedAnswer = step.correctAnswer;

        // updating colors for buttons with setting old correct buttons' color to "color_old" and others to "color_Default"
        Button[] buttons = gameObject.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Color oldColor = buttons[i].gameObject.GetComponent<Image>().color;
            if (oldColor.Equals(color_Old) || oldColor.Equals(color_Correct))
            {
                buttons[i].gameObject.GetComponent<Image>().color = color_Old;
            }
            else
            {
                buttons[i].gameObject.GetComponent<Image>().color = color_Default;
            }
        }
        print(gameObject.name + ":updated canvas content");

        if (steps[currentIndex].isAutoSolved)
        {
            if (steps[currentIndex].correctAnswerButton != null)
            {
                StartCoroutine(AutoAnswer());
            }
        }
    }

    private IEnumerator AutoAnswer()
    {
        print("autoAnswering");
        yield return new WaitForSeconds(2f);
        DEBUG_clickedButton = steps[currentIndex].correctAnswerButton.gameObject;
        Answer();
        DEBUG_clickedButton = null;
        //yield return new WaitForSeconds(1f);

    }

    // Used for testing in editor
    private float lastKeyboardClick = 0f;
    private void Update()
    {
        if (Input.GetKeyDown("z") && lastKeyboardClick + 1f < Time.time)
        {
            lastKeyboardClick = Time.time;
            Answer();
        }
    }

    // when this component is enable, step data should be applied to canvas component.
    private void OnEnable()
    {
        UpdateCanvasContent();
        print("multiStepCanvas::OnEnable");

    }
}