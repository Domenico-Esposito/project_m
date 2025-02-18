﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RVO;


public class RVOSimulator : MonoBehaviour
{

    public List<RVO.Vector2> agentPositions;
    public List<GameObject> rvoGameObj;
    public bool isCovidMode;

    // Use this for initialization
    void Start ()
    {
        agentPositions = new List<RVO.Vector2>();
        rvoGameObj = new List<GameObject>();

        //Simulator.Instance.setTimeStep( 0.01f );
        //Simulator.Instance.setAgentDefaults( 15.0f, 10, 5.0f, 5.0f, 2f, 5.0f, new RVO.Vector2( 0.0f, 0.0f ) );

    }

    public Simulator getSimulator ()
    {
        return Simulator.Instance;
    }

    Vector3 toUnityVector ( RVO.Vector2 param )
    {
        return new Vector3( param.x(), transform.position.y, param.y() );
    }

    RVO.Vector2 toRVOVector ( Vector3 param )
    {
        return new RVO.Vector2( param.x, param.z );
    }

    public RVO.Vector2 getAgentPosition ( int agentIndex )
    {
        return Simulator.Instance.getAgentPosition( agentIndex );
    }

    public void addNew ( int agentNo )
    {
        Simulator.Instance.doStep();
    }

    private float getRadiusByDistancing (float distancingInM)
    {
        // 1 Unità unity in m
        float scala = 1 / 1.80f;
        float minRadius = 1f;

        // Spalle + Distanziamento in scala
        return (scala * minRadius ) + ( scala * distancingInM); 
    }

    public int addAgentToSim ( Vector3 pos, GameObject ag, List<Vector3> paths )
    {
        //remove the initial position since the agent is already on it
        if ( paths != null && paths.Count > 0 )
            paths.Remove( paths[ 0 ] );

        //clear the simulation
        Simulator.Instance.Clear();
        //re initialize the simulation

        Vector3 agent_lossyScale = ag.transform.lossyScale;


        Simulator.Instance.setTimeStep( 0.125f );
        Simulator.Instance.setAgentDefaults( 2f, 15, 1.0f, 10.0f, 0.56f, 2.3f, new RVO.Vector2( 0.0f, 0.0f ) );
    

        //add all the previous agents
        int agentCount = agentPositions.Count;
        for ( int i = 0; i < agentCount; i++ )
        {
            Simulator.Instance.addAgent( agentPositions[ i ] );
        }

        //add the new agent
        rvoGameObj.Add( ag );
        agentPositions.Add( toRVOVector( pos ) );

        //return the index of the new added agent
        return Simulator.Instance.addAgent( toRVOVector( pos ) );
    }

    public bool stopSimulation = false;

    void FixedUpdate(){

        if( stopSimulation ){
            foreach (GameObject agent in rvoGameObj)
            {
                Destroy(agent);
            }
            rvoGameObj.Clear();
            agentPositions.Clear();
            return;
        }

        int agentNUmber = Simulator.Instance.getNumAgents();

        try
        {
            for ( int i = 0; i < agentNUmber; i++ )
            {
                RVO.Vector2 agentLoc = Simulator.Instance.getAgentPosition( i );
                RVO.Vector2 station = rvoGameObj[ i ].GetComponent<RVOAgent>().calculateNextStation() - agentLoc;

                Vector3 agent_lossyScale = rvoGameObj[ i ].transform.lossyScale;

                if ( RVOMath.absSq( station ) > 1.0f )
                {
                    station = RVOMath.normalize( station );
                }

                Simulator.Instance.setAgentPrefVelocity( i, station );
                agentPositions[ i ] = Simulator.Instance.getAgentPosition( i );

          
                if ( isCovidMode )
                {

                    if ( !rvoGameObj[ i ].GetComponent<RVOAgent>().destinazioneRaggiunta() )
                    {

                        Simulator.Instance.setAgentRadius( i, 0f );
                        Simulator.Instance.setAgentMaxSpeed( i, 0f );
                        Simulator.Instance.setAgentVelocity( i, new RVO.Vector2( 0, 0 ) );
                        Simulator.Instance.setAgentPrefVelocity( i, new RVO.Vector2( 0, 0 ) );
                    }
                    else
                    {
                        //..
                        //Simulator.Instance.setAgentDefaults( 2f, 15, 1.0f, 10.0f, getRadiusByDistancing( 1f ), 2.3f, new RVO.Vector2( 0.0f, 0.0f ) );
                        Simulator.Instance.setAgentRadius( i, getRadiusByDistancing(0.5f) );
                        Simulator.Instance.setAgentMaxSpeed( i, 2.3f );
                        rvoGameObj[ i ].GetComponent<CapsuleCollider>().radius = 0.1f;
                    }
                }
                else
                {

                    if ( Simulator.Instance.getAgentNumAgentNeighbors( i ) > 3 )
                    {
                        Simulator.Instance.setAgentRadius( i, 0.3f );
                        rvoGameObj[ i ].GetComponent<CapsuleCollider>().radius = 0.1f;
                    }
                    else
                    {
                        Simulator.Instance.setAgentRadius( i, 0.56f );
                        rvoGameObj[ i ].GetComponent<CapsuleCollider>().radius = 0.3f;
                    }
                }
            

            }
            Simulator.Instance.doStep();
        }
        catch ( System.Exception ex )
        {
            //Debug.Log( ex.StackTrace );
        }

    }
}
