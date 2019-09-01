using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MoveAutomaticBotAnt : MonoBehaviour
{
    private Rigidbody playerRidiBody;
    private List<GameObject> pictures;
    private IEnumerator<GameObject> picturesEnumerator;

    private List<GameObject> triggered = new List<GameObject>();

    private AnimateCharacter generalAnimation;
    private AnimationViewQuadro quadroViewAnimation;

    private NavMeshPath path;

    private readonly float tolleranceDestination = 0.5f;


    private Vector3 firstCornerTarget;
    private int indexCornerPath = 1;

    private float timedelta = 0f;
    private int indexPicture = 0;


    private void Awake()
    {
        pictures = new List<GameObject>(GameObject.FindGameObjectsWithTag("Quadro"));
        picturesEnumerator = pictures.GetEnumerator();

        playerRidiBody = GetComponent<Rigidbody>();

        generalAnimation = GetComponent<AnimateCharacter>();
        generalAnimation.turnBack = true;
        generalAnimation.tolleranceLeft = -3f;
        generalAnimation.tolleranceRight = 3f;
        generalAnimation.angleForTurnLeft = generalAnimation.angleForTurnRight = 60f;

        quadroViewAnimation = GetComponent<AnimationViewQuadro>();
        quadroViewAnimation.tolleranceLeft = -1.5f;
        quadroViewAnimation.tolleranceRight = 1.5f;
        quadroViewAnimation.angleForTurnLeft = quadroViewAnimation.angleForTurnRight = 50f;

        GenerateNewPath();

    }


    private void Update()
    {

        Move();
        generalAnimation.Animation();

    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Salto") && triggered.IndexOf(collision.gameObject) == -1)
        {
            triggered.Add(collision.gameObject);
            path = null;
            timedelta = 10f;
            GameObject esibizione = EsibizionePiuVicinaEn();
            int index = pictures.IndexOf(esibizione);
            pictures.RemoveRange(0, index);
            Debug.Log("Lunghezza: " + pictures.Count);

            picturesEnumerator = pictures.GetEnumerator();

            Debug.Log("Index: " + index);
            Debug.Log("OnCollisionEnter", esibizione);
        }

    }

    private void OnCollisionStay(Collision collision)
    {

        quadroViewAnimation.path = path;
        quadroViewAnimation.TurnTowardsPicture(collision);

        //Debug.Log("OnCollisionStay: " + timedelta);

        if (collision.gameObject.CompareTag("Quadro"))
            timedelta += Time.deltaTime;

    }


    private GameObject EsibizionePiuVicinaEn()
    {

        float minDistance = 1000f;
        GameObject esibizioneVicina = null;
        int indexCurrent = pictures.IndexOf(picturesEnumerator.Current);

        Debug.Log("IndexCurrent: " + indexCurrent + " | pictures.Count: " + pictures.Count);

        foreach (GameObject picture in pictures.GetRange(indexCurrent, pictures.Count-1))
        {
            if (Vector3.Distance(picture.GetComponentInParent<RectTransform>().transform.position, transform.position) <= minDistance)
            {
                esibizioneVicina = picture;
                minDistance = Vector3.Distance(picture.GetComponentInParent<RectTransform>().transform.position, transform.position);
            }
        }


        Debug.Log("Quadro più vicino", esibizioneVicina);
        return esibizioneVicina;
    }

    private GameObject EsibizionePiuVicina()
    {

        float minDistance = 1000f;
        GameObject esibizioneVicina = null;

        foreach (GameObject picture in pictures)
        {
            if (Vector3.Distance(picture.GetComponentInParent<RectTransform>().transform.position, transform.position) <= minDistance)
            {
                esibizioneVicina = picture;
                minDistance = Vector3.Distance(picture.GetComponentInParent<RectTransform>().transform.position, transform.position);
            }
        }


        Debug.Log("Quadro più vicino", esibizioneVicina);
        return esibizioneVicina;
    }

    private void OnCollisionExit(Collision collision)
    {
    

        //Debug.Log("OnCollisionExit");
        if (collision.gameObject.CompareTag("Quadro") && path != null)
            timedelta = 0f;
    }

    private void Move()
    {

        Walk();

    }

    private void Walk()
    {

        if (timedelta > 5f)
        {
            GenerateNewPath();

            timedelta = 0f;
            
        }

        DrawPath();

        generalAnimation.Turn();

        if (CheckTurn())
        {
            FollowPath();
        }

    }


    private void GenerateNewPath()
    {
        path = new NavMeshPath();

        NavMesh.CalculatePath(transform.position, RandomCoordinatesInFloorPicture(), 1, path);

        //Debug.Log("Percorso - Lunghezza: " + path.corners.Length);

        firstCornerTarget = path.corners[1] - transform.position;
        generalAnimation.angleBetweenPlayerAndTarget = Vector3.Angle(transform.forward, firstCornerTarget);
        generalAnimation.localPos = transform.InverseTransformPoint(path.corners[1]);
        indexCornerPath = 1;

    }


    private Vector3 RandomCoordinatesInFloorPicture()
    {
        GameObject next = GetNext();

        Collider floorPicture = next.GetComponent<Collider>();

        Vector3 floorPictureSize = floorPicture.bounds.size;
        float randomXInFloorPicture = Random.Range(-floorPictureSize.x / 2.5f, floorPictureSize.x / 2.5f);
        float randomYInFloorPicture = Random.Range(-floorPictureSize.y / 2.5f, floorPictureSize.y / 2.5f);

        Vector3 randomPositionInPlane = next.transform.position + new Vector3(randomXInFloorPicture, 0f, randomYInFloorPicture);

        return randomPositionInPlane;

    }


    private GameObject GetNext()
    {

        if (! picturesEnumerator.MoveNext())
        {
            picturesEnumerator.Reset();
        }


        return picturesEnumerator.Current;
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
