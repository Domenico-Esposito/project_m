using System.Collections.Generic;
using UnityEngine;

public class PoissonPattern : PathManager
{

    // Segui percorso
    public List<GameObject> poissonFloors;

    private IEnumerator<GameObject> pathPart;
    public List<GameObject> picturePlanes;

    private void Awake ()
    {
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", new Color32( 243, 24, 192, 1 ) );
    }

    public override void InitMovementPattern ()
    {
        picturePlanes = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) );
        poissonFloors = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );
        poissonFloors.Sort( utilitySort.SortByIndexPlace );

        pathPart = poissonFloors.GetEnumerator();
        maxDistanza = 400;
    }

    public override GameObject GetNextDestination ()
    {
        bool viewPicture = Random.Range( 0, 10 ) > 7 ? true : false;

        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueStatus > FatigueManager.MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( viewPicture )
        {

            utilitySort.transform = this.transform;
            poissonFloors.Sort( utilitySort.DistanzaPlane );
            int indexPathPartPiuVicino = poissonFloors[ 0 ].GetComponent<PictureInfo>().index;

            if ( poissonFloors.Count > 0 )
            {
                poissonFloors.RemoveAll( ( GameObject obj ) => obj.GetComponent<PictureInfo>().index <= indexPathPartPiuVicino );
                //Debug.Log( "IndexPathPartVicino: " + indexPathPartPiuVicino );
                pathPart = poissonFloors.GetEnumerator();
            }

            return GetPictureDestination();
        }

        return GetFishPlaneDestination();
    }

    private GameObject GetMostClosePicture ()
    {
        utilitySort.transform = this.transform;

        if ( picturePlanes.Count <= 0 )
            return GetPlaneOfExit();

        picturePlanes.Sort( utilitySort.Distanza );

        GameObject destinationPlane = picturePlanes[ 0 ];
        picturePlanes.Remove( destinationPlane );

        //Debug.Log( "Index considerata: " + destinationPlane.transform.parent.GetComponent<PictureInfo>().index + " | IndexAttuale: " + CurrentPictureIndex );


        if( VisitedPictures.Contains( destinationPlane.GetComponentInParent<PictureInfo>() ) || destinationPlane.GetComponentInParent<PictureInfo>().index <= CurrentPictureIndex )
        {
            return GetMostClosePicture();
        }

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

