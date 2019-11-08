using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A component that is responsible for moving fruits during the fruit game.
 * when this component is activated, fruits will be put in a grid format with the indicated rows and cols values with an offset in betwween fruits according to @offset.
 */
public class FruitMover : MonoBehaviour
{
    /**
     * all fruit that should be moved
     */
    public GameObject[] fruits;

    /**
     * object from where fruits should be starting to align
     */
    public GameObject OriginForFlyingFruit;

    /**
     * number of rows in fruits' grid.
     */
    public int rows = 4;

    /**
     * number of columns in fruits' grid.
     */
    public int cols = 3;

    /**
     * offset between fruits
     */
    public float offset = 20.0f;

    /**
     * Rotation speed when moving fruits.
     */
    public float rotationSpeed = 1;

    /**
     * movement speed when moving fruits.
     */
    public float movementSpeed = 1;

    Vector3 startLocation;
    Vector3 rightDirection;
    Vector3 downDirection;

    private void Start()
    {
        // getting values of origin transform to optimize performance.
        startLocation = OriginForFlyingFruit.transform.position;
        rightDirection = OriginForFlyingFruit.transform.right;
        downDirection = -OriginForFlyingFruit.transform.up;
        // saving when movement has started.
        startingTime = Time.time;
        // DEBUG: this line below is only used to show fruits in form of equation
        //StartCoroutine(MoveFruitForEquationForm());
    }

    private float startingTime;
    void Update()
    {
        // Moving fruits to form a grid.
        if (Time.time < startingTime + 5.0f)
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int index = r * cols + c;
                    if (fruits.Length > index)
                    {
                        fruits[index].transform.position =
                            Vector3.Lerp(
                                fruits[index].transform.position,
                                startLocation + rightDirection * offset * c + downDirection * offset * r,
                                Time.deltaTime * movementSpeed * 0.5f
                                );
                        fruits[index].transform.rotation =
                         Quaternion.Lerp(
                                 fruits[index].transform.rotation,
                                 OriginForFlyingFruit.transform.rotation,
                             Time.deltaTime * rotationSpeed
                             );
                    }
                    else break;
                }
    }

    /**
     * Class presenting equation data.
     */
    [System.Serializable]
    public class EquationData : System.Object
    {
        /**
         * text content for fruits.
         * a string as ...;...;...;... .. ...;... where ... presents only one fruit text and ; is the seperator between texts (must be the same number of fruits).
         */
        public string combination;

        /**
         * type for fruits.
         * a string as ...;...;...;... .. ...;... where ... presents only one fruit text and ; is the seperator between texts (must be the same number of fruits).
         */
        public string order;
    }

    // data for the equations. When fruits are presenting equations, this data will be used.
    public EquationData[] equationsData;

    // Moves fruits to show in equations form.
    public IEnumerator MoveFruitForEquationForm()
    {
        float time = Time.time;
        foreach (GameObject fruit in fruits) fruit.SetActive(true);
        while (Time.time < time + 6.0f)
        {
            for (int i = 0; i < equationsData.Length; i++)
            {
                string[] equation = equationsData[i].combination.Split(';');
                string[] orders = equationsData[i].order.Split(';');
                for (int j = 0; j < equation.Length; j++)
                {
                    int index = i * 6 + j;

                    fruits[index].GetComponent<Interactible>().value = equation[j];
                    fruits[index].GetComponent<Interactible>().text.text = equation[j];
                    fruits[index].GetComponent<Interactible>().type = orders[j];
                    fruits[index].transform.position =
                          Vector3.Lerp(
                              fruits[index].transform.position,
                              startLocation + rightDirection * offset * GetOffsetForIndex(j) + downDirection * offset * i,
                              Time.deltaTime * movementSpeed
                              );
                    fruits[index].transform.rotation =
                     Quaternion.Lerp(
                             fruits[index].transform.rotation,
                             OriginForFlyingFruit.transform.rotation,
                         Time.deltaTime * rotationSpeed
                         );
                }
                yield return new UnityEngine.WaitForSeconds(0.005f);
            }
        }
    }

    /**
     * when fruits are moved to present equations, this function returns the row that a fruit for a given index should be shown in.
     * used to make coefficients closer to variables.
     */

    private float GetOffsetForIndex(int index)
    {
        switch (index)
        {
            case 0: return 0f;
            case 1: return 0.6f;
            case 2: return 1.6f;
            case 3: return 2.6f;
            case 4: return 3.6f;
            case 5: return 4.6f;
            default: return 1f;
        }
    }


}
