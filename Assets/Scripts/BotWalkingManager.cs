using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BotWalkingManager : MonoBehaviour
{
    // Pattern movimento
    private Rigidbody rigidBody;

    // Animazione
    private AnimateCharacter generalAnimation;
    private AnimationViewQuadro quadroViewAnimation;

    // Segui percorso
    private NavMeshPath path;
    private Vector3 firstCornerTarget;
    private int indexCornerPath = 1;
    private float timedelta = 0f;

    public float pauseTime = 5f;
    public float rigidBodySpeed = 7f;


    private List<GameObject> poissonFloors;
    private GameObject currentPoissonFloor;

    private IEnumerator<GameObject> pathPart;

    private void Awake ()
    {
        poissonFloors = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Fish Floor" ) );
        poissonFloors.Sort( SortByIndexPlace );

        if( Random.Range(0f, 1f) > 0.5f )
        {
            Debug.Log( "Reverse" );
            poissonFloors.Reverse();
        }

        pathPart = poissonFloors.GetEnumerator();

        InitAnimationBheavior();
        GenerateNewPath();
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

        Move();
        generalAnimation.Animation();
    }

    private void OnCollisionStay ( Collision collision )
    {
        quadroViewAnimation.path = path;
        quadroViewAnimation.TurnTowardsPicture( collision );

        if ( collision.gameObject.CompareTag( "Fish Floor" ) )
        {
            timedelta += Time.deltaTime;
        }
    }

    private void OnCollisionExit ( Collision collision )
    {
        if ( collision.gameObject.CompareTag( "Fish Floor" ) && path != null )
        {
            timedelta = 0f;
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

    public float GetPathLength ( GameObject picture )
    {
        NavMeshPath p = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, picture.transform.GetChild( 0 ).transform.position, 1, p );

        float lng = 0;

        for ( int i = 0; i < p.corners.Length - 1; i++ )
        {
            lng += Vector3.Distance( p.corners[ i ], p.corners[ i + 1 ] );
        }

        Debug.Log( "Lunghezza path: " + lng + " | Status: " + p.status, picture );

        return lng;
    }

    private void GenerateNewPath ()
    {
        path = new NavMeshPath();
        
        NavMesh.CalculatePath( transform.position, RandomCoordinatesInFloorPicture(), 1, path );

        firstCornerTarget = path.corners[ 1 ] - transform.position;
        generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, firstCornerTarget );
        generalAnimation.localPos = transform.InverseTransformPoint( path.corners[ 1 ] );
        indexCornerPath = 1;

    }


    public GameObject GetNextDestination ()
    {

        if( pathPart.MoveNext() )
        {
            return pathPart.Current;
        }

        return poissonFloors[ 0 ];
    }


    public Vector3 RandomCoordinatesInFloorPicture ()
    {
        GameObject next = GetNextDestination();

        Collider floorPicture = next.GetComponent<Collider>();

        Vector3 floorPictureSize = floorPicture.bounds.size;
        float randomXInFloorPicture = Random.Range( -floorPictureSize.x / 2.5f, floorPictureSize.x / 2.5f );
        float randomYInFloorPicture = Random.Range( -floorPictureSize.y / 2.5f, floorPictureSize.y / 2.5f );

        Vector3 randomPositionInPlane = next.transform.position + new Vector3( randomXInFloorPicture, 0f, randomYInFloorPicture );

        return randomPositionInPlane;

    }


    private void DrawPath ()
    {
        if ( path != null )
        {
            for ( int i = 0; i < path.corners.Length - 1; i++ )
                Debug.DrawLine( path.corners[ i ], path.corners[ i + 1 ], Color.red );
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
                generalAnimation.speed = 0f;
                indexCornerPath = 1;
                path = null;
            }
        }
    }


    private int SortByIndexPlace ( GameObject x, GameObject y )
    {

        float index_1 = x.GetComponent<PictureInfo>().index;
        float index_2 = y.GetComponent<PictureInfo>().index;

        if ( index_1 < index_2 ) return -1;
        if ( index_1 > index_2 ) return 1;
        return 0;

    }

}

