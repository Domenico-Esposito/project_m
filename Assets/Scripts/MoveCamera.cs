using UnityEngine;
using System.Collections;


public class MoveCamera : MonoBehaviour
{
    public float lookSpeedH = 2f;
    public float lookSpeedV = 2f;
    public float zoomSpeed = 2f;
    public float dragSpeed = 6f;

    private float yaw = 0f;
    private float pitch = 0f;
    private float lastY = 14.4f;
    void Update ()
    {

        float xAxisValue = Input.GetAxis( "Horizontal" );
        float zAxisValue = Input.GetAxis( "Vertical" );
        transform.Translate( new Vector3( xAxisValue, 0.0f, zAxisValue ) );

        if ( Input.GetKeyDown( "space" ) )
        {
            //Debug.Log( "Reset camera" );
            transform.position = new Vector3( transform.position.x, 14.4f, transform.position.z );
            transform.rotation = new Quaternion( 0, 0, 0, 0 );

        }

        //Look around with Right Mouse
        if ( Input.GetMouseButton( 1 ) )
        {
            yaw += lookSpeedH * Input.GetAxis( "Mouse X" );
            pitch -= lookSpeedV * Input.GetAxis( "Mouse Y" );

            transform.eulerAngles = new Vector3( pitch, yaw, 0f );
        }

        //drag camera around with Middle Mouse
        if ( Input.GetMouseButton( 2 ) )
        {
            transform.Translate( -Input.GetAxisRaw( "Mouse X" ) * Time.deltaTime * dragSpeed, -Input.GetAxisRaw( "Mouse Y" ) * Time.deltaTime * dragSpeed, 0 );
        }

        //Zoom in and out with Mouse Wheel
        transform.Translate( 0, 0, Input.GetAxis( "Mouse ScrollWheel" ) * zoomSpeed, Space.Self );

        if ( Input.GetKey("tab") )
        {
            transform.position = new Vector3( transform.position.x, lastY, transform.position.z );
        }
        else
        {
            lastY = transform.position.y;
        }
    }
}