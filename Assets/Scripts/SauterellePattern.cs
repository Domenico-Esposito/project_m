using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    // Pattern movimento
    private IEnumerator<GameObject> picturesToWatch;

    private List<GameObject> pictures;

    public override void InitMovementPattern ()
    {
        colorDrawPath = Color.red;

        FindAllPicture();
        RefreshPicturesToWatch();
    }


    private void FindAllPicture ()
    {
        pictures = new List<GameObject>();

        foreach(GameObject picture in GameObject.FindGameObjectsWithTag( "PicturePlane" ) )
        {
            pictures.Add( picture.transform.parent.gameObject );
        }

        pictures.Sort( SortByIndexPicture );
    }

    public override GameObject GetNextDestination ()
    {

        if ( picturesToWatch.MoveNext() )
        {
            return picturesToWatch.Current.transform.GetChild(0).gameObject;
        }

        return GetPlaneOfExit();
    }


    private void RefreshPicturesToWatch ()
    {
   
        List<GameObject> picturesToWatch_list = new List<GameObject>();
        int lastPictureIndexAdded = 0;

        foreach ( GameObject picture in pictures )
        {
            int pictureIndex = picture.GetComponent<PictureInfo>().index;

            if ( Random.Range( 0, 10 ) > 6 || IsMaxJump( pictureIndex, lastPictureIndexAdded ) )
            {
                lastPictureIndexAdded = picture.GetComponent<PictureInfo>().index;
                picturesToWatch_list.Add( picture );
            }
        }


        picturesToWatch = picturesToWatch_list.GetEnumerator();
    }


    private bool IsMaxJump (int pictureIndex, int lastPictureIndex)
    {
        int maxJump = 6;

        if( pictureIndex >= maxJump + lastPictureIndex )
        {
            return true;
        }

        return false;
    }

    private int SortByIndexPicture ( GameObject x, GameObject y )
    {

        float index_1 = x.transform.GetComponentInParent<PictureInfo>().index;
        float index_2 = y.transform.GetComponentInParent<PictureInfo>().index;

        if ( index_1 < index_2 ) return -1;
        if ( index_1 > index_2 ) return 1;
        return 0;

    }

}
