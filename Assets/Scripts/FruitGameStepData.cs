using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Used to present a single step data in the fruit game process
 */
[System.Serializable]
public class FruitGameStepData : System.Object
{
    /**
     * @DemandedType a string that holds the awaited answer type.
     */
    public string DemandedType = "";

    /**
     * @starter the audio clip to play before starting this step
     */
    public AudioClip starter;

    /**
     * @combination a string holding a combination of all text content on fruits.
     * As ...;...;...;... .. ...;... where ... presents only one fruit text and ; is the seperator between texts (must be the same number of fruits).
     */
    public string combination;

    /**
     * @order a string holding types for all fruits.
     * As ...;...;...;... .. ...;... where ... presents only one fruit text and ; is the seperator between texts (must be the same number of fruits).
     */
    public string order;

    /**
     * @instruction a string holding the instruction which will be displayed on top of all fruits for what should be done.
     */
    public string instruction = "";
}
