using System.Collections.Generic;
using UnityEngine;

public class AntPattern_2 : SeguiPercorso
{
    // Pattern movimento
    private IEnumerator<GameObject> pictures;
    private List<GameObject> walls = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> pictureOnWall = new Dictionary<GameObject, List<GameObject>>();
    private GameObject currentWall;
 

    public override void InitMovementPattern()
    {
        FindWallsWithPictures();
        FindPicturesOnWalls();

        currentWall = GetNextPicture().transform.parent.gameObject;
        
        pictureOnWall[currentWall].Sort(Distanza);
        pictures = pictureOnWall[currentWall].GetEnumerator();

    }

    private void FindWallsWithPictures()
    {
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            if (wall.transform.childCount > 0)
            {
                walls.Add(wall);
                pictureOnWall.Add(wall, new List<GameObject>());
            }
        }
    }

    private void FindPicturesOnWalls()
    {
        foreach (GameObject wall in walls)
        {
            foreach (Transform picture in wall.transform)
            {
                if (picture.gameObject.transform.GetChild(0).CompareTag("Quadro"))
                    pictureOnWall[wall].Add(picture.gameObject);
            }

        }
    }

    private List<GameObject> GetWallsVisibleForBot()
    {
        int wallsLayer = 9;

        Vector3[] directions = new Vector3[]{
            -transform.right,
            transform.right,
            transform.forward,
            -transform.forward
/*             new Vector3(Mathf.Sin(22.5f * Mathf.Deg2Rad), 0, Mathf.Cos(22.5f * Mathf.Deg2Rad)),
            - new Vector3(Mathf.Sin(22.5f * Mathf.Deg2Rad), 0, Mathf.Cos(22.5f * Mathf.Deg2Rad)),
            new Vector3(Mathf.Sin(45 * Mathf.Deg2Rad), 0, Mathf.Cos(45 * Mathf.Deg2Rad)),
            - new Vector3(Mathf.Sin(45 * Mathf.Deg2Rad), 0, Mathf.Cos(45 * Mathf.Deg2Rad)),
            new Vector3(Mathf.Sin(67.5f * Mathf.Deg2Rad), 0, Mathf.Cos(67.5f * Mathf.Deg2Rad)),
            -new Vector3(Mathf.Sin(67.5f * Mathf.Deg2Rad), 0, Mathf.Cos(67.5f * Mathf.Deg2Rad)),
            new Vector3(Mathf.Sin(112.5f * Mathf.Deg2Rad), 0, Mathf.Cos(112.5f * Mathf.Deg2Rad)),
            -new Vector3(Mathf.Sin(112.5f * Mathf.Deg2Rad), 0, Mathf.Cos(112.5f * Mathf.Deg2Rad)),
            new Vector3(Mathf.Sin(135 * Mathf.Deg2Rad), 0, Mathf.Cos(135 * Mathf.Deg2Rad)),
            - new Vector3(Mathf.Sin(135 * Mathf.Deg2Rad), 0, Mathf.Cos(135 * Mathf.Deg2Rad)),
            new Vector3(Mathf.Sin(157.5f * Mathf.Deg2Rad), 0, Mathf.Cos(157.5f * Mathf.Deg2Rad)),
            -new Vector3(Mathf.Sin(157.5f * Mathf.Deg2Rad), 0, Mathf.Cos(157.5f * Mathf.Deg2Rad)) */
        };

        List<GameObject> wallsCollision = new List<GameObject>();

        foreach(Vector3 direction in directions)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, 25f);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if(hit.collider.gameObject.layer == wallsLayer){
                        Debug.Log("Hit: " + hit.collider.name, hit.collider.gameObject);
                        if(pictureOnWall.ContainsKey(hit.collider.gameObject)){
                            if(pictureOnWall[hit.collider.gameObject].Count > 0){
                                wallsCollision.Add(hit.collider.gameObject);
                            }
                        }
                    }
                }
                    
            }
        }

        return wallsCollision;
    }

    private GameObject GetNextPicture()
    {
        int nextPictureIndex = 0;
        GameObject nextPicture = null;

        List<GameObject> t = GetWallsVisibleForBot();

        foreach(GameObject wall in t)
        {
            if (wall == null)
                continue;

            foreach(GameObject picture in pictureOnWall[wall])
            {
                if (picture.GetComponent<PictureInfo>().index < nextPictureIndex || nextPictureIndex == 0)
                {
                    nextPictureIndex = picture.GetComponent<PictureInfo>().index;
                    nextPicture = picture;
                }
            }
        }
        
        return nextPicture;
    }

    private GameObject GetClosestWall()
    {
        float minDistance = Mathf.Infinity;
        GameObject closestWall = null;

        foreach (GameObject wall in walls)
        {
            if (Vector3.Distance(wall.transform.position, transform.position) <= minDistance)
            {
                if(pictureOnWall[wall].Count > 0)
                {
                    closestWall = wall;
                    minDistance = Vector3.Distance(wall.transform.position, transform.position);
                }
            }
        }

        return closestWall;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Uscita"))
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Quadro"))
        {
            // Il quadro non va visitato, non c'è nessun muro di interesse
            if (currentWall == null)
            {
                return;
            }

            if (pictureOnWall[currentWall].Contains(pictures.Current))
            {
                pictureOnWall[currentWall].Remove(pictures.Current);

                int currentPictureIndex = pictures.Current.GetComponent<PictureInfo>().index;

                foreach(GameObject wall in pictureOnWall.Keys)
                {
                    pictureOnWall[wall].RemoveAll(
                        delegate (GameObject x){

                            if(x.gameObject.GetComponent<PictureInfo>().index < currentPictureIndex)
                                return true;

                            return false;
                        });
                }

            }

            pictures = pictureOnWall[currentWall].GetEnumerator();

        }

    }

    public override GameObject GetNextDestination()
    {
        // Se ho visitato tutti i quadri sull'attuale muro
        if (!pictures.MoveNext())
        {
            walls.Remove(currentWall);
            currentWall = GetClosestWall();

            // Se sono finiti i muri da visitare
            if (currentWall == null || pictureOnWall[currentWall].Count == 0)
            {
                return GameObject.FindGameObjectWithTag("Uscita").gameObject;
            }

            pictureOnWall[currentWall].Sort(Distanza);

            pictures = pictureOnWall[currentWall].GetEnumerator();
            pictures.MoveNext();

        }

        return pictures.Current.transform.GetChild(0).gameObject;
    }

    private int Distanza(GameObject x, GameObject y)
    {
        float distance_1 = GetPathLength(x);
        float distance_2 = GetPathLength(y);

        if (distance_1 < distance_2) return -1;
        if (distance_1 > distance_2) return 1;
        return 0;
    }

    private int SortIndexPictures(GameObject x, GameObject y)
    {
        float distance_1 = x.GetComponent<PictureInfo>().index;
        float distance_2 = y.GetComponent<PictureInfo>().index;

        if (distance_1 < distance_2) return -1;
        if (distance_1 > distance_2) return 1;
        return 0;
    }

}

