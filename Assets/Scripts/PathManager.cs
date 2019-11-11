using UnityEngine;
using UnityEngine.AI;
using Museum.Utility;
using System.Collections.Generic;
using System.IO;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    public GameObject destination;
    protected GameObject destinationPoint;
    protected NavMeshAgent m_Agent;

    protected Sort utilitySort;

    public bool inPausa = false;
    public bool isHasty = true;

    Rigidbody m_Rigidbody;
    Animator m_Animator;

    // Animazione
    CharacterAnimator character;

    // Segui percorso
    public float timedelta = 0f;
    float baseTime;
    [SerializeField] float pauseTime = 5f;

    protected Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();

    public List<GameObject> visitedPictures = new List<GameObject>();
    public List<GameObject> importantPictures = new List<GameObject>();

    public int maxDistanza = 30;
    public float distanzaPercorsa = 0f;
    public int currentPictureIndex = 0;
    public List<GameObject> fishPlane;
    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();

    public GameObject lastPositionPattern;
    int maxIndexPictures = 0;

    public bool isCapoGruppo;
    public List<PathManager> groupElement = new List<PathManager>();
    public GameObject capo;

    protected virtual void Start ()
    {

        utilitySort = new Sort
        {
            transform = transform
        };

        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        maxIndexPictures = new List<GameObject>( GameObject.FindGameObjectsWithTag( "PicturePlane" ) ).Capacity;
        fishPlane = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Fish Floor" ) );


        InitNavMeshAgent();

        baseTime = pauseTime;

        InitAnimationBheavior();
        InitMovementPattern();

        foreach ( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            // "Opere medio/grandi per l'espositore", oppure, "Opere di interesse per l'agent"
            if ( picture.GetComponent<PictureInfo>().priority >= 1 || Random.Range(0, 2) > 1 )
            {
                importantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        importantPictures.Sort( utilitySort.SortByIndexPicture );
        importantPictures.Reverse();

        if ( isCapoGruppo )
        {
            foreach ( PathManager element in groupElement )
            {
                Debug.Log( "Rimuovo tutti i preferiti degli agenti nel gruppo", element );
                element.importantPictures.Clear();
                element.impostaCapo( gameObject );
            }
        }

        UpdateDestination();
        
    }

    void impostaCapo( GameObject leader )
    {
        capo = leader;
        Debug.Log( "Agente " + gameObject.name + ": Leader", capo );
    }

    void InitNavMeshAgent ()
    {
        //isHasty = Random.Range( 0, 5 ) > 2;
        m_Agent.updateRotation = false;
    }

    public void InitAnimationBheavior ()
    {
        character = GetComponent<CharacterAnimator>();
    }

    private void Update ()
    {

        if( inPausa )
        {
            if ( m_Agent.remainingDistance > m_Agent.stoppingDistance )
            {
                m_Agent.avoidancePriority = Random.Range(50, 60);
                character.Move( m_Agent.desiredVelocity );
            }
            else
            {
                m_Agent.avoidancePriority = 0;
                character.Move( Vector3.zero );
                timedelta += Time.deltaTime;
            }

            CheckDestinationFromPause();
            return;
        }


        if ( timedelta > pauseTime)
        {
            UpdateDestination();
            timedelta = 0f;
        }


        if ( m_Agent.remainingDistance > m_Agent.stoppingDistance )
        {
            m_Agent.avoidancePriority = Random.Range( 50, 60 );
            character.Move( m_Agent.desiredVelocity );
        }
        else
        {
            m_Agent.avoidancePriority = 0;
            character.Move( Vector3.zero );

            if ( destinationPoint.transform.parent.CompareTag( "PicturePlane" ) )   
            {
                Vector3 position = destination.transform.parent.transform.position;
                character.TurnToPicture( position );
            }

            timedelta += Time.deltaTime;

        }

        if ( IsExit() )
        {
            Destroy( gameObject );
        }

    }

    protected virtual void UpdateDestination () 
    {

        bool controllo = false;

        if ( lastPositionPattern == null )
        {
            destination = GetNextDestination();
            Debug.Log( "Chiedo nuova destinazione dal pattern", destination );
        }
        else
        {
            destination = lastPositionPattern;
            Debug.Log( "Uso destinazione già calcolata in precedenza", destination );
        }

        NavMeshPath staticPath = new NavMeshPath();
        m_Agent.CalculatePath( destination.transform.position, staticPath );

        float distanzaProssimaDestinazione = GetPathLenght( staticPath );

        foreach ( GameObject picture in importantPictures )
        {
            if ( destination.transform.parent.CompareTag( "Picture" ) && picture.GetComponent<PictureInfo>().index > destination.transform.parent.GetComponent<PictureInfo>().index + 5)
            {
                continue;
            }

            m_Agent.CalculatePath( picture.transform.GetChild( 0 ).gameObject.transform.position, staticPath );

            float distanzaPictureImportant = GetPathLenght( staticPath );

            // Immagine importante più vicina di immagine pattern
            if ( distanzaPictureImportant < distanzaProssimaDestinazione || 
            ( destination.transform.parent.CompareTag( "Picture" ) && picture.GetComponent<PictureInfo>().index < destination.transform.parent.GetComponent<PictureInfo>().index) )
            {
                importantPictures.Remove( picture );
                //visitedPictures.Add( picture );
                lastPositionPattern = destination;
                controllo = true;
                destination = picture.transform.GetChild( 0 ).gameObject;
                Debug.Log( "Questa destinazione viene salvata per dopo", lastPositionPattern );
                Debug.Log( "Prossima destinazione è importante", destination );
                break;
            }
        }

        if ( controllo == false )
        {
            lastPositionPattern = null;
        }

        if ( importantPictures.Contains( destination.transform.parent.gameObject ) )
        {
            importantPictures.Remove( destination.transform.parent.gameObject );
        }

        CheckNextDestination();

        //destination = GetNextDestination();
        //Debug.Log( "Destinazione del pattern", destination );

        //// Per il backtracking
        //foreach (GameObject picture in importantPictures )
        //{
        //    if ( destination != null && destination.CompareTag("PicturePlane")
        //        //&& picture.GetComponent<PictureInfo>().priority > 0 
        //        && (picture.GetComponent<PictureInfo>().index < destination.transform.parent.GetComponent<PictureInfo>().index || picture.GetComponent<PictureInfo>().index >= maxIndexPictures )
        //        && picture.GetComponent<PictureInfo>().ignoro == false
        //       ) // Quadro importante
        //    {
        //        if( !visitedPictures.Contains( picture ) )
        //        {
        //            if( picture.transform.GetChild( 0 ).GetComponent<GridSystem>().HaveAvailablePoint() )
        //            {
        //                if ( lastPositionPattern == null )
        //                {
        //                    lastPositionPattern = destination;
        //                }

        //                destination = picture.transform.GetChild( 0 ).gameObject;
        //                //if( destination.GetComponentInParent<PictureInfo>() )
        //                //{
        //                //    currentPictureIndex = picture.GetComponent<PictureInfo>().index;
        //                //}

        //                importantPictures.Remove( picture );
        //                //visitedPictures.Add( picture );
        //                CheckNextDestination();
        //                return;
        //            }
        //        }
        //    }
        //}

        //// Per il forwardtraking
        //if ( lastPositionPattern )
        //{
        //    GameObject tmp = destination;
        //    destination = lastPositionPattern;
        //    //if( destination.GetComponentInParent<PictureInfo>() )
        //    //{
        //    //    currentPictureIndex = destination.GetComponentInParent<PictureInfo>().index;
        //    //}
        //    lastPositionPattern = null;
        //}

        //if ( importantPictures.Contains(destination.transform.parent.gameObject) )
        //{
        //    importantPictures.Remove( destination.transform.parent.gameObject );
        //}

        //CheckNextDestination();
    }

    protected virtual GameObject GetPointInDestination ()
    {
        return destination.GetComponent<GridSystem>().GetAvailablePoint();
    }

    public void capoSceglie(GameObject destination )
    {
        if( destination.CompareTag( "PicturePlane" ) )
        {
            Debug.Log( gameObject.name + ": Capo ha scelto nuova destinazione importante", destination );
            if( !importantPictures.Contains( destination.transform.parent.gameObject ) && !visitedPictures.Contains( destination.transform.parent.gameObject ))
            {
                importantPictures.Add( destination.transform.parent.gameObject );
            }
        }
    }

    private void GoToDestinationPoint ()
    {
        m_Agent.SetDestination( destinationPoint.transform.position );

        if ( isCapoGruppo )
        {
            foreach ( PathManager element in groupElement )
            {
                element.capoSceglie( destination );
            }
        }

        NavMeshPath staticPath = new NavMeshPath();
        m_Agent.CalculatePath(destinationPoint.transform.position, staticPath);

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

    private void UpdateDestinationPoint ()
    {
        if ( destinationPoint != null )
            destinationPoint.GetComponent<DestinationPoint>().Libera();

        destinationPoint = GetPointInDestination();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

    public GameObject destinationPrePause;

    private void CheckDestinationFromPause ()
    {
    
        if ( destinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            destination = destinationPrePause;
            //if( destination.GetComponentInParent<PictureInfo>() )
            //{
            //    currentPictureIndex = destination.GetComponentInParent<PictureInfo>().index;
            //}

            visitedPictures.Add( destination.transform.parent.gameObject );
            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
            // Controllo tempo di attesa (l'agent si è scocciato di attendere e passa oltre)
            if( ( timedelta > 30f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority <= 1) || ( timedelta > 60f && destinationPrePause.transform.parent.GetComponent<PictureInfo>().priority > 1 ) )
            {
                Debug.Log( "È passato troppo tempo, passo oltre e ignoro questo quadro..." );
                visitedPictures.Remove( destinationPrePause.transform.parent.gameObject );
                importantPictures.Add( destinationPrePause.transform.parent.gameObject );
                destinationPrePause.GetComponentInParent<PictureInfo>().ignoro = true;
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
    private int LivelloStanchezza ()
    {

        if( distanzaPercorsa > (maxDistanza / 1.2f ) )
        {
            //Debug.Log( "Livello stanchezza: Stanco" );
            return 1;
        }

        if( distanzaPercorsa > maxDistanza )
        {
            //Debug.Log( "Livello stanchezza: Molto stanco" );
            return 2;
        }

        //Debug.Log( "Livello stanchezza: Non stanco" );
        return 0;
    }

    protected void CheckNextDestination ()
    {

        if ( destination.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            visitedPictures.Add( destination.transform.parent.gameObject );

            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
            // "Non sono stanco", oppure "Sono stanco ma il quadro è molto importante"
            if ( LivelloStanchezza() == 0 || (LivelloStanchezza() == 1 && destination.CompareTag("PicturePlane") && destination.transform.parent.GetComponent<PictureInfo>().priority > 1 ))
            {

                inPausa = true;
                destinationPrePause = destination;
                
                utilitySort.transform = destinationPrePause.transform;
                fishPlane.Sort( utilitySort.DistanzaPlane );

                Debug.Log( gameObject.name + ": Destinazione: ", destination );
                Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", fishPlane[ 0 ] );

                foreach( GameObject plane in fishPlane )
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
        if (destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, destinationPoint.transform.position) < 3f)
        {
            return true;
        }

        return false;
    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
