using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void ReceivData (List<GameObject> visitati, List<GameObject> non_visitati, float tempoDiAttesa)
    {
        numero_visitati += visitati.Count;
        numero_nonVisitati += non_visitati.Count;

        if( non_visitati.Count >= (visitati.Count + non_visitati.Count ) / 2  || tempoDiAttesa >= 120f)
        {
            utentiInsoddisfatti++;
        }
        else
        {
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

    }

    private void Update ()
    {
        like.text = utentiSoddisfatti.ToString();
        dislike.text = utentiInsoddisfatti.ToString();
        bots.text = utenti.ToString();
    }
}
