using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class Filler : MonoBehaviour
{

    public float pauseTime = 3f;
    public float pauseTimeGruppoSingolo = 3f;
    public float pauseTimeGruppoMisto = 3f;

    public int numeroGruppiSingoli = 0;
    public int numeroGruppiMisti = 0;

    private float deltaTime = 0f;
    private float deltaTimeGruppoMisto = 0f;
    private float deltaTimeGruppoSingolo = 0f;

    public int numberOfFourmi = 1;
    public int numberOfPapillon = 1;
    public int numberOfPoisson = 1;
    public int numberOfSauterelle = 1;
    public int index = 1;

    public bool gruppoMisto = false;
    public bool gruppoSingolo = false;

    public GameObject fourmiBot;
    public GameObject papillonBot;
    public GameObject poissonBot;
    public GameObject sauterelleBot;

    //private void Awake ()
    //{
    //    if( gruppoMisto )
    //    {
    //        fillGroup();
    //    }
    //    else if( gruppoSingolo )
    //    {
    //        fillGroupSingolo();
    //    }
    //    else
    //    {
    //        fill();
    //    }
    //}

    //private void Update ()
    //{

    //if( deltaTime > pauseTime )
    //{
    //    if ( gruppoMisto )
    //    {
    //        fillGroup();
    //    }
    //    else if( gruppoSingolo )
    //    {
    //        fillGroupSingolo();
    //    }
    //    else
    //    {
    //        fill();
    //    }

    //    deltaTime = 0;
    //}

    //deltaTime += Time.deltaTime;
    //}

    private void Awake ()
    {
        ReadData();
    }

    private void fillGroupSingolo (int patternType = -1)
    {
        List<PathManager> group = new List<PathManager>();

        if( patternType <= -1)
            patternType = Random.Range( 0, 3 );

        for ( int i = 0; i < Random.Range( 3, 10 ); i++ )
        {
            GameObject o;
            o = AddNewBot( patternType );
            group.Add( o.GetComponent<PathManager>() );
        }

        PathManager capo = group[ 0 ];
        group.Remove( capo );

        capo.GetComponent<PathManager>().setGroup( group );
    }

    private void fillGroup ()
    {

        List<PathManager> group = new List<PathManager>();

        for ( int i = 0; i < Random.Range( 3, 10); i++)
        {
            GameObject o;

            if( Random.Range(0, 2) >= 1)
            {
                o = AddNewBot( 0 );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                o = AddNewBot( 1 );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                o = AddNewBot( 2 );
            }
            else
            {
                o = AddNewBot( 3 );
            }

            group.Add( o.GetComponent<PathManager>() );
        }

        PathManager capo = group[ 0 ];
        group.Remove( capo );

        capo.GetComponent<PathManager>().setGroup( group );

    }

    //private void fill()
    //{
    //    int[ ] numbers = { numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle };


    //    for ( int i = 0; i < Mathf.Max( numbers ) ; i++)
    //    {
    //        if( i < numberOfFourmi )
    //        {
    //            AddNewBot( 0 );
    //        }

    //        if( i < numberOfPapillon )
    //        {
    //            AddNewBot( 1 );
    //        }

    //        if ( i < numberOfPoisson)
    //        {
    //            AddNewBot( 2 );
    //        }

    //        if ( i < numberOfSauterelle )
    //        {
    //            AddNewBot( 3 );
    //        }
    //    }
    //}


    IEnumerator addBot ()
    {

        Debug.Log( "Aggiunto.." );

        if(  numberOfFourmi > 0)
        {
            numberOfFourmi--;
            AddNewBot( 0 );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfPapillon > 0 )
        {
            numberOfPapillon--;
            AddNewBot( 1 );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfPoisson > 0)
        {
            numberOfPoisson--;
            AddNewBot( 2 );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfSauterelle > 0)
        {
            numberOfSauterelle--;
            AddNewBot( 3 );
            yield return new WaitForSeconds( pauseTime );
        }

        if( Mathf.Max( numberOfFourmi , numberOfPoisson, numberOfPapillon, numberOfSauterelle) > 0 )
        {
            running = true;
            yield return addBot();
        }
        else
        {
            running = false;
        }

    }

    bool running = false;

    List<Dictionary<string, int>> configurationsFill = new List<Dictionary<string, int>>();
    int confNumber = 1;

    private void Update ()
    {

        if( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle, numeroGruppiMisti, numeroGruppiSingoli ) == 0)
        {

            if( configurationsFill.Count > 0 )
            {

                string path = "Assets/dati_visite.txt";
                StreamWriter writer = new StreamWriter( path, true );
                writer.WriteLine( "# Configurazione " + confNumber++);
                writer.Close();

                Dictionary<string, int> configurazione = configurationsFill[ 0 ];
                configurationsFill.RemoveAt( 0 );

                foreach ( KeyValuePair<string, int> data in configurazione)
                {
                    switch ( data.Key )
                    {
                        case "pauseTime":
                            pauseTime = data.Value;
                            break;
                        case "numberOfFourmi":
                            numberOfFourmi = data.Value;
                            break;
                        case "numberOfPapillon":
                            numberOfPapillon = data.Value;
                            break;
                        case "numberOfPoisson":
                            numberOfPoisson = data.Value;
                            break;
                        case "numberOfSauterelle":
                            numberOfSauterelle = data.Value;
                            break;
                        case "pauseTimeGruppoSingolo":
                            pauseTimeGruppoSingolo = data.Value;
                            break;
                        case "numeroGruppiSingoli":
                            numeroGruppiSingoli = data.Value;
                            if ( pauseTimeGruppoSingolo > 0 )
                                gruppoSingolo = true;
                            break;
                        case "pauseTimeGruppoMisto":
                            pauseTimeGruppoMisto = data.Value;
                            break;
                        case "numeroGruppiMisti":
                            numeroGruppiMisti = data.Value;
                            if ( numeroGruppiMisti > 0 )
                                gruppoMisto = true;
                            break;
                    }
                }
            }

        }

        if ( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle ) > 0 && running == false )
        {
            running = true;
            StartCoroutine( addBot() );
        }

        if ( deltaTimeGruppoMisto > pauseTimeGruppoMisto )
        {
            if ( gruppoMisto && numeroGruppiMisti > 0 )
            {
                numeroGruppiMisti--;
                fillGroup();
            }

            deltaTimeGruppoMisto = 0f;
        }


        if ( deltaTimeGruppoSingolo > pauseTimeGruppoSingolo )
        {
            if ( gruppoSingolo && numeroGruppiSingoli > 0 )
            {
                numeroGruppiSingoli--;
                fillGroupSingolo();
            }

            deltaTimeGruppoSingolo = 0f;
        }


        deltaTimeGruppoMisto += Time.deltaTime;
        deltaTimeGruppoSingolo += Time.deltaTime;

        deltaTime += Time.deltaTime;
    }

    public void ReadData ()
    {
        string path = "Assets/dati_caricamento.txt";
        StreamReader reader = new StreamReader( path );
        string content = reader.ReadToEnd();

        Regex regex_conf = new Regex( "#CONF\\n(.*\\n?)*?#END" );
        MatchCollection configurations = regex_conf.Matches( content );

        Debug.Log( configurations[ 0 ].Value );

        foreach( Match configuration in configurations )
        {

            Dictionary<string, int> data = new Dictionary<string, int>();

            Regex regex_data = new Regex( @"\w+=\d+" );
            MatchCollection confs_data = regex_data.Matches( configuration.Value );

            foreach ( Match conf_data in confs_data )
            {
                string[ ] item = conf_data.Value.Split( '=' );
                Debug.Log( item[ 0 ] + " -- " + item[ 1 ] );
                data.Add(item[0], System.Int32.Parse(item[1]));
            }

            configurationsFill.Add( data );
        }

        Debug.Log( "Configurazioni salvate" );

        reader.Close();
    }

    private GameObject AddNewBot (int type)
    {

        GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().AddUser();

        switch ( type )
        {
        case 0:
            return AddNewFourmi();
        case 1:
            return AddNewPapillon();
        case 2:
            return AddNewPoisson();
        case 3:
            return AddNewSauterelle();
        }

        return null;
    }


    private GameObject AddNewFourmi ()
    {
        GameObject o = Instantiate( fourmiBot, transform, true );
        o.transform.position = new Vector3(Random.Range(-3.43f, -11.76f), 0, Random.Range(-14, -18.74f));
        o.name = "Agente " + index++;
        return o;
    }

    private GameObject AddNewPapillon ()
    {
        GameObject o = Instantiate( papillonBot, transform, true );
        o.transform.position = new Vector3( Random.Range( -3.43f, -11.76f ), 0, Random.Range( -14, -18.74f ) );
        o.name = "Agente " + index++;

        return o;

    }

    private GameObject AddNewPoisson ()
    {
        GameObject o = Instantiate( poissonBot, transform, true );
        o.transform.position = new Vector3( Random.Range( -3.43f, -11.76f ), 0, Random.Range( -14, -18.74f ) );
        o.name = "Agente " + index++;
        return o;
    }

    private GameObject AddNewSauterelle ()
    {
        GameObject o = Instantiate( sauterelleBot, transform, true );
        o.transform.position = new Vector3( Random.Range( -3.43f, -11.76f ), 0, Random.Range( -14, -18.74f ) );
        o.name = "Agente " + index++;

        return o;
    }
}
