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


    public override void InitMovementPattern ()
    {
        colorDrawPath = Color.yellow;

        FindWallsWithPictures();
        FindPicturesOnWalls();

        currentWall = startWall;

    }


    public override GameObject GetNextDestination ()
    {

        if ( IsNextPictureOnCurrentWall() )
        {
            return GetPlaneOfCurrentPicture();
        }

        if ( IsNextPictureOnNextWall() )
        {
            return GetPlaneOfCurrentPicture();
        }

        return GetPlaneOfExit();

    }


    private bool IsNextPictureOnCurrentWall ()
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


    private bool IsNextPictureOnNextWall ()
    {

        if ( ExistsNextWall() )
        {
            if ( picturesOnWalls.ContainsKey( currentWall ) )
            {
                pictures = picturesOnWalls[ currentWall ].GetEnumerator();
                pictures.MoveNext();
                RefreshCurrentPictureIndex();
                return true;
            }
        }

        return false;
    }


    private GameObject GetPlaneOfCurrentPicture ()
    {

        return pictures.Current.transform.GetChild( 0 ).gameObject;

    }


    private bool ExistsNextWall ()
    {

        walls.Remove( currentWall );

        List<GameObject> wallsIntersectCurrentWall = new List<GameObject>();
        wallsIntersectCurrentWall = GetWallsIntersectsWithCurrentWall();

        if ( wallsIntersectCurrentWall.Count > 0 )
        {
            if ( ExistsNextPictureIndex( wallsIntersectCurrentWall ) )
            {
                return true;
            }

        }

        if ( ExistsWallWithNextIndexPic() )
        {
            return true;
        }

        return false;
    }


    private bool ExistsNextPictureIndex ( List<GameObject> localWalls )
    {

        GameObject nextWall = null;
        int nextIndex_tmp = 1000;

        int maxDelayPictureIndex = 7;
        int maxNextPictureIndex = maxDelayPictureIndex + currentPictureIndex;

        foreach ( GameObject wall in localWalls )
        {

            GameObject firstPicWall = picturesOnWalls[ wall ][ 0 ];
            int indexfirstPicWall = firstPicWall.GetComponent<PictureInfo>().index;

            if ( indexfirstPicWall > currentPictureIndex )
            {
                if ( indexfirstPicWall < maxNextPictureIndex )
                {
                    if ( indexfirstPicWall < nextIndex_tmp )
                    {
                        nextIndex_tmp = indexfirstPicWall;
                        nextWall = wall;
                    }
                }
            }

        }

        if ( nextWall )
        {
            currentWall = nextWall;
            return true;
        }

        return false;

    }


    private List<GameObject> GetWallsIntersectsWithCurrentWall ()
    {

        List<GameObject> intersectsWalls = new List<GameObject>();

        foreach ( GameObject wall in walls )
        {
            if ( wall == currentWall )
                continue;

            if ( currentWall.GetComponent<MeshRenderer>().bounds.Intersects( wall.GetComponent<MeshRenderer>().bounds ) )
            {
                intersectsWalls.Add( wall );
            }

        }

        return intersectsWalls;

    }


    private GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }


    private bool ExistsWallWithNextIndexPic ()
    {

        foreach ( GameObject wall in walls )
        {
            if ( picturesOnWalls.ContainsKey( wall ) )
            {
                foreach ( GameObject picture in picturesOnWalls[ wall ] )
                {
                    if ( picture.GetComponent<PictureInfo>().index == currentPictureIndex + 1 )
                    {
                        currentWall = wall;
                        return true;
                    }
                }
            }
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

        foreach ( GameObject picturePlane in GameObject.FindGameObjectsWithTag( "Quadro" ) )
        {
            GameObject picture = (picturePlane.transform).parent.gameObject;
            GameObject wall = (picture.transform).parent.gameObject;

            picturesOnWalls[ wall ].Add( picture );

        }


        foreach ( List<GameObject> pics in picturesOnWalls.Values )
        {
            pics.Sort( SortByIndexPicture );
        }
    }


    private int SortByIndexPicture ( GameObject x, GameObject y )
    {

        float index_1 = x.GetComponent<PictureInfo>().index;
        float index_2 = y.GetComponent<PictureInfo>().index;

        if ( index_1 < index_2 ) return -1;
        if ( index_1 > index_2 ) return 1;
        return 0;

    }

}

