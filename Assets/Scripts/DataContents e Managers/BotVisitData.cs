using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BotVisitData : MonoBehaviour
{
    [NonSerialized]
    public List<PictureInfo> visitedPictures = new List<PictureInfo>();

    [NonSerialized]
    public List<PictureInfo> importantPictures = new List<PictureInfo>();

    [NonSerialized]
    public List<PictureInfo> importantIgnoratePicture = new List<PictureInfo>();

    [NonSerialized]
    public GameObject lastPositionPattern;

    [NonSerialized]
    public GameObject destinationPrePause;

    public GameObject destination;

    [NonSerialized]
    public GameObject destinationPoint;

    public int maxDistanza;
    public float totalDistance;
    public float waitingTime;
    public float visitDuration;
    public bool inPausa;
    public int chanceSkipDestination;

    [ NonSerialized]
    public int currentPictureIndex;

    public int configIndex;

    public void ClearData ()
    {
        visitedPictures.Clear();
        importantPictures.Clear();
        importantIgnoratePicture.Clear();

        totalDistance = 0f;
        lastPositionPattern = null;
        inPausa = false;
        destinationPrePause = null;

        waitingTime = 0f;
        visitDuration = 0f;
        currentPictureIndex = 0;
        configIndex = 0;

        destination = null;
        destinationPoint = null;

    }


    public string JSON (string patternType, bool soddisfatto)
    {
        string dati = ConvertToJson( "visited", visitedPictures );
        dati += ConvertToJson( "nonVisited", importantPictures );
        dati += ConvertToJson( "ignored", importantIgnoratePicture );
        dati += "\"patternType\": \"" + patternType + "\",";
        dati += "\"satisfied\": \"" + soddisfatto + "\",";

        return JsonUtility.ToJson( this, true ).Insert( 1, dati );
    }


    private string ConvertToJson(string key, IList list )
    {
        string dati = " \"" + key + "\": [ ";


        for ( int i = 0; i < list.Count; i++ )
        {
            dati += JsonUtility.ToJson( list[ i ] );

            if ( i < list.Count - 2 )
            {
                dati += ", ";
            }
        }

        dati += "],";

        return dati;
    }
}
