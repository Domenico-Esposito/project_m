using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public GameObject oggetto;


    private void Awake ()
    {
    }

    // Update is called once per frame
    private void Update()
    {
         if (Input.GetMouseButtonDown(0))
        {
        
            int layer_mask = LayerMask.GetMask( "Walls", "PicturePlane" );
            RaycastHit[ ] hits = Physics.RaycastAll( transform.position + new Vector3( 0, 5, 0 ), -transform.forward, 50f, layer_mask );

            Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), -transform.forward, Color.red, 10 );

            foreach ( RaycastHit hit in hits )
            {
                Instantiate( oggetto, hit.point, Quaternion.identity );
                Debug.Log( "Back Hit: " + hit.collider.name, hit.collider );
            }


            Vector3 backLeft = Quaternion.AngleAxis( 165, transform.up ) * transform.forward;
            hits = Physics.RaycastAll( transform.position + new Vector3( 0, 5, 0 ), backLeft, 500f, layer_mask );
            Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), backLeft, Color.yellow, 10 );

            foreach ( RaycastHit hit in hits )
            {
                Instantiate( oggetto, hit.point, Quaternion.identity);
                Debug.Log( "Left Hit: " + hit.collider.name, hit.collider );
                Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), -transform.forward, Color.red, 10 );

            }


            Vector3 backRight = Quaternion.AngleAxis( 195, transform.up ) * transform.forward;
            hits = Physics.RaycastAll( transform.position + new Vector3( 0, 5, 0 ), backRight, 500f, layer_mask );
            Debug.DrawRay( transform.position + new Vector3( 0, 5, 0 ), backRight, Color.blue, 10 );

            foreach ( RaycastHit hit in hits )
            {
                Instantiate( oggetto, hit.point, Quaternion.identity );
                Debug.Log( "Left Hit: " + hit.collider.name, hit.collider );
            }



        }
    }

}
