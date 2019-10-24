using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationViewPicture : CharacterAnimator
{

    private bool isFirstTry = true;

    public void TurnTowardsPicture ( Collision collision )
    {
        if (collision.gameObject.CompareTag( "PicturePlane" ) )
        {
            if ( CheckAngleToTarget( collision ) && isFirstTry )
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


    public void TurnPic ()
    {
        if ( localPos == Vector3.zero )
            return;

        speed = 0f;
        Animation_Walk();

        playerRidiBody.isKinematic = true;

        if ( localPos.x < tolleranceLeft )
        {
            Animation_TurnLeft();
        }
        else if ( localPos.x > tolleranceRight )
        {
            Animation_TurnRight();
        }

    }


    private bool CheckAngleToTarget ( Collision target )
    {
        bool isRotation = IsRotation();

        if ( !isRotation )
        {
            Vector3 picturePosition = target.gameObject.GetComponentInParent<RectTransform>().transform.position;
            angleBetweenPlayerAndTarget = Vector3.Angle( transform.forward, picturePosition );
            localPos = transform.InverseTransformPoint( picturePosition );

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
        }

        angleBetweenPlayerAndTarget = 0;
        localPos = Vector3.zero;

        return false;
    }


    public bool PlayerSeeQuadro ()
    {
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask( "Quadro" );

        if ( Physics.Raycast( transform.position + new Vector3( 0f, 2f, 0f ), transform.forward, out hit, 7f, layer_mask ) )
            return true;

        return false;
    }

}
