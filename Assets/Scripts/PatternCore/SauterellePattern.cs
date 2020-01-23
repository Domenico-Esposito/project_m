using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    // Pattern movimento
    private IEnumerator<PictureInfo> picturesToWatch;

    public List<PictureInfo> pictures;

    private int maxJump = 10;

    private void Awake ()
    {
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", new Color32( 202, 12, 12, 1 ) );
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

        utilitySort.transform = this.transform;
        pictures.Sort( utilitySort.DistanzaPicture );

    }

    public override GameObject GetNextDestination ()
    {
        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueStatus > FatigueManager.MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( picturesToWatch.MoveNext() )
        {
            if ( VisitedPictures.Contains( picturesToWatch.Current ) )
            {
                return GetNextDestination();
            }

            GameObject picturePlane = picturesToWatch.Current.transform.GetChild(0).gameObject;
            return picturePlane;
        }
        else
        {
            if ( ImportantPictures.Count > 0 )
                return ImportantPictures[ ImportantPictures.Count - 1 ].transform.GetChild( 0 ).gameObject;
        }

        return GetPlaneOfExit();
    }


    private void SetPictureToWatch ()
    {
        List<PictureInfo> picturesToWatch_list = new List<PictureInfo>();
        int lastPictureIndexAdded = 0;

        foreach ( PictureInfo picture in pictures )
        {
            int pictureIndex = picture.index;

            if ( Random.Range( 0, 10 ) > 8 || IsMaxJump( pictureIndex, lastPictureIndexAdded ) )
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
