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

        //if (Managers.Instance.role == Role.Client)
        //    DebugLogManager.Instance.gameObject.SetActive(false);

        //if (Managers.Instance.role == Role.Server)                          // 클라이언트 모드일때만 Room생성 요청 가능
        //    GetButton((int)Buttons.CreateRoomBtn).gameObject.SetActive(false);
        
        //Debug.Log("Load-------------------");
        //Debug.Log(Managers.Instance.role);
        //Debug.Log(Managers.Instance.protocol);
        //Debug.Log("-----------------------");

        #region 프로토콜과 역할에 따라 통신 오브젝트 정리
        if (Managers.Instance.role == Role.Client)
        {
            TCPServerObj.SetActive(false);
            UDPServerObj.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.TCP)       // 앞서 TCP를 선택했으면,
        {
            UDPServerObj.SetActive(false);
            UDPClientObj.SetActive(false);
        }
        if (Managers.Instance.protocol == Protocol.UDP)      // 앞서 UDP를 선택했으면,
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
        // UDP 버전도 만들자
        #endregion

        return true;
    }

}
