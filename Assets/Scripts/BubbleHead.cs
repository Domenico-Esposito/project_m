using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHead : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        Vector3 v = Camera.main.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt( Camera.main.transform.position - v );
    }
}
