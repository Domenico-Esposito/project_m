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
    private PathManager pathManager;

    void Start ()
    {
        visitData = GetComponent<BotVisitData>();
        pathManager = GetComponent<PathManager>();
    }

    public virtual int GetFatigueLevel ()
    {
        if ( visitData.distanzaPercorsa > pathManager.maxDistanza )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return (int) Level.MOLTO_STANCO;
        }

        if ( visitData.distanzaPercorsa > ( pathManager.maxDistanza / 1.2f ) )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return ( int ) Level.STANCO;
        }

        Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return ( int ) Level.NON_STANCO;
    }

}
