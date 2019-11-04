using System.Collections.Generic;
using UnityEngine;

public class PapillonPattern : PathManager
{

    // Pattern movimento
    private List<GameObject> walls = new List<GameObject>();
    
    private int currentPictureIndex = 0;

    private GameObject nextDestination;

    public int numberOfStop;
    
    public override void InitMovementPattern ()
    {
        FindWallsWithPictures();
        FindPicturesOnWalls();

        numberOfStop = Random.Range( 15, walls.Count );
    }


    public override GameObject GetNextDestination ()
    {

        if ( importantPictures.Count <= 0 || distanzaPercorsa > maxDistanza )
            return GetPlaneOfExit();

        if ( LookInBackward() )
        {
            if( Random.Range(1, 10)  >  7 )
            {
                LookNextIndex();
                return nextDestination;
            }

            return nextDestination;
        }

        if ( LookNextIndex() || LookNextIndex(0) )
        {
            if ( Random.Range( 0, 1 ) > 0.5f )
            {
                return GetNextDestination();
            }

            return nextDestination;
        }

        return GetPlaneOfExit();
    }


    private bool LookNextIndex (int maxJump = 5)
    {
        foreach(List<GameObject> pics in picturesOnWalls.Values)
        {
            foreach(GameObject pic in pics )
            {
                if( pic.GetComponent<PictureInfo>().index > currentPictureIndex && pic.GetComponent<PictureInfo>().index < currentPictureIndex + maxJump )
                {
                    nextDestination = pic.transform.GetChild( 0 ).gameObject;
                    currentPictureIndex = pic.GetComponent<PictureInfo>().index;
                    picturesOnWalls[ pic.transform.parent.gameObject ].Remove( pic );

                    if ( picturesOnWalls[ pic.transform.parent.gameObject ].Count <= 0 )
                        picturesOnWalls.Remove( pic.transform.parent.gameObject );

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
                if( picturesOnWalls.ContainsKey( hit.collider.gameObject ) )
                    considerateWall.Add( hit.collider.gameObject );
            }
        }

        if ( considerateWall.Count <= 0 )
            return false;

        return SelectNextPicInBackwardWalls(considerateWall);
    }

    private bool SelectNextPicInBackwardWalls(List<GameObject> considerateWall)
    {
        utilitySort.picturesOnWalls = picturesOnWalls;
        considerateWall.Sort( utilitySort.SortByIndexPictureInWalls );

        if( picturesOnWalls.ContainsKey(considerateWall[0]) )
        {
            List<GameObject> consideratePics = picturesOnWalls[ considerateWall[ 0 ] ];
            consideratePics.Sort( utilitySort.Distanza );

            GameObject mostClosePicture = consideratePics[ 0 ];

            nextDestination = mostClosePicture.transform.GetChild( 0 ).gameObject;
            currentPictureIndex = mostClosePicture.GetComponent<PictureInfo>().index;

            consideratePics.Remove( mostClosePicture );

            if ( consideratePics.Count <= 0 )
                picturesOnWalls.Remove( considerateWall[ 0 ] );

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
        foreach ( GameObject wall in walls )
        {
            foreach ( Transform picture in wall.transform )
            {
                if ( picture.gameObject.transform.GetChild( 0 ).CompareTag( "PicturePlane" ) )
                    picturesOnWalls[ wall].Add( picture.gameObject );
            }

            if( picturesOnWalls.ContainsKey(wall) )
                picturesOnWalls[ wall ].Sort( utilitySort.SortByIndexPicture );
        }
    }

}
