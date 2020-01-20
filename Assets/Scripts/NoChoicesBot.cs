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
        noChoices = true;
    }


    protected override void Behaviour ()
    {
        if ( !activeBot )
        {
            return;
        }

        DurataVisita += Time.deltaTime;

        if ( inPausa )
        {
            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                TempoInAttesa += Time.deltaTime;
            }

            if ( DestinationPrePause == null || VisitedPictures.Contains( DestinationPrePause.GetComponentInParent<PictureInfo>() ) )
            {
                utilitySort.transform = leader.GetComponent<BotVisitData>().destinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        Destination = plane;
                        break;
                    }
                }
            }
            else
            {
                if ( DestinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    Destination = DestinationPrePause;
                    VisitedPictures.Add( Destination.GetComponentInParent<PictureInfo>() );
                    UpdateDestinationPoint();
                    GoToDestinationPoint();

                    //Debug.Log( name + ": la destinazione si è liberata", destination );

                    inPausa = false;
                }
            }

            return;
        }

        if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
        {
            GetComponentInChildren<BubbleHead>().GuardoOpera();
            timedelta += Time.deltaTime;

        }
        else
        {
            GetComponentInChildren<BubbleHead>().VersoDestinazione();

        }

        if ( ImportantPictures.Count > 0 || timedelta > pauseTime )
        {
            //destinationPrePause = null;
            UpdateDestination();
            timedelta = 0f;
        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                DestinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }
    }

    protected override void UseLastDestinationOrNew ()
    {
        if ( ImportantPictures.Count > 0 )
        {
            Destination = ImportantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
            if ( ImportantPictures.Contains( Destination.GetComponentInParent<PictureInfo>()  ) )
            {
                ImportantPictures.Remove( Destination.GetComponentInParent<PictureInfo>() );
            }

            //CheckNextDestination();
        }
        else
        {
            inPausa = true;

            utilitySort.transform = leader.GetComponent<BotVisitData>().destinationPoint.transform;
            emptySpaces.Sort( utilitySort.DistanzaPlane );

            //Debug.Log( gameObject.name + ": Destinazione: ", destination );
            //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

            foreach ( GameObject plane in emptySpaces )
            {
                if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    Destination = plane;
                    //UpdateDestinationPoint();
                    //GoToDestinationPoint();
                    break;
                }
            }
        }

    }

    protected override void NotifyNewDestination(GameObject leaderDestination )
    {
        Debug.Log( name + ": impostata destinazione come uscita", leaderDestination );
        Destination = leaderDestination;

        if ( DestinationPrePause != null )
        {
            inPausa = false;
            importantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            ImportantPictures.Clear();
            DestinationPrePause = null;
        }

        UpdateDestinationPointForNoChoiceExit();
        GoToDestinationPoint();

        if ( noChoices && leaderDestination.CompareTag( "Empty Space" ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
        {
            if ( DestinationPrePause )
            {
                importantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            }

            ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
        }
        else if ( noChoices && leaderDestination.CompareTag( "PicturePlane" ) && !VisitedPictures.Contains( leaderDestination.GetComponentInParent<PictureInfo>() ) )
        {
            if ( DestinationPrePause )
            {
                importantIgnoratePicture.Add( DestinationPrePause.GetComponentInParent<PictureInfo>() );
            }
            ImportantPictures.Add( leaderDestination.GetComponentInParent<PictureInfo>() );
        }


    }

    private void UpdateDestinationPointForNoChoiceExit ()
    {
        StartCoroutine( LiberaPosto( DestinationPoint ) );
        DestinationPoint = Destination.GetComponent<GridSystem>().GetRandomPoint();
        DestinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

    protected override void GroupElementSetData (Color groupColor, bool leaderDespota )
    {
        base.GroupElementSetData(groupColor, leaderDespota );

        ImportantPictures.Clear();
        VisitedPictures.Clear();
    }
}
