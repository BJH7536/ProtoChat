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

        if (Managers.Instance.role == Role.Server)                          // Ŭ���̾�Ʈ ����϶��� Room���� ��û ����
            GetButton((int)Buttons.CreateRoomBtn).gameObject.SetActive(false);
        //Debug.Log("Load-------------------");
        //Debug.Log(Managers.Instance.role);
        //Debug.Log(Managers.Instance.protocol);
        //Debug.Log("-----------------------");

        #region �������ݰ� ���ҿ� ���� ��� ������Ʈ ����
        if (Managers.Instance.role == Role.Client)
        {
            TCPServer.SetActive(false);
            UDPServer.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.TCP)       // �ռ� TCP�� ����������,
        {
            UDPServer.SetActive(false);
            UDPClient.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.UDP)      // �ռ� UDP�� ����������,
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
