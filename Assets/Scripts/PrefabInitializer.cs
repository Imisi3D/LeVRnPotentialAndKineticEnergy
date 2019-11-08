using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomPrefabData : System.Object
{
    public GameObject prefab;
    public LocationAndRotation[] Transforms;
}

[System.Serializable]
public class LocationAndRotation : System.Object
{
    public Vector3 Location;
    public Vector3 Rotation;
}

public class PrefabInitializer : MonoBehaviour
{
    public CustomPrefabData[] prefabs;
    public ArrayList instantiatedPrefabs;

    public void BuildPrefabs()
    {
        instantiatedPrefabs = new ArrayList();
        foreach (CustomPrefabData obj in prefabs)
            foreach (LocationAndRotation LocRot in obj.Transforms)
                instantiatedPrefabs.Add(Instantiate(obj.prefab, LocRot.Location, Quaternion.Euler(LocRot.Rotation)));
    }

    public void ClearPrefabs()
    {
        print("clearing prefabs");
        print(instantiatedPrefabs);
        foreach(GameObject prefab in instantiatedPrefabs)
        {
            DestroyImmediate(prefab);
        }
        instantiatedPrefabs.Clear();
    }
}
