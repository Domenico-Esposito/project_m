using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    private Rigidbody rigidBody;
    protected GameObject destination;
    protected GameObject destinationPoint;
    private NavMeshAgent navMeshAgent;

    protected Color colorDrawPath = Color.red;
    // Animazione
    protected AnimationViewPicture animationManager;

    // Segui percorso
    protected NavMeshPath path;
    private Vector3 firstCornerTarget;
    public float timedelta = 0f;

    public float pauseTime = 5f;

    private float baseTime;

    public float rigidBodySpeed = 7f;

    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();

    protected virtual void Start ()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        baseTime = pauseTime;
        InitAnimationBheavior();
        InitMovementPattern();
        UpdateDestination();
    }

    public void InitAnimationBheavior ()
    {
        rigidBody = GetComponent<Rigidbody>();

        animationManager = GetComponent<AnimationViewPicture>();
        animationManager.turnBack = true;
        animationManager.tolleranceLeft = -1.5f;
        animationManager.tolleranceRight = 1.5f;
        animationManager.angleForTurnLeft = animationManager.angleForTurnRight = 50f;

    }


    private void Update ()
    {
        if ( timedelta > pauseTime)
        {
            UpdateDestination();
            timedelta = 0f;
        }
        
        Walk();

    }

    private void UpdateDestination ()
    {
        destination = GetNextDestination();
        CheckNextDestination();
    }

    private void CheckNextDestination ()
    {
        if( destination.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            if ( destinationPoint != null )
                destinationPoint.GetComponent<DestinationPoint>().Libera();

            destinationPoint = destination.GetComponent<GridSystem>().GetAvailablePoint();
            destinationPoint.GetComponent<DestinationPoint>().Occupa();

            animationManager.target = destinationPoint.transform.position;

        }
        else
        {
            UpdateDestination();
        }
    }

    protected virtual void OnCollisionStay ( Collision collision )
    {
        if ( collision.gameObject.CompareTag( "PicturePlane" ) && collision.gameObject == destination )
        {
            if( navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
            {
                animationManager.speed = 0f;
                animationManager.Animation_Walk();

                animationManager.TurnTowardsPicture( collision );
                timedelta += Time.deltaTime;
            }

        }
    }

    protected void OnCollisionEnter ( Collision collision )
    {
        if( collision.gameObject.CompareTag("Uscita") && collision.gameObject == destination )
        {
            Destroy( gameObject );
        }
    }



    private void Walk ()
    {
        if( !animationManager.IsRotation() )
        {
            animationManager.Turn();
        }

        if ( animationManager.CheckTurn())
        {
            if( destinationPoint != null )
            {
                navMeshAgent.SetDestination( destinationPoint.transform.position );
                animationManager.speed = 1f;
                animationManager.Animation_Walk();
            }
            else
            {
                Debug.Log( "DestinationPoint è NULL" );
            }
        }

    }


    public virtual Vector3 GetPositionInFloorPicture ()
    {
        destinationPoint = destination.GetComponent<GridSystem>().GetAvailablePoint();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        return destinationPoint.transform.position;
    }

    public float GetPathLength ( GameObject picture )
    {
        NavMeshPath p = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, picture.transform.GetChild( 0 ).transform.position, 1, p );

        float lng = 0;

        for ( int i = 0; i < p.corners.Length - 1; i++ )
        {
            lng += Vector3.Distance( p.corners[ i ], p.corners[ i + 1 ] );
        }

        return lng;
    }

    private void DrawPath ()
    {
        if ( path != null )
        {
            for ( int i = 0; i < path.corners.Length - 1; i++ )
                Debug.DrawLine( path.corners[ i ], path.corners[ i + 1 ], colorDrawPath, 500f );
        }
    }

    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

    protected int SortByIndexPicture ( GameObject x, GameObject y )
    {

        float distance_1 = x.GetComponent<PictureInfo>().index;
        float distance_2 = y.GetComponent<PictureInfo>().index;

        if ( distance_1 < distance_2 ) return -1;
        if ( distance_1 > distance_2 ) return 1;
        return 0;

    }

    protected int Distanza ( GameObject x, GameObject y )
    {
        float distance_1 = GetPathLength( x );
        float distance_2 = GetPathLength( y );

        if ( distance_1 < distance_2 ) return -1;
        if ( distance_1 > distance_2 ) return 1;
        return 0;
    }
}
