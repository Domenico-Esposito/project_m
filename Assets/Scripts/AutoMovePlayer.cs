using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AutoMovePlayer : MonoBehaviour
{

    private GameObject[] pictures;
    private NavMeshAgent navMeshAgent;

    private AnimateCharacter animator;

    float paintingTimeWait = 0f;
    int indexPicture = 0;


    private void Awake()
    {

        navMeshAgent = GetComponent<NavMeshAgent>();
        pictures = GameObject.FindGameObjectsWithTag("Quadro");
        animator = GetComponent<AnimateCharacter>();

    }


    private void FixedUpdate()
    {

        animator.Animation();
        animator.speed = navMeshAgent.velocity.magnitude;

        GoToFloorPicture();
        RotateBodyTowardsPicture();
    }


    private void GoToFloorPicture()
    {

        if (paintingTimeWait >= 10f && navMeshAgent.remainingDistance < 2f)
        {

            paintingTimeWait = 0f;

            Vector3 pictureFloorDestination = RandomCoordinatesInFloorPicture();
            navMeshAgent.SetDestination(pictureFloorDestination);

            indexPicture = indexPicture + 1;

            if (indexPicture >= pictures.Length)
                indexPicture = 0;

        }

    }


    private void RotateBodyTowardsPicture()
    {

        if (navMeshAgent.remainingDistance < 1f)
        {
            paintingTimeWait += Time.deltaTime;

            int prevIndexPicture = indexPicture - 1;
            if (prevIndexPicture < 1 || prevIndexPicture >= pictures.Length)
                prevIndexPicture = 0;

            RectTransform picture = pictures[prevIndexPicture].GetComponentInParent<RectTransform>();

            Quaternion targetRotation = Quaternion.LookRotation(picture.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);

        }

    }


    private Vector3 RandomCoordinatesInFloorPicture()
    {

        Collider floorPicture = pictures[indexPicture].GetComponent<Collider>();

        Vector3 floorPictureSize = floorPicture.bounds.size;
        float randomXInFloorPicture = Random.Range(-floorPictureSize.x / 2, floorPictureSize.x / 2);
        float randomYInFloorPicture = Random.Range(-floorPictureSize.y / 2, floorPictureSize.y / 2);

        Vector3 randomPositionInPlane = pictures[indexPicture].transform.position + new Vector3(randomXInFloorPicture, 0f, randomYInFloorPicture);

        return randomPositionInPlane;

    }


}
