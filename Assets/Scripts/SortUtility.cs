using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Museum.Utility
{
    public class Sort
    {

        public Transform transform;
        public Dictionary<GameObject, List<GameObject>> picturesOnWalls;

        public int SortByIndexPictureInWalls ( GameObject wallX, GameObject wallY )
        {

            GameObject quadro_x = picturesOnWalls[ wallX ][ 0 ];
            GameObject quadro_y = picturesOnWalls[ wallY ][ 0 ];

            return SortByIndexPicture( quadro_x, quadro_y );
        }

        public float GetPathLength ( GameObject picture )
        {
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath( transform.position, picture.transform.GetChild( 0 ).transform.position, 1, p );

            float lng = 0;

            for ( int i = 0; i < p.corners.Length - 1; i++ )
            {
                lng += Vector3.Distance( p.corners[ i ], p.corners[ i + 1 ] );
            }

            return lng;
        }

        public int SortByIndexPlace( GameObject x, GameObject y )
        {
            return SortByIndexPicture( x, y );
        }

        public int SortByIndexPicture ( GameObject x, GameObject y )
        {

            float index_1 = x.GetComponent<PictureInfo>().index;
            float index_2 = y.GetComponent<PictureInfo>().index;

            if ( index_1 < index_2 ) return -1;
            if ( index_1 > index_2 ) return 1;
            return 0;

        }

        public int Distanza ( GameObject x, GameObject y )
        {
            float distance_1 = GetPathLength( x );
            float distance_2 = GetPathLength( y );

            if ( distance_1 < distance_2 ) return -1;
            if ( distance_1 > distance_2 ) return 1;
            return 0;
        }

    }
}
