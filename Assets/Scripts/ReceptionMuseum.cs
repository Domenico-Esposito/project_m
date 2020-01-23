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

    public int utenti = 0;
    public int utentiInsoddisfatti = 0;
    public int utentiSoddisfatti = 0;

    public Text like;
    public Text dislike;
    public Text bots;

    private void Awake ()
    {
        string path = "Assets/dati_visite.txt";
        System.IO.File.WriteAllText( path, string.Empty );
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

    public void AddUser (int remove = 0)
    {
        List<PathManager> agenti = new List<PathManager>( FindObjectsOfType<PathManager>() );
        int utentiAttivi = agenti.FindAll( ( PathManager obj ) => obj.gameObject.activeInHierarchy ).Count + 1 - remove;
        bots.text = utentiAttivi.ToString();
    }

    public void ReceivData (string patternType, BotVisitData visitData)
    {
        AddUser(2);

        List<PictureInfo> visitati = visitData.visitedPictures;
        List<PictureInfo> non_visitati = visitData.importantPictures;
        List<PictureInfo> ignorati = visitData.importantIgnoratePicture;
        float tempoVisita = (float) visitData.durataVisita;
        float tempoDiAttesa = (float) visitData.tempoInAttesa;
        float distanza = (float) visitData.distanzaPercorsa;

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

        string path = "Assets/dati_visite.txt";

        //string resoconto = patternType + " | "  + ( soddisfatto ? "soddisfatto" : "insoddisfatto" ) + " | Visitati: " + visitati.Count + " | Non visitati: " + (non_visitati.Count + ignorati.Count) + " | Tempo: " + tempoVisita + " | Attesa: " + tempoDiAttesa + " | Distanza: " + distanza; 
        string resoconto = visitData.JSON(patternType, soddisfatto) + ", ";
        StreamWriter writer = new StreamWriter( path, true );
        writer.WriteLine( resoconto );
        writer.Close();

        Debug.Log( "Dati visita salvati" );

        //if( (utentiInsoddisfatti + utentiSoddisfatti) == utenti ){
        //    Debug.Log("Simulazione terminata");
        //    FindObjectOfType<RVOSimulator>().stopSimulation = true;
        //}

        like.text = utentiSoddisfatti.ToString();
        dislike.text = utentiInsoddisfatti.ToString();
    }

    public void TerminaSimulazione ()
    {
        string path = "Assets/dati_visite.txt";

        StreamReader reader = new StreamReader( path, true );
        string contenuto = reader.ReadToEnd();
        reader.Close();

        System.IO.File.WriteAllText( path, "[" + contenuto.Substring( 0, contenuto.Length - 3 ) + "]" );


    }

    private void Update ()
    {

    }
}
