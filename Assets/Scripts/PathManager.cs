﻿using UnityEngine;
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
    public GameObject destination;
    public GameObject destinationPoint;

    protected Sort utilitySort;

    public bool inPausa = false;

    // Segui percorso
    [SerializeField] 
    protected float pauseTime = 5f;

    public float timedelta = 0f;
    float baseTime;

    protected Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();

    protected List<GameObject> visitedPictures = new List<GameObject>();
    protected List<GameObject> importantPictures = new List<GameObject>();
    protected List<GameObject> importantIgnoratePicture = new List<GameObject>();

    private BotVisitData visitData;
    private MarkerManager markerManager;

    public int maxDistanza = 30;
    public float distanzaPercorsa = 0f;

    public int currentPictureIndex = 0;

    public List<GameObject> emptySpaces;

    public GameObject lastPositionPattern;
    public GameObject destinationPrePause;

    public bool despota = false;
    public bool noChoices = false;
    public bool isLeader = false;
    protected GameObject leader;
    public List<PathManager> group = new List<PathManager>();

    public float tempoInAttesa = 0f;
    public float durataVisita = 0f;

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

        visitedPictures = visitData.visitedPictures;
        importantPictures = visitData.importantPictures;
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


    protected virtual void GroupElementSetData (GameObject d, Color groupColor, bool leaderDespota)
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
                member.GroupElementSetData( destination, groupColor, despota);
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
        foreach ( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            // "Opere medio/grandi per l'espositore", oppure, "Opere di interesse per l'agent"
            if ( picture.GetComponent<PictureInfo>().priority > 1 || UnityEngine.Random.Range( 0, 2 ) > 1 )
            {
                importantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        importantPictures.Sort( utilitySort.SortByIndexPicture );
        importantPictures.Reverse();

    }

    protected virtual void Behaviour ()
    {
        durataVisita += Time.deltaTime;

        if ( inPausa )
        {

            timedelta += Time.deltaTime;

            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
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
                destinationPoint.GetComponent<DestinationPoint>().Libera();
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

        durataVisita += Time.deltaTime;

        if ( inPausa )
        {
            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
            }

            if( destinationPrePause == null || visitedPictures.Contains( destinationPrePause ) )
            {
                utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        destination = plane;
                        break;
                    }
                }
            }
            else
            {
                if ( destinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    destination = destinationPrePause;
                    visitedPictures.Add( destination.transform.parent.gameObject );
                    UpdateDestinationPoint();
                    GoToDestinationPoint();

                    //Debug.Log( name + ": la destinazione si è liberata", destination );

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

        if ( importantPictures.Count > 0 || timedelta > pauseTime)
        {
            //destinationPrePause = null;
            UpdateDestination();
            timedelta = 0f;
        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                destinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }

    }
    
    private void NormaleBot ()
    {
        durataVisita += Time.deltaTime;

        if ( inPausa )
        {

            timedelta += Time.deltaTime;

            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
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
                destinationPoint.GetComponent<DestinationPoint>().Libera();
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
        //        destination = importantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
        //        if ( importantPictures.Contains( destination.transform.parent.gameObject ) )
        //        {
        //            importantPictures.Remove( destination.transform.parent.gameObject );
        //        }

        //        //CheckNextDestination();
        //    }
        //    else
        //    {
        //        inPausa = true;

        //        utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
        //        emptySpaces.Sort( utilitySort.DistanzaPlane );

        //        //Debug.Log( gameObject.name + ": Destinazione: ", destination );
        //        //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

        //        foreach ( GameObject plane in emptySpaces )
        //        {
        //            if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
        //            {
        //                destination = plane;
        //                //UpdateDestinationPoint();
        //                //GoToDestinationPoint();
        //                break;
        //            }
        //        }
        //    }

        //    //CheckNextDestination();
        //    return;
        //}

        if ( lastPositionPattern == null )
        {
            destination = GetNextDestination();
            //Debug.Log( gameObject.name + ": Chiedo nuova destinazione dal pattern", destination );
        }
        else
        {
            destination = lastPositionPattern;

            if ( destination.CompareTag( "Picture" ) )
            {
                // Qui magari posso utilizzare la distanza dal quadro, invece che l'indice (sarebbe meglio)
                if( destination.GetComponent<PictureInfo>().index < currentPictureIndex - 5 )
                {
                    //Debug.Log( name + ": La destinazione già calcolata è un quadro con indice troppo basso per essere visitato ora." );
                    lastPositionPattern = null;
                    UseLastDestinationOrNew();
                }
            }

            //Debug.Log( gameObject.name + ": Uso destinazione già calcolata in precedenza", destination );

        }
    }

    protected void UpdateDestination () 
    {
        bool haveLastPositionPattern = false;
        float distanceFromDestination = 0;

        UseLastDestinationOrNew();

        NavMeshPath staticPath = new NavMeshPath();
        NavMesh.CalculatePath( transform.position, destination.transform.position, NavMesh.AllAreas, staticPath );

        distanceFromDestination = GetPathLenght( staticPath );
        Transform destinationPicture;

        destinationPicture = destination.transform.parent;

        utilitySort.transform = this.transform;
        importantPictures.Sort( utilitySort.Distanza );

        try
        {
            foreach ( GameObject picture in importantPictures )
            {
                GameObject picturePlane = picture.transform.GetChild( 0 ).gameObject;


                NavMesh.CalculatePath( transform.position, picturePlane.transform.position, NavMesh.AllAreas, staticPath );

                float distanzaFromPictureImportant = GetPathLenght( staticPath );

                // Immagine importante più vicina di immagine pattern
                if ( distanzaFromPictureImportant < distanceFromDestination || (
                     picture.GetComponent<PictureInfo>().index < destinationPicture.GetComponent<PictureInfo>().index && destination.CompareTag( "PicturePlane" ) ) )
                {
                    importantPictures.Remove( picture );
                    lastPositionPattern = destination;
                    haveLastPositionPattern = true;
                    destination = picturePlane;

                    //Debug.Log( "Questa destinazione viene salvata per dopo", lastPositionPattern );
                    //Debug.Log( "Prossima destinazione è importante", destination );
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
            lastPositionPattern = null;
        }

        if ( importantPictures.Contains( destination.transform.parent.gameObject ) )
        {
            importantPictures.Remove( destination.transform.parent.gameObject );
        }
        
        CheckNextDestination();

    }

    protected virtual GameObject GetPointInDestination ()
    {
        return destination.GetComponent<GridSystem>().GetAvailableRandomPoint();
    }

    //private void UpdateDestinationPointForNoChoiceExit(){
    //    StartCoroutine( LiberaPosto(destinationPoint) );
    //    destinationPoint = destination.GetComponent<GridSystem>().GetRandomPoint();
    //    destinationPoint.GetComponent<DestinationPoint>().Occupa();
    //}

    protected virtual void NotifyNewDestination(GameObject leaderDestination )
    {
        //if( noChoices && leaderDestination.CompareTag( "Uscita" ) )
        //{
        //    Debug.Log(name + ": impostata destinazione come uscita", leaderDestination);
        //    destination = leaderDestination;

        //    if( destinationPrePause != null)
        //    {
        //        inPausa = false;
        //        importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
        //        importantPictures.Clear();
        //        destinationPrePause = null;
        //    }

        //    UpdateDestinationPointForNoChoiceExit();
        //    GoToDestinationPoint();
        //    return;
        //}

        if ( leaderDestination.CompareTag( "PicturePlane" ) || leaderDestination.CompareTag( "Empty Space" ))
        {
            //Debug.Log( gameObject.name + ": Capo ha scelto nuova destinazione importante", leaderDestination );

            if( /* !noChoices && */ !importantPictures.Contains( leaderDestination.transform.parent.gameObject ) && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ))
            {
                importantPictures.Add( leaderDestination.transform.parent.gameObject );
            }

            //if ( noChoices && leaderDestination.CompareTag( "Empty Space" ) && !visitedPictures.Contains( leaderDestination ) )
            //{
            //    if( destinationPrePause )
            //    {
            //        importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
            //    }
                
            //    importantPictures.Add( leaderDestination );
            //}
            //else if ( noChoices && leaderDestination.CompareTag( "PicturePlane" ) && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ) )
            //{
            //    if( destinationPrePause )
            //    {
            //        importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
            //    }
            //    importantPictures.Add( leaderDestination.transform.parent.gameObject );
            //}

        }

    }


    protected void GoToDestinationPoint ()
    {
        GetComponent<RVOAgent>().UpdateTarget( destinationPoint.transform );
        GetComponent<RVOAgent>().Refresh();

        utilitySort.transform = transform;
        group.Sort( ( PathManager x, PathManager y ) => UnityEngine.Random.Range( -1, 1 ) );

        if ( isLeader )
        {
            foreach ( PathManager member in group )
            {
                try
                {
                    member.NotifyNewDestination( destination );
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
        NavMesh.CalculatePath( transform.position, destinationPoint.transform.position, NavMesh.AllAreas, staticPath );

        distanzaPercorsa += GetPathLenght( staticPath );
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
        StartCoroutine( LiberaPosto(destinationPoint) );

        destinationPoint = GetPointInDestination();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        if ( destination.CompareTag( "PicturePlane" ) )
        {
            currentPictureIndex = destination.transform.parent.GetComponent<PictureInfo>().index;
        }
    }
    
    protected void CheckDestinationFromPause ()
    {
    
        if ( destinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            destination = destinationPrePause;
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
        
            // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre)
            if ( timedelta > 15f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority <= 1 || timedelta > 20f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority > 1 )
            {
                //Debug.Log( "È passato troppo tempo, passo oltre e ignoro questo quadro..." );
                visitedPictures.Remove( destinationPrePause.transform.parent.gameObject );
                //importantPictures.Add( destinationPrePause.transform.parent.gameObject );
                importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
                inPausa = false;
                destination = null;
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
    
        if( distanzaPercorsa > maxDistanza )
        {
            //Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return MOLTO_STANCO;
        }

        if ( distanzaPercorsa > ( maxDistanza / 1.2f ) )
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
        if ( destination.GetComponent<GridSystem>().HaveAvailablePoint() && !importantIgnoratePicture.Contains(destination) )
        {
            if ( !destination.CompareTag( "Empty Space" ) || !noChoices)
            {
                visitedPictures.Add( destination.transform.parent.gameObject );
            }
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
            // "Non sono stanco", oppure "Sono stanco ma il quadro è molto importante"
            if ( !importantIgnoratePicture.Contains( destination ) && ( LivelloStanchezza() == 0 || 
                 ( LivelloStanchezza() == 1 && destination.CompareTag("PicturePlane") && destination.transform.parent.GetComponent<PictureInfo>().priority > 1 ) ))
            {

                inPausa = true;
                destinationPrePause = destination;
                
                utilitySort.transform = destinationPrePause.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach( GameObject plane in emptySpaces )
                {
                    if( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        destination = plane;
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
        if ( destination != null && destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, destinationPoint.transform.position) <= 5f)
        {
            GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().ReceivData( this.GetType().Name, visitedPictures, importantPictures, importantIgnoratePicture, durataVisita, tempoInAttesa, distanzaPercorsa );
            return true;
        }

        return false;
    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
