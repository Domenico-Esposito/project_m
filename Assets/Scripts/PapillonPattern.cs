using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PapillonPattern : PathManager
{

    // Pattern movimento
    private List<GameObject> allPictures = new List<GameObject>();

    private List<GameObject> walls = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> pictureOnWall = new Dictionary<GameObject, List<GameObject>>();

    private int currentPictureIndex = 0;

    private GameObject nextPicture;

    public override void InitMovementPattern ()
    {

        FindWallsWithPictures();
        FindPicturesOnWalls();
        
    }


    public override GameObject GetNextDestination ()
    { 
        nextPicture = null;

        if( LookInBackward() == false )
        {
            if(LookNextIndex() == false )
            {
                FirstPicture();
            }

        }

        return nextPicture;
    }


    private void FirstPicture ()
    {
        nextPicture = allPictures[0].transform.GetChild( 0 ).gameObject;
        currentPictureIndex = allPictures[0].GetComponent<PictureInfo>().index;
    }


    private bool LookNextIndex ()
    {

        foreach ( GameObject picture in allPictures )
        {
            if ( picture.GetComponent<PictureInfo>().index > currentPictureIndex )
            {
                currentPictureIndex = picture.GetComponent<PictureInfo>().index;
                nextPicture = picture.transform.GetChild( 0 ).gameObject;
                return true;
            }
        }

        return false;
    }



    private bool LookInBackward ()
    {
        Vector3 backLeft = -Vector3.RotateTowards( transform.forward, -transform.forward, 10 * Mathf.Deg2Rad, 0.0f );
        Vector3 backRight = -Vector3.RotateTowards( transform.forward, -transform.forward, 350 * Mathf.Deg2Rad, 0.0f );
        Vector3 back = -transform.forward;

        Vector3[] directions = { backLeft, backRight, back };

        Vector3 rayCastStartPoint = transform.position + new Vector3( 0, 1, 0 );

        RaycastHit hit;

        foreach ( Vector3 direction in directions )
        {
            if ( Physics.Raycast( rayCastStartPoint, direction, out hit, 50f ) )
            {
                if ( hit.collider.gameObject.CompareTag( "Wall" ) )
                {
                    GameObject hitWall = hit.collider.gameObject;

                    if ( pictureOnWall.ContainsKey( hitWall ) )
                    {
                        if ( pictureOnWall[hitWall].Count > 0 )
                        {
                            foreach ( GameObject picture in pictureOnWall[hitWall] )
                            {
                                if ( picture.GetComponent<PictureInfo>().index > currentPictureIndex )
                                {
                                    currentPictureIndex = picture.GetComponent<PictureInfo>().index;
                                    nextPicture = picture.transform.GetChild( 0 ).gameObject;
                                    return true;
                                }
                            }
                        }
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
                pictureOnWall.Add( wall, new List<GameObject>() );
            }
        }

    }


    private void FindPicturesOnWalls ()
    {

        foreach ( GameObject wall in walls )
        {
            foreach ( Transform picture in wall.transform )
            {
                if ( picture.gameObject.transform.GetChild( 0 ).CompareTag( "Quadro" ) )
                    pictureOnWall[wall].Add( picture.gameObject );
            }

        }

        foreach ( GameObject picture in GameObject.FindGameObjectsWithTag( "Quadro" ) )
        {
            allPictures.Add( picture.transform.parent.gameObject );
        }

        allPictures.Sort( SortByIndexPicture );


        foreach ( GameObject wall in pictureOnWall.Keys )
        {
            pictureOnWall[wall].Sort( SortByIndexPicture );
        }

    }


    private int SortByIndexPicture ( GameObject x, GameObject y )
    {

        float distance_1 = x.GetComponent<PictureInfo>().index;
        float distance_2 = y.GetComponent<PictureInfo>().index;

        if ( distance_1 < distance_2 ) return -1;
        if ( distance_1 > distance_2 ) return 1;
        return 0;

    }
}
