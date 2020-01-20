using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

using Museum.Utility;
using System.Collections.Generic;
using System;
using Pathfinding;

using System.Collections;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    protected GameObject DestinationPoint
    {
        get => visitData.destinationPoint;
        set => visitData.destinationPoint = value;
    }

    protected GameObject Destination
    {
        get => visitData.destination;
        set => visitData.destination = value;
    }

    protected Sort utilitySort;

    protected bool inPausa = false;

    // Segui percorso
    [SerializeField] 
    protected float pauseTime = 5f;

    protected float timedelta = 0f;
    float baseTime;

    protected Dictionary<GameObject, List<PictureInfo>> picturesOnWalls = new Dictionary<GameObject, List<PictureInfo>>();

    protected List<PictureInfo> importantIgnoratePicture = new List<PictureInfo>();

    protected List<PictureInfo> VisitedPictures
    {
        get => visitData.visitedPictures;
        set => visitData.visitedPictures = value;
    }

    protected List<PictureInfo> ImportantPictures
    {
        get => visitData.importantPictures;
        set => visitData.importantPictures = value;
    }

    private BotVisitData visitData;
    private MarkerManager markerManager;

    public int maxDistanza = 30;
    //protected float distanzaPercorsa = 0f;
    
    private float DistanzaPercorsa
    {
        get => visitData.distanzaPercorsa;
        set => visitData.distanzaPercorsa = value;
    }

    protected int CurrentPictureIndex
    {
        get => visitData.currentPictureIndex;
        set => visitData.currentPictureIndex = value;
    }

    protected List<GameObject> emptySpaces;

    protected GameObject DestinationPrePause
    {
        get => visitData.destinationPrePause;
        set => visitData.destinationPrePause = value;
    }

    private GameObject LastPositionPattern
    {
        get => visitData.lastPositionPattern;
        set => visitData.lastPositionPattern = value;
    }

    public bool despota = false;
    public bool noChoices = false;
    public bool isLeader = false;
    protected GameObject leader;
    public List<PathManager> group = new List<PathManager>();
    
    protected float DurataVisita
    {
        get => visitData.durataVisita;
        set => visitData.durataVisita = value;
    }

    protected float TempoInAttesa
    {
        get => visitData.tempoInAttesa;
        set => visitData.tempoInAttesa = value;
    }

    public abstract GameObject GetNextDestination ();
    public abstract void InitMovementPattern ();

    private bool firstChoices = true;

    public const int NON_STANCO = 0;
    public const int STANCO = 1;
    public const int MOLTO_STANCO = 2;

    public bool activeBot = false;

    protected void Start ()
    {
        markerManager = GetComponent<MarkerManager>();

        visitData = GetComponent<BotVisitData>();

        importantIgnoratePicture = visitData.importantIgnoratePicture;
        
        utilitySort = new Sort
        {
            transform = transform
        };

        baseTime = pauseTime;

        emptySpaces = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );

        InitMovementPattern();

        SelectImportantPictureStrategy();

        InitGroupData();
    }


    protected virtual void GroupElementSetData ( Color groupColor, bool leaderDespota)
    {
        SetLeader( gameObject );
        markerManager.SetColorGroup( groupColor );

        //if( noChoices )
        //{
        //    importantPictures.Clear();
        //    visitedPictures.Clear();
        //}
    }


    /*
     * 
     * Gestione gruppi    
     * 
     */
    void InitGroupData ()
    {
        if ( isLeader )
        {
            Color groupColor = GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().GetColor();
            markerManager.SetColorGroup( groupColor );

            markerManager.ShowLeader();

            foreach ( PathManager member in group )
            {
                member.GroupElementSetData(groupColor, despota);
            }
        }
        else
        {
            markerManager.ShowBase();
        }
    }

    public void SetGroup ( List<PathManager> groupMembers )
    {
        isLeader = true;
        group = groupMembers;
    }

    void SetLeader ( GameObject myLeader )
    {
        leader = myLeader;
    }

    void SelectImportantPictureStrategy ()
    {
        foreach ( PictureInfo picture in FindObjectsOfType<PictureInfo>() )
        {
            // "Opere medio/grandi per l'espositore", oppure, "Opere di interesse per l'agent"
            if ( picture.priority > 1 || UnityEngine.Random.Range( 0, 2 ) > 1 )
            {
                ImportantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        ImportantPictures.Sort( utilitySort.SortByIndex );
        ImportantPictures.Reverse();

    }

    protected virtual void Behaviour ()
    {
        DurataVisita += Time.deltaTime;

        if ( inPausa )
        {

            timedelta += Time.deltaTime;

            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                TempoInAttesa += Time.deltaTime;
            }

            CheckDestinationFromPause();
            return;
        }

        if ( timedelta > pauseTime || firstChoices )
        {
            firstChoices = false;
            UpdateDestination();
            timedelta = 0f;
        }


        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            GetComponentInChildren<BubbleHead>().GuardoOpera();
            timedelta += Time.deltaTime;

        }
        else
        {
            GetComponentInChildren<BubbleHead>().VersoDestinazione();

        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                DestinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }
    }

    private void NoChoicesBot ()
    {
        if( !activeBot )
        {
            return;
        }

        DurataVisita += Time.deltaTime;

        if ( inPausa )
        {
            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                TempoInAttesa += Time.deltaTime;
            }

            if( DestinationPrePause == null || VisitedPictures.Contains( DestinationPrePause.GetComponent<PictureInfo>() ) )
            {
                utilitySort.transform = leader.GetComponent<PathManager>().DestinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        Destination = plane;
                        break;
                    }
                }
            }
            else
            {
                if ( DestinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    Destination = DestinationPrePause;
                    VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
                    UpdateDestinationPoint();
                    GoToDestinationPoint();

                    //Debug.Log( name + ": la destinazione si è liberata", Destination );

                    inPausa = false;
                }
            }

            return;
        }

        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            GetComponentInChildren<BubbleHead>().GuardoOpera();
            timedelta += Time.deltaTime;

        }
        else
        {
            GetComponentInChildren<BubbleHead>().VersoDestinazione();

        }

        if ( ImportantPictures.Count > 0 || timedelta > pauseTime)
        {
            //DestinationPrePause = null;
            UpdateDestination();
            timedelta = 0f;
        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                DestinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }

    }
    
    private void NormaleBot ()
    {
        DurataVisita += Time.deltaTime;

        if ( inPausa )
        {

            timedelta += Time.deltaTime;

            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                TempoInAttesa += Time.deltaTime;
            }

            CheckDestinationFromPause();
            return;
        }

        if ( timedelta > pauseTime || firstChoices )
        {
            firstChoices = false;
            UpdateDestination();
            timedelta = 0f;
        }


        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            GetComponentInChildren<BubbleHead>().GuardoOpera();
            timedelta += Time.deltaTime;

        }
        else
        {
            GetComponentInChildren<BubbleHead>().VersoDestinazione();

        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                DestinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }
    }

    private void Update ()
    {

        Behaviour();

        //if ( noChoices )
        //{
        //    NoChoicesBot();
        //}
        //else
        //{
        //    NormaleBot();
        //}
    }

    protected virtual void UseLastDestinationOrNew ()
    {

        //if( noChoices )
        //{
        //    if( importantPictures.Count > 0 )
        //    {
        //        Destination = importantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
        //        if ( importantPictures.Contains( Destination.transform.parent.gameObject ) )
        //        {
        //            importantPictures.Remove( Destination.transform.parent.gameObject );
        //        }

        //        //CheckNextDestination();
        //    }
        //    else
        //    {
        //        inPausa = true;

        //        utilitySort.transform = leader.GetComponent<PathManager>().DestinationPoint.transform;
        //        emptySpaces.Sort( utilitySort.DistanzaPlane );

        //        //Debug.Log( gameObject.name + ": Destinazione: ", Destination );
        //        //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

        //        foreach ( GameObject plane in emptySpaces )
        //        {
        //            if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
        //            {
        //                Destination = plane;
        //                //UpdateDestinationPoint();
        //                //GoToDestinationPoint();
        //                break;
        //            }
        //        }
        //    }

        //    //CheckNextDestination();
        //    return;
        //}

        if ( LastPositionPattern == null )
        {
            Destination = GetNextDestination();
            //Debug.Log( gameObject.name + ": Chiedo nuova destinazione dal pattern", Destination );
        }
        else
        {
            Destination = LastPositionPattern;

            if ( Destination.CompareTag( "Picture" ) )
            {
                // Qui magari posso utilizzare la distanza dal quadro, invece che l'indice (sarebbe meglio)
                if( Destination.GetComponent<PictureInfo>().index < CurrentPictureIndex - 5 )
                {
                    //Debug.Log( name + ": La destinazione già calcolata è un quadro con indice troppo basso per essere visitato ora." );
                    LastPositionPattern = null;
                    UseLastDestinationOrNew();
                }
            }

            //Debug.Log( gameObject.name + ": Uso destinazione già calcolata in precedenza", Destination );

        }
    }

    protected void UpdateDestination () 
    {
        bool haveLastPositionPattern = false;
        float distanceFromDestination = 0;

        UseLastDestinationOrNew();

        NavMeshPath staticPath = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, Destination.transform.position, NavMesh.AllAreas, staticPath );

        distanceFromDestination = GetPathLenght( staticPath );
        Transform destinationPicture;

        destinationPicture = Destination.transform.parent;

        utilitySort.transform = this.transform;
        ImportantPictures.Sort( utilitySort.DistanzaPicture );

        try
        {
            foreach ( PictureInfo picture in ImportantPictures )
            {
                GameObject picturePlane = picture.gameObject.transform.GetChild( 0 ).gameObject;

                NavMesh.CalculatePath( transform.position, picturePlane.transform.position, NavMesh.AllAreas, staticPath );

                float distanzaFromPictureImportant = GetPathLenght( staticPath );

                // Immagine importante più vicina di immagine pattern
                if ( distanzaFromPictureImportant < distanceFromDestination || (
                     picture.index < destinationPicture.GetComponent<PictureInfo>().index && Destination.CompareTag( "PicturePlane" ) ) )
                {
                    ImportantPictures.Remove( picture );
                    LastPositionPattern = Destination;
                    haveLastPositionPattern = true;
                    Destination = picturePlane;

                    //Debug.Log( "Questa destinazione viene salvata per dopo", LastPositionPattern );
                    //Debug.Log( "Prossima destinazione è importante", Destination );
                    break;
                }
            }
        }
        catch( NullReferenceException e )
        {
            //Debug.Log( name + ": nessuna destinazione" );
        }

        if ( !haveLastPositionPattern )
        {
            LastPositionPattern = null;
        }

        if ( ImportantPictures.Contains( Destination.GetComponentInParent<PictureInfo>() ) )
        {
            ImportantPictures.Remove( Destination.GetComponentInParent<PictureInfo>() );
        }
        
        CheckNextDestination();

    }

    protected virtual GameObject GetPointInDestination ()
    {
        return Destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
    }

    //private void UpdateDestinationPointForNoChoiceExit(){
    //    StartCoroutine( LiberaPosto(DestinationPoint) );
    //    DestinationPoint = Destination.GetComponent<GridSystem>().GetRandomPoint();
    //    DestinationPoint.GetComponent<DestinationPoint>().Occupa();
    //}

    protected virtual void NotifyNewDestination(GameObject leaderDestination )
    {
        //if( noChoices && leaderDestination.CompareTag( "Uscita" ) )
        //{
        //    Debug.Log(name + ": impostata destinazione come uscita", leaderDestination);
        //    Destination = leaderDestination;

        //    if( DestinationPrePause != null)
        //    {
        //        inPausa = false;
        //        importantIgnoratePicture.Add( DestinationPrePause.transform.parent.gameObject );
        //        importantPictures.Clear();
        //        DestinationPrePause = null;
        //    }

        //    UpdateDestinationPointForNoChoiceExit();
        //    GoToDestinationPoint();
        //    return;
        //}

        if ( leaderDestination.CompareTag( "PicturePlane" ) || leaderDestination.CompareTag( "Empty Space" ))
        {
            //Debug.Log( gameObject.name + ": Capo ha scelto nuova destinazione importante", leaderDestination );

            if( /* !noChoices && */ !ImportantPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
            {
                ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
            }

            //if ( noChoices && leaderDestination.CompareTag( "Empty Space" ) && !visitedPictures.Contains( leaderDestination ) )
            //{
            //    if( DestinationPrePause )
            //    {
            //        importantIgnoratePicture.Add( DestinationPrePause.transform.parent.gameObject );
            //    }
                
            //    importantPictures.Add( leaderDestination );
            //}
            //else if ( noChoices && leaderDestination.CompareTag( "PicturePlane" ) && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ) )
            //{
            //    if( DestinationPrePause )
            //    {
            //        importantIgnoratePicture.Add( DestinationPrePause.transform.parent.gameObject );
            //    }
            //    importantPictures.Add( leaderDestination.transform.parent.gameObject );
            //}

        }

    }


    protected void GoToDestinationPoint ()
    {
        GetComponent<RVOAgent>().UpdateTarget( DestinationPoint.transform );
        GetComponent<RVOAgent>().Refresh();

        utilitySort.transform = transform;
        group.Sort( ( PathManager x, PathManager y ) => UnityEngine.Random.Range( -1, 1 ) );

        if ( isLeader )
        {
            foreach ( PathManager member in group )
            {
                try
                {
                    member.NotifyNewDestination( Destination );
                    if( despota )
                    {
                        member.activeBot = true;
                    }
                }
                catch( MissingReferenceException e )
                {
                    //Debug.Log( gameObject.name + ": un membro del gruppo ha già abbandonato il museo." );
                }
            }
        }

        NavMeshPath staticPath = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, DestinationPoint.transform.position, NavMesh.AllAreas, staticPath );

        DistanzaPercorsa += GetPathLenght( staticPath );
    }

    protected float GetPathLenght ( NavMeshPath path )
    {

        Vector3[ ] corners = path.corners;

        float lng = 0;

        for ( int i = 0; i < corners.Length - 1; i++ )
        {
            lng += Vector3.Distance( corners[ i ], corners[ i + 1 ] );
        }

        return lng;
    }

    protected IEnumerator LiberaPosto (GameObject oldDestination)
    {
        yield return new WaitForSeconds( 1.5f );

        if ( oldDestination != null )
            oldDestination.GetComponent<DestinationPoint>().Libera();

    }

    protected void UpdateDestinationPoint ()
    {
        StartCoroutine( LiberaPosto(DestinationPoint) );

        DestinationPoint = GetPointInDestination();
        DestinationPoint.GetComponent<DestinationPoint>().Occupa();

        if ( Destination.CompareTag( "PicturePlane" ) )
        {
            CurrentPictureIndex = Destination.transform.parent.GetComponent<PictureInfo>().index;
        }
    }
    
    protected void CheckDestinationFromPause ()
    {
    
        if ( DestinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            Destination = DestinationPrePause;
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
        
            // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre)
            if ( timedelta > 15f && DestinationPrePause.transform.parent.GetComponent<PictureInfo>().priority <= 1 || timedelta > 20f && DestinationPrePause.transform.parent.GetComponent<PictureInfo>().priority > 1 )
            {
                //Debug.Log( "È passato troppo tempo, passo oltre e ignoro questo quadro..." );
                VisitedPictures.Remove( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                //importantPictures.Add( DestinationPrePause.transform.parent.gameObject );
                importantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                inPausa = false;
                Destination = null;
            }

        }
    }

    /*
     * 0 = Non stanco
     * 1 = Stanco
     * 2 = Molto stanco
     */
    public int LivelloStanchezza ()
    {
    
        if( DistanzaPercorsa > maxDistanza )
        {
            //Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return MOLTO_STANCO;
        }

        if ( DistanzaPercorsa > ( maxDistanza / 1.2f ) )
        {
            //Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return STANCO;
        }

        //Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return NON_STANCO;
    }

    protected void CheckNextDestination ()
    {

        //// Posti disponibili e non è tra quelle da ignorare
        if ( Destination.GetComponent<GridSystem>().HaveAvailablePoint() && !importantIgnoratePicture.Contains(Destination.GetComponentInParent<PictureInfo>() ) )
        {
            if ( !Destination.CompareTag( "Empty Space" ) || !noChoices)
            {
                VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
            }
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
            // "Non sono stanco", oppure "Sono stanco ma il quadro è molto importante"
            if ( !importantIgnoratePicture.Contains( Destination.GetComponentInParent<PictureInfo>() ) && ( LivelloStanchezza() == 0 || 
                 ( LivelloStanchezza() == 1 && Destination.CompareTag("PicturePlane") && Destination.GetComponentInParent<PictureInfo>().priority > 1 ) ))
            {

                inPausa = true;
                DestinationPrePause = Destination;
                
                utilitySort.transform = DestinationPrePause.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", Destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach( GameObject plane in emptySpaces )
                {
                    if( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        Destination = plane;
                        UpdateDestinationPoint();
                        GoToDestinationPoint();
                        break;
                    }
                }

            }
            else
            {
                UpdateDestination();
            }

        }
    }

    protected bool IsExit ( )
    {
        if ( Destination != null && Destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, DestinationPoint.transform.position) <= 5f)
        {
            GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().ReceivData( this.GetType().Name, visitData);
            return true;
        }

        return false;
    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
