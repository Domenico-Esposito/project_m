using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class ReceptionMuseum : MonoBehaviour
{

    public IEnumerator groupColor;

    private int numero_nonVisitati;
    private int numero_visitati;

    private int utenti;
    private int utentiInsoddisfatti;
    private int utentiSoddisfatti;

    public Text like;
    public Text dislike;
    public Text agents;

    private string visitDataFile {
        get => Application.persistentDataPath  + "/visitData.json";
    }

    private void Awake ()
    {
        System.IO.File.WriteAllText( visitDataFile, string.Empty );
        Color[ ] colors = { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.grey, Color.yellow };
        groupColor = colors.GetEnumerator();   
    }

    public Color GetColor ()
    {
        if( groupColor.MoveNext() )
        {
            return (Color) groupColor.Current;
        }

        groupColor.Reset();

        return GetColor();
    }

    public void UpdateAgentsCounter (bool remove = false)
    {
        List<PathManager> agenti = new List<PathManager>( FindObjectsOfType<PathManager>() );
        int utentiAttivi = agenti.FindAll( ( PathManager obj ) => obj.gameObject.activeInHierarchy ).Count + 1;

        if ( remove )
            utentiAttivi -= 2;

        agents.text = utentiAttivi.ToString();
    }

    private void UpdateLikeCounters ()
    {
        like.text = utentiSoddisfatti.ToString();
        dislike.text = utentiInsoddisfatti.ToString();
    }

    public void ReceivData (string patternType, BotVisitData visitData)
    {
        UpdateAgentsCounter( true );

        bool satisfaction = GetAgentSatisfaction( visitData );

        string resoconto = visitData.JSON(patternType, satisfaction ) + ", ";
        WriteVisitData( resoconto );
        UpdateLikeCounters();
    }

    private bool GetAgentSatisfaction (BotVisitData visitData)
    {
        bool satisfaction;

        List<PictureInfo> visitati = visitData.visitedPictures;
        List<PictureInfo> non_visitati = visitData.importantPictures;
        List<PictureInfo> ignorati = visitData.importantIgnoratePicture;
        float tempoVisita = visitData.durataVisita;
        float tempoDiAttesa = visitData.tempoInAttesa;
        float distanza = visitData.distanzaPercorsa;

        // -1 rappresenta l'uscita
        numero_visitati += visitati.Count - 1;
        numero_nonVisitati += non_visitati.Count;

        int nonVisitati = ignorati.Count + non_visitati.Count;

        if ( nonVisitati > visitati.Count / 3 || tempoDiAttesa >= 30f )
        {
            satisfaction = false;
            utentiInsoddisfatti++;
        }
        else
        {
            satisfaction = true;
            utentiSoddisfatti++;
        }

        return satisfaction;
    }

    private void WriteVisitData (string data)
    {
        StreamWriter writer = new StreamWriter( visitDataFile, true );
        writer.WriteLine( data );
        writer.Close();

        Debug.Log( "Dati visita salvati" );
    }

    public void TerminaSimulazione ()
    {
        StreamReader reader = new StreamReader( visitDataFile, true );
        string contenuto = reader.ReadToEnd();
        reader.Close();

        System.IO.File.WriteAllText( visitDataFile, "[" + contenuto.Substring( 0, contenuto.Length - 3 ) + "]" );
    }

}
