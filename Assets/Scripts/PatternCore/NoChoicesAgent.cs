using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoChoicesAgent : BaseAgent
{

    private PictureInfo lastDestinationLeader;

    private void Awake ()
    {
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", Color.white );
    }

    public override GameObject GetNextDestination ()
    {
        throw new System.NotImplementedException();
    }

    public override void InitMovementPattern ()
    {
        //...
    }

    private void CheckDestinationFromPause ()
    {
        if ( DestinationPrePause != null && DestinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
        {
            if ( !Destination.CompareTag( "Empty Space" ) )
            {
                VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
            }

            timedelta = 0;
            Destination = DestinationPrePause;
            DestinationPrePause = null;
            InPausa = false;
            UpdateDestinationPoint();
            GoToDestinationPoint();
        }
    }

    private void AfterPictureView ()
    {
        Destination = GetMostCloseEmptySpace( transform );
        UpdateDestinationPoint();
        GoToDestinationPoint();
    }

    private void CheckLeaderDestination ()
    {
        if ( lastDestinationLeader.CompareTag( "Empty Space" ) )
        {
            Destination = lastDestinationLeader.gameObject;
        }
        else
        {
            Destination = lastDestinationLeader.GetComponentInChildren<GridSystem>().gameObject;
        }

        bool IgnoreDestination = ImportantIgnoratePicture.Contains( Destination.GetComponentInParent<PictureInfo>() );

        if( !IgnoreDestination )
        {
            if ( Destination.GetComponent<GridSystem>().HaveAvailablePoint() )
            {
                if ( !Destination.CompareTag( "Empty Space" ) )
                {
                    VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
                }
            }
            else
            {
                InPausa = true;

                DestinationPrePause = Destination;
                Destination = GetMostCloseEmptySpace( groupData.leader.GetComponent<BotVisitData>().destinationPoint.transform );
            }
        }

        lastDestinationLeader = null;
        UpdateDestinationPoint();
        GoToDestinationPoint();
    }

    private GameObject GetMostCloseEmptySpace (Transform position)
    {
        GameObject closestEmptySpace = null;

        utilitySort.transform = position;
        emptySpaces.Sort( utilitySort.DistanzaPlane );

        foreach ( GameObject plane in emptySpaces )
        {
            if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
            {
                Debug.Log( name + ": vado in un posto vicino (2) ", plane );
                closestEmptySpace = plane;
                break;
            }
        }

        return closestEmptySpace;
    }

    protected override void Behaviour ()
    {
        if ( !activeBot )
        {
            return;
        }


        if ( InPausa )
        {
            comicBalloon.InAttesa();

            if( lastDestinationLeader != null )
            {
                InPausa = false;
            }
            else 
            {
                CheckDestinationFromPause();
            }

            return;
        }

        // Ho finito di vedere il quadro, ma il leader è ancora fermo. Mi metto in posto vicino.
        if ( CheckTimer )
        {
            CheckTimer = false;
            AfterPictureView();
        }


        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            comicBalloon.GuardoOpera();
        }
        else
        {
            comicBalloon.VersoDestinazione();
        }


        // Ho un quadro importante da vedere.
        if ( lastDestinationLeader != null )
        {
            CheckLeaderDestination();

        }

        if ( IsExit() )
        {
            ExitStrategy();
        }
    }

    public override void ReceiveLeaderChoice ( GameObject leaderDestination )
    {
        if( leaderDestination.CompareTag( "Uscita" ) )
        {
            Debug.Log( name + ": ricevuta nuova destinazione del leader (despota)", leaderDestination );
            Destination = leaderDestination;

            if ( DestinationPrePause != null )
            {
                InPausa = false;
                ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                lastDestinationLeader = null;
                DestinationPrePause = null;
            }

            UpdateDestinationPointForNoChoiceExit();
            GoToDestinationPoint();
            return;
        }

        if ( !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
        {
            if ( DestinationPrePause )
            {
                ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            }

            lastDestinationLeader = leaderDestination.GetComponentInParent<PictureInfo>();
        }

    }

    private void UpdateDestinationPointForNoChoiceExit ()
    {
        StartCoroutine( LiberaPosto( DestinationPoint ) );
        DestinationPoint = Destination.GetComponent<GridSystem>().GetRandomPoint();
        DestinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

}
