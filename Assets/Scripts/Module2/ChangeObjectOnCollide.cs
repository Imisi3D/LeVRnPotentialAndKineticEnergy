using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeObjectOnCollide : MonoBehaviour
{
    public Mesh mesh1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnCollisionEnter(Collision collision)
    {
        print("AAAAAAAqqqqqqAAA");
         this.gameObject.GetComponent<MeshFilter>().mesh = mesh1; 
    }
}
