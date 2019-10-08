﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauterellePattern : PathManager
{
    // Pattern movimento
    private IEnumerator<GameObject> picturesToWatch;

    private List<GameObject> pictures;

    private int maxJump = 6;

    public override void InitMovementPattern ()
    {
        colorDrawPath = Color.red;

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

        pictures.Sort( SortByIndexPicture );
    }

    public override GameObject GetNextDestination ()
    {
        if ( picturesToWatch.MoveNext() )
        {
            GameObject picturePlane = picturesToWatch.Current.transform.GetChild(0).gameObject;
            return picturePlane;
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
        if( pictureIndex >= maxJump + lastPictureIndex )
            return true;

        return false;
    }

}
