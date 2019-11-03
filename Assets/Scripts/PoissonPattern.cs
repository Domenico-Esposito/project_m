using System.Collections.Generic;
using UnityEngine;

public class PoissonPattern : PathManager
{

    // Segui percorso
    private List<GameObject> poissonFloors;

    private IEnumerator<GameObject> pathPart;


    public override void InitMovementPattern ()
    {
        poissonFloors = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Fish Floor" ) );
        poissonFloors.Sort( utilitySort.SortByIndexPlace );

        if ( Random.Range( 0f, 1f ) > 0.5f )
        {
            poissonFloors.Reverse();
        }

        pathPart = poissonFloors.GetEnumerator();
    }


    protected override GameObject GetPointInDestination ()
    {
        destinationPoint = destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        return destinationPoint;
    }


    public override GameObject GetNextDestination ()
    {
        bool viewPicture = Random.Range( 0, 10 ) > 5 ? true : false;

        if( viewPicture )
            return GetPictureDestination();

        return GetFishPlaneDestination();
    }

    private GameObject GetMostClosePicture ()
    {
        List<GameObject> planes = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) );
        planes.Sort( utilitySort.Distanza );

        return planes[ 0 ];
    }

    private GameObject GetPictureDestination ()
    {
        return GetMostClosePicture();
    }


    private GameObject GetFishPlaneDestination ()
    {
        if ( pathPart.MoveNext() )
            return pathPart.Current;

        return GetPlaneOfExit();
    }


}

