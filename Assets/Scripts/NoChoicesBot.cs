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

        durataVisita += Time.deltaTime;

        if ( inPausa )
        {
            GetComponentInChildren<BubbleHead>().InAttesa();

            if ( GetComponent<RVOAgent>().destinazioneRaggiunta() )
            {
                tempoInAttesa += Time.deltaTime;
            }

            if ( destinationPrePause == null || visitedPictures.Contains( destinationPrePause ) )
            {
                utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
                emptySpaces.Sort( utilitySort.DistanzaPlane );

                //Debug.Log( gameObject.name + ": Destinazione: ", destination );
                //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

                foreach ( GameObject plane in emptySpaces )
                {
                    if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                    {
                        destination = plane;
                        break;
                    }
                }
            }
            else
            {
                if ( destinationPrePause.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    destination = destinationPrePause;
                    visitedPictures.Add( destination.transform.parent.gameObject );
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

        if ( importantPictures.Count > 0 || timedelta > pauseTime )
        {
            //destinationPrePause = null;
            UpdateDestination();
            timedelta = 0f;
        }

        if ( IsExit() )
        {
            if ( gameObject.activeInHierarchy )
            {
                destinationPoint.GetComponent<DestinationPoint>().Libera();
                gameObject.SetActive( false );
                GetComponent<RVOAgent>().SetPositionInactive();
                transform.position = new Vector3( 30f, 0f, 30f );
            }
        }
    }

    protected override void UseLastDestinationOrNew ()
    {
        if ( importantPictures.Count > 0 )
        {
            destination = importantPictures[ 0 ].transform.GetChild( 0 ).gameObject;
            if ( importantPictures.Contains( destination.transform.parent.gameObject ) )
            {
                importantPictures.Remove( destination.transform.parent.gameObject );
            }

            //CheckNextDestination();
        }
        else
        {
            inPausa = true;

            utilitySort.transform = leader.GetComponent<PathManager>().destinationPoint.transform;
            emptySpaces.Sort( utilitySort.DistanzaPlane );

            //Debug.Log( gameObject.name + ": Destinazione: ", destination );
            //Debug.Log( gameObject.name + ": Scelgo di attendere in un posto vuoto, vicino alla destinazione", emptySpaces[ 0 ] );

            foreach ( GameObject plane in emptySpaces )
            {
                if ( plane.GetComponent<GridSystem>().HaveAvailablePoint() )
                {
                    destination = plane;
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
        destination = leaderDestination;

        if ( destinationPrePause != null )
        {
            inPausa = false;
            importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
            importantPictures.Clear();
            destinationPrePause = null;
        }

        UpdateDestinationPointForNoChoiceExit();
        GoToDestinationPoint();

        if ( noChoices && leaderDestination.CompareTag( "Empty Space" ) && !visitedPictures.Contains( leaderDestination ) )
        {
            if ( destinationPrePause )
            {
                importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
            }

            importantPictures.Add( leaderDestination );
        }
        else if ( noChoices && leaderDestination.CompareTag( "PicturePlane" ) && !visitedPictures.Contains( leaderDestination.transform.parent.gameObject ) )
        {
            if ( destinationPrePause )
            {
                importantIgnoratePicture.Add( destinationPrePause.transform.parent.gameObject );
            }
            importantPictures.Add( leaderDestination.transform.parent.gameObject );
        }


    }

    private void UpdateDestinationPointForNoChoiceExit ()
    {
        StartCoroutine( LiberaPosto( destinationPoint ) );
        destinationPoint = destination.GetComponent<GridSystem>().GetRandomPoint();
        destinationPoint.GetComponent<DestinationPoint>().Occupa();
    }

    protected override void GroupElementSetData ( GameObject d, Color groupColor, bool leaderDespota )
    {
        base.GroupElementSetData(d, groupColor, leaderDespota );

        importantPictures.Clear();
        visitedPictures.Clear();
    }
}
