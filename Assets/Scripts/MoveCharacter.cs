using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveCharacter : MonoBehaviour
{

    private Rigidbody playerRidiBody;

    private CharacterAnimator generalAnimation;
    private AnimationViewPicture quadroViewAnimation;

    private NavMeshPath path;


    private readonly float tolleranceDestination = 0.5f;


    private Vector3 firstCornerTarget;
    private int indexCornerPath = 1;


    private void Awake()
    {

        playerRidiBody = GetComponent<Rigidbody>();

        generalAnimation = GetComponent<CharacterAnimator>();
        generalAnimation.turnBack = true;
        generalAnimation.tolleranceLeft = -3f;
        generalAnimation.tolleranceRight = 3f;
        generalAnimation.angleForTurnLeft = generalAnimation.angleForTurnRight = 60f;

        quadroViewAnimation = GetComponent<AnimationViewPicture>();
        quadroViewAnimation.tolleranceLeft = -1.5f;
        quadroViewAnimation.tolleranceRight = 1.5f;
        quadroViewAnimation.angleForTurnLeft = quadroViewAnimation.angleForTurnRight = 50f;
    }


    private void Update()
    {

        Move();
        generalAnimation.Animation_Walk();

    }


    private void OnCollisionStay(Collision collision)
    {

        quadroViewAnimation.path = path;
        quadroViewAnimation.TurnTowardsPicture(collision);

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

        generalAnimation.Turn();

        if (CheckTurn())
        {
            FollowPath();
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
            //Debug.Log("Percorso - Lunghezza: " + path.corners.Length);

            firstCornerTarget = path.corners[1] - transform.position;
            generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle(transform.forward, firstCornerTarget);

            generalAnimation.localPos = transform.InverseTransformPoint(path.corners[1]);
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
        bool isRotation = generalAnimation.IsRotation();

        if (!isRotation)
            playerRidiBody.isKinematic = false;

        if (!isRotation && path != null)
        {
            generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle(transform.forward, path.corners[indexCornerPath] - transform.position);
            generalAnimation.localPos = transform.InverseTransformPoint(path.corners[indexCornerPath]);

            if (generalAnimation.angleBetweenPlayerAndTarget > generalAnimation.angleForTurnLeft)
            {
                return false;
            }

            return true;
        }

        generalAnimation.angleBetweenPlayerAndTarget = 0;
        generalAnimation.localPos = Vector3.zero;

        return false;
    }


    private void FollowPath()
    {
        //Debug.Log("Movimento - indexCornerPath: " + indexCornerPath + " | Corners: " + path.corners.Length);

        playerRidiBody.MovePosition(Vector3.MoveTowards(transform.position, path.corners[indexCornerPath], Time.deltaTime * 5f));
        generalAnimation.speed = 1f;

        float distanceFromCorner = Vector3.Distance(transform.position, path.corners[indexCornerPath]);

        if (distanceFromCorner > 0.1f)
            RotationToTarget(path.corners[indexCornerPath], 10f);

        // Passa al corner successivo
        if (distanceFromCorner < 0.5f)
        {
            //Debug.Log("Passa punto successivo.");
            indexCornerPath++;

            if (indexCornerPath > path.corners.Length - 1)
            {
                //Debug.Log("Movimento - Destinazione raggiunta");
                generalAnimation.speed = 0f;
                indexCornerPath = 1;
                path = null;
            }
        }
    }


}