using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneScript : MonoBehaviour
{

    public GameObject oggetto;

    public float colonne = 3;
    public float righe = 1;
    
    private List<GameObject> pointOfInterest = new List<GameObject>();

    private void Awake ()
    {

        Vector3 size = GetComponent<MeshFilter>().mesh.bounds.size;

        float sizeX = size.x;
        float sizeZ = size.z;

        // Devono essere dispari
        float divX = (sizeX / colonne);
        float divZ = (sizeZ / righe);


        for ( float x = -Mathf.Ceil( colonne / 2 ) + 1; x <= Mathf.Ceil( colonne / 2 ) - 1; x++ )
        {
            for ( float z = -Mathf.Ceil( righe / 2 ) + 1; z <= Mathf.Ceil( righe / 2 ) - 1; z++ )
            {
                GameObject o = Instantiate( oggetto, transform.position, Quaternion.identity, transform );
                o.AddComponent<PuntoDiInteresse>();
                o.transform.localPosition = new Vector3( divX * x, 0f, divZ * z );
                pointOfInterest.Add( o );
            }
        }

    }


    public bool HaveAvailablePoint ()
    {
        foreach ( GameObject point in pointOfInterest )
        {
            if ( point.GetComponent<PuntoDiInteresse>().isAvailable == true )
            {
                return true;
            }
        }

        return false;
    }


    public GameObject GetAvailablePoint ()
    {

        foreach(GameObject point in pointOfInterest )
        {
            if ( point.GetComponent<PuntoDiInteresse>().isAvailable )
                return point;
            
        }

        return null;
    }


    public GameObject GetAvailableRandomPoint ()
    {

        System.Predicate<GameObject> predicate = isAvailable;
        List<GameObject> availablePoint = pointOfInterest.FindAll( predicate );


        return availablePoint[ Random.Range( 0, availablePoint.Count ) ];
    }


    public bool isAvailable (GameObject point)
    {
        return point.GetComponent<PuntoDiInteresse>().isAvailable;
    }

}
