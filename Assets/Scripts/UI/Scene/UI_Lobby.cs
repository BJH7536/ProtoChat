using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_Lobby : UI_Scene
{
    public GameObject TCPServerObj;
    public GameObject TCPClientObj;

    public GameObject UDPServerObj;
    public GameObject UDPClientObj;

    enum Buttons
    {
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

        if (Managers.Instance.role == Role.Client)
            DebugLogManager.Instance.gameObject.SetActive(false);

        //if (Managers.Instance.role == Role.Server)                          // Ŭ���̾�Ʈ ����϶��� Room���� ��û ����
        //    GetButton((int)Buttons.CreateRoomBtn).gameObject.SetActive(false);
        
        //Debug.Log("Load-------------------");
        //Debug.Log(Managers.Instance.role);
        //Debug.Log(Managers.Instance.protocol);
        //Debug.Log("-----------------------");

        #region �������ݰ� ���ҿ� ���� ��� ������Ʈ ����
        if (Managers.Instance.role == Role.Client)
        {
            TCPServerObj.SetActive(false);
            UDPServerObj.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.TCP)       // �ռ� TCP�� ����������,
        {
            UDPServerObj.SetActive(false);
            UDPClientObj.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.UDP)      // �ռ� UDP�� ����������,
        {
            TCPServerObj.SetActive(false);
            TCPClientObj.SetActive(false);
        }

        if(Managers.Instance.protocol == Protocol.TCP)
        {
            if (Managers.Instance.role == Role.Server)
                TCPServer.instance.ServerCreate();
            TCPClient.instance.ConnectToServer();
        }

        // TODO
        // UDP ������ ������
        #endregion

        return true;
    }

}
