using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoChoicesBot : PathManager
{
    public override GameObject GetNextDestination ()
    {
        throw new System.NotImplementedException();
    }

    public override void InitMovementPattern ()
    {

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

    private void CheckImportantPicture ()
    {
        if ( ImportantPictures[ 0 ].CompareTag( "Empty Space" ) )
        {
            Destination = ImportantPictures[ 0 ].gameObject;
        }
        else
        {
            Destination = ImportantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
        }

        if ( Destination.GetComponent<GridSystem>().HaveAvailablePoint() && !ImportantIgnoratePicture.Contains( Destination.GetComponentInParent<PictureInfo>() ) )
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

        ImportantPictures.Clear();
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

            if(ImportantPictures.Count > 0 )
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
        if ( okTimer )
        {
            okTimer = false;
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
        if ( ImportantPictures.Count > 0 )
        {
            CheckImportantPicture();

        }

        if ( IsExit() )
        {
            ExitStrategy();
        }
    }

    public override void SendLeaderChoices ( GameObject leaderDestination )
    {
        if( leaderDestination.CompareTag( "Uscita" ) )
        {
            Debug.Log( name + ": ricevuta nuova destinazione del leader (despota)", leaderDestination );
            Destination = leaderDestination;

            if ( DestinationPrePause != null )
            {
                InPausa = false;
                ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                ImportantPictures.Clear();
                DestinationPrePause = null;
            }

            UpdateDestinationPointForNoChoiceExit();
            GoToDestinationPoint();
            return;
        }

        if ( leaderDestination.CompareTag( "PicturePlane" ) || leaderDestination.CompareTag( "Empty Space" ) )
        {
            if ( leaderDestination.CompareTag( "Empty Space" ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
            {
                if ( DestinationPrePause )
                {
                    ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                }

                ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
            }
            else if ( leaderDestination.CompareTag( "PicturePlane" ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
            {
                if ( DestinationPrePause )
                {
                    ImportantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
                }
                ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
            }
        }

    }

    private void UpdateDestinationPointForNoChoiceExit ()
    {
        StartCoroutine( LiberaPosto( DestinationPoint ) );
        DestinationPoint = Destination.GetComponent<GridSystem>().GetRandomPoint();
        DestinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

}
