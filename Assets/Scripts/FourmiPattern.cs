using System.Collections.Generic;
using UnityEngine;

public class FourmiPattern : PathManager
{
    // Pattern movimento
    private IEnumerator<GameObject> pictures;
    private List<GameObject> walls = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();

    public GameObject startWall;
    private GameObject currentWall;
    private int currentPictureIndex = 0;

    public int numberOfStop;

    public override void InitMovementPattern ()
    { 
        colorDrawPath = Color.yellow;

        FindWallsWithPictures();
        FindPicturesOnWalls();
        SortPicturesOnWalls();

        //currentWall = GameObject.FindGameObjectsWithTag( "Wall" )[ Random.Range(0, GameObject.FindGameObjectsWithTag( "Wall" ).Length) ];
        currentWall = startWall;

        numberOfStop = Random.Range( 13, walls.Count );
    }


    public override GameObject GetNextDestination ()
    {
        if( numberOfStop <= 0 )
            return GetPlaneOfExit();

        if ( MoveToNextPicOnCurrentWall() )
        {
            if ( Random.Range( 1, 10 ) > 6 )    //Salto un quadro
                return GetNextDestination();

            numberOfStop -= 1;
        }
        else if ( MoveToNextPicOnAnotherWall() )
        {
            if ( Random.Range( 1, 10 ) > 5 )    //Salto un muro
                return GetNextDestination();

            numberOfStop -= 1;
        }
        else{
            return GetPlaneOfExit();
        }

        return GetPlaneOfCurrentPicture();
    }


    private bool MoveToNextPicOnCurrentWall ()
    {
        if ( pictures == null )
            return false;

        if ( pictures.MoveNext() )
        {
            RefreshCurrentPictureIndex();
            return true;
        }

        return false;
    }


    private void RefreshCurrentPictureIndex ()
    {   
        currentPictureIndex = pictures.Current.GetComponent<PictureInfo>().index;
    }


    private bool MoveToNextPicOnAnotherWall ()
    {
        walls.Remove( currentWall );
        walls.RemoveAll( ( GameObject wall ) => picturesOnWalls[ wall ][ 0 ].GetComponent<PictureInfo>().index < currentPictureIndex );
        walls.Sort( SortByIndexPictureInWalls );

        if ( NextPictureIsInClosestWall() || NextPictureIsInDetachedWall() ) 
        {
            pictures = picturesOnWalls[ currentWall ].GetEnumerator();
            pictures.MoveNext();
            RefreshCurrentPictureIndex();
            return true;
        }

        return false;
    }

    

    private bool NextPictureIsInClosestWall ()
    {
        List<GameObject> wallsIntersectCurrentWall = GetWallsClosestToCurrent();
        
        if ( wallsIntersectCurrentWall.Count > 0 )
        {
            GameObject wallWithMinPicIndex = wallsIntersectCurrentWall[ 0 ];
            GameObject picWithMinIndex = picturesOnWalls[ wallWithMinPicIndex ][ 0 ];

            if ( picWithMinIndex.GetComponent<PictureInfo>().index >= currentPictureIndex )
            {
                currentWall = wallWithMinPicIndex;
                return true;
            }
        }

        return false;
    }



    private List<GameObject> GetWallsClosestToCurrent ()
    {
        List<GameObject> intersectsWalls = new List<GameObject>();

        foreach ( GameObject wall in walls )
        {
            if ( wall == currentWall )
                continue;

            Bounds currentWallBounds = currentWall.GetComponent<MeshRenderer>().bounds;
            Bounds wallBounds = wall.GetComponent<MeshRenderer>().bounds;
            if ( currentWallBounds.Intersects( wallBounds ) )
            {
                if( picturesOnWalls[wall][0].GetComponent<PictureInfo>().index < currentPictureIndex + 10 )
                    intersectsWalls.Add( wall );
            }
        }

        intersectsWalls.Sort( SortByIndexPictureInWalls );

        return intersectsWalls;
    }

    private GameObject GetPlaneOfCurrentPicture ()
    {
        return pictures.Current.transform.GetChild( 0 ).gameObject;
    }


    private bool NextPictureIsInDetachedWall ()
    {
        if(walls.Count <= 0 )
            return false;

        if( picturesOnWalls.ContainsKey(walls[0]) )
        {
            currentWall = walls[ 0 ];
            return true;
        }

        return false;
    }


    private void FindWallsWithPictures ()
    {

        foreach ( GameObject wall in GameObject.FindGameObjectsWithTag( "Wall" ) )
        {
            if ( wall.transform.childCount > 0 )
            {
                walls.Add( wall );
                picturesOnWalls.Add( wall, new List<GameObject>() );
            }
        }

    }


    private void FindPicturesOnWalls ()
    {

        foreach ( GameObject picturePlane in GameObject.FindGameObjectsWithTag( "PicturePlane" ) )
        {
            GameObject picture = (picturePlane.transform).parent.gameObject;
            GameObject wall = (picture.transform).parent.gameObject;

            picturesOnWalls[ wall ].Add( picture );

        }

    }


    private void SortPicturesOnWalls ()
    {

        foreach ( List<GameObject> pics in picturesOnWalls.Values )
        {
            pics.Sort( SortByIndexPicture );
        }

    }


    private int SortByIndexPictureInWalls ( GameObject wallX, GameObject wallY )
    {

        GameObject quadro_x = picturesOnWalls[ wallX ][ 0 ];
        GameObject quadro_y = picturesOnWalls[ wallY ][ 0 ];

        return SortByIndexPicture( quadro_x, quadro_y );
    }

}

