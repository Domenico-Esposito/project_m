﻿using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

using Museum.Utility;
using System.Collections.Generic;
using System;

public abstract class PathManager : MonoBehaviour
{
    // Pattern movimento
    public GameObject destination;
    protected GameObject destinationPoint;
    protected NavMeshAgent agent;

    protected Sort utilitySort;

    public bool inPausa = false;

    Animator m_Animator;

    // Animazione
    CharacterAnimator character;

    // Segui percorso
    [SerializeField] float pauseTime = 5f;
    public float timedelta = 0f;
    float baseTime;

    protected Dictionary<GameObject, List<GameObject>> picturesOnWalls = new Dictionary<GameObject, List<GameObject>>();
    protected List<GameObject> visitedPictures = new List<GameObject>();
    protected List<GameObject> importantPictures = new List<GameObject>();

    public int maxDistanza = 30;
    public float distanzaPercorsa = 0f;

    protected int currentPictureIndex = 0;

    public List<GameObject> emptySpaces;

    GameObject lastPositionPattern;

    public bool isLeader = false;
    protected GameObject myLeader;
    List<PathManager> group = new List<PathManager>();
    GameObject destinationPrePause;

    public abstract GameObject GetNextDestination ();
    public abstract void InitMovementPattern ();

    public float tempoInAttesa = 0f;

    [SerializeField]
    Image bottom;

    [SerializeField]
    GameObject[ ] status;

    protected void SetColorGroup(Color color )
    {
        bottom.color = color;
        transform.Find( "Bottom" ).gameObject.transform.Find( "Base" ).gameObject.GetComponent<Image>().color = color;
    }

    protected virtual void Start ()
    {
   
        utilitySort = new Sort
        {
            transform = transform
        };

        baseTime = pauseTime;

        agent = GetComponent<NavMeshAgent>();
        emptySpaces = new List<GameObject>( GameObject.FindGameObjectsWithTag( "Empty Space" ) );

        InitNavMeshAgent();
        
        InitAnimationBheavior();
        InitMovementPattern();

        SelectImportantPictureStrategy();
        
        InitGroupData();

        UpdateDestination();

    }

    void InitGroupData ()
    {
    
        if ( isLeader )
        {
            Color groupColor = GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().GetColor();

            transform.Find( "Bottom" ).gameObject.transform.Find( "Base Leader" ).gameObject.SetActive( true );
            transform.Find( "Bottom" ).gameObject.transform.Find( "Base Leader" ).gameObject.GetComponent<Image>().color = groupColor;

            SetColorGroup( groupColor );

            foreach ( PathManager bot in group )
            {
                bot.importantPictures.Clear();
                bot.setLeader( gameObject );
                bot.SetColorGroup( groupColor ); 
            }
        }
        else
        {
            transform.Find( "Bottom" ).gameObject.transform.Find( "Base" ).gameObject.SetActive( true );
        }
    }

    void SelectImportantPictureStrategy ()
    {
        foreach ( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            // "Opere medio/grandi per l'espositore", oppure, "Opere di interesse per l'agent"
            if ( picture.GetComponent<PictureInfo>().priority >= 1 || UnityEngine.Random.Range( 0, 2 ) > 1 )
            {
                importantPictures.Add( picture );
            }
        }

        utilitySort.transform = transform;
        importantPictures.Sort( utilitySort.SortByIndexPicture );
        importantPictures.Reverse();

    }

    public void setGroup ( List<PathManager> members )
    {
        isLeader = true;
        group = members;
    }

    void setLeader( GameObject leader )
    {
        myLeader = leader;
    }

    void InitNavMeshAgent ()
    {
        agent.updateRotation = false;
    }

    public void InitAnimationBheavior ()
    {
        character = GetComponent<CharacterAnimator>();
    }

    private void Update ()
    {

        if ( inPausa )
        {


            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 0 ].SetActive( true );

            if ( agent.remainingDistance > agent.stoppingDistance )
            {
                agent.avoidancePriority = UnityEngine.Random.Range(50, 60);
                character.Move( agent.desiredVelocity );
            }
            else
            {
                agent.avoidancePriority = 0;
                character.Move( Vector3.zero );
                timedelta += Time.deltaTime;
            }

            tempoInAttesa += Time.deltaTime;

            CheckDestinationFromPause();
            return;
        }


        if ( timedelta > pauseTime)
        {
            UpdateDestination();
            timedelta = 0f;
        }


        if ( agent.remainingDistance > agent.stoppingDistance )
        {
            foreach(GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 1 ].SetActive( true );

            agent.avoidancePriority = UnityEngine.Random.Range( 50, 60 );
            character.Move( agent.desiredVelocity );
        }
        else
        {

            foreach ( GameObject s in status )
            {
                s.SetActive( false );
            }
            status[ 2 ].SetActive( true );

            agent.avoidancePriority = 0;
            character.Move( Vector3.zero );

            if( destination.CompareTag("PicturePlane") )
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

    private void UseLastDestinationOrNew ()
    {
        if ( lastPositionPattern == null )
        {
            destination = GetNextDestination();
            Debug.Log( gameObject.name + ": Chiedo nuova destinazione dal pattern", destination );
        }
        else
        {
            destination = lastPositionPattern;
            Debug.Log( gameObject.name + ": Uso destinazione già calcolata in precedenza", destination );
        }
    }

    private void UpdateDestination () 
    {

        bool haveLastPositionPattern = false;
        float distanceFromDestination = 0;

        UseLastDestinationOrNew();

        NavMeshPath staticPath = new NavMeshPath();
        agent.CalculatePath( destination.transform.position, staticPath );

        distanceFromDestination = GetPathLenght( staticPath );

        Transform destinationPicture = destination.transform.parent;

        try
        {
            foreach ( GameObject picture in importantPictures )
            {
                GameObject picturePlane = picture.transform.GetChild( 0 ).gameObject;

                agent.CalculatePath( picturePlane.transform.position, staticPath );
                float distanzaFromPictureImportant = GetPathLenght( staticPath );

                    // Immagine importante più vicina di immagine pattern
                    if ( distanzaFromPictureImportant < distanceFromDestination ||
                         picture.GetComponent<PictureInfo>().index < destinationPicture.GetComponent<PictureInfo>().index )
                    {
                        importantPictures.Remove( picture );
                        lastPositionPattern = destination;
                        haveLastPositionPattern = true;
                        destination = picturePlane;

                        Debug.Log( "Questa destinazione viene salvata per dopo", lastPositionPattern );
                        Debug.Log( "Prossima destinazione è importante", destination );
                        break;
                    }
            }
        }
        catch ( NullReferenceException e )
        {
            Debug.Log( gameObject.name + ": Destinazione è uno spazio vuoto" );
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
        return destination.GetComponent<GridSystem>().GetAvailablePoint();
    }

    public void NotifyNewDestination(GameObject destination )
    {
        if( destination.CompareTag( "PicturePlane" ) )
        {
            Debug.Log( gameObject.name + ": Capo ha scelto nuova destinazione importante", destination );
            if( !importantPictures.Contains( destination.transform.parent.gameObject ) && 
                !visitedPictures.Contains( destination.transform.parent.gameObject ))
            {
                importantPictures.Add( destination.transform.parent.gameObject );
            }
        }
    }

    private void GoToDestinationPoint ()
    {
        agent.SetDestination( destinationPoint.transform.position );

        if ( isLeader )
        {
            foreach ( PathManager bot in group )
            {
                try
                {
                    bot.NotifyNewDestination( destination );
                }
                catch( MissingReferenceException e )
                {
                    Debug.Log( gameObject.name + ": un membro del gruppo ha già abbandonato il museo." );
                }
            }
        }

        NavMeshPath staticPath = new NavMeshPath();
        agent.CalculatePath(destinationPoint.transform.position, staticPath);

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

    public const int NON_STANCO = 0;
    public const int STANCO = 1;
    public const int MOLTO_STANCO = 2;

    /*
     * 0 = Non stanco
     * 1 = Stanco
     * 2 = Molto stanco
     */
    public int LivelloStanchezza ()
    {

        if( distanzaPercorsa > (maxDistanza / 1.2f ) )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Stanco" );
            return STANCO;
        }

        if( distanzaPercorsa > maxDistanza )
        {
            Debug.Log( gameObject.name + ": Livello stanchezza: Molto stanco" );
            return MOLTO_STANCO;
        }

        Debug.Log( gameObject.name + ": Livello stanchezza: Non stanco" );
        return NON_STANCO;
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
            if ( LivelloStanchezza() == 0 || 
                 ( LivelloStanchezza() == 1 && destination.CompareTag("PicturePlane") && destination.transform.parent.GetComponent<PictureInfo>().priority > 1 ) )
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
        if (destination.gameObject.CompareTag("Uscita") && Vector3.Distance(transform.position, destinationPoint.transform.position) < 3f)
        {
            GameObject.FindWithTag( "Museo" ).GetComponent<ReceptionMuseum>().ReceivData( visitedPictures, importantPictures, tempoInAttesa );
            return true;
        }

        return false;
    }


    protected GameObject GetPlaneOfExit ()
    {
        return GameObject.FindGameObjectWithTag( "Uscita" ).gameObject;
    }

}
