using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayer : MonoBehaviour
{
    private Rigidbody playerRidiBody;
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private float movementSpeed = 1f;


    private void Awake()
    {
        playerRidiBody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
        Animation();
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
                navMeshAgent.SetDestination(hit.point);
            }
        }

    }

    private void Run()
    {
        if (Input.GetKey(KeyCode.E))
        {
            movementSpeed = 4f;
        }
        else
        {
            movementSpeed = 1f;
        }
    }

    private void Animation()
    {
        Animation_Walk();
    }

    private void Animation_Walk()
    {        
        float agentSpeed = Mathf.Max(
                        Mathf.Abs(navMeshAgent.velocity.x),
                        Mathf.Abs(navMeshAgent.velocity.y)
                    );
        float animatorSpeed = agentSpeed * movementSpeed;
        animator.SetFloat("speed", animatorSpeed);
    }

}
