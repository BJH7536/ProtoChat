using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_Lobby : UI_Scene
{
    public GameObject TCPServer;
    public GameObject TCPClient;

    public GameObject UDPServer;
    public GameObject UDPClient;

    enum Buttons
    {
        RefreshRoomsBtn,
        CreateRoomBtn,
    }

    private void Start()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        GetButton((int)Buttons.RefreshRoomsBtn).gameObject.BindEvent(refreshRooms);

        if (Managers.Instance.role == Role.Server)                          // 클라이언트 모드일때만 Room생성 요청 가능
            GetButton((int)Buttons.CreateRoomBtn).gameObject.SetActive(false);
        //Debug.Log("Load-------------------");
        //Debug.Log(Managers.Instance.role);
        //Debug.Log(Managers.Instance.protocol);
        //Debug.Log("-----------------------");

        #region 프로토콜과 역할에 따라 통신 오브젝트 정리
        if (Managers.Instance.role == Role.Client)
        {
            TCPServer.SetActive(false);
            UDPServer.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.TCP)       // 앞서 TCP를 선택했으면,
        {
            UDPServer.SetActive(false);
            UDPClient.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.UDP)      // 앞서 UDP를 선택했으면,
        {
            TCPServer.SetActive(false);
            TCPClient.SetActive(false);
        }
        #endregion

        return true;
    }

    public void refreshRooms()
    {

    }
}
