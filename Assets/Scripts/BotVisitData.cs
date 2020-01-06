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
    }

}
