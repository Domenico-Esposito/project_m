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
    float timedelta = 0f;
    float baseTime;
    [SerializeField] float pauseTime = 5f;

    protected Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();

    protected List<GameObject> visitedPictures = new List<GameObject>();
    public List<GameObject> importantPictures = new List<GameObject>();

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
            CheckNextDestination();
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

        IsExit();

    }

    private void UpdateDestination ()
    {
        // Per il backtracking
        foreach (GameObject picture in importantPictures )
        {
            if ( destination != null 
                && picture.GetComponent<PictureInfo>().priority > 0 
                && picture.GetComponent<PictureInfo>().index < destination.transform.parent.GetComponent<PictureInfo>().index
               ) // Quadro importante
            {
                if( !visitedPictures.Contains( picture ) && 
                    picture.transform.GetChild( 0 ).GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    if ( lastPositionPattern == null )
                    {
                        lastPositionPattern = GetNextDestination();
                    }

                    destination = picture.transform.GetChild( 0 ).gameObject;
                    importantPictures.Remove( picture );
                    visitedPictures.Add( destination.transform.parent.gameObject );
                    CheckNextDestination();
                    return;
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
    }

    private void UpdateDestinationPoint ()
    {
        if ( destinationPoint != null )
            destinationPoint.GetComponent<DestinationPoint>().Libera();

        destinationPoint = GetPointInDestination();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

    private void CheckNextDestination ()
    {
        if( destination.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            visitedPictures.Add( destination.transform.parent.gameObject );

            UpdateDestinationPoint();
            GoToDestinationPoint();

            inPausa = false;
        }
        else
        {
        
            // Qui euristica di scelta prossima destinazione
            if ( isHasty )
            {
                inPausa = true;
            }
            else
            {
                UpdateDestination();
            }
        }
    }


    

    protected bool IsExit ( )
    {
        if( destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, destinationPoint.transform.position) < 3f)
        {
            //WriteLog();

            Debug.Log( GetStringVisitati() );
            Debug.Log( GetStringNonVisitati() );

            Destroy( gameObject );
            return true;
        }

        return false;
    }


    private string GetStringNonVisitati ()
    {

        string quadriImportantiNonVisitati = "Importanti non visitati:";

        foreach ( GameObject p in importantPictures )
        {
            quadriImportantiNonVisitati = quadriImportantiNonVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        }
        
        return quadriImportantiNonVisitati;
    }

    private string GetStringVisitati ()
    {
        string quadriVisitati = "Visitati:";

        foreach ( GameObject p in visitedPictures )
        {
            quadriVisitati = quadriVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        }
        
        return quadriVisitati;
    }

    private void WriteLog ()
    {
        string filePath = "logs.txt";

        StreamWriter writer = new StreamWriter( filePath, true );

        string quadriVisitati = "Visitati:";

        foreach ( GameObject p in visitedPictures ) {
            quadriVisitati = quadriVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        }

        quadriVisitati = quadriVisitati.Substring( 0, quadriVisitati.Length - 2 );

        writer.WriteLine( quadriVisitati);

        string quadriImportantiNonVisitati = "Importanti non visitati:";

        foreach ( GameObject p in importantPictures )
        {
            quadriImportantiNonVisitati = quadriImportantiNonVisitati + " " + p.GetComponent<PictureInfo>().index + ", ";
        }

        quadriImportantiNonVisitati = quadriImportantiNonVisitati.Substring( 0, quadriImportantiNonVisitati.Length - 2 );

        writer.WriteLine( GetType() );
        writer.WriteLine( quadriVisitati );
        writer.WriteLine( quadriImportantiNonVisitati );
        writer.WriteLine( "---" );
        writer.Close();

        Debug.Log( GetType() );


    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
