using System.Collections.Generic;
using UnityEngine;

public class SauterelleAgent : BaseAgent
{
    private IEnumerator<PictureInfo> picturesToWatch;
    private List<PictureInfo> pictures;
    private int maxJump;

    private void Awake ()
    {
        Color32 red = new Color32( 202, 12, 12, 1 );
        GetComponentInChildren<Renderer>().material.SetColor( "_Color", red );

        maxJump = Random.Range( 6, 10 );
    }

    public override void InitMovementPattern ()
    {
        MaxDistanza = 200;
        ChanceSkipDestination = 2;

        FindAllPicture();
        SetPictureToWatch();
    }

    private void FindAllPicture ()
    {
        pictures = new List<PictureInfo>();

        foreach( GameObject picture in GameObject.FindGameObjectsWithTag( "Picture" ) )
        {
            pictures.Add( picture.GetComponent<PictureInfo>() );
        }

        utilitySort.transform = transform;
        pictures.Sort( utilitySort.DistanzaPicture );

    }

    public override GameObject GetNextDestination ()
    {
        if ( ( ImportantPictures.Count <= 0 && groupData.LeaderIsAlive ) || FatigueLevel >= FatigueManager.Level.MOLTO_STANCO )
            return GetPlaneOfExit();

        if ( picturesToWatch.MoveNext() )
        {
            if ( VisitedPictures.Contains( picturesToWatch.Current ) )
            {
                return GetNextDestination();
            }

            GameObject pictureGrid = picturesToWatch.Current.GetComponentInChildren<GridSystem>().gameObject;
            return pictureGrid;
        }

        if ( ImportantPictures.Count > 0 )
        {
            PictureInfo importantPicture = ImportantPictures[ ImportantPictures.Count - 1 ];
            ImportantPictures.Remove( importantPicture );

            return importantPicture.GetComponentInChildren<GridSystem>().gameObject;
        }
    

        return GetPlaneOfExit();
    }

    public List<PictureInfo> picturesToWatch_list;
    private void SetPictureToWatch ()
    {   
        picturesToWatch_list = new List<PictureInfo>();
        int lastPictureIndexAdded = 0;

        foreach ( PictureInfo picture in pictures )
        {
            int chanceSelectDestination = 20;
            bool selectPicture = Random.Range( 0, 100 ) < chanceSelectDestination;

            if ( selectPicture || IsMaxJump( picture.index, lastPictureIndexAdded ) )
            {
                Debug.Log( "Aggiungo " + picture.index );
                lastPictureIndexAdded = picture.index;
                if( !ImportantPictures.Contains( picture ))
                    picturesToWatch_list.Add( picture );
            }
        }
        
        utilitySort.transform = transform;
        picturesToWatch_list.Sort( utilitySort.SortByIndex );

        picturesToWatch = picturesToWatch_list.GetEnumerator();
    }


    private bool IsMaxJump (int pictureIndex, int lastPictureIndex)
    {
        if( pictureIndex >= maxJump + lastPictureIndex )
            return true;

        return false;
    }

}
