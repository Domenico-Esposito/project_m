using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PictureInfo : MonoBehaviour
{

    public int index;

    /*
        0 = Neutro
        1 = Importante
    */
    public int priority;

    /*
     * True = non considero, non cerco di visitarlo
     * False = altrimenti
     */
    public bool ignoro;

}
