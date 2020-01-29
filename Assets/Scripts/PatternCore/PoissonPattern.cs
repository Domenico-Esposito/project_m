using System.Collections.Generic;
using UnityEngine;

public class PoissonPattern : PathManager
{
    private IEnumerator<GameObject> pathPart;
    public List<GameObject> picturePlanes;

    private void Awake ()
    {
        Color32 pink = new Color32( 243, 24, 192, 1 );
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", pink );
    }

    public override void InitMovementPattern ()
    {
        picturePlanes = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) );
        pathPart = emptySpaces.GetEnumerator();

        maxDistanza = 400;
    }

    public override GameObject GetNextDestination ()
    {
        bool viewPicture = Random.Range( 0, 10 ) > 7;

        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueLevel >= FatigueManager.MOLTO_STANCO )
            return GetPlaneOfExit();

        utilitySort.transform = transform;
        emptySpaces.Sort( utilitySort.DistanzaPlane );
        
        if ( viewPicture )
        {
            GameObject mostCloseEmptySpace = emptySpaces[ 0 ];
            int indexOfMostCloseEmptySpace = mostCloseEmptySpace.GetComponent<PictureInfo>().index;

            if ( emptySpaces.Count > 0 )
            {
                emptySpaces.RemoveAll( ( GameObject obj ) => obj.GetComponent<PictureInfo>().index <= indexOfMostCloseEmptySpace );
                pathPart = emptySpaces.GetEnumerator();
            }

            return GetPictureDestination();
        }

        return GetFishPlaneDestination();
    }

    private GameObject GetMostClosePicture ()
    {
        if ( picturePlanes.Count <= 0 )
            return GetPlaneOfExit();

        utilitySort.transform = transform;
        picturePlanes.Sort( utilitySort.Distanza );

        GameObject mostClosePicturePlane = picturePlanes[ 0 ];
        picturePlanes.Remove( mostClosePicturePlane );

        if( VisitedPictures.Contains( mostClosePicturePlane.GetComponentInParent<PictureInfo>() ) || mostClosePicturePlane.GetComponentInParent<PictureInfo>().index <= CurrentPictureIndex )
        {
            return GetMostClosePicture();
        }

        return mostClosePicturePlane;
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

