using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        float r = Vector3.Distance( transform.position, target.position );

        return r <= 0.05f;

        //return Vector3.Distance( toUnityVector( s.getAgentPosition( agentIndex ) ), target.position ) < 0.07f;

        //if ( RVOMath.absSq( s.getAgentPosition( agentIndex ) - toRVOVector( target.transform.position ) ) <= 0.020f )
        //{
        //    Debug.Log( "È <= 0: " + RVOMath.absSq( s.getAgentPosition( agentIndex ) - toRVOVector( target.transform.position ) ) );
        //}
        //else
        //{
        //    Debug.Log( RVOMath.absSq( s.getAgentPosition( agentIndex ) - toRVOVector( target.transform.position ) ) );
        //}

        //return RVOMath.absSq( s.getAgentPosition( agentIndex ) - toRVOVector( target.transform.position ) ) <= 0.020f;

    }

    Vector3 station;
    bool doStep = true;

    // Update is called once per frame
    void Update ()
    {

        if ( destinazioneRaggiunta() )
        {

            //GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentMaxSpeed( agentIndex, 0f);
            //GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentVelocity( agentIndex, new RVO.Vector2( 0f, 0f ) );
            //GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentRadius( agentIndex, 0.8f );

            //if ( doStep )
            //{
            //    doStep = false;
            //    GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().doStep();
            //}

            //Debug.Log( name + ": arrivato a destinazione ", gameObject );
            //GetComponent<CharacterAnimator>().Move( Vector3.Lerp(toUnityVector( simulator.getAgentPosition( agentIndex ) ) - transform.position, Vector3.zero, 3f));

            GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentVelocity( agentIndex, new RVO.Vector2( 0f, 0f ) );
            GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentPosition( agentIndex, toRVOVector( transform.position ) );
            GetComponent<CharacterAnimator>().Move( Vector3.zero );

            if ( GetComponent<PathManager>().destination.CompareTag( "PicturePlane" ) )
            {
                Vector3 position = GetComponent<PathManager>().destination.transform.parent.transform.position;
                GetComponent<CharacterAnimator>().TurnToPicture( position );
            }
        }
        else
        {

            if ( isAbleToStart && agentIndex != -1 )
            {
                GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentMaxSpeed( agentIndex, 2.3f );
                doStep = true;
                transform.position = Vector3.Lerp( transform.position, toUnityVector( simulator.getAgentPosition( agentIndex ) ), Time.deltaTime * 2.3f );
                GetComponent<CharacterAnimator>().Move( toUnityVector( simulator.getAgentPosition( agentIndex ) ) - transform.position );
                //GameObject.FindGameObjectWithTag( "RVOSim" ).GetComponent<RVOSimulator>().getSimulator().setAgentPosition(agentIndex, toRVOVector(transform.position) );
            }

        }


        //if ( GetComponent<AIPath>().reachedEndOfPath )
        //{
        //    Debug.Log(name + ": arrivato a destinazione ", gameObject);
        //    GetComponent<CharacterAnimator>().Move( Vector3.zero );

        //    if( GetComponent<PathManager>().destination.CompareTag("PicturePlane") )
        //    {
        //        Vector3 position = GetComponent<PathManager>().destination.transform.parent.transform.position;
        //        GetComponent<CharacterAnimator>().TurnToPicture( position );
        //    }
        //}
        //else
        //{
        //    if ( isAbleToStart && agentIndex != -1 )
        //    {
        //        transform.position = Vector3.Lerp( transform.position, toUnityVector( simulator.getAgentPosition( agentIndex ) ), Time.deltaTime*3.2f);
        //        GetComponent<CharacterAnimator>().Move( toUnityVector( simulator.getAgentPosition( agentIndex ) ) - transform.position );
        //    }
        //}
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
