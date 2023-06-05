using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshRooms : MonoBehaviour
{
    public GameObject TCP_Client;
    public GameObject RoomsScrollView;

    public void SendRefreshReq()
    {
        Debug.Log("Refresh Rooms...");

        if (TCP_Client.activeSelf)
        {
            RoomsScrollView.GetComponent<RoomScrollViewController>().ClearAllObjects();
            TCPClient.instance.SendRoomListReq();
        }
        else
        {
            // TODO
            // UDP버전도 추가해야 함

        }
    }

}
