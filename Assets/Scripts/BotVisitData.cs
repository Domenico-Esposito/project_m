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
    [NonSerialized]
    public GameObject destination;
    [NonSerialized]
    public GameObject destinationPoint;

    public float distanzaPercorsa;
    public float tempoInAttesa;
    public float durataVisita;

    [NonSerialized]
    public int currentPictureIndex;

    public int configurazioneDiIngresso;

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
        configurazioneDiIngresso = 0;

        destination = null;
        destinationPoint = null;

    }


    public string JSON (string patternType, bool soddisfatto)
    {
        string dati = ConvertToJson( "visitati", visitedPictures );
        dati += ConvertToJson( "nonVisitate", importantPictures );
        dati += ConvertToJson( "ignorate", importantIgnoratePicture );
        dati += "\"patternType\": \"" + patternType + "\",";
        dati += "\"soddisfatto\": \"" + soddisfatto + "\",";

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
