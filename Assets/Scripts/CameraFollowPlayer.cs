using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{

    public GameObject player;
    Vector3 offset;
    public int limitRayCastWalls;
    private Stack<WallsManager> hiddenWalls = new Stack<WallsManager>();

    void Awake()
    {
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        FollowPlayer();
        HideWallsBetweenCameraAndTarget();
    }

    void FollowPlayer()
    {
        Vector3 cameraPosition = player.transform.position + offset;
        transform.position = cameraPosition;
    }

    void HideWallsBetweenCameraAndTarget()
    {
        RaycastHit hit;
        WallsManager wall;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, limitRayCastWalls, 1 << 9))
        {
            wall = hit.collider.GetComponent<WallsManager>();
            if (wall != null)
            {
                wall.Hide();
                if (!hiddenWalls.Contains(wall))
                {
                    hiddenWalls.Push(wall);
                }
            }
        }
        else
        {
            if (hiddenWalls.Count > 0)
            {
                wall = hiddenWalls.Pop();
                wall.Show();
            }
        }

    }

}
