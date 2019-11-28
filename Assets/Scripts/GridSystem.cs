using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class GridSystem : MonoBehaviour
{
    private readonly object gridPointsLock = new object();
    public GameObject gridPoint;

    // Devono essere dispari
    public float columns = 3;
    public float rows = 1;

    public bool autoLibera = false;

    private List<GameObject> gridPoints = new List<GameObject>();

    private void Awake ()
    {
        Vector3 size = GetComponent<MeshFilter>().mesh.bounds.size;

        for ( float x = -Mathf.Ceil( columns / 2 ) + 1; x <= Mathf.Ceil( columns / 2 ) - 1; x++ )
        {
            for ( float z = -Mathf.Ceil( rows / 2 ) + 1; z <= Mathf.Ceil( rows / 2 ) - 1; z++ )
            {
                GameObject point = Instantiate( gridPoint, transform.position, Quaternion.identity, transform );
                point.AddComponent<DestinationPoint>();
                point.transform.localPosition = new Vector3( (size.x / columns) * x, 0f, (size.z / rows) * z );
                gridPoints.Add( point );
            }
        }
    }


    public bool HaveAvailablePoint ()
    {
        lock( gridPointsLock )
        {
            if ( GetAvailablePoint() || autoLibera)
                return true;

            return false;
        }
    }


    public GameObject GetAvailablePoint ()
    {
        lock ( gridPointsLock )
        {
            foreach ( GameObject point in gridPoints )
            {
                if ( point.GetComponent<DestinationPoint>().isAvailable || autoLibera )
                    return point;
            }

            return null;
        }
    }


    public GameObject GetAvailableRandomPoint ()
    {
        List<GameObject> availablePoint;

        lock ( gridPointsLock )
        { 
            availablePoint = gridPoints.FindAll( (System.Predicate<GameObject>) IsAvailable );
            return availablePoint[ Random.Range( 0, availablePoint.Count ) ];
        }

    }


    private bool IsAvailable (GameObject point)
    {
        return point.GetComponent<DestinationPoint>().isAvailable || autoLibera;
    }

}
