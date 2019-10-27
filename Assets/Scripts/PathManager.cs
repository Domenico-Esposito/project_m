using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    private Rigidbody rigidBody;
    public GameObject destination;
    public GameObject destinationPoint;
    private NavMeshAgent navMeshAgent;
    public bool inPausa = false;
    public bool isHasty = false;

    Rigidbody m_Rigidbody;
    Animator m_Animator;

    protected Color colorDrawPath = Color.red;
    // Animazione

    // Segui percorso
    protected NavMeshPath path;
    private Vector3 firstCornerTarget;
    public float timedelta = 0f;

    public float pauseTime = 5f;

    private float baseTime;

    public float rigidBodySpeed = 7f;

    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();
    public bool controllo = false;

    protected virtual void Start ()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.avoidancePriority = Random.Range( 0, 100);
        isHasty = Random.Range( 0, 5 ) > 2;

        baseTime = pauseTime;
        InitAnimationBheavior();
        InitMovementPattern();
        UpdateDestination();
    }

    public void InitAnimationBheavior ()
    {
        rigidBody = GetComponent<Rigidbody>();
    }


    private void Update ()
    {
        if( inPausa )
        {
            CheckNextDestination();
        }
        else
        {

            if ( timedelta > pauseTime)
            {
                UpdateDestination();
                timedelta = 0f;
            }
        
        }

        TimerDestinazione();
        Walk();

        TimerExit();

    }

    private void UpdateDestination ()
    {
        destination = GetNextDestination();
        CheckNextDestination();
    }

    protected virtual GameObject GetPointInDestination ()
    {
        return destination.GetComponent<GridSystem>().GetAvailablePoint();
    }

    private void CheckNextDestination ()
    {
        if( destination.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            if ( destinationPoint != null )
            {
                destinationPoint.GetComponent<DestinationPoint>().Libera();
                Debug.Log( "Libero il posto: ", destinationPoint );
            }

            inPausa = false;

            destinationPoint = GetPointInDestination();
            destinationPoint.GetComponent<DestinationPoint>().Occupa();
            navMeshAgent.SetDestination( destinationPoint.transform.position );

        }
        else
        {
            // Qui euristica di scelta prossima destinazione
            if( isHasty )
            {
                inPausa = true;
            }
            else
            {
                UpdateDestination();
            }
        }
    }


    void UpdateTurn ( float turnValue, float turnTime )
    {
        m_Animator.SetFloat( "Turn", turnValue, turnTime, Time.deltaTime );
    }

    void UpdateForward ( float forwardValue, float forwardTime )
    {
        m_Animator.SetFloat( "Forward", forwardValue, forwardTime, Time.deltaTime );

    }

    /*   
     *    localPos.x < 0, localPos.y > 0 | localPos.x > 0, localPos.y > 0
     *    ---------------------------------------------------------------
     *    localPos.x < 0, localPos.y < 0 | localPos.x > 0, localPos.y < 0
     */
    void ControlloDirezione ( Vector3 destination )
    {

        float angoloPlayerTarget = Vector3.Angle( transform.forward, destination );
        Vector3 localPos = transform.InverseTransformPoint( destination );

        localPos.Normalize();
        UpdateForward(0.5f, 0.1f );
        UpdateTurn( Mathf.Atan2( localPos.x, localPos.z ), 0.2f );

        //float turnSpeed = Mathf.Lerp(180, 360, localPos.z );
        //transform.Rotate( 0, Mathf.Atan2( localPos.x, localPos.z ) * turnSpeed * Time.deltaTime, 0 );

    }

    private void TimerDestinazione ()
    {
        if ( destinationPoint == null )
            return;

        if( navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance || Vector3.Distance(transform.position, destinationPoint.transform.position) <= 1f)
        {
            if ( destinationPoint.gameObject.GetComponentInParent<RectTransform>() )
            {
                Vector3 localPos = transform.InverseTransformPoint( destinationPoint.gameObject.GetComponentInParent<RectTransform>().transform.position );
                localPos.Normalize();
                UpdateTurn( Mathf.Atan2( localPos.x, localPos.z ), 0.2f );
                //float turnSpeed = Mathf.Lerp( 180, 360, localPos.z );
                //transform.Rotate( 0, Mathf.Atan2( localPos.x, localPos.z ) * turnSpeed * Time.deltaTime, 0 );
                UpdateForward( 0f, 0.5f );
            }

            timedelta += Time.deltaTime;
        }
    }

    protected virtual void OnCollisionStay ( Collision collision )
    {
        //if ( collision.gameObject.CompareTag( "PicturePlane" ) && collision.gameObject == destination )
        //{
        //    Debug.Log( "Colpito." );
        //    if( navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        //    {
        //        Vector3 localPos = transform.InverseTransformPoint( collision.gameObject.GetComponentInParent<RectTransform>().transform.position );
        //        UpdateTurn( Mathf.Atan2( localPos.x, localPos.z ), 0.1f );
        //        UpdateForward( 0f, 1f );
        //        timedelta += Time.deltaTime;
        //    }
        //}
    }

    protected void TimerExit ( )
    {
        if( destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, destinationPoint.transform.position) < 3f)
        {
            Debug.Log( "ciao" );
            Destroy( gameObject );
        }
    }



    private void Walk ()
    {
        if( destinationPoint == null )
        {
            UpdateForward( 0f, 0.1f );
            UpdateTurn( 0f, 0.1f );
            return;
        }

        if ( navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance || Vector3.Distance( transform.position, destinationPoint.transform.position ) > navMeshAgent.stoppingDistance )
        {   
            ControlloDirezione( navMeshAgent.steeringTarget );
        }
        else
        {
            UpdateForward( 0f, 0.1f );
            UpdateTurn( 0f, 0.1f );
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
