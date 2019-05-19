﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayer : MonoBehaviour
{
    private Rigidbody playerRidiBody;
    private AnimateCharacter animator;

    private NavMeshPath path;


    private readonly float tolleranceLeft = -2f;
    private readonly float tolleranceRight = 2f;

    private readonly float angleForTurnLeft = 60f;
    private readonly float tolleranceDestination = 0.5f;
    private readonly float angleForTurnRight = 60f;
    private readonly float angleForTurnBack = 150f;


    private Vector3 localPos;
    private Vector3 firstCornerTarget;
    private float angleBetweenPlayerAndTarget;
    private int indexCornerPath = 1;


    private void Awake()
    {

        playerRidiBody = GetComponent<Rigidbody>();
        animator = GetComponent<AnimateCharacter>();
    }


    private void Update()
    {

        Move();
        animator.Animation();

    }


    private void OnCollisionStay(Collision collision)
    {

        TurnTowardsPicture(collision);

    }


    private void Move()
    {

        Walk();

    }


    private void Walk()
    {

        if (Input.GetMouseButton(0))
        {
            GeneratePathMousePosition();
        }

        DrawPath();

        if (CheckTurn())
        {
            FollowPath();
        }

    }


    private void TurnTowardsPicture(Collision collision)
    {

        if(path == null && collision.gameObject.CompareTag("Quadro"))
        {

            RectTransform picture = collision.gameObject.GetComponentInParent<RectTransform>();
            RotationToTarget(picture.transform.position, 5f);
            
        }

    }


    private void GeneratePathMousePosition()
    {
        RaycastHit hit;
        Ray rayCameraToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayCameraToMouse, out hit, 100, 1 << 10))
        {
            path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, hit.point, 1, path);
            Debug.Log("Percorso - Lunghezza: " + path.corners.Length);

            firstCornerTarget = path.corners[1] - transform.position;
            angleBetweenPlayerAndTarget = Vector3.Angle(transform.forward, firstCornerTarget);

            localPos = transform.InverseTransformPoint(path.corners[1]);
            indexCornerPath = 1;
        }
    }


    private void DrawPath()
    {
        if (path != null)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }
    }


    private void RotationToTarget(Vector3 target, float speed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
        targetRotation.x = 0f;
        targetRotation.z = 0f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }


    private bool CheckTurn()
    {
        Turn();

        bool inRotation = (animator.animator.GetCurrentAnimatorStateInfo(0).IsName("TurnL") || animator.animator.GetCurrentAnimatorStateInfo(0).IsName("TurnR") || animator.animator.GetCurrentAnimatorStateInfo(0).IsName("TurnTL") || animator.animator.GetCurrentAnimatorStateInfo(0).IsName("TurnTR"));

        if (!inRotation && path != null)
        {

            angleBetweenPlayerAndTarget = Vector3.Angle(transform.forward, firstCornerTarget);
            localPos = transform.InverseTransformPoint(path.corners[indexCornerPath]);

            if (angleBetweenPlayerAndTarget > angleForTurnLeft)
            {
                return false;
            }

            return true;
        }

        angleBetweenPlayerAndTarget = 0;
        localPos = Vector3.zero;

        return false;
    }


    private void FollowPath()
    {
        Debug.Log("Movimento - indexCornerPath: " + indexCornerPath + " | Corners: " + path.corners.Length);

        playerRidiBody.MovePosition(Vector3.MoveTowards(transform.position, path.corners[indexCornerPath], Time.deltaTime * 5f));
        animator.speed = 1f;

        float distanceFromCorner = Vector3.Distance(transform.position, path.corners[indexCornerPath]);

        if (distanceFromCorner > 0.1f)
            RotationToTarget(path.corners[indexCornerPath], 10f);

        // Passa al corner successivo
        if (distanceFromCorner < 0.5f)
        {
            indexCornerPath++;

            if (indexCornerPath > path.corners.Length - 1)
            {
                Debug.Log("Movimento - Destinazione raggiunta");
                animator.speed = 0f;
                indexCornerPath = 1;
                path = null;
            }
        }
    }


    private void Turn()
    {
        if (localPos == Vector3.zero)
            return;

        animator.speed = 0f;

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
            animator.animator.Play("TurnL", 0);
    }


    private void TurnRight()
    {
        if (angleBetweenPlayerAndTarget > angleForTurnRight)
            animator.animator.Play("TurnR", 0);
    }


    private void TurnBack()
    {
        if (angleBetweenPlayerAndTarget > angleForTurnBack)
        {
            if (localPos.x < 0.0f)
                animator.animator.Play("TurnTL", 0);
            else
                animator.animator.Play("TurnTR", 0);
        }
    }




}
