using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class Filler : MonoBehaviour
{

    int index = 1;
    IEnumerator spawnPoint;
    RVOSimulator simulator;

    [Header( "Singoli agenti" )]
    public float pauseTime;
    public int numberOfFourmi;
    public int numberOfPapillon;
    public int numberOfPoisson;
    public int numberOfSauterelle;
    float deltaTime;

    [Header( "Gruppi agenti con singolo pattern" )]
    public int numberOfGroupSinglePattern;
    public float pauseTimeGroupSinglePattern;
    public bool activeGroupSinglePattern;
    float deltaTimeGruppoSingolo;

    [Header( "Gruppi agenti con pattern random" )]
    public float pauseTimeMixedGroup;
    public int numberOfMixedGroup;
    public bool activeMixedGroup;
    float deltaTimeGruppoMisto;

    [Header( "Proprietà per leader despota" )]
    [Range( 0, 11 )]
    public int probabilitaLiberta;
    public bool leaderDespota;

    [Header( "Prefabs e Spawn Point" )]
    public string RadiceNomeAgenti = "Agente";
    public GameObject lowPolyMan;
    public GridSystem spawnPoints;


    List<Dictionary<string, int>> configurations = new List<Dictionary<string, int>>();

    bool running = false;
    bool terminate = false;

    int configurationNumber = 1;

    private System.Type FOURMI {
        get => typeof(FourmiPattern );
    }

    private System.Type PAPILLON
    {
        get => typeof( PapillonPattern );
    }
    
    private System.Type POISSON
    {
        get => typeof( PoissonPattern );
    }
    
    private System.Type SAUTERELLE
    {
        get => typeof( SauterellePattern );
    }       

    private void Awake ()
    {
        ReadData();
    }

    private void Start ()
    {
        spawnPoint = spawnPoints.GetEnumerator();
        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
    }

    private void AddGroupSinglePattern ()
    {
        List<GroupData> group = new List<GroupData>();

        int numberOfMember = Random.Range( 3, 8 );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            GameObject o;
            o = AddAgent( GetRandomMovementPattern() );
            if ( leaderDespota )
            {
                o.GetComponent<PathManager>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) && i < numberOfMember - 1)
                {
                    Destroy( o.GetComponent<PathManager>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesBot>();
                }
            }

            group.Add( o.GetComponent<GroupData>() );
        }

        GroupData capo = group[ group.Count - 1];
        group.Remove( capo );

        if ( leaderDespota )
        {
            capo.despota = true;
            capo.GetComponent<PathManager>().activeBot = true;
            capo.GetComponent<PathManager>().pauseTime *= 2;

        }
        capo.SetGroup( group );
    }

    private System.Type GetRandomMovementPattern ()
    {
        int numberOfPattern = 3;
        switch( Random.Range( 0, numberOfPattern ) )
        {
            case 0:
                return typeof(FourmiPattern);
            case 1:
                return typeof(PapillonPattern);
            case 2:
                return typeof(PoissonPattern);
            case 3:
                return typeof(SauterellePattern);
        }

        return typeof( PathManager );
    }

    private void AddMixedGroup ()
    {

        List<GroupData> group = new List<GroupData>();
        int numberOfMember = Random.Range( 3, 8 );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            GameObject o;

            o = InitializeAgent( GetRandomMovementPattern() );

            if ( leaderDespota )
            {
                o.GetComponent<PathManager>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) && i < numberOfMember - 1 )
                {
                    Destroy( o.GetComponent<PathManager>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesBot>();
                }
            }

            group.Add( o.GetComponent<GroupData>() );

        }

        GroupData capo = group[ group.Count - 1];
        group.Remove( capo );

        if( leaderDespota )
        {
            capo.despota = true;
            capo.GetComponent<PathManager>().activeBot = true;
        }

        capo.SetGroup( group );


    }


    IEnumerator GenerateSingleAgents ()
    {
    
        if(  numberOfFourmi > 0)
        {
            numberOfFourmi--;
            AddAgent( FOURMI );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfPapillon > 0 )
        {
            numberOfPapillon--;
            AddAgent( PAPILLON );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfPoisson > 0)
        {
            numberOfPoisson--;
            AddAgent( POISSON );
            yield return new WaitForSeconds( pauseTime );
        }

        if ( numberOfSauterelle > 0)
        {
            numberOfSauterelle--;
            AddAgent( SAUTERELLE );
            yield return new WaitForSeconds( pauseTime );
        }

        if( Mathf.Max( numberOfFourmi , numberOfPoisson, numberOfPapillon, numberOfSauterelle) > 0 )
        {
            running = true;
            yield return GenerateSingleAgents();
        }
        else
        {
            running = false;
        }

    }

    private void Update ()
    {

        if( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle, numberOfMixedGroup, numberOfGroupSinglePattern ) == 0)
        {

            if( configurations.Count > 0 )
            {

                string path = "Assets/dati_visite.txt";
                StreamWriter writer = new StreamWriter( path, true );
                writer.WriteLine( "# Configurazione " + configurationNumber++);
                writer.Close();

                Dictionary<string, int> configurazione = configurations[ 0 ];
                configurations.RemoveAt( 0 );

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
                        case "pauseTimeGroupSinglePattern":
                            pauseTimeGroupSinglePattern = data.Value;
                            break;
                        case "numberOfGroupSinglePattern":
                            numberOfGroupSinglePattern = data.Value;
                            if ( pauseTimeGroupSinglePattern > 0 )
                                activeGroupSinglePattern = true;
                            break;
                        case "pauseTimeMixedGroup":
                            pauseTimeMixedGroup = data.Value;
                            break;
                        case "numberOfMixedGroup":
                            numberOfMixedGroup = data.Value;
                            if ( numberOfMixedGroup > 0 )
                                activeMixedGroup = true;
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
            else
            {
                if ( terminate == false )
                {
                    List<GameObject> agents = simulator.rvoGameObj.FindAll( ( GameObject obj ) => obj.activeInHierarchy );
                    if ( agents.Count == 0 )
                    {
                        terminate = true;
                        GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().TerminaSimulazione();
                    }
                }
            }

        }

        if ( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle ) > 0 && running == false )
        {
            running = true;
            StartCoroutine( GenerateSingleAgents() );
        }

        if ( deltaTimeGruppoMisto > pauseTimeMixedGroup )
        {
            if ( activeMixedGroup && numberOfMixedGroup > 0 )
            {
                numberOfMixedGroup--;
                AddMixedGroup();
            }

            deltaTimeGruppoMisto = 0f;
        }


        if ( deltaTimeGruppoSingolo > pauseTimeGroupSinglePattern )
        {
            if ( activeGroupSinglePattern && numberOfGroupSinglePattern > 0 )
            {
                numberOfGroupSinglePattern--;
                AddGroupSinglePattern();
            }

            deltaTimeGruppoSingolo = 0f;
        }


        deltaTimeGruppoMisto += Time.deltaTime;
        deltaTimeGruppoSingolo += Time.deltaTime;

        deltaTime += Time.deltaTime;
    }

    public void ReadData ()
    {
        string configFile = "Assets/dati_caricamento.txt";
        StreamReader reader = new StreamReader( configFile );
        string content = reader.ReadToEnd();

        Regex regex_conf = new Regex( "#CONF\\n(.*\\n?)*?#END" );
        MatchCollection FileConfigurations = regex_conf.Matches( content );
        
        foreach( Match configuration in FileConfigurations )
        {

            Dictionary<string, int> data = new Dictionary<string, int>();

            Regex regex_data = new Regex( @"\w+=\d+" );
            MatchCollection confs_data = regex_data.Matches( configuration.Value );

            foreach ( Match conf_data in confs_data )
            {
                string[ ] item = conf_data.Value.Split( '=' );
                data.Add(item[0], System.Int32.Parse(item[1]));
            }

            configurations.Add( data );
        }

        Debug.Log( "Configurazioni recuperate con successo" );

        reader.Close();
    }


    private GameObject AddAgent (System.Type type)
    {

        GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().UpdateAgentsCounter();

        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
        GameObject agent = simulator.rvoGameObj.Find( ( GameObject obj ) => !obj.activeInHierarchy );

        if ( agent != null )
        {
            return RecyclesAgent( agent, type );
        }

        return InitializeAgent( type );
    }

    private GameObject RecyclesAgent (GameObject oldAgent, System.Type type)
    {
        DestroyImmediate( oldAgent.GetComponent<PathManager>() );
        oldAgent.GetComponent<BotVisitData>().ClearData();
        oldAgent.GetComponent<BotVisitData>().configurazioneDiIngresso = configurationNumber - 1;

        oldAgent.AddComponent( type );

        Vector3 position = GetSpawnPoint();
        int rvoGameIndex = simulator.rvoGameObj.IndexOf( oldAgent );
        simulator.getSimulator().setAgentPosition( rvoGameIndex, new RVO.Vector2( position.x, position.z ) );
        oldAgent.transform.position = position;
        oldAgent.name = RadiceNomeAgenti + " " + index++;
        oldAgent.SetActive( true );

        simulator.rvoGameObj[ rvoGameIndex ] = oldAgent;

        return simulator.rvoGameObj[ rvoGameIndex ];
    }


    private GameObject InitializeAgent (System.Type type)
    {
        GameObject o = Instantiate( lowPolyMan, transform, true );
        o.AddComponent(type);
        o.transform.position = GetSpawnPoint();
        o.name = RadiceNomeAgenti + " " + index++;
        o.GetComponent<BotVisitData>().configurazioneDiIngresso = configurationNumber - 1;

        return o;
    }

    private Vector3 GetSpawnPoint ()
    {
        if ( spawnPoint.MoveNext() )
        {
            return ( ( GameObject )spawnPoint.Current ).transform.position;
        }

        spawnPoint.Reset();
        return GetSpawnPoint();

    }
}
