using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    private Rigidbody rigidBody;
    protected GameObject destination;
    public bool inPausa = true;
    protected  GameObject destinationPoint;
    
    protected Color colorDrawPath = Color.red;
    // Animazione
    protected AnimationViewPicture animationManager;

    // Segui percorso
    protected NavMeshPath path;
    private Vector3 firstCornerTarget;
    private int indexCornerPath = 1;
    protected float timedelta = 0f;

    public float pauseTime = 5f;

    public float rigidBodySpeed = 7f;

    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();

    bool isHasty = false;

    protected virtual void Start ()
    {
        InitAnimationBheavior();
        InitMovementPattern();
        UpdateIsHasty();
        UpdatePauseTime();
        UpdatePathAndDestination();
    }

    public virtual void UpdateIsHasty(){
        isHasty = (Random.Range(0 , 1) > 0.5f);
    } 

    public void UpdatePauseTime ()
    {
        pauseTime = (pauseTime * Random.Range( 1f, 1.7f ));
    }

    public void InitAnimationBheavior ()
    {
        rigidBody = GetComponent<Rigidbody>();

        //generalAnimation = GetComponent<AnimateCharacter>();
        //generalAnimation.turnBack = true;
        //generalAnimation.tolleranceLeft = -3f;
        //generalAnimation.tolleranceRight = 3f;
        //generalAnimation.angleForTurnLeft = generalAnimation.angleForTurnRight = 60f;
        
        animationManager = GetComponent<AnimationViewPicture>();
        animationManager.turnBack = true;
        animationManager.tolleranceLeft = -1.5f;
        animationManager.tolleranceRight = 1.5f;
        animationManager.angleForTurnLeft = animationManager.angleForTurnRight = 50f;

    }


    private void Update ()
    {
        if ( inPausa )
        {
            CheckNextDestination();
        }
        else
        {
            if ( timedelta > pauseTime )
            {
                UpdatePathAndDestination();
                timedelta = 0f;
            }

            Walk();
            animationManager.Animation_Walk();
        }
    }


    protected void OnCollisionEnter ( Collision collision )
    {
        if( collision.gameObject.CompareTag( "Uscita") && collision.gameObject == destination )
        {
            Destroy( gameObject );
        }
    }


    protected virtual void OnCollisionStay ( Collision collision )
    {
        if ( collision.gameObject.CompareTag( "PicturePlane" ) && collision.gameObject == destination )
        {
            animationManager.path = path;
            animationManager.TurnTowardsPicture( collision );
            timedelta += Time.deltaTime;
        }
    }


    private void Walk ()
    {
        DrawPath();
        animationManager.Turn();
        FollowPath();
    }


    private void UpdatePathAndDestination ( )
    {
        path = new NavMeshPath();
        destination = GetNextDestination();
        UpdatePauseTime();
        CheckNextDestination();
    }

    private void CheckNextDestination ()
    {
        if ( destinationPoint != null )
            destinationPoint.GetComponent<DestinationPoint>().Libera();

        if ( destination.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            GeneratePath();
            if( !isHasty )
                inPausa = false;
        }
        else
        {
            if( !isHasty )
                inPausa = true;
            else
                UpdatePathAndDestination();
        }
    }

    private void GeneratePath(){
        NavMesh.CalculatePath( transform.position, GetPositionInFloorPicture(), 1, path );
        firstCornerTarget = path.corners[ 1 ] - transform.position;
        animationManager.angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, firstCornerTarget );
        animationManager.localPos = transform.InverseTransformPoint( path.corners[ 1 ] );
        indexCornerPath = 1;
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
                Debug.DrawLine( path.corners[ i ], path.corners[ i + 1 ], colorDrawPath, 500f);
        }
    }

    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

    private void FollowPath ()
    {
        SetAnimationData();
        
        if ( animationManager.CheckTurn() )
        {
            rigidBody.MovePosition( Vector3.MoveTowards( transform.position, path.corners[ indexCornerPath ], Time.deltaTime * rigidBodySpeed ) );
            animationManager.speed = 1f;
            GetComponent<Collider>().isTrigger = true;

            float distanceFromCorner = Vector3.Distance( transform.position, path.corners[ indexCornerPath ] );

            if ( distanceFromCorner > 0.1f )
                animationManager.RotationToTarget( path.corners[ indexCornerPath ], 10f );

            // Passa al corner successivo
            if ( distanceFromCorner < 0.5f )
            {
                if ( !PathHaveNextCorner() )
                {
                    GetComponent<Collider>().isTrigger = false;
                    animationManager.speed = 0f;
                }
            }
        }
    }

    private bool PathHaveNextCorner ()
    {
        indexCornerPath++;

        // Destinazione raggiunta
        if ( indexCornerPath > path.corners.Length - 1 )
        {
            path = null;
            indexCornerPath = 1;

            return false;
        }

        return true;
    }

    private void SetAnimationData ()
    {
        if ( path != null && path.corners.Length > indexCornerPath )
        {
            animationManager.target = path.corners[ indexCornerPath ];
        }
        else
        {
            animationManager.target = Vector3.one;
        }
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
