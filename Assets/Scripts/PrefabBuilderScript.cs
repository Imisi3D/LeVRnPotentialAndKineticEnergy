using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBuilderScript : MonoBehaviour
{
    public GameObject prefab;
    public Vector3[] locations;
    public ArrayList instantiatedPrefabs;

    public void BuildPrefab()
    {
        foreach (Vector3 location in locations)
            instantiatedPrefabs.Add(Instantiate(prefab, location, Quaternion.identity));

    }

    public void ClearPrefabs()
    {
        foreach(GameObject obj in instantiatedPrefabs)
        {
            Destroy(obj);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
