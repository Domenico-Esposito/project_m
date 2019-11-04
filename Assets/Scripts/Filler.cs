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

    public GameObject fourmiBot;
    public GameObject papillonBot;
    public GameObject poissonBot;
    public GameObject sauterelleBot;

    private void Awake ()
    {
        fill();
    }

    private void Update ()
    {
     
        if( deltaTime > pauseTime )
        {
            fill();
            deltaTime = 0;
        }

        deltaTime += Time.deltaTime;
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

    private void AddNewBot (int type)
    {
    
        switch ( type )
        {
        case 0:
            AddNewFourmi();
            break;
        case 1:
            AddNewPapillon();
            break;
        case 2:
            AddNewPoisson();
            break;
        case 3:
            AddNewSauterelle();
            break;
        }

    }


    private void AddNewFourmi ()
    {
        Instantiate( fourmiBot, transform, true ).name = "Agente " + index++;
    }

    private void AddNewPapillon ()
    {
        Instantiate( papillonBot, transform, true ).name = "Agente " + index++;

    }

    private void AddNewPoisson ()
    {
        Instantiate( poissonBot, transform, true ).name = "Agente " + index++;

    }

    private void AddNewSauterelle ()
    {
        Instantiate( sauterelleBot, transform, true ).name = "Agente " + index++;

    }
}
