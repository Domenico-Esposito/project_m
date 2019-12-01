using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RVO;
using Pathfinding;

public class RVOAgent : MonoBehaviour
{
    
    [ SerializeField]
    Transform target;

    Seeker agentSeeker;
    private List<Vector3> pathNodes = null;
    RVOSimulator simulator = null;
    public int agentIndex = -1;
    int currentNodeInThePath = 0;
    bool isAbleToStart = false;

    CharacterAnimator character;

    // Use this for initialization
    IEnumerator Start ()
    {
        
        currentNodeInThePath = 0;
        simulator = GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>();
        pathNodes = new List<Vector3>();
        yield return StartCoroutine( StartPaths() );
        if( agentIndex == -1 )
        {
            agentIndex = simulator.addAgentToSim( transform.position, gameObject, pathNodes );
        }
        isAbleToStart = true;
    }

    IEnumerator StartPaths ()
    {
        agentSeeker = gameObject.GetComponent<Seeker>();
        Path path = agentSeeker.StartPath( transform.position, target.position, OnPathComplete );

        yield return StartCoroutine( path.WaitForPath() );
    }

    public void UpdateTarget(Transform newTarget )
    {
        isAbleToStart = true;
        target = newTarget;
    }

    public void Refresh ()
    {
        StopAllCoroutines();
        StartCoroutine( Start() );
    }

    public void OnPathComplete ( Path p )
    {
        //We got our path back
        if ( p.error )
        {
            Debug.Log( "" + this.gameObject.name + " ---- -" + p.errorLog );
        }
        else
        {
            pathNodes = p.vectorPath;
        }
    }

    public bool destinazioneRaggiunta ()
    {
        try
        {
            float r = Vector3.Distance( transform.position, target.transform.position);
            return r <= 0.2;
        }
        catch( UnassignedReferenceException e )
        {
            return false;
        }
    }

    Vector3 station;
    bool doStep = true;

    public void RemoveAgent ()
    {
        simulator.removeAgent( agentIndex );
    }

    // Update is called once per frame
    void Update ()
    {



        Vector3 animationPosition = Vector3.zero;


        /*
         * Perturb a little to avoid deadlocks due to perfect symmetry.
         */

        if ( destinazioneRaggiunta() )
        {
            ChangePosition( 1f );
            GetComponent<CharacterAnimator>().Move( animationPosition );

            try
            {
                if ( GetComponent<PathManager>().destination != null && GetComponent<PathManager>().destination.CompareTag( "PicturePlane" ) )
                {
                    Vector3 picturePosition = GetComponent<PathManager>().destination.transform.parent.transform.position;
                    GetComponent<CharacterAnimator>().TurnToPicture( picturePosition );
                }
            }
            catch( NullReferenceException e )
            {
                Debug.Log( name + ": la destinazione non è un quadro");
            }

        }
        else
        {
            if ( isAbleToStart && agentIndex != -1 )
            {
                //System.Random r = new System.Random();

                //float angle = ( float )r.NextDouble() * 2.0f * ( float )Math.PI;
                //float dist = ( float )r.NextDouble() * 0.0001f;

                //simulator.getSimulator().setAgentPrefVelocity( agentIndex, simulator.getSimulator().getAgentPrefVelocity( agentIndex ) +
                //                          dist * new RVO.Vector2( ( float )Math.Cos( angle ), ( float )Math.Sin( angle ) ) );
                //simulator.getSimulator().doStep();

                ChangePosition( 2.3f );
                animationPosition = toUnityVector( simulator.getAgentPosition( agentIndex ) ) - transform.position;
                GetComponent<CharacterAnimator>().Move( animationPosition );
            }
        }


    }

    private void ChangePosition (float movimentTime)
    {
        transform.position = Vector3.Lerp( transform.position, toUnityVector( simulator.getAgentPosition( agentIndex ) ), Time.deltaTime * movimentTime );
    }

    public void SetPositionInactive ()
    {
        UpdateTarget(transform);
        simulator.getSimulator().setAgentPosition( agentIndex, new RVO.Vector2( 30, 30 ) );
        simulator.getSimulator().setAgentDefaults( 0, 0, 0, 0, 0, 0, new RVO.Vector2( 0, 0 ) );
        simulator.getSimulator().doStep();
    }

    public RVO.Vector2 calculateNextStation ()
    {
        if ( currentNodeInThePath < pathNodes.Count )
        {
            station = pathNodes[ currentNodeInThePath ];
            float distance = Vector3.Distance( station, transform.position );
            if ( distance >= 0 && distance < 1 )
            {
                station = pathNodes[ currentNodeInThePath ];
                currentNodeInThePath++;
            }
        }
        else
        {
            station = pathNodes[ pathNodes.Count - 1 ];
        }
        return toRVOVector( station );
    }

    Vector3 toUnityVector ( RVO.Vector2 param )
    {
        return new Vector3( param.x(), transform.position.y, param.y() );
    }

    RVO.Vector2 toRVOVector ( Vector3 param )
    {
        return new RVO.Vector2( param.x, param.z );
    }
}
