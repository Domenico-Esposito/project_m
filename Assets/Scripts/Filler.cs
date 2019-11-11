using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filler : MonoBehaviour
{

    public float pauseTime = 3f;
    private float deltaTime = 0f;
    public int numberOfFourmi = 1;
    public int numberOfPapillon = 1;
    public int numberOfPoisson = 1;
    public int numberOfSauterelle = 1;
    public int index = 1;

    public bool gruppi = true;

    public GameObject fourmiBot;
    public GameObject papillonBot;
    public GameObject poissonBot;
    public GameObject sauterelleBot;

    private void Awake ()
    {
        if( gruppi )
        {
            fillGroup();
        }
        else
        {
            fill();
        }
    }

    private void Update ()
    {
     
        if( deltaTime > pauseTime )
        {
            if ( gruppi )
            {
                fillGroup();
            }
            else
            {
                fill();
            }
            deltaTime = 0;
        }

        deltaTime += Time.deltaTime;
    }

    private void fillGroup ()
    {

        List<PathManager> group = new List<PathManager>();

        for ( int i = 0; i < Random.Range( 3, 10); i++)
        {
            if( Random.Range(0, 2) >= 1)
            {
                group.Add( AddNewBot( 0 ).GetComponent<PathManager>() );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                group.Add( AddNewBot( 1 ).GetComponent<PathManager>() );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                group.Add( AddNewBot( 2 ).GetComponent<PathManager>() );
            }
            else
            {
                group.Add( AddNewBot( 3 ).GetComponent<PathManager>() );
            }

        }

        PathManager p = group[ 0 ];
        group.RemoveAt( 0 );
        p.isCapoGruppo = true;
        p.groupElement = group;

    }

    private void fill()
    {
        int[ ] numbers = { numberOfFourmi, numberOfPoisson, numberOfPapillon, numberOfSauterelle };


        for ( int i = 0; i < Mathf.Max( numbers ) ; i++)
        {
            if( i < numberOfFourmi )
            {
                AddNewBot( 0 );
            }

            if( i < numberOfPapillon )
            {
                AddNewBot( 1 );
            }

            if ( i < numberOfPoisson)
            {
                AddNewBot( 2 );
            }

            if( i < numberOfSauterelle )
            {
                AddNewBot( 3 );
            }
        }
    }

    private GameObject AddNewBot (int type)
    {
    
        switch ( type )
        {
        case 0:
            return AddNewFourmi();
        case 1:
            return AddNewPapillon();
        case 2:
            return AddNewPoisson();
        case 3:
            return AddNewSauterelle();
        }

        return null;
    }


    private GameObject AddNewFourmi ()
    {
        GameObject o = Instantiate( fourmiBot, transform, true );
        o.name = "Agente " + index++;

        return o;
    }

    private GameObject AddNewPapillon ()
    {
        GameObject o = Instantiate( papillonBot, transform, true );
        o.name = "Agente " + index++;

        return o;

    }

    private GameObject AddNewPoisson ()
    {
        GameObject o = Instantiate( poissonBot, transform, true );
        o.name = "Agente " + index++;

        return o;
    }

    private GameObject AddNewSauterelle ()
    {
        GameObject o = Instantiate( sauterelleBot, transform, true );
        o.name = "Agente " + index++;

        return o;
    }
}
