using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ReceptionMuseum : MonoBehaviour
{

    public IEnumerator groupColor;
    public Color color;

    public int numero_nonVisitati = 0;
    public int numero_visitati = 0;

    public Dictionary<string, int> v = new Dictionary<string, int>();
    public Dictionary<string, int> nV = new Dictionary<string, int>();

    public int utenti = 0;
    public int utentiInsoddisfatti = 0;
    public int utentiSoddisfatti = 0;

    public Text like;
    public Text dislike;
    public Text bots;

    private void Awake ()
    {
        Color[ ] colors = { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.grey, Color.yellow };
        groupColor = colors.GetEnumerator();   

    }

    public Color GetColor ()
    {
        if( groupColor.MoveNext() )
        {
            return (Color) groupColor.Current;
        }
        else
        {
            groupColor.Reset();
        }

        return GetColor();
    }

    public void AddUser ()
    {
        utenti++;
    }

    public void ReceivData (string patternType, List<GameObject> visitati, List<GameObject> non_visitati, List<GameObject> ignorati, float tempoVisita, float tempoDiAttesa, float distanza)
    {
        // -1 è l'uscita
        numero_visitati += visitati.Count - 1;
        numero_nonVisitati += non_visitati.Count;

        bool soddisfatto;
        int nonVisitati = ignorati.Count + non_visitati.Count;

        if ( nonVisitati > (visitati.Count-1)/3 || tempoDiAttesa >= 30f )
        {
            soddisfatto = false;
            utentiInsoddisfatti++;
        }
        else
        {
            soddisfatto = true;
            utentiSoddisfatti++;
        }

        foreach (GameObject o in visitati )
        {
            if( v.ContainsKey(o.name) )
            {
                v[ o.name ] = v[ o.name ] + 1;
            }
            else
            {
                v.Add( o.name, 1 );
            }
        }


        foreach ( GameObject o in non_visitati )
        {
            if ( nV.ContainsKey( o.name ) )
            {
                nV[ o.name ] = nV[ o.name ] + 1;
            }
            else
            {
                nV.Add( o.name, 1 );
            }
        }

        string path = "Assets/dati_visite.txt";

        string resoconto = patternType + " | "  + ( soddisfatto ? "soddisfatto" : "insoddisfatto" ) + " | Visitati: " + visitati.Count + " | Non visitati: " + non_visitati.Count + " | Rinunce: " + ignorati.Count + " | Tempo: " + tempoVisita + " | Attesa: " + tempoDiAttesa + " | Distanza: " + distanza; 
        StreamWriter writer = new StreamWriter( path, true );
        writer.WriteLine( resoconto );
        writer.Close();

        Debug.Log( "Dati visita salvati" );

    }

    private void Update ()
    {
        like.text = utentiSoddisfatti.ToString();
        dislike.text = utentiInsoddisfatti.ToString();
        bots.text = utenti.ToString();
    }
}
