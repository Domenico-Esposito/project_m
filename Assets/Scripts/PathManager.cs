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
    protected AnimationViewQuadro managerAnimation;

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

        //generalAnimation = GetComponent<AnimateCharacter>();
        //generalAnimation.turnBack = true;
        //generalAnimation.tolleranceLeft = -3f;
        //generalAnimation.tolleranceRight = 3f;
        //generalAnimation.angleForTurnLeft = generalAnimation.angleForTurnRight = 60f;
        
        managerAnimation = GetComponent<AnimationViewQuadro>();
        managerAnimation.turnBack = true;
        managerAnimation.tolleranceLeft = -1.5f;
        managerAnimation.tolleranceRight = 1.5f;
        managerAnimation.angleForTurnLeft = managerAnimation.angleForTurnRight = 50f;

    }


    private void Update ()
    {
        if ( inPausa )
        {
            GeneratePath();
        }
        else
        {
            Move();
            managerAnimation.Animation_Walk();
        }
    }


    protected void OnCollisionEnter ( Collision collision )
    {
        if( collision.gameObject.CompareTag( "Uscita") && collision.gameObject == destination )
        {
            Debug.Log( "Cico ciao", gameObject);
            Destroy( gameObject );
        }
    }


    protected virtual void OnCollisionStay ( Collision collision )
    {
        if ( collision.gameObject.CompareTag( "Quadro" ) && collision.gameObject == destination )
        {
            managerAnimation.path = path;
            managerAnimation.TurnTowardsPicture( collision );
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
        managerAnimation.Turn();

        FollowPath();
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

        GeneratePath();
    }

    private void GeneratePath ()
    {
        if ( destination.GetComponent<PlaneScript>().HaveAvailablePoint() )
        {
            NavMesh.CalculatePath( transform.position, GetPositionInFloorPicture(), 1, path );
            firstCornerTarget = path.corners[ 1 ] - transform.position;
            managerAnimation.angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, firstCornerTarget );
            managerAnimation.localPos = transform.InverseTransformPoint( path.corners[ 1 ] );
            indexCornerPath = 1;
            //inPausa = false;
        }
        else
        {
            //inPausa = true;
            GenerateNewPath();
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
        
        if ( managerAnimation.CheckTurn() )
        {
            rigidBody.MovePosition( Vector3.MoveTowards( transform.position, path.corners[ indexCornerPath ], Time.deltaTime * rigidBodySpeed ) );
            managerAnimation.speed = 1f;
            GetComponent<Collider>().isTrigger = true;

            float distanceFromCorner = Vector3.Distance( transform.position, path.corners[ indexCornerPath ] );

            if ( distanceFromCorner > 0.1f )
                managerAnimation.RotationToTarget( path.corners[ indexCornerPath ], 10f );

            // Passa al corner successivo
            if ( distanceFromCorner < 0.5f )
            {
                if ( !NextCorner() )
                {
                    GetComponent<Collider>().isTrigger = false;
                    managerAnimation.speed = 0f;
                }
            }
        }
    }

    private bool NextCorner ()
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
            managerAnimation.target = path.corners[ indexCornerPath ];
        }
        else
        {
            managerAnimation.target = Vector3.one;
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
}
