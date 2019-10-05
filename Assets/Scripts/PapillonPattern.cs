using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PapillonPattern : PathManager
{

    // Pattern movimento
    private List<GameObject> walls = new List<GameObject>();

    private Dictionary<GameObject, List<GameObject>> picturesOnWall = new Dictionary<GameObject, List<GameObject>>();

    private int currentPictureIndex = 0;

    private GameObject nextPicture;

    public int numberOfStop;

    public override void InitMovementPattern ()
    {
        colorDrawPath = new Color( 1.0f, 0.64f, 0.0f ); //orange
        FindWallsWithPictures();
        FindPicturesOnWalls();

        numberOfStop = Random.Range( 15, walls.Count );
    }


    public override GameObject GetNextDestination ()
    {
    
        if ( numberOfStop <= 0 )
            return GetPlaneOfExit();

        if ( LookInBackward() )
        {
            if( Random.Range(1, 10)  >  7 )
            {
                LookNextIndex();
                return nextPicture;
            }

            numberOfStop -= 1;
            return nextPicture;
        }

        if ( LookNextIndex() || LookNextIndex(0) )
        {
            if ( Random.Range( 0, 1 ) > 0.5f )
            {
                return GetNextDestination();
            }

            numberOfStop -= 1;
            return nextPicture;
        }

        return GetPlaneOfExit();
    }


    private bool LookNextIndex (int maxJump = 5)
    {

        foreach(List<GameObject> pics in picturesOnWall.Values)
        {
            foreach(GameObject pic in pics )
            {
                if( pic.GetComponent<PictureInfo>().index > currentPictureIndex && pic.GetComponent<PictureInfo>().index < currentPictureIndex + maxJump )
                {
                    nextPicture = pic.transform.GetChild( 0 ).gameObject;
                    currentPictureIndex = pic.GetComponent<PictureInfo>().index;
                    picturesOnWall[ pic.transform.parent.gameObject ].Remove( pic );

                    if ( picturesOnWall[ pic.transform.parent.gameObject ].Count <= 0 )
                    {
                        picturesOnWall.Remove( pic.transform.parent.gameObject );
                    }

                    return true;
                }
            }
        }


        return false;
    }


    private bool LookInBackward ()
    {

        Vector3[ ] directions =
        {   -transform.forward,
            Quaternion.AngleAxis( 150, transform.up ) * transform.forward,
            Quaternion.AngleAxis( 210, transform.up ) * transform.forward,
        };

        List<GameObject> considerateWall = new List<GameObject>();
        RaycastHit hit; 

        int layer_mask = LayerMask.GetMask( "Walls" );

        //Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), directions[ 0 ], Color.red, 10 );
        //Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), directions[ 1 ], Color.yellow, 10 );
        //Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), directions[ 2 ], Color.blue, 10 );

        foreach ( Vector3 direction in directions )
        {

            if ( Physics.Raycast( transform.position + new Vector3( 0, 5, 0 ), direction, out hit, 50f, layer_mask ) )
            {
                if( picturesOnWall.ContainsKey( hit.collider.gameObject ) )
                {
                    considerateWall.Add( hit.collider.gameObject );
                }
            }

        }


        if ( considerateWall.Count <= 0 )
            return false;

        considerateWall.Sort( SortByIndexPictureInWalls );

        if( picturesOnWall.ContainsKey(considerateWall[0]) )
        {

            List<GameObject> consideratePic = picturesOnWall[ considerateWall[ 0 ] ];
            consideratePic.Sort( Distanza );

            nextPicture = consideratePic[ 0 ].transform.GetChild( 0 ).gameObject;
            currentPictureIndex = consideratePic[ 0 ].GetComponent<PictureInfo>().index;

            picturesOnWall[ considerateWall[ 0 ] ].Remove( consideratePic[ 0 ] );

            if ( picturesOnWall[ considerateWall[0]].Count <= 0 )
            {
                picturesOnWall.Remove( considerateWall[ 0 ] );
            }

            return true;
        }


        return false;
    }


    private int Distanza ( GameObject x, GameObject y )
    {
        float distance_1 = GetPathLength( x );
        float distance_2 = GetPathLength( y );

        if ( distance_1 < distance_2 ) return -1;
        if ( distance_1 > distance_2 ) return 1;
        return 0;
    }


    private int SortByIndexPictureInWalls ( GameObject wallX, GameObject wallY )
    {

        GameObject quadro_x = picturesOnWall[ wallX ][ 0 ];
        GameObject quadro_y = picturesOnWall[ wallY ][ 0 ];

        return SortByIndexPicture( quadro_x, quadro_y );
    }


    private void FindWallsWithPictures ()
    {

        foreach ( GameObject wall in GameObject.FindGameObjectsWithTag( "Wall" ) )
        {
            if ( wall.transform.childCount > 0 )
            {
                walls.Add( wall );
                picturesOnWall.Add( wall, new List<GameObject>() );
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
                {
                    picturesOnWall[wall].Add( picture.gameObject );
                }
            }

            if( picturesOnWall.ContainsKey(wall) )
            {
                picturesOnWall[ wall ].Sort( SortByIndexPicture );
            }
        }


    }

}
