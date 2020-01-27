using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatigueManager : MonoBehaviour
{
    public const int NON_STANCO = 0;
    public const int STANCO = 1;
    public const int MOLTO_STANCO = 2;

    private BotVisitData visitData;
    private PathManager pathManager;

    void Start ()
    {
        visitData = GetComponent<BotVisitData>();
        pathManager = GetComponent<PathManager>();
    }

    public virtual int LivelloStanchezza ()
    {

        if ( visitData.distanzaPercorsa > pathManager.maxDistanza )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return MOLTO_STANCO;
        }

        if ( visitData.distanzaPercorsa > ( pathManager.maxDistanza / 1.2f ) )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return STANCO;
        }

        Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return NON_STANCO;
    }

}
