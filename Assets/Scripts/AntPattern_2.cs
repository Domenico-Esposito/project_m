using System.Collections.Generic;
using UnityEngine;

public class AntPattern_2 : SeguiPercorso
{
    // Pattern movimento
    private IEnumerator<GameObject> pictures;
    private List<GameObject> walls = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> pictureOnWall = new Dictionary<GameObject, List<GameObject>>();

    public GameObject startWall;
    private GameObject currentWall;
    private GameObject currentPicture;
    private int currentPictureIndex = 1;

    public override void InitMovementPattern()
    {
        FindWallsWithPictures();
        FindPicturesOnWalls();

        currentWall = startWall;
        pictures = pictureOnWall[currentWall].GetEnumerator();
        currentPicture = GetNextDestination();
    }


    public override GameObject GetNextDestination()
    {
    
        if (!pictures.MoveNext())
        {
            currentWall = GetMostCloseWallComparedToCurrentWall();
            pictures = pictureOnWall[currentWall].GetEnumerator();
            pictures.MoveNext();

        }

        currentPictureIndex = pictures.Current.GetComponent<PictureInfo>().index;

        Debug.Log("PictureIndex: " + currentPictureIndex, pictures.Current);
        return pictures.Current.transform.GetChild(0).gameObject;
    }


    private GameObject GetMostCloseWallComparedToCurrentWall()
    {

        GameObject mostCloseWall = null;
        bool presentIntersec = false;
        List<GameObject> mostCloseWalls = new List<GameObject>();

        walls.Remove(currentWall);

        foreach (GameObject wall in walls)
        {
            if( wall == currentWall)
                continue;

            Debug.Log("Wall Considerato: ", currentWall);

            if (currentWall.GetComponent<MeshRenderer>().bounds.Intersects(wall.GetComponent<MeshRenderer>().bounds))
            {
                mostCloseWalls.Add(wall);
                presentIntersec = true;
            }
        }


        Debug.Log("mostConsoleWall: " + mostCloseWalls.Count);

        if (presentIntersec == true)
        {
            foreach (GameObject wall in mostCloseWalls)
            {
                foreach(GameObject picture in pictureOnWall[wall])
                {
                    if(picture.GetComponent<PictureInfo>().index > currentPictureIndex)
                    {
                        currentPictureIndex = picture.GetComponent<PictureInfo>().index;
                        Debug.Log("Wall: ", wall);
                        return wall;
                    }
                }
            }
        }


        foreach (GameObject wall in walls)
        {
            if (pictureOnWall.ContainsKey(wall))
            {
                foreach (GameObject picture in pictureOnWall[wall])
                {
                    Debug.Log(picture.GetComponent<PictureInfo>().index);

                    if (picture.GetComponent<PictureInfo>().index == currentPictureIndex + 1)
                    {
                        Debug.Log("Wall: ", wall);
                        return wall;
                    }
                }
            }
        }
        

        return mostCloseWall;
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

    private int SortByPathLength(GameObject x, GameObject y)
    {
        float distance_1 = GetPathLength(x);
        float distance_2 = GetPathLength(y);

        if (distance_1 < distance_2) return -1;
        if (distance_1 > distance_2) return 1;
        return 0;
    }

    private int SortByIndexPicture(GameObject x, GameObject y)
    {
        float distance_1 = x.GetComponent<PictureInfo>().index;
        float distance_2 = y.GetComponent<PictureInfo>().index;

        if (distance_1 < distance_2) return -1;
        if (distance_1 > distance_2) return 1;
        return 0;
    }

}

