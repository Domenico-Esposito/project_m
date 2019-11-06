using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    // Pattern movimento
    private IEnumerator<GameObject> picturesToWatch;

    private List<GameObject> pictures;

    private int maxJump = 10;

    public override void InitMovementPattern ()
    {
        FindAllPicture();
        SetPictureToWatch();
    }


    private void FindAllPicture ()
    {
        pictures = new List<GameObject>();

        foreach(GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            pictures.Add( picture );
        }

        pictures.Sort( utilitySort.SortByIndexPicture );
    }

    public override GameObject GetNextDestination ()
    {
        if ( distanzaPercorsa > maxDistanza )
            return GetPlaneOfExit();

        if ( picturesToWatch.MoveNext() )
        {
            if ( visitedPictures.Contains( picturesToWatch.Current ) )
            {
                return GetNextDestination();
            }

            GameObject picturePlane = picturesToWatch.Current.transform.GetChild(0).gameObject;
            return picturePlane;
        }
        else
        {
            if ( importantPictures.Count > 0 )
                return importantPictures[ importantPictures.Count - 1 ].transform.GetChild( 0 ).gameObject;
        }

        return GetPlaneOfExit();
    }


    private void SetPictureToWatch ()
    {
        List<GameObject> picturesToWatch_list = new List<GameObject>();
        int lastPictureIndexAdded = 0;

        foreach ( GameObject picture in pictures )
        {
            int pictureIndex = picture.GetComponent<PictureInfo>().index;

            if ( Random.Range( 0, 10 ) > 8 || IsMaxJump( pictureIndex, lastPictureIndexAdded ) )
            {
                lastPictureIndexAdded = picture.GetComponent<PictureInfo>().index;
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
