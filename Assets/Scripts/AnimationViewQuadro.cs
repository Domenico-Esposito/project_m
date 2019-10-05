using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationViewQuadro : AnimateCharacter
{

    private bool isFirstTry = true;

    public void TurnTowardsPicture ( Collision collision )
    {

        if ( path == null && collision.gameObject.CompareTag( "Quadro" ) )
        {
            if ( CheckTurnQuadro( collision ) && isFirstTry )
            {
                Turn();
                isFirstTry = false;
            }
            else
            {
                RotationToTarget( collision.gameObject.GetComponentInParent<RectTransform>().transform.position, 1.5f );
            }
        }
        else
        {
            isFirstTry = true;
        }

    }


    private bool CheckTurnQuadro ( Collision collision )
    {

        bool isRotation = IsRotation();

        if ( !isRotation && path == null )
        {

            angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, collision.gameObject.GetComponentInParent<RectTransform>().transform.position );
            localPos = transform.InverseTransformPoint( collision.gameObject.GetComponentInParent<RectTransform>().transform.position );

            if ( angleBetweenPlayerAndTarget > 60f )
            {
                if ( PlayerSeeQuadro() )
                {
                    angleBetweenPlayerAndTarget = 0;
                    localPos = Vector3.zero;
                    return false;
                }

                return true;
            }
            else
            {
                RotationToTarget( collision.gameObject.GetComponentInParent<RectTransform>().transform.position, 1.5f );
            }
        }

        angleBetweenPlayerAndTarget = 0;
        localPos = Vector3.zero;

        return false;
    }


    private bool PlayerSeeQuadro ()
    {

        RaycastHit hit;

        if ( Physics.Raycast( transform.position + new Vector3( 0f, 2f, 0f ), transform.forward, out hit, 7f, 1 << 11 ) )
        {
            return true;
        }

        return false;

    }

}
