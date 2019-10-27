using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animation : MonoBehaviour
{


    Rigidbody m_Rigidbody;
    Animator m_Animator;
    const float k_Half = 0.5f;

    Vector3 m_GroundNormal;
    Vector3 m_CapsuleCenter;

    private NavMeshAgent navMeshAgent;

    private GameObject[ ] planes;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        planes = GameObject.FindGameObjectsWithTag( "PicturePlane" );
    }


    // Update is called once per frame
    void Update()
    {

        if ( Input.GetKeyDown( "n" ) )
        {
            navMeshAgent.SetDestination( planes[ Random.Range(0, planes.Length) ].transform.position );
        }

        if ( navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance )
        {
            ControlloDirezione( navMeshAgent.steeringTarget );
        }
        else
        {
            UpdateForward( 0f, 0.1f );
            UpdateTurn( 0f, 0.1f );
        }
    }

    void UpdateTurn (float turnValue, float turnTime )
    {
        m_Animator.SetFloat( "Turn", turnValue, turnTime, Time.deltaTime );
    }

    void UpdateForward (float forwardValue, float forwardTime )
    {
        m_Animator.SetFloat( "Forward", forwardValue, forwardTime, Time.deltaTime );

    }

    /*   
     *    localPos.x < 0, localPos.y > 0 | localPos.x > 0, localPos.y > 0
     *    ---------------------------------------------------------------
     *    localPos.x < 0, localPos.y < 0 | localPos.x > 0, localPos.y < 0
     */

    void ControlloDirezione (Vector3 destination)
    {

        float angoloPlayerTarget = Vector3.Angle( transform.forward, destination );
        Vector3 localPos = transform.InverseTransformPoint( destination );

        if ( localPos.x < 0 && localPos.z < 0 && angoloPlayerTarget > 100 )
        {
            UpdateForward( 0f, 0.5f );
            UpdateTurn( -1f, 0.1f );

        }
        else if ( localPos.x > 0 && localPos.z < 0 && angoloPlayerTarget > 100 )
        {
            UpdateForward( 0f, 0.5f );
            UpdateTurn( 1f, 0.1f );

        }
        else
        {
            localPos.Normalize();

            UpdateForward( 0.5f, 0.1f );
            UpdateTurn( Mathf.Atan2( localPos.x, localPos.z ), 0.1f );
        }


    }

}
