using System.Collections.Generic;
using UnityEngine;

public class PapillonPattern : PathManager
{

    // Pattern movimento
    private List<GameObject> walls = new List<GameObject>();
    
    //private int CurrentPictureIndex = 0;

    private GameObject nextDestination;
    private Dictionary<GameObject, List<PictureInfo>> picturesOnWalls = new Dictionary<GameObject, List<PictureInfo>>();

    public int numberOfStop;

    private void Awake ()
    {
        GetComponentInChildren<Renderer>().material.SetColor("_Color", new Color32( 241, 108, 0, 1 ) );
    }

    public override void InitMovementPattern ()
    {
        FindWallsWithPictures();
        FindPicturesOnWalls();

        numberOfStop = Random.Range( 15, walls.Count );
        maxDistanza = 300;
    }

    public override GameObject GetNextDestination ()
    {

        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueStatus > FatigueManager.MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( LookInBackward() )
        {
            Debug.Log( "Ho già visitato questo quadro? " + VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ), nextDestination.transform.parent.gameObject );

            if( Random.Range(1, 10)  >  7 || VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ) )
            {
                LookNextIndex();
                return nextDestination;
            }

            return nextDestination;
        }

        if ( LookNextIndex() || LookNextIndex(0) )
        {
            Debug.Log( "Ho già visitato questo quadro? " + VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ), nextDestination.transform.parent.gameObject );

            if ( Random.Range( 0, 1 ) > 0.5f || VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ) )
            {
                return GetNextDestination();
            }

            return nextDestination;
        }


        return GetPlaneOfExit();
    }


    private bool LookNextIndex (int maxJump = 5)
    {
        foreach(List<PictureInfo> pics in picturesOnWalls.Values)
        {
            foreach(PictureInfo pic in pics )
            {
                if( pic.index > CurrentPictureIndex && pic.index < CurrentPictureIndex + maxJump )
                {
                    nextDestination = pic.transform.GetChild( 0 ).gameObject;
                    CurrentPictureIndex = pic.index;
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

        //Debug.DrawRay( transform.position + new Vector3( 0, 0, 0 ), directions[ 0 ], Color.red, 10 );
        //Debug.DrawRay( transform.position + new Vector3( 0, 0, 0 ), directions[ 1 ], Color.yellow, 10 );
        //Debug.DrawRay( transform.position + new Vector3( 0, 0, 0 ), directions[ 2 ], Color.blue, 10 );

        foreach ( Vector3 direction in directions )
        {
            if ( Physics.Raycast( transform.position + new Vector3( 0, 0, 0 ), direction, out hit, 150f, layer_mask ) )
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
            List<PictureInfo> consideratePics = picturesOnWalls[ considerateWall[ 0 ] ];
            consideratePics.Sort( utilitySort.DistanzaPicture );

            PictureInfo mostClosePicture = consideratePics[ 0 ];

            nextDestination = mostClosePicture.transform.GetChild( 0 ).gameObject;
            CurrentPictureIndex = mostClosePicture.index;

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
                picturesOnWalls.Add( wall, new List<PictureInfo>() );
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
                    picturesOnWalls[ wall].Add( picture.GetComponent<PictureInfo>() );
            }

            if( picturesOnWalls.ContainsKey(wall) )
                picturesOnWalls[ wall ].Sort( utilitySort.SortByIndex );
        }
    }

}
