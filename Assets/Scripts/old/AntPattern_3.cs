using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntPattern_3 : PathManager
{

    public override void InitMovementPattern()
    {

    }

 
    public override GameObject GetNextDestination()
    {
        return null;     
    }


    public GameObject GetMostCloseWall()
    {
        int wallsLayer = 9;
        float minDistance = Mathf.Infinity;
        GameObject mostCloseWall = null;

        Vector3[] directions = new Vector3[]{
            -transform.right,
            transform.right,
            transform.forward,
            -transform.forward
        };


        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), direction, out hit, 20))
            {
                if (hit.collider.gameObject.layer == wallsLayer)
                {
                    if(hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        mostCloseWall = hit.collider.gameObject;
                    }
                }
            }
        }


        return mostCloseWall;
    }



}

