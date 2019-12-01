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
    public GameObject destination;
    protected GameObject destinationPoint;

    protected Sort utilitySort;

    public bool inPausa = false;

    // Segui percorso
    [SerializeField] float pauseTime = 5f;
    public float timedelta = 0f;
    float baseTime;

    protected Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();
    public List<GameObject> visitedPictures = new List<GameObject>();
    public List<GameObject> importantPictures = new List<GameObject>();

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

    [SerializeField]
    GameObject[ ] status;

    public List<GameObject> importantIgnoratePicture = new List<GameObject>();

    public const int NON_STANCO = 0;
    public const int STANCO = 1;
    public const int MOLTO_STANCO = 2;

    protected void SetColorGroup(Color color )
    {
        transform.Find( "Bottom" ).gameObject.transform.Find( "Base" ).gameObject.GetComponent<Image>().color = color;
        transform.Find( "Bottom" ).gameObject.transform.Find( "Base Leader" ).gameObject.GetComponent<Image>().color = color;
    }

    protected virtual void Start ()
    {
    
        utilitySort = new Sort
        {
            transform = transform
        };

        baseTime = pauseTime;

        emptySpaces = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );

        InitNavMeshAgent();
        
        InitAnimationBheavior();
        InitMovementPattern();

        SelectImportantPictureStrategy();

        InitGroupData();

        UpdateDestination();

        foreach ( PathManager member in group )
        {
            member.NotifyNewDestination( destination );
        }

    }

    void GroupElementSetData (GameObject d, Color groupColor, bool leaderDespota)
    {
        if( leaderDespota )
        {
            noChoices = true;
        }

        SetLeader( gameObject );
        SetColorGroup( groupColor );

        importantPictures.Clear();
        visitedPictures.Clear();

        //StartCoroutine( checkDestination() );
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
            SetColorGroup( groupColor );

            transform.Find( "Bottom" ).gameObject.transform.Find( "Base Leader" ).gameObject.SetActive( true );

            foreach ( PathManager member in group )
            {
                member.GroupElementSetData( destination, groupColor, despota);
            }
        }
        else
        {
            transform.Find( "Bottom" ).gameObject.transform.Find( "Base" ).gameObject.SetActive( true );
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

    void InitNavMeshAgent ()
    {
        //agent.updateRotation = false;
    }

    public void InitAnimationBheavior ()
    {

    }

    //IEnumerator checkDestination ()
    //{
    //    yield return new WaitForSeconds( 1f );

    //    if ( importantPictures.Count > 0 )
    //    {
    //        destinationPrePause = null;
    //        UpdateDestination();
    //        timedelta = 0f;
    //    }

    //    yield return checkDestination();
    //}

    private void NoChoicesBot ()
    {
        durataVisita += Time.deltaTime;

        if ( inPausa )
        {
            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 0 ].SetActive( true );

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
            }

            if( destinationPrePause == null || visitedPictures.Contains( destinationPrePause ) )
            {
                utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                Debug.Log( gameObject.name + ": Destinazione: ", destination );
                Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

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

                    Debug.Log( name + ": la destinazione si è liberata", destination );

                    inPausa = false;
                }
            }

            return;
        }

        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 2 ].SetActive( true );
            timedelta += Time.deltaTime;

        }
        else
        {
            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 1 ].SetActive( true );

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

            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 0 ].SetActive( true );

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
            }

            CheckDestinationFromPause();
            return;
        }

        if ( timedelta > pauseTime )
        {
            UpdateDestination();
            timedelta = 0f;
        }


        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 2 ].SetActive( true );
            timedelta += Time.deltaTime;

        }
        else
        {
            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 1 ].SetActive( true );

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

        if ( noChoices )
        {
            NoChoicesBot();
        }
        else
        {
            NormaleBot();
        }
    }

    private void UseLastDestinationOrNew ()
    {

        if( noChoices )
        {
            if( importantPictures.Count > 0 )
            {
                destination = importantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
                if ( importantPictures.Contains( destination.transform.parent.gameObject ) )
                {
                    importantPictures.Remove( destination.transform.parent.gameObject );
                }

                Debug.Log( name + ": noChoices ha scelto", destination );

                //CheckNextDestination();
            }
            else
            {
                inPausa = true;

                utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                Debug.Log( gameObject.name + ": Destinazione: ", destination );
                Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        destination = plane;
                        //UpdateDestinationPoint();
                        //GoToDestinationPoint();
                        break;
                    }
                }
            }

            CheckNextDestination();
            return;
        }

        if ( lastPositionPattern == null )
        {
            destination = GetNextDestination();
            Debug.Log( gameObject.name + ": Chiedo nuova destinazione dal pattern", destination );
        }
        else
        {
            destination = lastPositionPattern;

            if ( destination.CompareTag( "Picture" ) )
            {
                // Qui magari posso utilizzare la distanza dal quadro, invece che l'indice (sarebbe meglio)
                if( destination.GetComponent<PictureInfo>().index < currentPictureIndex - 5 )
                {
                    Debug.Log( name + ": La destinazione già calcolata è un quadro con indice troppo basso per essere visitato ora." );
                    lastPositionPattern = null;
                    UseLastDestinationOrNew();
                }
            }

            Debug.Log( gameObject.name + ": Uso destinazione già calcolata in precedenza", destination );

        }
    }


    private void UpdateDestination () 
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

                    //Debug.Log( "distanzaFromPictureImportant (" + distanzaFromPictureImportant + ") < distanceFromDestination (" + distanceFromDestination + ")" );
                    Debug.Log( "Questa destinazione viene salvata per dopo", lastPositionPattern );
                    Debug.Log( "Prossima destinazione è importante", destination );
                    break;
                }
            }
        }
        catch( NullReferenceException e )
        {
            Debug.Log( name + ": nessuna destinazione" );
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

    public void NotifyNewDestination(GameObject leaderDestination )
    {
        if( noChoices && leaderDestination.CompareTag( "Uscita" ) )
        {
            destination = leaderDestination;
            UpdateDestinationPoint();
            GoToDestinationPoint();
        }

        if ( leaderDestination.CompareTag( "PicturePlane" ) || leaderDestination.CompareTag( "Empty Space" ))
        {
            Debug.Log( gameObject.name + ": Capo ha scelto nuova destinazione importante", leaderDestination );
            if( !noChoices && !importantPictures.Contains( leaderDestination.transform.parent.gameObject ) && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ))
            {
                importantPictures.Add( leaderDestination.transform.parent.gameObject );
            }

            if ( noChoices && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ) )
            {
                importantPictures.Add( leaderDestination.transform.parent.gameObject );
            }
        }
    }


    private void GoToDestinationPoint ()
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
                }
                catch( MissingReferenceException e )
                {
                    Debug.Log( gameObject.name + ": un membro del gruppo ha già abbandonato il museo." );
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

    IEnumerator LiberaPosto (GameObject oldDestination)
    {
        yield return new WaitForSeconds( 2f );

        if ( oldDestination != null )
            oldDestination.GetComponent<DestinationPoint>().Libera();

    }

    private void UpdateDestinationPoint ()
    {
        StartCoroutine( LiberaPosto(destinationPoint) );

        destinationPoint = GetPointInDestination();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();

        if ( destination.CompareTag( "PicturePlane" ) )
        {
            currentPictureIndex = destination.transform.parent.GetComponent<PictureInfo>().index;
        }
    }
    
    private void CheckDestinationFromPause ()
    {
    
        if ( destinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            destination = destinationPrePause;
            visitedPictures.Add( destination.transform.parent.gameObject );
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
        
            // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre)
            if ( timedelta > 15f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority <= 1 || timedelta > 20f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority > 1 )
            {
                Debug.Log( "È passato troppo tempo, passo oltre e ignoro questo quadro..." );
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
            Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return MOLTO_STANCO;
        }

        if ( distanzaPercorsa > ( maxDistanza / 1.2f ) )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return STANCO;
        }

        Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return NON_STANCO;
    }

    protected void CheckNextDestination ()
    {

        //// Posti disponibili e non è tra quelle da ignorare
        if ( destination.GetComponent<GridSystem>().HaveAvailablePoint() && !importantIgnoratePicture.Contains(destination) )
        {
            visitedPictures.Add( destination.transform.parent.gameObject );

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

                Debug.Log( gameObject.name + ": Destinazione: ", destination );
                Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

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
