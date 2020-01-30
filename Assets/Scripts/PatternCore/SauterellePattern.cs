using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    private IEnumerator<PictureInfo> picturesToWatch;
    public List<PictureInfo> pictures;
    private int maxJump;

    private void Awake ()
    {
        Color32 red = new Color32( 202, 12, 12, 1 );
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", red );

        maxJump = Random.Range( 5, 10 );
    }

    public override void InitMovementPattern ()
    {
        maxDistanza = 200;
        FindAllPicture();
        SetPictureToWatch();
    }

    private void FindAllPicture ()
    {
        pictures = new List<PictureInfo>();

        foreach( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            pictures.Add( picture.GetComponent<PictureInfo>() );
        }

        utilitySort.transform = transform;
        pictures.Sort( utilitySort.DistanzaPicture );

    }

    public override GameObject GetNextDestination ()
    {
        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueLevel >= FatigueManager.MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( picturesToWatch.MoveNext() )
        {
            if ( VisitedPictures.Contains( picturesToWatch.Current ) )
            {
                return GetNextDestination();
            }

            GameObject pictureGrid = picturesToWatch.Current.GetComponentInChildren<GridSystem>().gameObject;
            return pictureGrid;
        }

        if ( ImportantPictures.Count > 0 )
        {
            GameObject importantDestination = ImportantPictures[ ImportantPictures.Count - 1 ].GetComponentInChildren<GridSystem>().gameObject;
            ImportantPictures.RemoveAt( ImportantPictures.Count - 1 );

            return importantDestination;
        }
    

        return GetPlaneOfExit();
    }


    private void SetPictureToWatch ()
    {
        List<PictureInfo> picturesToWatch_list = new List<PictureInfo>();
        int lastPictureIndexAdded = 0;

        foreach ( PictureInfo picture in pictures )
        {
            bool selectPicture = Random.Range( 0, 10 ) > 8;

            if ( selectPicture || IsMaxJump( picture.index, lastPictureIndexAdded ) )
            {
                lastPictureIndexAdded = picture.index;
                picturesToWatch_list.Add( picture );
            }
        }

        picturesToWatch = picturesToWatch_list.GetEnumerator();
    }


    private bool IsMaxJump (int pictureIndex, int lastPictureIndex)
    {
        if( pictureIndex >= maxJump + lastPictureIndex )
            return true;

        return false;
    }

}
