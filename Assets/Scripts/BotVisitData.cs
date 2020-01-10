using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotVisitData : MonoBehaviour
{

    public List<GameObject> visitedPictures = new List<GameObject>();
    public List<GameObject> importantPictures = new List<GameObject>();
    public List<GameObject> importantIgnoratePicture = new List<GameObject>();

    public void ClearData ()
    {
        visitedPictures.Clear();
        importantPictures.Clear();
        importantIgnoratePicture.Clear();

        distanzaPercorsa = 0f;
        lastPositionPattern = null;
        destinationPrePause = null;

        tempoInAttesa = 0f;
        durataVisita = 0f;
        currentPictureIndex = 0;

        destination = null;
        destinationPoint = null;

    }

    public float distanzaPercorsa = 0f;

    public GameObject lastPositionPattern;
    public GameObject destinationPrePause;

    public float tempoInAttesa = 0f;
    public float durataVisita = 0f;

    public int currentPictureIndex = 0;

    public GameObject destination;
    public GameObject destinationPoint;

}
