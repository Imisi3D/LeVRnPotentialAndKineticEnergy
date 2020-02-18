using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deactivator : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
