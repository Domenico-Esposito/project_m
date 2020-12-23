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
    
    private List<GameObject> gridPoints = new List<GameObject>();

    private void Awake ()
    {
        Vector3 size = GetComponent<MeshFilter>().mesh.bounds.size;

        for( float x = -size.x/2; x < size.x/2; x += size.x/columns)
        {
            for( float z = -size.z/2; z< size.z/2; z += size.z / rows )
            {
                GameObject point = Instantiate( gridPoint, transform.position, Quaternion.identity, transform );
                point.AddComponent<DestinationPoint>();
                point.transform.localPosition = new Vector3( x + (size.x/columns)/2, 0f, z + ( size.z / rows) / 2 );
                point.transform.rotation = new Quaternion( 0f, 0f, 0f, 0f );
                point.transform.localScale = new Vector3(0.1f/transform.lossyScale.x, 0f, 0.1f/transform.lossyScale.z );
                gridPoints.Add( point );
            }
        }
    }

    public IEnumerator GetEnumerator ()
    {
        return gridPoints.GetEnumerator();
    }

    public bool HaveAvailablePoint ()
    {
        lock( gridPointsLock )
        {
            if ( GetAvailablePoint() )
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
                if ( point.GetComponent<DestinationPoint>().isAvailable )
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
            return availablePoint[ Random.Range( 0, availablePoint.Count-1) ];
        }

    }

    public GameObject GetRandomPoint()
    {
        return gridPoints[Random.Range(0, gridPoints.Count)];
    }

    private bool IsAvailable (GameObject point)
    {
        return point.GetComponent<DestinationPoint>().isAvailable;
    }

}
