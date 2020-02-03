using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

using Museum.Utility;
using System.Collections.Generic;
using System;
using Pathfinding;

using System.Collections;

public abstract class BaseAgent : MonoBehaviour
{
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
    
    protected bool InPausa
    {
        get => visitData.inPausa;
        set => visitData.inPausa = value;
    }

    public float pauseTime = 10f;
    protected float timedelta = 0f;
    float baseTime;

    protected Sort utilitySort;

    protected FatigueManager.Level FatigueLevel
    {
        get => fatigueManager.GetFatigueLevel();
    }

    protected List<PictureInfo> ImportantIgnoratePicture
    {
        get => visitData.importantIgnoratePicture;
        set => visitData.importantIgnoratePicture = value;
    }

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
    private FatigueManager fatigueManager;
    protected ComicBalloon comicBalloon;
    
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

    protected GroupData groupData;

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

    protected int ChanceSkipDestination
    {
        get => visitData.chanceSkipDestination;
        set => visitData.chanceSkipDestination = value;
    }

    protected int MaxDistanza
    {
        get => visitData.maxDistanza;
        set => visitData.maxDistanza = value;
    }

    public abstract GameObject GetNextDestination ();
    public abstract void InitMovementPattern ();

    private bool firstChoices = true;
    protected bool CheckTimer = false;

    public bool activeBot = false;

    private const int SECOND = 1;

    protected void Start ()
    {
        markerManager = GetComponent<MarkerManager>();
        fatigueManager = GetComponent<FatigueManager>();
        visitData = GetComponent<BotVisitData>();
        groupData = GetComponent<GroupData>();
        comicBalloon = GetComponentInChildren<ComicBalloon>();

        utilitySort = new Sort
        {
            transform = transform
        };

        baseTime = pauseTime;

        emptySpaces = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );

        InitMovementPattern();

        SelectImportantPictureStrategy();

        StartCoroutine( ClockManager() );
    }

    protected virtual void SelectImportantPictureStrategy ()
    {
        foreach ( PictureInfo picture in FindObjectsOfType<PictureInfo>() )
        {
            int chanceAddPicture = 60;
            bool isImportantForAgent = UnityEngine.Random.Range( 0, 100) > chanceAddPicture;
            bool isImportantPicture = picture.priority > ( int )PictureInfo.Priority.OPERA_MEDIA;
            if ( isImportantForAgent || isImportantPicture )
            {
                ImportantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        ImportantPictures.Sort( utilitySort.SortByIndex );
        ImportantPictures.Reverse();

    }
    
    IEnumerator ClockManager ()
    {
        while ( true )
        {
            DurataVisita += SECOND;

            if ( InPausa )
            {
                timedelta += SECOND;

                if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
                {
                    TempoInAttesa += SECOND;
                }
            }
            else
            {
                if( timedelta > pauseTime )
                {
                    CheckTimer = true;
                    timedelta = 0;
                }

                if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
                {
                    timedelta += SECOND;
                }
            }

            yield return new WaitForSeconds( 1 );
        }
    }
    
    protected virtual void Behaviour ()
    {

        if ( InPausa )
        {

            comicBalloon.InAttesa();

            CheckDestinationFromPause();
            return;
        }

        if ( CheckTimer || firstChoices )
        {
            firstChoices = false;
            CheckTimer = false;
            UpdateDestination();
        }


        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            comicBalloon.GuardoOpera();

        }
        else
        {
            comicBalloon.VersoDestinazione();

        }

        if ( IsExit() )
        {
            ExitStrategy();
        }
    }

    protected virtual void ExitStrategy ()
    {
        if ( gameObject.activeInHierarchy )
        {
            DestinationPoint.GetComponent<DestinationPoint>().Libera();
            gameObject.SetActive( false );
            GetComponent<RVOAgent>().SetPositionInactive();
            transform.position = new Vector3( 30f, 0f, 30f );
        }
    }

    void Update ()
    {

        Behaviour();

    }

    private void UseLastDestinationOrNew ()
    {
        if ( LastPositionPattern == null )
        {
            Destination = GetNextDestination();
            Debug.Log( gameObject.name + ": Chiedo nuova destinazione dal pattern", Destination );
        }
        else
        {
            Destination = LastPositionPattern;

            if ( Destination.CompareTag( "Picture" ) )
            {
                int indexMaxDifference = 5;

                int minIndexAccepted = CurrentPictureIndex - indexMaxDifference;
                if ( Destination.GetComponent<PictureInfo>().index < minIndexAccepted )
                {
                    Debug.Log( name + ": La destinazione già calcolata è un quadro con indice troppo basso per essere visitato ora." );
                    LastPositionPattern = null;
                    UseLastDestinationOrNew();
                }
            }

            Debug.Log( gameObject.name + ": Uso destinazione già calcolata in precedenza", Destination );

        }
    }

    private void UpdateDestination () 
    {

        UseLastDestinationOrNew();

        SelectImportantPicMostClosest();

        CheckDestination();

    }

    private void SelectImportantPicMostClosest ()
    {

        NavMeshPath staticPath = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, Destination.transform.position, NavMesh.AllAreas, staticPath );

        float  distanceFromDestination = GetPathLenght( staticPath );
        Transform destinationPicture;

        if( Destination.CompareTag( "PicturePlane" ) )
        {
            destinationPicture = Destination.transform.parent;
        }
        else
        {
            destinationPicture = Destination.transform;
        }

        utilitySort.transform = this.transform;
        ImportantPictures.Sort( utilitySort.DistanzaPicture );
        
        foreach ( PictureInfo picture in ImportantPictures )
        {
            GameObject pictureGrid = picture.GetComponentInChildren<GridSystem>().gameObject;

            NavMesh.CalculatePath( transform.position, pictureGrid.transform.position, NavMesh.AllAreas, staticPath );

            float distanzaFromPictureImportant = GetPathLenght( staticPath );

            // Immagine importante più vicina di immagine pattern
            if ( distanzaFromPictureImportant < distanceFromDestination || (
                 picture.index < destinationPicture.GetComponent<PictureInfo>().index && Destination.CompareTag( "PicturePlane" ) ) )
            {
                ImportantPictures.Remove( picture );
                LastPositionPattern = Destination;
                Destination = pictureGrid;
                return;
            }
        }
        

        LastPositionPattern = null;
    }

    public virtual void ReceiveLeaderChoice ( GameObject leaderDestination )
    {

        if ( leaderDestination.CompareTag( "PicturePlane" ) || leaderDestination.CompareTag( "Empty Space" ))
        {
            Debug.Log( name + ": ricevuta nuova destinazione del leader (no despota)", leaderDestination );

            if ( !ImportantPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
            {
                ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
            }

        }

    }

    protected void GoToDestinationPoint ()
    {
        GetComponent<RVOAgent>().UpdateTarget( DestinationPoint.transform );
        GetComponent<RVOAgent>().Refresh();
        
        if ( groupData.isLeader )
        {
            groupData.NotifyDestinationChoice();
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

        DestinationPoint = Destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
        DestinationPoint.GetComponent<DestinationPoint>().Occupa();

        if ( Destination.CompareTag( "PicturePlane" ) )
        {
            CurrentPictureIndex = Destination.transform.parent.GetComponent<PictureInfo>().index;
        }
    }
    
    private void CheckDestinationFromPause ()
    {

        if ( DestinationPrePause.TryGetComponent( out GridSystem gridSystem ) && DestinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            Destination = DestinationPrePause;
            UpdateDestinationPoint();
            GoToDestinationPoint();
            InPausa = false;
        }
        else
        {
            TooLongWaitingStrategy();
        }
    }

    protected virtual void TooLongWaitingStrategy ()
    {

        float maxTimeForWaiting_OPERA_MEDIA = 15f;
        float maxTimeForWaiting_OPERA_MAGGIORE = 30f;

        int priorityOfDestinationPrePausa = DestinationPrePause.GetComponentInParent<PictureInfo>().priority;

        // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre). Il tempo è maggiore per i quadri importanti.
        if ( ( timedelta > maxTimeForWaiting_OPERA_MEDIA && priorityOfDestinationPrePausa <= ( int ) PictureInfo.Priority.OPERA_MEDIA )
              || ( timedelta > maxTimeForWaiting_OPERA_MAGGIORE && priorityOfDestinationPrePausa >= ( int ) PictureInfo.Priority.OPERA_MAGGIORE ) )
        {
            PictureInfo pictureDestinationPrePausa = DestinationPrePause.GetComponentInParent<PictureInfo>();
            VisitedPictures.Remove( pictureDestinationPrePausa );
            ImportantIgnoratePicture.Add( pictureDestinationPrePausa );
            InPausa = false;
            Destination = null;
        }
    }


    protected void CheckDestination ()
    {
        bool ignoreDestination = ImportantIgnoratePicture.Contains( Destination.GetComponentInParent<PictureInfo>() );
        bool destinationHaveAvailablePoint = Destination.TryGetComponent( out GridSystem gridSystem ) && Destination.GetComponent<GridSystem>().HaveAvailablePoint();

        if ( destinationHaveAvailablePoint && !ignoreDestination )
        {
            if ( !Destination.CompareTag( "Empty Space" ) )
            {
                PictureInfo pictureDestination = Destination.GetComponentInParent<PictureInfo>();
                VisitedPictures.Add( pictureDestination );
                ImportantPictures.Remove( pictureDestination );
            }
            UpdateDestinationPoint();
            GoToDestinationPoint();

            InPausa = false;
        }
        else
        {
            int destinationPriority = Destination.GetComponentInParent<PictureInfo>().priority;
            bool destinationIsImportantPic = Destination.CompareTag( "PicturePlane" ) && destinationPriority > ( int ) PictureInfo.Priority.OPERA_MEDIA; 

            // "Non sono stanco", oppure "Sono stanco ma il quadro è molto importante"
            if ( !ignoreDestination && ( FatigueLevel == FatigueManager.Level.NON_STANCO || ( FatigueLevel == FatigueManager.Level.STANCO && destinationIsImportantPic ) ) )
            {

                Debug.Log( name + ": Non sono stanco, oppur stanco ma quadro importante" );

                InPausa = true;
                DestinationPrePause = Destination;

                utilitySort.transform = DestinationPrePause.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        Destination = plane;
                        UpdateDestinationPoint();
                        GoToDestinationPoint();
                        break;
                    }
                }

                if( Destination == DestinationPrePause )
                {
                    Debug.Log( name + ": Non ci sono spazi vuoti per attendere" );
                    UpdateDestination();
                }
            }
            else
            {
                Debug.Log( name + ": Richiedo nuova destinazione" );
                UpdateDestination();
            }
        }

    }

    protected bool IsExit ( )
    {
        float exitPointRadius = 5f;

        if ( Destination != null && Destination.gameObject.CompareTag("Uscita") )
        {
            if( Vector3.Distance( transform.position, DestinationPoint.transform.position ) <= exitPointRadius )
            {
                GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().ReceivData( GetType().Name, visitData);
                return true;
            }
        }

        return false;
    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
