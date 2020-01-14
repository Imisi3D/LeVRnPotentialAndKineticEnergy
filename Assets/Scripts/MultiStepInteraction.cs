using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MultiStepInteraction : MonoBehaviour
{
    [System.Serializable]
    public class HolderInfo
    {
        // holder game object (a holder is a game object with a VRDraggableObjectTarget component).
        public GameObject holder;

        // where @holder should be located and rotated in this step.
        public Transform location;

        // the new required type for @holder in for this step.
        public string requiredType;

        // Audio clip to play when out of trials for @holder.
        public AudioClip WhenOutOfTrials;
    }

    [System.Serializable]
    public class Step
    {
        // background image (if it is not set, nothing will change).
        public Sprite background_sprite;

        // array holding states for holders in this particular step.
        public HolderInfo[] holdersInfo;

        // maximum number of trials for this step.
        public int maximumNumberOfTrials = 2;

        // all new options for this step separated by a ';'
        public string newOptionsTexts;

        // number of required answers for this step
        public int requiredAnswers = 2;
    }

    // audio source used to play audios
    public AudioSource audioSource;

    // reference for the background image
    public Image background_image;

    // reference for audio synchronizer
    public VoiceImageCanvasSync synchronizer;

    // steps managed by this manager
    public Step[] steps;

    // list of all draggable objects managed by this manager.
    // used to update their text in each step.
    public VRDraggableObject[] options;

    // current step
    public int currentStepIndex = 0;
    public int currentNumberOfCorrectAnswers = 0;
    public Pointer pointer;

    private void Start()
    {
        AdaptToStep();
    }

    /* 
     * Answers for a given pointer and a given holder.
     * Pointer used to get the attached object to 
     */
    public void Answer(VRDraggableObject obj, VRDraggableObjectTarget holder)
    {
        // check if an argument is null.
        if (obj == null || holder == null)
        {
            Debug.LogError("Can't answer without an object or without a holder.");
            return;
        }

        AudioClip clip2Play = null;
        // correct
        if (holder.bIsAnswerHolder && holder.containsType(obj.type))
        {
            currentNumberOfCorrectAnswers++;
        }
        // wrong
        else
        {
            holder.remainingTrials--;
            if (holder.remainingTrials == 0)
            {
                // searching for the clip to play when out of tries
                foreach (HolderInfo info in steps[currentStepIndex].holdersInfo)
                {
                    if (info.holder == holder.gameObject)
                    {
                        clip2Play = info.WhenOutOfTrials;
                        // searching for the right answer
                        foreach (VRDraggableObject option in options)
                        {
                            // when answer is found, attach it to the corresponding holder.
                            if (holder.containsType(option.type))
                            {
                                VRDraggableObject attached = holder.gameObject.GetComponentInChildren<VRDraggableObject>();
                                if (attached != null) // if the holder has an object attached to it, return it to its default location
                                    Utility.AttachToObject(attached.gameObject, attached.defaultHolder);
                                Utility.AttachToObject(option.gameObject, holder.gameObject);
                                currentNumberOfCorrectAnswers++;
                            }
                        }
                        break;
                    }
                }
                if (clip2Play != null)
                    audioSource.PlayOneShot(clip2Play);
            }
        }

        // when all answers are answered.
        if (steps[currentStepIndex].requiredAnswers == currentNumberOfCorrectAnswers)
        {
            currentStepIndex++;
            float delay = 1f;
            if (clip2Play != null) delay = clip2Play.length + 1f;
            StartCoroutine(
                CallAfterDelay(() =>
                {
                    synchronizer.NextSync();
                    AdaptToStep();
                    gameObject.SetActive(false);
                }, delay)
                );
        }
    }

    public void AdaptToStep()
    {
        if (currentStepIndex >= steps.Length) return;
        Step currentStep = steps[currentStepIndex];
        currentNumberOfCorrectAnswers = 0;
        if (currentStep != null)
        {
            // update background image's sprite
            if (currentStep.background_sprite != null)
                background_image.sprite = currentStep.background_sprite;

            // update holders state
            foreach (HolderInfo info in currentStep.holdersInfo)
            {
                VRDraggableObjectTarget target = info.holder.GetComponent<VRDraggableObjectTarget>();
                target.applyHighlight(HighlightOptions.none, true);
                if (target != null)
                {
                    target.requiredType = info.requiredType;
                    Utility.AttachToObject(info.holder, info.location.gameObject);
                    target.remainingTrials = currentStep.maximumNumberOfTrials;
                }
                else
                {
                    Debug.LogError(info.holder.name + " must have a VRDraggableObjectTarget component.");
                }
            }

            string[] texts = currentStep.newOptionsTexts.Split(';');
            int i = 0;
            // update options state
            foreach (VRDraggableObject option in options)
            {
                // removing any highlight.
                option.applyHighlight(HighlightOptions.none, true);
                // reattaching option to its original location/rotation.
                if (option.defaultHolder != null)
                {
                    Utility.AttachToObject(option.gameObject, option.defaultHolder);

                }
                // updating text
                if (texts.Length <= i)
                    Debug.LogError("Step " + currentStepIndex + " is lacking a text for at least an option");
                else
                    option.gameObject.GetComponentInChildren<Text>().text = texts[i];
                i++;
            }
        }
        if (pointer.attachedObject != null)
        {
            VRDraggableObject draggable = pointer.attachedObject.GetComponent<VRDraggableObject>();
            if (draggable.defaultHolder != null)
            {
                pointer.Drop(draggable.defaultHolder);
            }
            pointer.attachedObject = null;
        }
    }

    private IEnumerator CallAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
