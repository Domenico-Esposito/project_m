using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveAutomaticBotAntV2 : MonoBehaviour
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

    private List<GameObject> walls = new List<GameObject>();

    private Dictionary<GameObject, List<GameObject>> map = new Dictionary<GameObject, List<GameObject>>();
    private GameObject currentWall;

    private void Awake()
    {
      

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


        RecuperoMuri();
        RecuperaCoppieMuroQuadro();


        currentWall = QuadroConIndexPiuPiccolo().transform.parent.gameObject;
        Debug.Log("CurrentWall", currentWall);
        map[currentWall].Sort(Distanza);

        picturesEnumerator = map[currentWall].GetEnumerator();

        GenerateNewPath();
    }


    private void RecuperoMuri()
    {
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {

            if (wall.transform.childCount > 0)
            {
                walls.Add(wall);
                map.Add(wall, new List<GameObject>());
            }

        }
    }


    private void RecuperaCoppieMuroQuadro()
    {
        foreach (GameObject wall in walls)
        {

            foreach (Transform picture in wall.transform)
            {
                if (picture.gameObject.transform.GetChild(0).CompareTag("Quadro"))
                {
                    map[wall].Add(picture.gameObject);
                }
            }

        }
    }

    //private GameObject MuroPiuVicinoFirst() {
    

    //    List<Vector3> directions = new List<Vector3>();
    //    directions.Add(-transform.right);
    //    directions.Add(transform.right);
    //    directions.Add(transform.forward);
    //    directions.Add(-transform.forward);

    //    float min = Mathf.Infinity;
    //    GameObject piuVicino = null;

    //    foreach(Vector3 direction in directions)
    //    {
    //        RaycastHit hit;
    //        if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), direction, out hit, Mathf.Infinity))
    //        {
    //            float distanza = hit.distance;
    //            Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
    //            Debug.Log("Collider: " + distanza, hit.collider.gameObject);
    //            if (distanza < min)
    //            {
    //                piuVicino = hit.collider.gameObject;
    //                min = distanza;
    //            }
    //        }
    //    }

    //    Debug.Log("Oggetto più vicino", piuVicino);
    //    return piuVicino;
    //}


    private GameObject[] MuriPiuVicinoFirst()
    {

        Vector3[] directions = new Vector3[4];
        directions[0] = -transform.right;
        directions[1] = transform.right;
        directions[2] = transform.forward;
        directions[3] = -transform.forward;

        GameObject[] walls_vicini = new GameObject[4];

        int i = 0;
        foreach(Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), direction, out hit, 20))
            {
                float distanza = hit.distance;
                Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
                Debug.Log("Collider: " + distanza, hit.collider.gameObject);
                if(hit.collider.gameObject.layer == 9)
                    walls_vicini[i++] = hit.collider.gameObject;
            }
        }

        return walls_vicini;
    }


    private GameObject QuadroConIndexPiuPiccolo()
    {

        int t = 999;
        GameObject p = null;

        foreach(GameObject wall in MuriPiuVicinoFirst())
        {
            if (wall == null)
                continue;

            Debug.Log("Controllo muro. Ha " + map[wall].Count + " quadri", wall);

            foreach(GameObject picture in map[wall])
            {
                Debug.Log("Controllo", picture);
                if (picture.GetComponent<PictureInfo>().index < t)
                {
                    Debug.Log("Nuovo piu piccolo", picture);
                    t = picture.GetComponent<PictureInfo>().index;
                    p = picture;
                }
            }
        }

        Debug.Log("Quadro vicino piu piccolo", p);
        Debug.Log("Muro del quadro piu piccolo", p.transform.parent);
        return p;
    }


    private GameObject MuroPiuVicino()
    {

        float minDistance = 1000f;
        GameObject muroVicino = null;

        foreach (GameObject wall in walls)
        {
            if (Vector3.Distance(wall.transform.position, transform.position) <= minDistance && map[wall].Count > 0)
            {
                muroVicino = wall;
                minDistance = Vector3.Distance(wall.transform.position, transform.position);
            }
        }

        Debug.Log("Scelgo questo muro...", muroVicino);
        return muroVicino;
    }


    private void Update()
    {
        Move();
        generalAnimation.Animation();
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Uscita"))
        {
            Debug.Log("Abbandono museo");
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Quadro"))
        {

            if (currentWall == null)
            {
                return;
            }

            if (map[currentWall].Contains(picturesEnumerator.Current))
            {
                map[currentWall].Remove(picturesEnumerator.Current);

                int currentIndex = picturesEnumerator.Current.GetComponent<PictureInfo>().index;

                foreach(GameObject wall in map.Keys)
                {
                    map[wall].RemoveAll(delegate (GameObject x)
                    {
                        if(x.gameObject.GetComponent<PictureInfo>().index < currentIndex)
                        {
                            return true;
                        }

                        return false;
                    });
                }

            }

            picturesEnumerator = map[currentWall].GetEnumerator();

        }

    }

    private void OnCollisionStay(Collision collision)
    {

        quadroViewAnimation.path = path;
        quadroViewAnimation.TurnTowardsPicture(collision);
        
        if (collision.gameObject.CompareTag("Quadro"))
        {
            timedelta += Time.deltaTime;
        }

    }


    private GameObject EsibizionePiuVicina()
    {

        float minDistance = 1000f;
        GameObject esibizioneVicina = null;

        foreach (GameObject picture in map[currentWall])
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
        {
            timedelta = 0f;
        }
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


    private GameObject GetNext()
    {

        if (!picturesEnumerator.MoveNext())
        {
            Debug.Log("Finiti i quadri su questo muro", currentWall);
            walls.Remove(currentWall);

            currentWall = MuroPiuVicino();
            Debug.Log("Nuovo muro selezionato", currentWall);

            if (currentWall == null || map[currentWall].Count == 0)
            {
                Debug.Log("Mi dirigo verso l'uscita");
                return GameObject.FindGameObjectWithTag("Uscita").gameObject;
            }

            map[currentWall].Sort(Distanza);

            picturesEnumerator = map[currentWall].GetEnumerator();
            picturesEnumerator.MoveNext();

        }

        Debug.Log("Prossimo quadro", picturesEnumerator.Current);

        return picturesEnumerator.Current.transform.GetChild(0).gameObject;
    }


    private float GetPathLength(GameObject picture)
    {
        NavMeshPath p = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, picture.transform.GetChild(0).transform.position, 1, p);


        float lng = 0;

        for (int i = 0; i < p.corners.Length - 1; i++)
        {
            lng += Vector3.Distance(p.corners[i], p.corners[i + 1]);
        }

        Debug.Log("Lunghezza path: " + lng + " | Status: " + p.status, picture);

        return lng;
    }


    private void GenerateNewPath()
    {
        path = new NavMeshPath();

        NavMesh.CalculatePath(transform.position, RandomCoordinatesInFloorPicture(), 1, path);

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


    private int Distanza(GameObject x, GameObject y)
    {
        float distance_1 = GetPathLength(x);
        float distance_2 = GetPathLength(y);

        Debug.Log("Distanza 1: " + distance_1, x);
        Debug.Log("Distanza 2: " + distance_2, y);

        if (distance_1 < distance_2) return -1;
        if (distance_1 > distance_2) return 1;
        return 0;
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

