using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Filler : MonoBehaviour
{

    private float timeDelta = 0f;
    private float pauseTime = 3f;

    public GameObject fourmiBot;
    public GameObject papillonBot;
    public GameObject poissonBot;
    public GameObject sauterelleBot;
    
    private void Update ()
    {
        if( timeDelta > pauseTime )
        {
            int numberOfNPC = GameObject.FindGameObjectsWithTag( "NPC" ).Length;
            if ( numberOfNPC < 50 )
            {
                AddNewBot();
            }
            Debug.Log( "Numero di NPC: " + (numberOfNPC+1) );
            timeDelta = 0f;
        }

        timeDelta += Time.deltaTime;
    }


    private void AddNewBot ()
    {

        int random = Random.Range( 0, 3 );

        switch ( random )
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
        Instantiate( fourmiBot, transform, true );
    }

    private void AddNewPapillon ()
    {
        Instantiate( papillonBot, transform, true );

    }

    private void AddNewPoisson ()
    {
        Instantiate( poissonBot, transform, true );

    }

    private void AddNewSauterelle ()
    {
        Instantiate( sauterelleBot, transform, true );

    }
}
