using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Museum.Utility
{
    public class Sort
    {

        public Transform transform;
        public Dictionary<GameObject, List<PictureInfo>> picturesOnWalls;

        public int SortByIndexPictureInWalls ( GameObject wallX, GameObject wallY )
        {

            PictureInfo quadro_x = picturesOnWalls[ wallX ][ 0 ];
            PictureInfo quadro_y = picturesOnWalls[ wallY ][ 0 ];

            return SortByIndex( quadro_x, quadro_y );
        }

        public float GetPathLength ( GameObject picture )
        {
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath( transform.position, picture.transform.GetChild( 0 ).transform.position, 1, p );

            return GetPathLenght( p );
        }

        public float GetPathLengthPlane ( GameObject plane )
        {
            NavMeshPath p = new NavMeshPath();
            NavMesh.CalculatePath( transform.position, plane.transform.position, NavMesh.AllAreas, p );

            return GetPathLenght( p );
        }

        public float GetPathLenght(NavMeshPath path )
        {
            float lng = 0;

            for ( int i = 0; i < path.corners.Length - 1; i++ )
            {
                lng += Vector3.Distance( path.corners[ i ], path.corners[ i + 1 ] );
            }

            return lng;
        }

        public int SortByIndex(PictureInfo x, PictureInfo y )
        {
            float index_1 = x.index;
            float index_2 = y.index;

            if ( index_1 < index_2 ) return -1;
            if ( index_1 > index_2 ) return 1;
            return 0;
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

        public int DistanzaPicture(PictureInfo x, PictureInfo y )
        {
            return Distanza( x.gameObject, y.gameObject );
        }

        public int Distanza ( GameObject x, GameObject y )
        {
            float distance_1 = GetPathLength( x );
            float distance_2 = GetPathLength( y );

            if ( distance_1 < distance_2 ) return -1;
            if ( distance_1 > distance_2 ) return 1;
            return 0;
        }


        public int DistanzaPlane ( GameObject x, GameObject y )
        {
            float distance_1 = GetPathLengthPlane( x );
            float distance_2 = GetPathLengthPlane( y );

            if ( distance_1 < distance_2 ) return -1;
            if ( distance_1 > distance_2 ) return 1;
            return 0;
        }
    }
}
