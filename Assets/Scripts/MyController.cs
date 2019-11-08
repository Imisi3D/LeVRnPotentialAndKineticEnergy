using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyController : MonoBehaviour
{
    public int chosenNumber = 0;
    public int savedNumber = 0;
    public string inputFieldContent = "";
    public Text inputField;
    public GameObject inputContainer;
    public GameObject btn_done;
    public Text finalOutputText;
    public GameObject finalOutputTextContainer;
    public int currentStep = 1;
    public bool bIsNewInput = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    // checks if text content is a number between 0 and 10
    public void VerifyText(string text)
    {
        int num;
        Debug.Log(text);
        int.TryParse(text, out num);
        Debug.Log("the given number is : " + num);
        if (num < 0 || num > 10)
            Debug.Log("the given number should be from 0 to 10.");
    }

    public void addToInputField()
    {
        if (bIsNewInput)
        {
            inputFieldContent = "";
            bIsNewInput = false;
        }

        switch (EventSystem.current.currentSelectedGameObject.name)
        {
            case "num1":
                {
                    inputFieldContent += "1";
                    break;
                }
            case "num2":
                {
                    inputFieldContent += "2";
                    break;
                }
            case "num3":
                {
                    inputFieldContent += "3";
                    break;
                }
            case "num4":
                {
                    inputFieldContent += "4";
                    break;
                }
            case "num5":
                {
                    inputFieldContent += "5";
                    break;
                }
            case "num6":
                {
                    inputFieldContent += "6";
                    break;
                }
            case "num7":
                {
                    inputFieldContent += "7";
                    break;
                }
            case "num8":
                {
                    inputFieldContent += "8";
                    break;
                }
            case "num9":
                {
                    inputFieldContent += "9";
                    break;
                }
            case "num0":
                {
                    inputFieldContent += "0";
                    break;
                }
        }

        inputField.text = inputFieldContent;
        btn_done.SetActive(true);
    }

    public void clearInputField()
    {
        inputField.text = inputFieldContent = "0";
        bIsNewInput = true;
    }

    public void deleteLastInInputField()
    {
        inputField.text = inputFieldContent = inputFieldContent.Substring(0, inputFieldContent.Length - 1);
    }

    public void ToggleInputCanvasVisibility()
    {
        inputContainer.SetActive(!inputContainer.activeSelf);
    }

    public int minimumInputPossible = 1;
    public int maximumInputPossible = 9;
    public Text errorText;

    public void checkInput()
    {
        int num;
        int.TryParse(inputField.text, out num);
        if (num < minimumInputPossible || num > maximumInputPossible)
            if (maximumInputPossible != minimumInputPossible)
            {
                errorText.text = "Please enter a number between " + (minimumInputPossible - 1) + " and " + (maximumInputPossible + 1);
            }
            else
            {
                errorText.text = "Please verify your result.";
            }
        else
        {
            Debug.Log("ready for next task.");
            currentStep++;
            savedNumber = num;
            btn_done.SetActive(false);
            inputContainer.SetActive(false);
            setupForStep();
            bIsNewInput = true;
            errorText.text = "";
        }
    }

    public void setupForStep()
    {
        switch (currentStep)
        {
            case 2:
                {
                    chosenNumber = savedNumber;
                    maximumInputPossible = savedNumber * 2;
                    minimumInputPossible = savedNumber * 2;
                    break;
                }
            case 3:
                {
                    maximumInputPossible = savedNumber + 10;
                    minimumInputPossible = savedNumber + 10;
                    break;
                }
            case 4:
                {
                    maximumInputPossible = savedNumber /2;
                    minimumInputPossible = savedNumber /2;
                    break;
                }
            case 5:
                {
                    maximumInputPossible = savedNumber-3;
                    minimumInputPossible = savedNumber-3;
                    break;
                }
            case 7:
                {
                    maximumInputPossible = savedNumber;
                    minimumInputPossible = savedNumber;
                    break;
                }
            case 6:
                {
                    finalOutputText.text =
                        "You chose " + chosenNumber + "\n" +
                        chosenNumber + " x 2 : " + (chosenNumber * 2) + "\n" +
                        (chosenNumber * 2) + " + 10 : " + (chosenNumber * 2 + 10) + "\n" +
                        (chosenNumber * 2 + 10) + " / 2 : " + ((chosenNumber * 2 + 10) / 2) + "\n" +
                        ((chosenNumber * 2 + 10) / 2) + " - 3 : " + ((chosenNumber * 2 + 10) / 2 - 3) + "\n" +
                        "Finally you find the number \nyou chose in the beginning."
                        ;
                    finalOutputTextContainer.SetActive(true);
                    break;
                }
        }
    }
}
