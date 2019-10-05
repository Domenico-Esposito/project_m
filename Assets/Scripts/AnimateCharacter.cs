using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimateCharacter : MonoBehaviour
{

    private Animator animator;
    private Rigidbody playerRidiBody;
    public NavMeshPath path;
    public Vector3 target;

    [HideInInspector]
    public Vector3 localPos;

    [HideInInspector]
    public float speed;

    [HideInInspector]
    public float angleBetweenPlayerAndTarget;

    [HideInInspector]
    public float tolleranceLeft = -2f, tolleranceRight = 2f;

    [HideInInspector]
    public float angleForTurnLeft = 60f, angleForTurnRight = 60f, angleForTurnBack = 150f;

    private readonly float tolleranceDestination = 0.5f;

    [HideInInspector]
    public bool turnBack;

    private void Awake()
    {

        animator = GetComponent<Animator>();
        playerRidiBody = GetComponent<Rigidbody>();

    }


    public void Animation_Walk()
    {

        animator.SetFloat("speed", speed);

    }


    public void Animation_TurnLeft()
    {
        animator.Play("TurnL", 0);
    }


    public void Animation_TurnRight()
    {
        animator.Play("TurnR", 0);
    }


    public void Animation_TurnBackLeft()
    {
        animator.Play("TurnTL", 0);
    }

    public void Animation_TurnBackRight()
    {
        animator.Play("TurnTR", 0);
    }


    public void Turn()
    {
        if (localPos == Vector3.zero)
            return;
            
        speed = 0f;
        playerRidiBody.isKinematic = true;

        if (localPos.x < tolleranceLeft)
        {
            TurnLeft();
        }
        else if (localPos.x > tolleranceRight)
        {
            TurnRight();
        }
        else
        {
            TurnBack();
        }


    }

    private void TurnLeft()
    {
        if (angleBetweenPlayerAndTarget > angleForTurnLeft)
            Animation_TurnLeft();
    }


    private void TurnRight()
    {
        if (angleBetweenPlayerAndTarget > angleForTurnRight)
            Animation_TurnRight();
    }


    private void TurnBack()

    {
        if (angleBetweenPlayerAndTarget > angleForTurnBack)
        {
            if (localPos.x < 0.0f)
                Animation_TurnBackLeft();
            else
                Animation_TurnBackRight();
        }
    }


    public bool IsRotation()
    {
        return (animator.GetCurrentAnimatorStateInfo(0).IsName("TurnL") || animator.GetCurrentAnimatorStateInfo(0).IsName("TurnR") || animator.GetCurrentAnimatorStateInfo(0).IsName("TurnTL") || animator.GetCurrentAnimatorStateInfo(0).IsName("TurnTR"));
    }


    public void RotationToTarget ( Vector3 target, float speed )
    {
        Quaternion targetRotation = Quaternion.LookRotation( target - transform.position );
        targetRotation.x = 0f;
        targetRotation.z = 0f;
        transform.rotation = Quaternion.Slerp( transform.rotation, targetRotation, Time.deltaTime * speed );
    }


    public bool CheckTurn ()
    {
        bool isRotation = IsRotation();

        if ( !isRotation )
        {
            playerRidiBody.isKinematic = false;
        }

        if ( !isRotation && target != Vector3.one )
        {
            angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, ( target - transform.position ) );
            localPos = transform.InverseTransformPoint( target );

            if ( angleBetweenPlayerAndTarget > angleForTurnLeft )
            {
                return false;
            }

            return true;
        }
        
        angleBetweenPlayerAndTarget = 0;
        localPos = Vector3.zero;

        return false;
    }
}
