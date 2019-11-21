using System.Collections;
using System.Collections.Generic;
using RVO;
using UnityEngine;
using Vector2 = RVO.Vector2;

public class ObstacleCollect : MonoBehaviour
{
    void Start ()
    {

        BoxCollider[ ] boxColliders = GetComponentsInChildren<BoxCollider>();

        for ( int i = 0; i < boxColliders.Length; i++ )
        {

            Debug.Log("Aggiunto " + boxColliders[i].GetComponent<Transform>().gameObject.name, boxColliders[ i ].GetComponent<Transform>().gameObject);
            float minX = boxColliders[ i ].transform.position.x -
                         boxColliders[ i ].size.x * boxColliders[ i ].transform.lossyScale.x * 0.5f;
            float minZ = boxColliders[ i ].transform.position.z -
                         boxColliders[ i ].size.z * boxColliders[ i ].transform.lossyScale.z * 0.5f;
            float maxX = boxColliders[ i ].transform.position.x +
                         boxColliders[ i ].size.x * boxColliders[ i ].transform.lossyScale.x * 0.5f;
            float maxZ = boxColliders[ i ].transform.position.z +
                         boxColliders[ i ].size.z * boxColliders[ i ].transform.lossyScale.z * 0.5f;

            IList<Vector2> obstacle = new List<Vector2>();
            obstacle.Add( new Vector2( maxX, maxZ ) );
            obstacle.Add( new Vector2( minX, maxZ ) );
            obstacle.Add( new Vector2( minX, minZ ) );
            obstacle.Add( new Vector2( maxX, minZ ) );
            GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().addObstacle( obstacle );
        }


    }

    // Update is called once per frame
    void Update ()
    {
    }
}