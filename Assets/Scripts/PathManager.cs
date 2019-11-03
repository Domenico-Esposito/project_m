using UnityEngine;
using UnityEngine.AI;
using Museum.Utility;
using System.Collections.Generic;
using System.IO;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    protected GameObject destination;
    protected GameObject destinationPoint;
    NavMeshAgent m_Agent;

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

    public float distanzaPercorsa = 0f;

    public abstract GameObject GetNextDestination ();

    public abstract void InitMovementPattern ();

    private GameObject lastPositionPattern;

    protected virtual void Start ()
    {
        utilitySort = new Sort
        {
            transform = transform
        };

        m_Agent = GetComponent<NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        InitNavMeshAgent();

        baseTime = pauseTime;

        InitAnimationBheavior();
        InitMovementPattern();

        foreach ( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            // Importante per l'agente || Importante per l'espositore
            if ( Random.Range(0, 1) == 1 || picture.GetComponent<PictureInfo>().priority > 0 )
            {
                importantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        importantPictures.Sort( utilitySort.SortByIndexPicture );
        importantPictures.Reverse();

        UpdateDestination();
        
    }

    void InitNavMeshAgent ()
    {
        m_Agent.avoidancePriority = Random.Range( 0, 100 );
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
                character.Move( m_Agent.desiredVelocity );
            }
            else
            {
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
            character.Move( m_Agent.desiredVelocity );
        }
        else
        {
            character.Move( Vector3.zero );

            if( destinationPoint.transform.parent.CompareTag( "PicturePlane" ) )   
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

    private void UpdateDestination ()
    {
        // Per il backtracking
        foreach (GameObject picture in importantPictures )
        {
            if ( destination != null 
                && picture.GetComponent<PictureInfo>().priority > 0 
                && picture.GetComponent<PictureInfo>().index < destination.transform.parent.GetComponent<PictureInfo>().index
                && picture.GetComponent<PictureInfo>().ignoro == false
               ) // Quadro importante
            {
                if( !visitedPictures.Contains( picture ) )
                {
                    if( picture.transform.GetChild( 0 ).GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        if ( lastPositionPattern == null )
                        {
                            lastPositionPattern = GetNextDestination();
                        }

                        destination = picture.transform.GetChild( 0 ).gameObject;
                        importantPictures.Remove( picture );
                        visitedPictures.Add( picture );
                        CheckNextDestination();
                        return;
                    }
                }
            }
        }

        // Per il forwardtraking
        if ( lastPositionPattern )
        {
            destination = lastPositionPattern;
            lastPositionPattern = null;
        }
        else
        {
            destination = GetNextDestination();
        }

        if( importantPictures.Contains(destination.transform.parent.gameObject) )
        {
            importantPictures.Remove( destination.transform.parent.gameObject );
        }

        CheckNextDestination();
    }

    protected virtual GameObject GetPointInDestination ()
    {
        return destination.GetComponent<GridSystem>().GetAvailablePoint();
    }

    private void GoToDestinationPoint ()
    {
        m_Agent.SetDestination( destinationPoint.transform.position );
        Debug.Log("Distanza rimanente: " + m_Agent.remainingDistance );

        distanzaPercorsa += (m_Agent.remainingDistance +  m_Agent.stoppingDistance);
    }

    private void UpdateDestinationPoint ()
    {
        if ( destinationPoint != null )
            destinationPoint.GetComponent<DestinationPoint>().Libera();

        destinationPoint = GetPointInDestination();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

    GameObject destinationPrePause;

    private void CheckDestinationFromPause ()
    {

        Debug.Log( "DestinationPrePause", destinationPrePause );
        Debug.Log( "Destination", destination );

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
            if( timedelta > 30f )
            {
                Debug.Log( "Mi scocci di attendere oltre, passo avanti..." );
                visitedPictures.Remove( destinationPrePause );
                importantPictures.Add( destinationPrePause );
                destinationPrePause.GetComponent<PictureInfo>().ignoro = true;
            }

        }
    }

    private void CheckNextDestination ()
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
            // Euristica "Vado in pausa" oppure passo oltre
            if (true || Random.Range(0, 1) == 1 )
            {

                inPausa = true;
                destinationPrePause = destination;

                List<GameObject> fishPlane = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Fish Floor" ) );
                utilitySort.transform = transform;
                fishPlane.Sort( utilitySort.Distanza );
                Debug.Log( "Scelgo di attendere in un posto vuoto", fishPlane[ 0 ] );
                destination = fishPlane[ 0 ];

                UpdateDestinationPoint();
                GoToDestinationPoint();
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
            //string importantiNonVisitati = "Importanti non visitati: ";

            //foreach ( GameObject p in importantPictures )
            //{
            //    importantiNonVisitati += p.GetComponent<PictureInfo>().index + ", ";
            //}

            //importantiNonVisitati = importantiNonVisitati.Substring( 0, importantiNonVisitati.Length - 2 );

            //Debug.Log( importantiNonVisitati );

            //string visitati = "Visitati: ";

            //foreach (GameObject i in visitedPictures)
            //{
            //    visitati += i.GetComponent<PictureInfo>().index + ", ";
            //}

            //visitati = visitati.Substring( 0, visitati.Length - 2 );

            //Debug.Log( visitati );

            return true;
        }

        return false;
    }


    private void WriteLog ()
    {
        //string filePath = "logs.txt";

        //StreamWriter writer = new StreamWriter( filePath, true );

        //string quadriVisitati = "Visitati:";

        //foreach ( GameObject p in visitedPictures ) {
        //    quadriVisitati = quadriVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        //}

        //quadriVisitati = quadriVisitati.Substring( 0, quadriVisitati.Length - 2 );

        //writer.WriteLine( quadriVisitati);

        //string quadriImportantiNonVisitati = "Importanti non visitati:";

        //foreach ( GameObject p in importantPictures )
        //{
        //    quadriImportantiNonVisitati = quadriImportantiNonVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        //}

        //quadriImportantiNonVisitati = quadriImportantiNonVisitati.Substring( 0, quadriImportantiNonVisitati.Length - 2 );

        //writer.WriteLine( GetType() );
        //writer.WriteLine( quadriVisitati );
        //writer.WriteLine( quadriImportantiNonVisitati );
        //writer.WriteLine( "---" );
        //writer.Close();

        //Debug.Log( GetType() );


    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
