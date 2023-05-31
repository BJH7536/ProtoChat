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
        //GetButton((int)Buttons.SubmitBtn).gameObject.BindEvent(submit);

        //Debug.Log("Load-------------------");
        //Debug.Log(Managers.Instance.role);
        //Debug.Log(Managers.Instance.protocol);
        //Debug.Log("-----------------------");

        #region �������ݰ� ���ҿ� ���� ������Ʈ ����
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

}
