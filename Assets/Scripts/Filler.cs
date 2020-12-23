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

    [Header( "Ingresso membri gruppi" )]
    public float pauseTimeGroupMember;

    [ Header( "Proprietà per leader despota" )]
    [Range( 0, 11 )]
    public int probabilitaLiberta;
    public bool leaderDespota;

    [Header( "Prefabs e Spawn Point" )]
    public string RadiceNomeAgenti = "Agente";
    public GameObject lowPolyMan;
    public GridSystem spawnPoints;


    List<Dictionary<string, int>> configurations = new List<Dictionary<string, int>>();

    bool isRunMixedgroup = false;
    bool isRunGenSingAgent = false;
    bool isRunGroupSingle = false;

    bool terminate = false;

    int configurationNumber = 1;

    private System.Type FOURMI {
        get => typeof( FourmiAgent );
    }

    private System.Type PAPILLON
    {
        get => typeof( PapillonAgent );
    }
    
    private System.Type POISSON
    {
        get => typeof( PoissonAgent );
    }
    
    private System.Type SAUTERELLE
    {
        get => typeof( SauterelleAgent );
    }       

    private void Awake ()
    {
        ReadConfigurationData();
    }

    public void ReadConfigurationData ()
    {
        string configFile = Application.persistentDataPath + "/config.txt";
        Debug.Log( configFile );

        StreamReader reader = new StreamReader( configFile );
        string content = reader.ReadToEnd();

        Regex regex_conf = new Regex( "#CONF\\n(.*\\n?)*?#END" );
        MatchCollection FileConfigurations = regex_conf.Matches( content );

        foreach ( Match configuration in FileConfigurations )
        {

            Dictionary<string, int> data = new Dictionary<string, int>();

            Regex regex_data = new Regex( @"\w+=\d+" );
            MatchCollection confs_data = regex_data.Matches( configuration.Value );

            foreach ( Match conf_data in confs_data )
            {
                string[ ] item = conf_data.Value.Split( '=' );
                data.Add( item[ 0 ], System.Int32.Parse( item[ 1 ] ) );
            }

            configurations.Add( data );
        }

        Debug.Log( "Configurazioni recuperate con successo" );

        reader.Close();
    }

    private void Start ()
    {
        spawnPoint = spawnPoints.GetEnumerator();
        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
    }

    IEnumerator GenerateSingleAgents ()
    {
        isRunGenSingAgent = true;

        while ( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle ) > 0 )
        {

            if ( numberOfFourmi > 0 )
            {
                yield return new WaitForSeconds( pauseTime );
                numberOfFourmi--;
                AddAgent( FOURMI );
            }

            if ( numberOfPapillon > 0 )
            {
                yield return new WaitForSeconds( pauseTime );
                numberOfPapillon--;
                AddAgent( PAPILLON );
            }

            if ( numberOfPoisson > 0 )
            {
                yield return new WaitForSeconds( pauseTime );
                numberOfPoisson--;
                AddAgent( POISSON );
            }

            if ( numberOfSauterelle > 0 )
            {
                yield return new WaitForSeconds( pauseTime );
                numberOfSauterelle--;
                AddAgent( SAUTERELLE );
            }
           
        }

        isRunGenSingAgent = false;

        yield return null;
    }

    private System.Type GetRandomMovementPattern ()
    {
        int numberOfPattern = 3;
        switch ( Random.Range( 0, numberOfPattern ) )
        {
        case 0:
            return typeof( FourmiAgent );
        case 1:
            return typeof( PapillonAgent );
        case 2:
            return typeof( PoissonAgent );
        case 3:
            return typeof( SauterelleAgent );
        }

        return typeof( BaseAgent );
    }

    private IEnumerator AddGroupSinglePattern ()
    {
        Color groupColor = GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().GetColor();

        List<GroupData> group = new List<GroupData>();

        int numberOfMember = Random.Range( 3, 8 ); ;

        System.Type agentType = GetRandomMovementPattern();

        GameObject obj = InitializeAgent( agentType );
        GroupData capo = obj.GetComponent<GroupData>();

        capo.isLeader = true;
        if ( leaderDespota )
        {
            capo.despota = true;
            capo.GetComponent<BaseAgent>().pauseTime *= 2;
        }

        capo.SetGroup( groupColor );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            yield return new WaitForSeconds( pauseTimeGroupMember );

            GameObject o;

            o = InitializeAgent( GetRandomMovementPattern() );
            
            if ( leaderDespota )
            {
                o.GetComponent<BaseAgent>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) )
                {
                    Destroy( o.GetComponent<BaseAgent>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesAgent>();
                    o.GetComponent<NoChoicesAgent>().ReceiveLeaderChoiceQueue( capo.GetComponent<BotVisitData>().destination );
                    o.GetComponent<NoChoicesAgent>().activeBot = true;
                }
            }

            o.GetComponent<GroupData>().GroupElementSetData( groupColor, true, capo.gameObject );
            capo.AddMember( o.GetComponent<GroupData>() );
        }
        
        yield break;
    }

    private IEnumerator AddMixedGroup ()
    {
        Color groupColor = GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().GetColor();

        List<GroupData> group = new List<GroupData>();
        int numberOfMember = Random.Range( 3, 8 ); ;

        GameObject obj = InitializeAgent( GetRandomMovementPattern() );
        GroupData capo = obj.GetComponent<GroupData>();

        capo.isLeader = true;
        if ( leaderDespota )
        {
            capo.despota = true;
            capo.GetComponent<BaseAgent>().pauseTime *= 2;
        }
        capo.SetGroup( groupColor );

        for ( int i = 0; i < numberOfMember; i++ )
        {
            yield return new WaitForSeconds( pauseTimeGroupMember );

            GameObject o;

            o = InitializeAgent( GetRandomMovementPattern() );

            group.Add( o.GetComponent<GroupData>() );

            if ( leaderDespota )
            {
                o.GetComponent<BaseAgent>().activeBot = false;
                if ( probabilitaLiberta > Random.Range( 1, 10 ) )
                {
                    Destroy( o.GetComponent<BaseAgent>() );
                    o.GetComponent<BotVisitData>().ClearData();
                    o.AddComponent<NoChoicesAgent>();
                    o.GetComponent<NoChoicesAgent>().ReceiveLeaderChoiceQueue( capo.GetComponent<BotVisitData>().destination );
                    o.GetComponent<NoChoicesAgent>().activeBot = true;

                }
            }

            o.GetComponent<GroupData>().GroupElementSetData( groupColor, true, capo.gameObject );
            capo.AddMember( o.GetComponent<GroupData>() );
        }

        yield break;
    }

    private void Update ()
    {

        if( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle, numberOfMixedGroup, numberOfGroupSinglePattern ) == 0)
        {
            if ( configurations.Count > 0 )
            {
                LoadNextConfiguration();
            }
            else
            {
                CheckEndOfSimulation();
            }
        }

        // Restart if add manually data in Unity Editor

        if ( Mathf.Max( numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle ) > 0 && !isRunGenSingAgent)
        {
            StartCoroutine( GenerateSingleAgents() );
        }

        if ( numberOfMixedGroup > 0 && !isRunMixedgroup )
        {
            StartCoroutine( ManageMixedGroupCreation() );
        }

        if( numberOfGroupSinglePattern  > 0 && !isRunGroupSingle)
        {
            StartCoroutine( ManageSingleGroupCreation() );
        }
    }

    private void LoadNextConfiguration ()
    {
        configurationNumber++;

        Dictionary<string, int> configurazione = configurations[ 0 ];
        configurations.RemoveAt( 0 );

        foreach ( KeyValuePair<string, int> data in configurazione )
        {
            UpdateConfigurationData( data.Key, data.Value );
        }

        if( !isRunGenSingAgent )
        {
            StartCoroutine( GenerateSingleAgents() );
        }

        if( !isRunGroupSingle )
        {
            StartCoroutine( ManageSingleGroupCreation() );
        }

        if( !isRunMixedgroup )
        {
            StartCoroutine( ManageMixedGroupCreation() );
        }
    }

    private void UpdateConfigurationData ( string name, int value )
    {
        switch ( name )
        {
        case "pauseTime":
            pauseTime = value;
            break;
        case "numberOfFourmi":
            numberOfFourmi = value;
            break;
        case "numberOfPapillon":
            numberOfPapillon = value;
            break;
        case "numberOfPoisson":
            numberOfPoisson = value;
            break;
        case "numberOfSauterelle":
            numberOfSauterelle = value;
            break;
        case "pauseTimeGroupSinglePattern":
            pauseTimeGroupSinglePattern = value;
            break;
        case "numberOfGroupSinglePattern":
            numberOfGroupSinglePattern = value;
            if ( pauseTimeGroupSinglePattern > 0 )
                activeGroupSinglePattern = true;
            break;
        case "pauseTimeMixedGroup":
            pauseTimeMixedGroup = value;
            break;
        case "numberOfMixedGroup":
            numberOfMixedGroup = value;
            if ( numberOfMixedGroup > 0 )
                activeMixedGroup = true;
            break;
        case "probabilitaLiberta":
            probabilitaLiberta = value;
            break;
        case "leaderDespota":
            if ( value == 1 )
            {
                leaderDespota = true;
            }
            else
            {
                leaderDespota = false;
            }
            break;
        case "pauseTimeGroupMember":
            pauseTimeGroupMember = value;
            break;
        }
    }

    private void CheckEndOfSimulation ()
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

    private IEnumerator ManageSingleGroupCreation ()
    {
        isRunGroupSingle = true;

        while ( numberOfGroupSinglePattern > 0 )
        {

            if ( activeGroupSinglePattern )
            {
                numberOfGroupSinglePattern--;
                yield return new WaitForSeconds( pauseTimeGroupSinglePattern );
                yield return StartCoroutine( AddGroupSinglePattern() );
            }
        }

        isRunGroupSingle = false;

        yield break;
    }

    private IEnumerator ManageMixedGroupCreation ()
    {
        isRunMixedgroup = true;

        while ( numberOfMixedGroup  > 0 )
        {

            if ( activeMixedGroup )
            {
                numberOfMixedGroup--;
                yield return new WaitForSeconds( pauseTimeMixedGroup );
                yield return StartCoroutine ( AddMixedGroup());
            }

        }

        isRunMixedgroup = false;

        yield break;
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
        DestroyImmediate( oldAgent.GetComponent<BaseAgent>() );
        oldAgent.GetComponent<BotVisitData>().ClearData();
        oldAgent.GetComponent<BotVisitData>().configIndex = configurationNumber - 1;

        oldAgent.AddComponent( type );

        Vector3 spawnPointPosition = GetSpawnPoint();
        int rvoGameIndex = simulator.rvoGameObj.IndexOf( oldAgent );

        simulator.getSimulator().setAgentPosition( rvoGameIndex, new RVO.Vector2( spawnPointPosition.x, spawnPointPosition.z ) );
        oldAgent.transform.position = spawnPointPosition;
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
        o.GetComponent<BotVisitData>().configIndex = configurationNumber - 1;

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
