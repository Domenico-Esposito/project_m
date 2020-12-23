using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatigueManager : MonoBehaviour
{
    public enum Level
    {
        NON_STANCO,
        STANCO,
        MOLTO_STANCO
    };

    private BotVisitData visitData;

    void Start ()
    {
        visitData = GetComponent<BotVisitData>();
    }

    public virtual Level GetFatigueLevel ()
    {
        if ( visitData.totalDistance > visitData.maxDistanza )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return Level.MOLTO_STANCO;
        }

        if ( visitData.totalDistance > ( visitData.maxDistanza / 1.2f ) )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return Level.STANCO;
        }

        Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return Level.NON_STANCO;
    }

}
