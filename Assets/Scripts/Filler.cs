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

    public int numberMax = 3;

    public bool gruppoMisto = false;
    public bool gruppoSingolo = false;

    public int probabilitaLiberta = 0;
    public bool leaderDespota = false;


    public GameObject fourmiBot;
    public GameObject papillonBot;
    public GameObject poissonBot;
    public GameObject sauterelleBot;

    public GridSystem spawnPoints;
    public IEnumerator spawnPoint;
    RVOSimulator simulator = null;

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

    private void Start ()
    {
        spawnPoint = spawnPoints.GetEnumerator();
        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
    }

    private void fillGroupSingolo (int patternType = -1)
    {
        List<PathManager> group = new List<PathManager>();

        if( patternType <= -1)
            patternType = Random.Range( 0, 3 );

        int numberOfMember = Random.Range( 3, 8 );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            GameObject o;
            o = AddNewBot( patternType );
            if ( leaderDespota )
            {
                o.GetComponent<PathManager>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) && i < numberOfMember - 1)
                {
                    //o.GetComponent<PathManager>().noChoices = true;
                    Destroy( o.GetComponent<PathManager>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesBot>();
                }
            }

            if( o.GetComponent<NoChoicesBot>() != null )
            {
                group.Add( o.GetComponent<NoChoicesBot>() );
            }
            else
            {
                group.Add( o.GetComponent<PathManager>() );

            }
        }

        PathManager capo = group[ group.Count - 1];
        group.Remove( capo );

        if ( leaderDespota )
        {
            capo.GetComponent<PathManager>().despota = true;
            capo.GetComponent<PathManager>().noChoices = false;
            capo.GetComponent<PathManager>().activeBot = true;

        }
        capo.GetComponent<PathManager>().SetGroup( group );
    }

    private void fillGroup ()
    {

        List<PathManager> group = new List<PathManager>();
        int numberOfMember = Random.Range( 3, 8 );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            GameObject o;

            if ( Random.Range(0, 2) >= 1)
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

            if ( leaderDespota )
            {
                o.GetComponent<PathManager>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) && i < numberOfMember - 1 )
                {
                    //o.GetComponent<PathManager>().noChoices = true;
                    Destroy( o.GetComponent<PathManager>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesBot>();
                }
            }

            if ( o.GetComponent<NoChoicesBot>() != null )
            {
                group.Add( o.GetComponent<NoChoicesBot>() );
            }
            else
            {
                group.Add( o.GetComponent<PathManager>() );

            }
        }

        PathManager capo = group[ group.Count - 1];
        group.Remove( capo );

        if( leaderDespota )
        {
            capo.GetComponent<PathManager>().despota = true;
            capo.GetComponent<PathManager>().noChoices = false;
            capo.GetComponent<PathManager>().activeBot = true;
        }

        capo.GetComponent<PathManager>().SetGroup( group );


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
                        case "probabilitaLiberta":
                            probabilitaLiberta = data.Value;
                            break;
                        case "leaderDespota":
                            if(data.Value == 1){
                                leaderDespota = true;
                            }
                            else
                            {
                                leaderDespota = false;
                            }
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
        
        foreach( Match configuration in configurations )
        {

            Dictionary<string, int> data = new Dictionary<string, int>();

            Regex regex_data = new Regex( @"\w+=\d+" );
            MatchCollection confs_data = regex_data.Matches( configuration.Value );

            foreach ( Match conf_data in confs_data )
            {
                string[ ] item = conf_data.Value.Split( '=' );
                data.Add(item[0], System.Int32.Parse(item[1]));
            }

            configurationsFill.Add( data );
        }

        Debug.Log( "Configurazioni recuperate con successo" );

        reader.Close();
    }

    private bool CheckType(GameObject agent, int type )
    {
    
        switch ( type )
        {
        case 0:
            return agent.TryGetComponent( out FourmiPattern fourmi );
        case 1:
            return agent.TryGetComponent( out PapillonPattern papillon );
        case 2:
            return agent.TryGetComponent( out PoissonPattern poisson );
        case 3:
            return agent.TryGetComponent( out SauterellePattern sauterelle );
        }

        return false;
    }

    private GameObject AddNewBot (int type)
    {

        GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().AddUser();

        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
        GameObject agent = simulator.rvoGameObj.Find( ( GameObject obj ) => !obj.activeInHierarchy );

        if ( agent != null )
        {

            //GameObject agent = simulator.rvoGameObj.Find( ( GameObject obj ) => !obj.activeInHierarchy && CheckType(obj, type) );
            DestroyImmediate( agent.GetComponent<PathManager>() );
            agent.GetComponent<BotVisitData>().ClearData();

            if( type == 0 )
            {
                agent.AddComponent<FourmiPattern>();
            }

            if ( type == 1 )
            {
                agent.AddComponent<PapillonPattern>();
            }

            if ( type == 2 )
            {
                agent.AddComponent<PoissonPattern>();
            }

            if ( type == 3 )
            {
                agent.AddComponent<SauterellePattern>();
            }

            Vector3 position = GetSpawnPoint();
            int rvoGameIndex = simulator.rvoGameObj.IndexOf( agent );
            simulator.getSimulator().setAgentPosition( rvoGameIndex, new RVO.Vector2(position.x, position.z));
            agent.transform.position = position;
            agent.name = "Agente " + index++;
            agent.SetActive( true );

            simulator.rvoGameObj[ rvoGameIndex ] = agent;

            return simulator.rvoGameObj[ rvoGameIndex ];

        }

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

    private Vector3 GetSpawnPoint ()
    {
        if( spawnPoint.MoveNext() )
        {
            return ((GameObject) spawnPoint.Current).transform.position;
        }
        else
        {
            spawnPoint.Reset();
            return GetSpawnPoint();
        }
    }

    private GameObject AddNewFourmi ()
    {
        GameObject o = Instantiate( fourmiBot, transform, true );
        o.transform.position = GetSpawnPoint();
        o.name = "Agente " + index++;
        return o;
    }

    private GameObject AddNewPapillon ()
    {
        GameObject o = Instantiate( papillonBot, transform, true );
        o.transform.position = GetSpawnPoint();
        o.name = "Agente " + index++;

        return o;

    }

    private GameObject AddNewPoisson ()
    {
        GameObject o = Instantiate( poissonBot, transform, true );
        o.transform.position = GetSpawnPoint();
        o.name = "Agente " + index++;
        return o;
    }

    private GameObject AddNewSauterelle ()
    {
        GameObject o = Instantiate( sauterelleBot, transform, true );
        o.transform.position = GetSpawnPoint();
        o.name = "Agente " + index++;

        return o;
    }
}
