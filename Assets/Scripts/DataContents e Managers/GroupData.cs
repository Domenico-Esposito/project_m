using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupData : MonoBehaviour
{

    public bool despota = false;
    public bool isLeader = false;
    public GameObject leader;

    public List<GroupData> group = new List<GroupData>();

    private MarkerManager markerManager;
    private BotVisitData visitData;

    private Color groupColor; 

    private void Awake ()
    {
        markerManager = GetComponent<MarkerManager>();
        visitData = GetComponent<BotVisitData>();
        InitGroupData();
    }

    public void InitGroupData ()
    {
        if ( isLeader )
        {
            markerManager.SetColorGroup( groupColor );
            markerManager.ShowLeader();
        }
        else
        {
            markerManager.ShowBase();
        }
    }

    public void AddMember ( GroupData memeber )
    {
        group.Add( memeber );
    }

    public void SetGroup ( Color color )
    {
        isLeader = true;
        groupColor = color;
        InitGroupData();
    }

    public void SetLeader ( GameObject myLeader )
    {
        leader = myLeader;
    }

    public void GroupElementSetData ( Color groupColor, bool leaderDespota, GameObject myLeader )
    {
        SetLeader( myLeader );
        markerManager.SetColorGroup( groupColor );

        if( TryGetComponent(out NoChoicesAgent noChoicesBot ) )
        {
            visitData.importantPictures.Clear();
            visitData.visitedPictures.Clear();
        }
    }

    public void CheckMembers ()
    {
        group = ShuffleList( group );
        group.RemoveAll( ( GroupData obj ) => obj.gameObject.activeInHierarchy && obj.leader == this );
    }

    public bool LeaderIsAlive
    {
        get => CheckLeader();
    }

    private bool CheckLeader ()
    {
        return leader && !leader.activeInHierarchy;
    }


    public void NotifyDestinationChoice ()
    {
        CheckMembers();

        foreach ( GroupData member in group )
        {
            member.GetComponent<BaseAgent>().ReceiveLeaderChoice( GetComponent<BotVisitData>().destination );
            if ( despota )
            {
                member.GetComponent<BaseAgent>().activeBot = true;
            }
        }
    }

    private List<E> ShuffleList<E> ( List<E> inputList )
    {
        List<E> randomList = new List<E>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while ( inputList.Count > 0 )
        {
            randomIndex = r.Next( 0, inputList.Count );
            randomList.Add( inputList[ randomIndex ] );
            inputList.RemoveAt( randomIndex );
        }

        return randomList;
    }

}
