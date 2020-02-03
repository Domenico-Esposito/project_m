using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FourmiAgent : BaseAgent
{
    private IEnumerator<PictureInfo> picsOnCurrentWall;
    public List<GameObject> walls = new List<GameObject>();

    public GameObject startWall;
    private GameObject currentWall;
    private Dictionary<GameObject, List<PictureInfo>> picturesOnWalls = new Dictionary<GameObject, List<PictureInfo>>();

    public int maxDiffNextPicIndexInClosestWall = 10;

    private void Awake ()
    {
        Color32 yellow = new Color32( 250, 231, 44, 1 );
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", yellow );
    }

    public override void InitMovementPattern ()
    { 
        FindWallsWithPictures();
        FindPicturesOnWalls();
        SortPicturesOnWalls();

        currentWall = GameObject.FindGameObjectsWithTag( "Wall" )[Random.Range(0, 3)];

        MaxDistanza = 280;
        ChanceSkipDestination = 65;
    }


    public override GameObject GetNextDestination ()
    {
        if ( ImportantPictures.Count <= 0 && !groupData.LeaderIsAlive || FatigueLevel >= FatigueManager.Level.MOLTO_STANCO)
            return GetPlaneOfExit();
            
        if ( MoveToNextPicOnCurrentWall() || MoveToNextPicOnAnotherWall() ) 
        {
            bool skipNewDestination = Random.Range( 0, 100 ) < ChanceSkipDestination;

            if ( skipNewDestination )
                    return GetNextDestination();

        }
        else
        {
            return GetPlaneOfExit();
        }

        if( VisitedPictures.Contains( picsOnCurrentWall.Current ) )
        {
            return GetNextDestination();
        }

        return GetPlaneOfCurrentPicture();
    }


    private bool MoveToNextPicOnCurrentWall ()
    {
        if ( picsOnCurrentWall == null )
            return false;

        if ( picsOnCurrentWall.MoveNext() )
        {
            RefreshCurrentPictureIndex();
            return true;
        }

        return false;
    }


    private void RefreshCurrentPictureIndex ()
    {   
        if( picsOnCurrentWall.Current )
        {
            CurrentPictureIndex = picsOnCurrentWall.Current.index;
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
            picsOnCurrentWall = picturesOnWalls[ currentWall ].GetEnumerator();
            picsOnCurrentWall.MoveNext();
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
                    if ( picturesOnWalls[wall][0].index - CurrentPictureIndex < maxDiffNextPicIndexInClosestWall )
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
        if( picsOnCurrentWall.Current )
        {
            if( picsOnCurrentWall.Current.transform.GetChild(0) )
                return picsOnCurrentWall.Current.transform.GetChild( 0 ).gameObject;
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
            int numberOfPictureOnWall = wall.GetComponentsInChildren<PictureInfo>().Length;
            if ( numberOfPictureOnWall > 0 )
            {
                walls.Add( wall );
                picturesOnWalls.Add( wall, new List<PictureInfo>() );
            }
        }
    }


    private void FindPicturesOnWalls ()
    {
        foreach ( GameObject pictureGrid in GameObject.FindGameObjectsWithTag( "PicturePlane" ) )
        {
            PictureInfo picture = pictureGrid.GetComponentInParent<PictureInfo>();
            GameObject wall = (picture.transform).parent.gameObject;

            picturesOnWalls[ wall ].Add( picture );
        }
    }


    private void SortPicturesOnWalls ()
    {
        foreach ( List<PictureInfo> pics in picturesOnWalls.Values )
            pics.Sort( utilitySort.SortByIndex );
    }


}

