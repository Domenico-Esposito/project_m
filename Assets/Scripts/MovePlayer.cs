using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayer : MonoBehaviour
{
    private Rigidbody playerRidiBody;
    private NavMeshAgent navMeshAgent;

    private AnimateCharacter animator;

    public float movementSpeed = 1f;

    private NavMeshPath path;


    private void Awake()
    {

        playerRidiBody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<AnimateCharacter>();

    }


    private void Update()
    {

        Move();
        animator.Animation();
        animator.speed = navMeshAgent.velocity.magnitude * movementSpeed;

    }


    private void Move()
    {

        Walk();
        Run();

    }


    private void Walk()
    {

        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray rayCameraToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(rayCameraToMouse, out hit, 100, 1 << 10))
            {
                path = new NavMeshPath();
                navMeshAgent.CalculatePath(hit.point, path);
                navMeshAgent.SetPath(path);
            }
        }

    }


    private void Run()
    {

        if (Input.GetKey(KeyCode.E))
        {
            movementSpeed = 2f;
        }
        else
        {
            movementSpeed = 1f;
        }

    }

}
