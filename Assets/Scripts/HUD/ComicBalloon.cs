using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComicBalloon : MonoBehaviour
{
    [SerializeField]
    GameObject[ ] status;

    void Update()
    {
        Vector3 positionDiff = Camera.main.transform.position - transform.position;
        positionDiff.x = positionDiff.z = 0.0f;
        transform.LookAt( Camera.main.transform.position - positionDiff );
    }

    public void InAttesa ()
    {
        HideAll();
        status[ 0 ].SetActive( true );
    }

    public void VersoDestinazione ()
    {
        HideAll();
        status[ 1 ].SetActive( true );
    }

    public void GuardoOpera()
    {
        HideAll();
        status[ 2 ].SetActive( true );
    }

    private void HideAll()
    {
        foreach ( GameObject s in status )
        {
            s.SetActive( false );
        }
    }
}
