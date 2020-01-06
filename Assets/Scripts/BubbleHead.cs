using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleHead : MonoBehaviour
{

    [SerializeField]
    GameObject[ ] status;

    // Update is called once per frame
    void Update()
    {
        Vector3 v = Camera.main.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt( Camera.main.transform.position - v );
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
