using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    public bool isAvailable = true;

    public void Occupa()
    {
        isAvailable = false;
    }

    public void Libera ()
    {
        isAvailable = true;
    }

}
