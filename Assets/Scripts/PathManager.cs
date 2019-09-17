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
    protected AnimateCharacter generalAnimation;
    protected AnimationViewQuadro quadroViewAnimation;

    // Segui percorso
    protected NavMeshPath path;
    private Vector3 firstCornerTarget;
    private int indexCornerPath = 1;
    protected float timedelta = 0f;

    public float pauseTime = 5f;
    public float basePauseTime;

    public float rigidBodySpeed = 7f;

    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();

    protected virtual void Start ()
    {
        InitAnimationBheavior();
        InitMovementPattern();

        basePauseTime = pauseTime;

        UpdatePauseTime();
        GenerateNewPath();
    }

    public void UpdatePauseTime ()
    {
        pauseTime = basePauseTime * Random.Range( 1f, 1.7f );
    }

    public void InitAnimationBheavior ()
    {
        rigidBody = GetComponent<Rigidbody>();

        generalAnimation = GetComponent<AnimateCharacter>();
        generalAnimation.turnBack = true;
        generalAnimation.tolleranceLeft = -3f;
        generalAnimation.tolleranceRight = 3f;
        generalAnimation.angleForTurnLeft = generalAnimation.angleForTurnRight = 60f;

        quadroViewAnimation = GetComponent<AnimationViewQuadro>();
        quadroViewAnimation.tolleranceLeft = -1.5f;
        quadroViewAnimation.tolleranceRight = 1.5f;
        quadroViewAnimation.angleForTurnLeft = quadroViewAnimation.angleForTurnRight = 50f;

    }


    private void Update ()
    {
        if ( inPausa )
        {
            GeneratePathFromDestinationInPause();
        }
        else
        {
            Move();
            generalAnimation.Animation();
        }
    }


    protected virtual void OnCollisionStay ( Collision collision )
    {
        if ( collision.gameObject.CompareTag( "Quadro" ) && collision.gameObject == destination )
        {
            quadroViewAnimation.path = path;
            quadroViewAnimation.TurnTowardsPicture( collision );
            timedelta += Time.deltaTime;
        }
    }


    private void Move ()
    {
        Walk();
    }


    private void Walk ()
    {

        if ( timedelta > pauseTime )
        {
            GenerateNewPath();
            timedelta = 0f;
        }

        DrawPath();
        generalAnimation.Turn();

        if ( CheckTurn() )
        {
                FollowPath();
        }

    }


    private void GenerateNewPath ( )
    {
    
        path = new NavMeshPath();
        destination = GetNextDestination();
        UpdatePauseTime();

        if ( destinationPoint != null )
        {
            destinationPoint.GetComponent<PuntoDiInteresse>().Libera();
        }

        GeneratePathFromDestinationInPause();
    }

    private void GeneratePathFromDestinationInPause ()
    {
        if ( destination.GetComponent<PlaneScript>().HaveAvailablePoint() )
        {
            NavMesh.CalculatePath( transform.position, GetPositionInFloorPicture(), 1, path );
            firstCornerTarget = path.corners[ 1 ] - transform.position;
            generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, firstCornerTarget );
            generalAnimation.localPos = transform.InverseTransformPoint( path.corners[ 1 ] );
            indexCornerPath = 1;
            inPausa = false;
        }
        else
        {
            inPausa = true;
        }
    }

    public virtual Vector3 GetPositionInFloorPicture ()
    {
      
        destinationPoint = destination.GetComponent<PlaneScript>().GetAvailablePoint();
        destinationPoint.GetComponent<PuntoDiInteresse>().Occupa();
        
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
                Debug.DrawLine( path.corners[ i ], path.corners[ i + 1 ], colorDrawPath );
        }
    }


    private void RotationToTarget ( Vector3 target, float speed )
    {
        Quaternion targetRotation = Quaternion.LookRotation( target - transform.position );
        targetRotation.x = 0f;
        targetRotation.z = 0f;
        transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, Time.deltaTime * speed );
    }


    private bool CheckTurn ()
    {
        bool isRotation = generalAnimation.IsRotation();

        if ( !isRotation )
            rigidBody.isKinematic = false;

        if ( !isRotation && path != null )
        {
            generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, path.corners[ indexCornerPath ] - transform.position );
            generalAnimation.localPos = transform.InverseTransformPoint( path.corners[ indexCornerPath ] );

            if ( generalAnimation.angleBetweenPlayerAndTarget > generalAnimation.angleForTurnLeft )
            {
                return false;
            }

            return true;
        }

        generalAnimation.angleBetweenPlayerAndTarget = 0;
        generalAnimation.localPos = Vector3.zero;

        return false;
    }

    private void FollowPath ()
    {
        rigidBody.MovePosition( Vector3.MoveTowards( transform.position, path.corners[ indexCornerPath ], Time.deltaTime * rigidBodySpeed ) );
        generalAnimation.speed = 1f;
        GetComponent<Collider>().isTrigger = true;

        float distanceFromCorner = Vector3.Distance( transform.position, path.corners[ indexCornerPath ] );

        if ( distanceFromCorner > 0.1f )
            RotationToTarget( path.corners[ indexCornerPath ], 10f );

        // Passa al corner successivo
        if ( distanceFromCorner < 0.5f )
        {
            indexCornerPath++;

            if ( indexCornerPath > path.corners.Length - 1 )
            {
                // Destinazione raggiunta
                GetComponent<Collider>().isTrigger = false;
                generalAnimation.speed = 0f;
                indexCornerPath = 1;
                path = null;
            }
        }
    }

}
