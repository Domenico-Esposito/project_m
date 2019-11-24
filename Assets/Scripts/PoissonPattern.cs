using System.Collections.Generic;
using UnityEngine;

public class PoissonPattern : PathManager
{

    // Segui percorso
    private List<GameObject> poissonFloors;

    private IEnumerator<GameObject> pathPart;
    List<GameObject> picturePlanes;

    public override void InitMovementPattern ()
    {
        picturePlanes = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) );
        poissonFloors = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );
        poissonFloors.Sort( utilitySort.SortByIndexPlace );

        //if ( Random.Range( 0f, 1f ) > 0.5f )
        //{
        //    poissonFloors.Reverse();
        //}

        pathPart = poissonFloors.GetEnumerator();
        maxDistanza = 400;
    }


    protected override GameObject GetPointInDestination ()
    {
        destinationPoint = destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        return destinationPoint;
    }


    public override GameObject GetNextDestination ()
    {
        bool viewPicture = Random.Range( 0, 10 ) > 7 ? true : false;

        if ( LivelloStanchezza() > MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( viewPicture )
            return GetPictureDestination();

        return GetFishPlaneDestination();
    }

    private GameObject GetMostClosePicture ()
    {
        utilitySort.transform = transform;
        picturePlanes.Sort( utilitySort.Distanza );

        GameObject destinationPlane = picturePlanes[ 0 ];
        picturePlanes.Remove( destinationPlane );

        return destinationPlane;
    }

    private GameObject GetPictureDestination ()
    {
        return GetMostClosePicture();
    }


    private GameObject GetFishPlaneDestination ()
    {
        if ( pathPart.MoveNext() )
        {
            if( pathPart.Current.GetComponent<GridSystem>().HaveAvailablePoint() )
            {
                return pathPart.Current;
            }
            else
            {
                return GetFishPlaneDestination();
            }
        }

        return GetPlaneOfExit();
    }


}

