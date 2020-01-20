using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FourmiPattern : PathManager
{
    // Pattern movimento
    private IEnumerator<PictureInfo> pictures;
    public List<GameObject> walls = new List<GameObject>();

    public GameObject startWall;
    private GameObject currentWall;
    //private int CurrentPictureIndex = 0;

    public int numberOfStop;

    private void Awake ()
    {
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", new Color32( 250, 231, 44, 1 ) );
    }

    public override void InitMovementPattern ()
    { 
        FindWallsWithPictures();
        FindPicturesOnWalls();
        SortPicturesOnWalls();

        currentWall = GameObject.FindGameObjectsWithTag( "Wall" )[Random.Range(0, 3)];
        //currentWall = startWall;

        numberOfStop = Random.Range( 13, walls.Count );
        maxDistanza = 280;
    }


    public override GameObject GetNextDestination ()
    {
        if ( ( ImportantPictures.Count <= 0 && leader && !leader.activeInHierarchy ) || LivelloStanchezza() > MOLTO_STANCO)
            return GetPlaneOfExit();

        if ( MoveToNextPicOnCurrentWall() )
        {
            if ( Random.Range( 1, 10 ) > 6 )    //Salto un quadro
                return GetNextDestination();

        }
        else if ( MoveToNextPicOnAnotherWall() )
        {
            if ( Random.Range( 1, 10 ) > 5 )    //Salto un muro
                return GetNextDestination();

        }
        else{
            return GetPlaneOfExit();
        }

        if( VisitedPictures.Contains( pictures.Current ) )
        {
            return GetNextDestination();
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
        if( pictures.Current )
        {
            CurrentPictureIndex = pictures.Current.index;
        }
    }


    private bool MoveToNextPicOnAnotherWall ()
    {
        walls.Remove( currentWall );
        walls.RemoveAll( ( GameObject wall ) => picturesOnWalls[wall].Count <= 0 );
        walls.RemoveAll( ( GameObject wall ) => picturesOnWalls[ wall ][ 0 ].index < CurrentPictureIndex );

        utilitySort.picturesOnWalls = picturesOnWalls;
        walls.Sort( utilitySort.SortByIndexPictureInWalls );

        if ( NextPictureIsInClosestWall() || NextPictureIsInDetachedWall() ) 
        {
            picturesOnWalls[ currentWall ].RemoveAll( ( pic ) => VisitedPictures.Contains( pic ) );
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
            GameObject wallWithMinPicIndex = wallsIntersectCurrentWall[0];
            picturesOnWalls[ wallWithMinPicIndex ].RemoveAll( ( pic ) => VisitedPictures.Contains( pic ) );
            PictureInfo picWithMinIndex = picturesOnWalls[ wallWithMinPicIndex ][ 0 ];

            if ( picWithMinIndex.index >= CurrentPictureIndex )
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
                picturesOnWalls[ wall ].RemoveAll( ( pic ) => VisitedPictures.Contains( pic ) );
                if( picturesOnWalls[wall].Count > 0 )
                {
                    if ( picturesOnWalls[wall][0].index < CurrentPictureIndex + 10 )
                        intersectsWalls.Add( wall );
                }
            }
        }

        utilitySort.picturesOnWalls = picturesOnWalls;
        intersectsWalls.Sort( utilitySort.SortByIndexPictureInWalls );

        return intersectsWalls;
    }

    private GameObject GetPlaneOfCurrentPicture ()
    {
        if( pictures.Current )
        {
            if( pictures.Current.transform.GetChild(0) )
                return pictures.Current.transform.GetChild( 0 ).gameObject;
        }

        return GetPlaneOfExit();
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
                picturesOnWalls.Add( wall, new List<PictureInfo>() );
            }
        }

    }


    private void FindPicturesOnWalls ()
    {

        foreach ( GameObject picturePlane in GameObject.FindGameObjectsWithTag( "PicturePlane" ) )
        {
            GameObject picture = (picturePlane.transform).parent.gameObject;
            GameObject wall = (picture.transform).parent.gameObject;

            picturesOnWalls[ wall ].Add( picture.GetComponent<PictureInfo>() );
        }

    }


    private void SortPicturesOnWalls ()
    {
        foreach ( List<PictureInfo> pics in picturesOnWalls.Values )
            pics.Sort( utilitySort.SortByIndex );
    }


}

