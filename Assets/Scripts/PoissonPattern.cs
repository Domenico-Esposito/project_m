﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PoissonPattern : PathManager
{

    // Segui percorso
    private List<GameObject> poissonFloors;

    private IEnumerator<GameObject> pathPart;


    public override void InitMovementPattern ()
    {
        colorDrawPath = Color.magenta;

        poissonFloors = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Fish Floor" ) );
        poissonFloors.Sort( RandomIndex );
        //poissonFloors.Sort( SortByIndexPlace );

        //if ( Random.Range( 0f, 1f ) > 0.5f )
        //{
        //    poissonFloors.Reverse();
        //}

        pathPart = poissonFloors.GetEnumerator();
    }

    //protected override void OnCollisionStay ( Collision collision )
    //{
    //    base.OnCollisionStay( collision );

    //    if ( collision.gameObject.CompareTag( "Fish Floor" ) )
    //    {
    //        timedelta += Time.deltaTime;
    //    }

    //}

    //private void OnCollisionExit ( Collision collision )
    //{
    //    if ( collision.gameObject.CompareTag( "Fish Floor" ) && path != null )
    //    {
    //        timedelta = 0f;
    //    }
    //}


    protected override GameObject GetPointInDestination ()
    {
        destinationPoint = destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        return destinationPoint;
    }


    public override GameObject GetNextDestination ()
    {
        bool viewPicture = Random.Range( 0, 10 ) > 5 ? true : false;

        if( viewPicture )
            return GetPictureDestination();

        return GetFishPlaneDestination();
    }

    private GameObject GetMostClosePicture ()
    {
        List<GameObject> planes = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) );
        planes.Sort( Distanza );

        return planes[ 0 ];
    }

    private int Distanza ( GameObject x, GameObject y )
    {
        float distance_1 = GetPathLength( x );
        float distance_2 = GetPathLength( y );

        if ( distance_1 < distance_2 ) return -1;
        if ( distance_1 > distance_2 ) return 1;
        return 0;
    }

    private GameObject GetPictureDestination ()
    {
        return GetMostClosePicture();
    }


    private GameObject GetFishPlaneDestination ()
    {
        if ( pathPart.MoveNext() )
            return pathPart.Current;

        return GetPlaneOfExit();
    }


    private int SortByIndexPlace ( GameObject x, GameObject y )
    {

        float index_1 = x.GetComponent<PictureInfo>().index;
        float index_2 = y.GetComponent<PictureInfo>().index;

        if ( index_1 < index_2 ) return -1;
        if ( index_1 > index_2 ) return 1;
        return 0;

    }

    private int RandomIndex ( GameObject x, GameObject y )
    {
        float hx = Random.Range( 0, 10 );
        float hy = Random.Range( 0, 10 );

        if ( hx < hy ) return -1;
        if ( hx > hy ) return 1;
        return 0;
    }

}

