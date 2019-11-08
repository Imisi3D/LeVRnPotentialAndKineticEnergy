using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System;

/**
 * A hard coded component for handling Script 1.
 */
public class NumberInputInteraction : MonoBehaviour
{
    // Text component that holds the instruction text.
    public Text InteractionText;
    // current step in script.
    private int CurrentStep = 0;
    // button done reference.
    public GameObject btn_done;
    // Canvas holder for the final number found by the player.
    public GameObject FinalNumberAnswerCanvas;
    // collider for FinalNumberAnswerCanvas.
    public GameObject FinalNumberAnswerCanvas_collider;
    // Answer button reference.
    public GameObject btn_answer;
    // Answer button's text reference.
    public Text AnswerText;
    // reference for YesNoCanvas.
    public GameObject YesNoCanvas;
    // reference for canvas.
    public GameObject ExplinationCanvas;
    // current step in first explanation.
    int currentStepIn1stExplination = 0;
    public Image explinationCoverImage;
    // image object reference to display sprites.
    public GameObject img;
    // input canvas.
    public GameObject inputCanvas;
    // input canvas collider.
    public GameObject inputCanvas_collider;

    // The body animator.
    public Animator animator_body;
    // The cloth animator.
    public Animator animator_cloth;

    // part2 object which when is activated Script 2 will start.
    public GameObject part2;

    // used only for debuging in editor.
    private float lastFrameClick = 0f;
    private void Update()
    {
        audioSource.pitch = Time.timeScale;
        if (Input.GetKeyDown("k") && Time.time > lastFrameClick + 1f)
        {
            lastFrameClick = Time.time;
            ToNextStep();
        }
        if (Input.GetKeyDown("j") && Time.time > lastFrameClick + 1f)
        {
            lastFrameClick = Time.time;
            ChooseAnswer(7);
        }
        if (Input.GetKeyDown("a") && Time.time > lastFrameClick + 1f)
        {
            lastFrameClick = Time.time;
            Explain();
        }
        if (Input.GetKeyDown("q") && Time.time > lastFrameClick + 1f)
        {
            lastFrameClick = Time.time;
            SkipExplination();
        }
        if (Input.GetKeyDown("z") && Time.time > lastFrameClick + 1f)
        {
            lastFrameClick = Time.time;
            DoYouAgreeFeedbackReaction("yes");
        }
    }

    public void Start()
    {
        // updating quality settings.
        XRSettings.eyeTextureResolutionScale = 1.5f;
        QualitySettings.antiAliasing = 8;
        // starting Script 1.
        StartCoroutine(initialVoice());
        //StartCoroutine(Recap());
        //StartFruitGame();
        // Explain();
        //part2.SetActive(true);
        for (int i = 0; i < numbers_sprites.Length - 1; i++)
            numbers_sprites[i].GetComponent<Image>().sprite = numbers[3];
        numbers_sprites[numbers_sprites.Length - 1].GetComponent<Image>().sprite = numbers[1];
    }

    // clip for introduction
    public AudioClip hello;

    // playing the first part (for choosing number).
    private IEnumerator initialVoice()
    {
        yield return new UnityEngine.WaitForSeconds(0.5f);
        // re-updating quality settings because of overridden quality in LWRP.
        QualitySettings.antiAliasing = 8;
        XRSettings.eyeTextureResolutionScale = 1.5f;
        yield return new UnityEngine.WaitForSeconds(1.5f);
        explain();
        playSound(hello);
        yield return new UnityEngine.WaitForSeconds(hello.length + 0.75f);
        playSound(pickANumber);
        yield return new UnityEngine.WaitForSeconds(6f);
        ask();
        yield return new UnityEngine.WaitForSeconds(pickANumber.length - 6f);
        idle();
        inputCanvas.GetComponent<Canvas>().enabled = true;
        inputCanvas_collider.SetActive(true);
    }

    // updates input canvas visibility
    private void SetActiveForInputCanvas(bool newActive)
    {
        inputCanvas.GetComponent<Canvas>().enabled = newActive;
        inputCanvas_collider.SetActive(newActive);
    }

    // updates input canvas visibility after delay.
    private IEnumerator SetActiveForInputCanvasAfterDelay(bool newActive, float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        idle();
        inputCanvas.GetComponent<Canvas>().enabled = newActive;
        inputCanvas_collider.SetActive(newActive);
    }

    // to next step in the process.    
    public void ToNextStep()
    {
        if (audioSource.isPlaying) return;
        SetActiveForInputCanvas(false);
        CurrentStep++;
        switch (CurrentStep)
        {
            case 1:
                {
                    playSound(multiplyBy2);
                    StartCoroutine(SetActiveForInputCanvasAfterDelay(true, multiplyBy2.length));
                    InteractionText.text = "Multiply the number by 2";
                    break;
                }
            case 2:
                {
                    playSound(add10);
                    StartCoroutine(SetActiveForInputCanvasAfterDelay(true, add10.length));
                    InteractionText.text = "Add 10";
                    break;
                }
            case 3:
                {
                    playSound(divideBy2);
                    StartCoroutine(SetActiveForInputCanvasAfterDelay(true, divideBy2.length));
                    InteractionText.text = "Divide by 2";
                    break;
                }
            case 4:
                {
                    playSound(subtract3);
                    StartCoroutine(SetActiveForInputCanvasAfterDelay(true, subtract3.length));
                    InteractionText.text = "Subtract 3";
                    break;
                }
            case 5:
                {
                    playSound(explination_finalAnswer);
                    explain();
                    StartCoroutine(SetActiveForInputCanvasAfterDelay(true, explination_finalAnswer.length));
                    InteractionText.text = "What is your final answer?";
                    btn_done.SetActive(false);
                    StartCoroutine(CallAfterDelay(
                        () =>
                        {
                            FinalNumberAnswerCanvas.SetActive(true);
                            FinalNumberAnswerCanvas_collider.SetActive(true);
                        }
                        , explination_finalAnswer.length)
                        );
                    break;
                }
        }
    }

    // call a funtion after delay.
    private IEnumerator CallAfterDelay(Action Param, float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        Param();
    }

    private int player_answer = 3;
    private IEnumerator SetActiveForGameObjectAfterSeconds(GameObject obj, bool newActive, float seconds)
    {
        yield return new UnityEngine.WaitForSeconds(seconds);
        obj.SetActive(newActive);
    }

    public void ChooseAnswer(int answer)
    {
        if (audioSource.isPlaying) return;
        player_answer = answer;
        FinalNumberAnswerCanvas.SetActive(false);
        FinalNumberAnswerCanvas_collider.SetActive(false);
        btn_answer.SetActive(true);
        AnswerText.text = (answer).ToString();
        for (int i = 0; i < numbers_sprites.Length - 1; i++)
            numbers_sprites[i].GetComponent<Image>().sprite = numbers[answer];
        numbers_sprites[numbers_sprites.Length - 1].GetComponent<Image>().sprite = numbers[answer - 2];
        StartCoroutine(showInitialNumber(answer - 2));
    }

    IEnumerator showInitialNumber(int finalAnswer)
    {
        explain();
        yield return new UnityEngine.WaitForSeconds(1.0f);
        AnswerText.text = finalAnswer.ToString();
        InteractionText.text = "The number you chose is";
        btn_answer.GetComponent<Image>().color = new Color(23, 156, 255);
        playSound(youChose[finalAnswer]);
        yield return new UnityEngine.WaitForSeconds(youChose[finalAnswer].length);
        yield return new UnityEngine.WaitForSeconds(0.5f);
        playSound(doYouKnowHowIDidIt);
        ask();
        yield return new UnityEngine.WaitForSeconds(doYouKnowHowIDidIt.length + 0.3f);
        inputCanvas.GetComponent<Canvas>().enabled = false;
        inputCanvas_collider.SetActive(false);
        YesNoCanvas.SetActive(true);
    }

    // starts explanation process
    public void Explain()
    {
        print("explain");
        //if (audioSource.isPlaying) return;
        print("explaining");

        explain();
        StartCoroutine(ShowExplination());
    }

    public GameObject YesNoCanvas_collider;
    IEnumerator ShowExplination()
    {
        playSound(AnswerForYes);
        explain();
        inputCanvas.GetComponent<Canvas>().enabled = false;
        inputCanvas_collider.SetActive(false);
        yield return new UnityEngine.WaitForSeconds(0.5f);
        YesNoCanvas.GetComponent<Canvas>().enabled = false;
        YesNoCanvas_collider.SetActive(false);
        yield return new UnityEngine.WaitForSeconds(AnswerForYes.length + 0.2f);

        StartCoroutine(nextExplination());
    }

    private int explination_spriteIndex = 0;
    IEnumerator nextExplination()
    {
        yield return new UnityEngine.WaitForSeconds(0.5f);
        switch (currentStepIn1stExplination)
        {
            case 0:
                {
                    playSound(explination_variable);
                    yield return new UnityEngine.WaitForSeconds(2f);
                    ExplinationCanvas.SetActive(true);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    yield return new UnityEngine.WaitForSeconds(16f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(explination_variable.length + 0.2f - 18f);
                    break;
                }
            case 1:
                {
                    playSound(explination_algebraicTerm);
                    yield return new UnityEngine.WaitForSeconds(2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    yield return new UnityEngine.WaitForSeconds(6f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(explination_algebraicTerm.length + 0.2f - 8f);
                    break;
                }
            case 2:
                {
                    playSound(explination_coefficient);

                    yield return new UnityEngine.WaitForSeconds(5.2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(explination_coefficient.length + 0.2f - 5.2f);
                    break;
                }
            case 3:
                {
                    playSound(explination_algerbraicExpression);
                    yield return new UnityEngine.WaitForSeconds(2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(7.8f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(explination_algerbraicExpression.length + 0.2f - 9.8f);
                    break;
                }
            case 4:
                {
                    playSound(explination_continous);
                    yield return new UnityEngine.WaitForSeconds(1.2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(6.3f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(explination_continous.length + 0.2f - 7.5f);
                    break;
                }
            case 5:
                {
                    explinationCoverImage.rectTransform.anchoredPosition = new Vector3(0, -550, 0);
                    playSound(explination_FinallyYouToldMe[player_answer - 2]);

                    yield return new UnityEngine.WaitForSeconds(3.5f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(explination_FinallyYouToldMe[player_answer - 2].length + 0.2f - 3.5f);

                    playSound(explination_ThisIsCalledAlgebraicEquation[player_answer - 2]);
                    yield return new UnityEngine.WaitForSeconds(1.5f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(6f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(19f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[0].SetActive(false);
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(explination_ThisIsCalledAlgebraicEquation[player_answer - 2].length + 0.2f - 26.5f);

                    playSound(explination_ToDivideAlgebraicTerm[player_answer - 2]);

                    yield return new UnityEngine.WaitForSeconds(6f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];

                    yield return new UnityEngine.WaitForSeconds(2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(7.2f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(explination_ToDivideAlgebraicTerm[player_answer - 2].length + 0.2f - 17.2f);

                    playSound(explination_MakeXTheSubjectOfTheFormula[player_answer - 2]);

                    yield return new UnityEngine.WaitForSeconds(8f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(4.5f);
                    explinationImage.sprite = explinationSprites[explination_spriteIndex++];
                    numbers_sprites[++lastRevealedSpriteIndex].SetActive(true);

                    yield return new UnityEngine.WaitForSeconds(explination_MakeXTheSubjectOfTheFormula[player_answer - 2].length + 0.2f - 12f);

                    playSound(explination_end);
                    yield return new UnityEngine.WaitForSeconds(explination_end.length + 0.3f);
                    StartCoroutine(Recap());
                    break;
                }


        }

        currentStepIn1stExplination++;
        if (currentStepIn1stExplination < 6)
            StartCoroutine(nextExplination());
    }

    public Sprite explination_recap_01;
    public Sprite explination_recap_02;
    public Sprite explination_recap_03;
    public Sprite explination_recap_04;
    public Sprite explination_recap_05;
    public Sprite explination_recap_06;
    public Sprite explination_recap_07;
    public Sprite explination_recap_08;
    public Sprite explination_recap_09;
    public Sprite explination_recap_10;
    public Sprite explination_recap_11;
    //public Sprite explination_recap_12;
    public AudioClip explination_recap;
    public IEnumerator Recap()
    {
        playSound(explination_recap);
        yield return new UnityEngine.WaitForSeconds(6.0f);
        foreach (GameObject obj in numbers_sprites)
            obj.SetActive(false);
        explinationImage.sprite = explination_recap_01;
        yield return new UnityEngine.WaitForSeconds(7.0f); // 13
        explinationImage.sprite = explination_recap_02;
        yield return new UnityEngine.WaitForSeconds(8.0f); // 21
        explinationImage.sprite = explination_recap_03;
        yield return new UnityEngine.WaitForSeconds(7.0f); // 28
        explinationImage.sprite = explination_recap_04;
        yield return new UnityEngine.WaitForSeconds(9.5f); // 37.5
        explinationImage.sprite = explination_recap_05;
        yield return new UnityEngine.WaitForSeconds(1.0f); // 38.5
        explinationImage.sprite = explination_recap_06;
        yield return new UnityEngine.WaitForSeconds(4.5f); // 43
        explinationImage.sprite = explination_recap_07;
        yield return new UnityEngine.WaitForSeconds(2.0f); // 45
        explinationImage.sprite = explination_recap_08;
        yield return new UnityEngine.WaitForSeconds(6.0f); // 51
        explinationImage.sprite = explination_recap_09;
        yield return new UnityEngine.WaitForSeconds(2.0f); // 53
        explinationImage.sprite = explination_recap_10;
        yield return new UnityEngine.WaitForSeconds(7.0f); // 60
        explinationImage.sprite = explination_recap_11;
        yield return new UnityEngine.WaitForSeconds(7.0f); // 60

        idle();
        StartFruitGame();
    }

    public AudioClip FruitGame_starter;
    public AudioClip FruitGame_wrong;
    public AudioClip FruitGame_algebraicTermStarter;
    public AudioClip FruitGame_algebraicEquationStarter;
    public AudioClip FruitGame_variableStarter;
    public AudioClip FruitGame_howWasTheGame;
    public AudioClip FruitGame_coefficient;
    public AudioClip FruitGame_constant;
    public AudioClip FruitGame_greatToHear;
    public AudioClip FruitGame_weShallDoMoreInterestingThings;
    public AudioClip FruitGame_algebraIsNotHard;
    public AudioClip FruitGame_thatsGreat;
    public AudioClip FruitGame_youAreInForSurprise;
    public AudioClip Good;
    public AudioClip Correct;
    public AudioClip Great_Job;
    public AudioClip Excellent;

    public GameObject FruitGame_feedbackCanvas;
    public GameObject FruitGame_OptionsCanvas;
    public GameObject DoYouAgree_Canvas;
    public Text FeedbackQuestionText;

    public string FruitGame_DemandedType = "algebraic_expression";
    public Text FruitGame_instructionText;
    public int FruitGame_currentProgress = 0;
    public int FruitGame_currentConsecutive = 0;
    public void PlaySoundForCurrentFruitGameProgress()
    {
        switch (FruitGame_currentConsecutive)
        {
            case 1: { playSound(Good); break; }
            case 2:
                {
                    playSound(Correct);
                    break;
                }
            case 3: { playSound(Great_Job); break; }
            case 4: { playSound(Excellent); break; }
        }
        if (FruitGame_currentProgress == 4 ||
            (FruitGame_DemandedType.Equals("coefficient") && FruitGame_currentProgress == 2)
            )
        {
            StartCoroutine(FruitGame_NextStep());
        }
    }

    public FruitGameStepData[] FruitGameSteps;
    public int FruitGame_CurrentStep = 0;
    public FruitMover fruitMover;
    public IEnumerator FruitGame_NextStep()
    {
        idle();
        if (FruitGame_CurrentStep >= FruitGameSteps.Length)
        {
            FruitGame_instructionText.transform.parent.gameObject.SetActive(false);
            ToyGun.SetActive(false);
            StartCoroutine(GetFeedback());
        }
        else
        {
            // skip the instruction if the player already picked up the gun.
            if (FruitGame_CurrentStep == 0 && FruitGame_hasToyGun) FruitGame_CurrentStep++;
            FruitGameStepData nextStepData = FruitGameSteps[FruitGame_CurrentStep++];
            yield return new UnityEngine.WaitForSeconds(1.0f);
            playSound(nextStepData.starter);
            explain();
            yield return new UnityEngine.WaitForSeconds(0.5f);
            fruitMover.enabled = true;

            // show instruction text (which fruits to pick)
            if (FruitGame_CurrentStep == 2)
            {
                FruitGame_instructionText.transform.parent.gameObject.SetActive(true);
            }
            if (nextStepData.DemandedType.Equals("coefficient"))
            {
                StartCoroutine(fruitMover.MoveFruitForEquationForm());
            }
            yield return new UnityEngine.WaitForSeconds(nextStepData.starter.length - 0.2f);
            idle();
            FruitGame_DemandedType = nextStepData.DemandedType;
            if (!(FruitGame_CurrentStep == 1 && FruitGame_hasToyGun))
                FruitGame_instructionText.text = nextStepData.instruction;
            FruitGame_currentProgress = 0;
            FruitGame_currentConsecutive = 0;
            if (!FruitGame_DemandedType.Equals("coefficient") && !FruitGame_DemandedType.Equals("constant"))
            {
                string[] newCombination = nextStepData.combination.Split(';');
                string[] newOrder = nextStepData.order.Split(';');

                // randomizing combination.
                for (int i = 0; i < 20; i++)
                {
                    int indexA = UnityEngine.Random.Range(0, newCombination.Length);
                    int indexB = UnityEngine.Random.Range(0, newCombination.Length);
                    if (indexA == indexB) continue;
                    string combinationA = newCombination[indexA];
                    string orderA = newOrder[indexA];
                    newCombination[indexA] = newCombination[indexB];
                    newCombination[indexB] = combinationA;
                    newOrder[indexA] = newOrder[indexB];
                    newOrder[indexB] = orderA;
                }

                // updating fruits info.
                for (int i = 0; i < newCombination.Length; i++)
                {
                    if (fruitMover.fruits.Length > i)
                    {
                        fruitMover.fruits[i].SetActive(true);
                        fruitMover.fruits[i].GetComponent<Interactible>().value = newCombination[i];
                        fruitMover.fruits[i].GetComponent<Interactible>().text.text = newCombination[i];
                        fruitMover.fruits[i].GetComponent<Interactible>().type = newOrder[i];
                    }
                }
            }
        }
    }

    private IEnumerator GetFeedback()
    {
        yield return new UnityEngine.WaitForSeconds(0.5f);
        foreach (GameObject obj in fruitMover.fruits)
        {
            obj.SetActive(false);
        }
        yield return new UnityEngine.WaitForSeconds(1.0f);
        playSound(FruitGame_howWasTheGame);
        ask();
        yield return new UnityEngine.WaitForSeconds(FruitGame_howWasTheGame.length);
        FruitGame_feedbackCanvas.SetActive(true);
        FruitGame_OptionsCanvas.SetActive(true);
    }

    public void ReceiveFeedback(string feedback)
    {
        StartCoroutine(ReactToFeedback(feedback));
    }

    private IEnumerator ReactToFeedback(string feedback)
    {
        FruitGame_OptionsCanvas.SetActive(false);
        explain();
        if (feedback.Equals("fun"))
        {
            playSound(FruitGame_greatToHear);
            DoYouAgree_Canvas.GetComponent<Canvas>().enabled = false;
            yield return new UnityEngine.WaitForSeconds(FruitGame_greatToHear.length + 1.0f);
        }
        else if (feedback.Equals("boring"))
        {
            playSound(FruitGame_weShallDoMoreInterestingThings);
            DoYouAgree_Canvas.GetComponent<Canvas>().enabled = false;
            yield return new UnityEngine.WaitForSeconds(FruitGame_weShallDoMoreInterestingThings.length + 1.0f);
        }

        playSound(FruitGame_algebraIsNotHard);
        yield return new UnityEngine.WaitForSeconds(6f);
        ask();
        yield return new UnityEngine.WaitForSeconds(FruitGame_algebraIsNotHard.length - 6f);
        DoYouAgree_Canvas.GetComponent<Canvas>().enabled = true;
        FeedbackQuestionText.text = "Do you agree?";
    }

    public void DoYouAgreeFeedbackReaction(string answer)
    {
        if (answer.Equals("yes"))
        {
            playSound(FruitGame_thatsGreat);
        }
        else
        {
            playSound(FruitGame_youAreInForSurprise);
        }
        explain();
        DoYouAgree_Canvas.SetActive(false);
        FruitGame_feedbackCanvas.SetActive(false);
        StartCoroutine(
            CallAfterDelay(
                () =>
                {
                    idle();
                    part2.SetActive(true);
                    //part2.GetComponent<VoiceImageCanvasSync>().Start();
                },
                1f
                )
            );
    }

    public bool FruitGame_hasToyGun = false;

    public void SkipExplination()
    {
        inputCanvas.GetComponent<Canvas>().enabled = false;
        inputCanvas_collider.SetActive(false);
        YesNoCanvas.SetActive(false);
        YesNoCanvas_collider.SetActive(false);
        StartFruitGame();
    }
    public GameObject ToyGunTextCanvas;
    private void StartFruitGame()
    {
        StartCoroutine(FruitGame_NextStep());
        if (!FruitGame_hasToyGun)
        {
            shouldHightlightToygun = true;
            ToyGunTextCanvas.SetActive(true);
            StartCoroutine(hightlightToyGunEveryDelay(0.5f));
        }
        ExplinationCanvas.SetActive(false);
    }

    public GameObject ToyGun;
    public bool shouldHightlightToygun = true;
    private bool ToyGunHightlighteningToggle = false;
    public Material hightlightMaterial;
    public Material DefaultMaterial;


    private IEnumerator hightlightToyGunEveryDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        MeshRenderer renderer = ToyGun.GetComponent<MeshRenderer>();
        if (shouldHightlightToygun)
        {
            if (ToyGunHightlighteningToggle)
            {
                renderer.material = hightlightMaterial;
            }
            else
            {
                renderer.material = DefaultMaterial;
            }
            ToyGunHightlighteningToggle = !ToyGunHightlighteningToggle;
        }
        else
        {
            renderer.material = DefaultMaterial;
        }
        StartCoroutine(hightlightToyGunEveryDelay(delay));
    }


    public Image explinationImage;
    public Sprite _2ndExplinationImage;

    public AudioSource audioSource;
    public AudioClip pickANumber;
    public AudioClip multiplyBy2;
    public AudioClip add10;
    public AudioClip divideBy2;
    public AudioClip subtract3;
    public AudioClip[] youChose;
    public AudioClip doYouKnowHowIDidIt;
    public AudioClip AnswerForYes;
    public AudioClip explination_variable;
    public AudioClip explination_algebraicTerm;
    public AudioClip explination_coefficient;
    public AudioClip explination_algerbraicExpression;
    public AudioClip explination_continous;
    public AudioClip explination_finalAnswer;
    public AudioClip error;
    public AudioClip gun_shot;

    public Sprite[] explinationSprites;
    public Sprite[] numbers;
    public GameObject[] numbers_sprites;
    private int lastRevealedSpriteIndex = -1;

    public AudioClip[] explination_FinallyYouToldMe;
    public AudioClip[] explination_ThisIsCalledAlgebraicEquation;
    public AudioClip[] explination_ToDivideAlgebraicTerm;
    public AudioClip[] explination_MakeXTheSubjectOfTheFormula;

    public AudioClip explination_division;
    public AudioClip explination_formula;
    public AudioClip explination_end;
    public void playSound(AudioClip clip)
    {
        explain();

        audioSource.PlayOneShot(clip);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ask()
    {
        animator_cloth.SetBool("idle", false);
        animator_cloth.SetBool("ask", true);

        animator_body.SetBool("idle", false);
        animator_body.SetBool("ask", true);
    }

    private void idle()
    {
        animator_cloth.SetBool("idle", true);
        animator_body.SetBool("idle", true);

        animator_cloth.SetBool("ask", false);
        animator_body.SetBool("ask", false);
    }

    private void explain()
    {
        int randomInt = UnityEngine.Random.Range(0, 2);

        animator_cloth.SetBool("idle", false);
        animator_cloth.SetInteger("talkingIndex", randomInt);
        animator_cloth.SetBool("ask", false);

        animator_body.SetBool("idle", false);
        animator_body.SetInteger("talkingIndex", randomInt);
        animator_body.SetBool("ask", false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
