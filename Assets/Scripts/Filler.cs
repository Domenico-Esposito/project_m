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

        List<GameObject> group = new List<GameObject>();

        for ( int i = 0; i < Random.Range( 3, 10); i++)
        {
            GameObject o;

            if( Random.Range(0, 2) >= 1)
            {
                o = AddNewBot( 0 );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                o = AddNewBot( 1 );
            }
            else if ( Random.Range( 0, 2 ) >= 1 )
            {
                o = AddNewBot( 2 );
            }
            else
            {
                o = AddNewBot( 3 );
            }

            group.Add( o );
        }

        GameObject capo = group[ 0 ];
        capo.GetComponent<PathManager>().isCapoGruppo = true;
        group.Remove( capo );

        foreach(GameObject g in group )
        {
            capo.GetComponent<PathManager>().groupElement.Add(g.GetComponent<PathManager>());
        }

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
