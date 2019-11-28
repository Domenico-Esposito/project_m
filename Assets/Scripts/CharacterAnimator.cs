using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] float m_MovingTurnSpeed = 360;
    [SerializeField] float m_StationaryTurnSpeed = 180;

    Rigidbody m_Rigidbody;
    Animator m_Animator;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    CapsuleCollider m_Capsule;


    void Start ()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ |  RigidbodyConstraints.FreezePositionY;
    }


    public void Move ( Vector3 move )
    {

        if ( move.magnitude > 1f ) move.Normalize();
        move = transform.InverseTransformDirection( move );
        move = Vector3.ProjectOnPlane( move, m_GroundNormal );
        m_TurnAmount = Mathf.Atan2( move.x, move.z );
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        UpdateAnimator( move );
    }


    void UpdateAnimator ( Vector3 move )
    {
        m_Animator.SetFloat( "Forward", m_ForwardAmount, 0.1f, Time.deltaTime );
        m_Animator.SetFloat( "Turn", m_TurnAmount, 0.1f, Time.deltaTime );

        m_Animator.speed = 1;

    }

    public void TurnToPicture ( Vector3 move )
    {

        Quaternion targetRotation = Quaternion.LookRotation( move - transform.position );
        targetRotation.x = 0f;
        targetRotation.z = 0f;
        float angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, ( move - transform.position ) );

        m_Animator.SetFloat( "Forward", 0, 1f, Time.deltaTime );

        if ( angleBetweenPlayerAndTarget > 40 )
        {
            //move = move.normalized;
            //move = transform.InverseTransformDirection( move );
            //move = Vector3.ProjectOnPlane( move, m_GroundNormal );
            //m_TurnAmount = Mathf.Atan2( move.x, move.z );
            //m_Animator.SetFloat( "Turn", m_TurnAmount, 0.2f, Time.deltaTime );

            m_Animator.SetFloat( "Turn", 0.4f );
            transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, Time.deltaTime * 2f );

        }
        else
        {
            m_Animator.SetFloat( "Turn", 0f, 0.1f, Time.deltaTime );
            transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, Time.deltaTime * 1.2f );
        }


    }

    void ApplyExtraTurnRotation ()
    {
        float turnSpeed = Mathf.Lerp( m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount );
        transform.Rotate( 0, m_TurnAmount * turnSpeed * Time.deltaTime, 0 );
    }


}