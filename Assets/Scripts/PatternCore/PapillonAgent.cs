using System.Collections.Generic;
using UnityEngine;

public class PapillonAgent : BaseAgent
{

    private List<GameObject> walls = new List<GameObject>();
    private GameObject nextDestination;
    private Dictionary<GameObject, List<PictureInfo>> picturesOnWalls = new Dictionary<GameObject, List<PictureInfo>>();
    
    private void Awake ()
    {
        Color32 orange = new Color32( 241, 108, 0, 1 );
        GetComponentInChildren<Renderer>().material.SetColor("_Color", orange );
    }

    public override void InitMovementPattern ()
    {
        FindWallsWithPictures();
        FindPicturesOnWalls();

        ChanceSkipDestination = 65;
        MaxDistanza = 300;
    }

    public override GameObject GetNextDestination ()
    {

        if ( ( ImportantPictures.Count <= 0 && !groupData.LeaderIsAlive ) || FatigueLevel >=  FatigueManager.Level.MOLTO_STANCO )
            return GetPlaneOfExit();

        bool skipDestination = Random.Range( 0, 100 ) < ChanceSkipDestination;

        if ( LookInBackward() )
        {
            if ( skipDestination || VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ) )
            {
                LookNextIndex();
                return nextDestination;
            }

            return nextDestination;
        }

        if ( LookNextIndex() || LookNextIndex(0) )
        {
            if ( skipDestination || VisitedPictures.Contains( nextDestination.GetComponentInParent<PictureInfo>() ) )
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
                    nextDestination = pic.GetComponentInChildren<GridSystem>().gameObject;
                    CurrentPictureIndex = pic.index;
                    GameObject currentPictureWall = pic.transform.parent.gameObject;
                    picturesOnWalls[ currentPictureWall ].Remove( pic );

                    if ( picturesOnWalls[ currentPictureWall ].Count <= 0 )
                        picturesOnWalls.Remove( currentPictureWall );

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

        foreach ( Vector3 direction in directions )
        {
            float maxRayCastLenght = 150f;
            if ( Physics.Raycast( transform.position, direction, out hit, maxRayCastLenght, layer_mask ) )
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

        GameObject wallWithPictureSmallerIndex = considerateWall[ 0 ];

        if ( picturesOnWalls.ContainsKey( wallWithPictureSmallerIndex ) )
        {
            List<PictureInfo> consideratePics = picturesOnWalls[ wallWithPictureSmallerIndex ];
            consideratePics.Sort( utilitySort.DistanzaPicture );

            PictureInfo mostClosePicture = consideratePics[ 0 ];

            nextDestination = mostClosePicture.GetComponentInChildren<GridSystem>().gameObject;
            CurrentPictureIndex = mostClosePicture.index;

            consideratePics.Remove( mostClosePicture );

            if ( consideratePics.Count <= 0 )
                picturesOnWalls.Remove( wallWithPictureSmallerIndex );

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
                if ( picture.GetComponentInChildren<GridSystem>().CompareTag( "PicturePlane" ) )
                    picturesOnWalls[ wall ].Add( picture.GetComponent<PictureInfo>() );
            }

            if( picturesOnWalls.ContainsKey(wall) )
                picturesOnWalls[ wall ].Sort( utilitySort.SortByIndex );
        }
    }

}
