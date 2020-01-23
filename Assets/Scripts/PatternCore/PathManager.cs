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

    protected int FatigueStatus
    {
        get => fatigueManager.LivelloStanchezza();
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

    public int maxDistanza = 30;

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

    public abstract GameObject GetNextDestination ();
    public abstract void InitMovementPattern ();

    private bool firstChoices = true;
    protected bool okTimer = false;

    public bool activeBot = false;

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

    IEnumerator ClockManager ()
    {
        while ( true )
        {
            DurataVisita += 1;

            if ( InPausa )
            {
                timedelta += 1;

                if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
                {
                    TempoInAttesa += 1;
                }
            }
            else
            {
                if( timedelta > pauseTime )
                {
                    okTimer = true;
                    timedelta = 0;
                }

                if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
                {
                    timedelta += 1;
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

        if ( okTimer || firstChoices )
        {
            firstChoices = false;
            okTimer = false;
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
                // Qui magari posso utilizzare la distanza dal quadro, invece che l'indice (sarebbe meglio)
                if( Destination.GetComponent<PictureInfo>().index < CurrentPictureIndex - 5 )
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

                    break;
                }
            }
        }
        catch( NullReferenceException e )
        {
            Debug.Log("Errore UpdateDestination: " + e);
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

    public virtual void SendLeaderChoices(GameObject leaderDestination )
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

    private void NotifyDestinationChoice ()
    {
        groupData.CheckMembers();

        foreach ( GroupData member in groupData.group )
        {
            member.GetComponent<PathManager>().SendLeaderChoices( Destination );
            if ( groupData.despota )
            {
                member.GetComponent<PathManager>().activeBot = true;
            }
        }
    }

    protected void GoToDestinationPoint ()
    {
        GetComponent<RVOAgent>().UpdateTarget( DestinationPoint.transform );
        GetComponent<RVOAgent>().Refresh();
        
        if ( groupData.isLeader )
        {
            NotifyDestinationChoice();
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
        // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre). Il tempo è maggiore per i quadri importanti.
        if ( timedelta > 15f && DestinationPrePause.GetComponentInParent<PictureInfo>().priority <= 1 || timedelta > 20f && DestinationPrePause.transform.parent.GetComponent<PictureInfo>().priority > 1 )
        {
            VisitedPictures.Remove( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            InPausa = false;
            Destination = null;
        }
    }

    protected void CheckNextDestination ()
    {
        // Il quadro ha posti disponibili e non è tra quelli da ignorare
        if ( Destination.TryGetComponent(out GridSystem gridSystem) && Destination.GetComponent<GridSystem>().HaveAvailablePoint() && !ImportantIgnoratePicture.Contains(Destination.GetComponentInParent<PictureInfo>() ) )
        {
            if ( !Destination.CompareTag( "Empty Space" ) )
            {
                VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
            }
            UpdateDestinationPoint();
            GoToDestinationPoint();

            InPausa = false;
        }
        else
        {
            // "Non sono stanco", oppure "Sono stanco ma il quadro è molto importante"
            if ( !ImportantIgnoratePicture.Contains( Destination.GetComponentInParent<PictureInfo>() ) && 
                ( FatigueStatus == 0 || ( FatigueStatus == 1 && Destination.CompareTag("PicturePlane") && Destination.GetComponentInParent<PictureInfo>().priority > 1 )) )
            {

                InPausa = true;
                DestinationPrePause = Destination;
                
                utilitySort.transform = DestinationPrePause.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );
                
                foreach( GameObject plane in emptySpaces )
                {
                    if( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        //Debug.Log( name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", plane);
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
