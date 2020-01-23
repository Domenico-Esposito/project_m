using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour
{

    public GameObject marker_base;
    public GameObject marker_leader;
    
    public void SetColorGroup ( Color color )
    {
        marker_base.GetComponent<Image>().color = color;
        marker_leader.GetComponent<Image>().color = color;
    }

    private void HideAllMarker ()
    {
        marker_base.SetActive( false );
        marker_leader.SetActive( false );
    }

    public void ShowLeader ()
    {
        HideAllMarker();
        marker_leader.SetActive( true );
    }

    public void ShowBase ()
    {
        HideAllMarker();
        marker_base.SetActive( true );
    }
}
