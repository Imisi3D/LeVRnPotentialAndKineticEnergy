using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CanvasControl : MonoBehaviour
{
    public VoiceImageCanvasSync voiceImageCanvasSync;
    public AudioSource audioSource;
    public AudioClip wrong;
    public AudioClip correct;
    public GameObject collider;

    public Text instructionText;
    public Text btn_1_text;
    public Text btn_2_text;
    public Text btn_3_text;
    public Text btn_4_text;

    private void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            React("correct");
        }
        if (Input.GetKeyDown("q"))
        {
            React("wrong");
        }
    }

    public void React(string reaction)
    {
        if (reaction.ToUpper().Equals("wrong".ToUpper()))
        {
            audioSource.PlayOneShot(wrong);
        }
        else if (reaction.ToUpper().Equals("correct".ToUpper()))
        {
            audioSource.PlayOneShot(correct);

            // hiding canvas.
            gameObject.GetComponent<Canvas>().enabled = false;
            BoxCollider col = gameObject.GetComponent<BoxCollider>();
            if (col != null) col.enabled = false;

            if (collider != null) collider.SetActive(false);

            // starting next step in voice (the delay is used to skip "correct" being said).
            StartCoroutine(ToNextStepAfterDelay(1f));

            // updating canvas for the next state.
            instructionText.text = "What is 3 x 7?";
            btn_1_text.text = "21";
            btn_2_text.text = "5";
            btn_3_text.text = "10";
            btn_4_text.text = "37";
        }
    }

    private IEnumerator ToNextStepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        voiceImageCanvasSync.NextSync();
    }

    public void test1(Button nextCorrect)
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        obj.tag = "wrong";
        nextCorrect.gameObject.tag = "correct";
    }

}
