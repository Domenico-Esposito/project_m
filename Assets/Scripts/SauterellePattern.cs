using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    // Pattern movimento
    private IEnumerator<GameObject> picturesToWatch;

    private GameObject[] pictures;

    public override void InitMovementPattern ()
    {
        RefreshPicturesToWatch();
    }

    public override GameObject GetNextDestination ()
    {

        if ( picturesToWatch.MoveNext() )
        {
            return picturesToWatch.Current;
        }

        RefreshPicturesToWatch();

        return picturesToWatch.Current;
    }


    private void RefreshPicturesToWatch ()
    {

        pictures = GameObject.FindGameObjectsWithTag( "Quadro" );

        List<GameObject> picturesToWatch_list = new List<GameObject>();


        foreach ( GameObject picture in pictures )
        {
            if ( Random.Range( 0, 20 ) > 15 ) // 75%
            {
                picturesToWatch_list.Add( picture );
            }
        }

        picturesToWatch_list.Sort( SortByIndexPicture );

        picturesToWatch = picturesToWatch_list.GetEnumerator();
        picturesToWatch.MoveNext();

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
