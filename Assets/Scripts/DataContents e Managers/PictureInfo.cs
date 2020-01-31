using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PictureInfo : MonoBehaviour
{

    public enum Priority
    {
        OPERA_MINORE,
        OPERA_MEDIA,
        OPERA_MAGGIORE
    }

    public int index;

    public int priority;


}
