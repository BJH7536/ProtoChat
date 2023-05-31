using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Define;

public class UI_Init : UI_Scene
{
    // TODO
    // 여기서 라디오버튼으로
    // Role과 Protocol을 정해서 submit버튼을 누르면
    // 다음으로 넘어가는데,
    // 여기서 정한 거에 따라 다른 Scene으로 넘어가게끔.

    // 1. Server을 정했으면
    // 넘어감과 동시에 port를 입력받아서 서버를 열게끔

    // 2. Client를 정했으면
    // 넘어갈 때 서버 IP와 Port를 입력받아서 해당 서버를 연결 할 수 있게끔

    enum Buttons
    {
        SubmitBtn,
    }

    public GameObject Selection4Role;
    public GameObject Selection4Protocol;

    public Role role;
    public Protocol protocol;

    private void Start()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        GetButton((int)Buttons.SubmitBtn).gameObject.BindEvent(submit);

        return true;
    }

    public Role findRole()
    {
        if (Selection4Role.transform.Find("Server").GetComponent<Toggle>().isOn)
            return Role.Server;
        else
            return Role.Client;
    }

    public Protocol findProtocol()
    {
        if (Selection4Protocol.transform.Find("TCP").GetComponent<Toggle>().isOn)
            return Protocol.TCP;
        else
            return Protocol.UDP;

    }

    public void submit()        // 서버/클라이언트 와 TCP/UDP를 결정한 후 버튼
    {
        Managers.Instance.role = role = findRole();
        Managers.Instance.protocol = protocol = findProtocol();

        //Debug.Log("Submitted!---------------");
        //Debug.Log($"Role : {role}");
        //Debug.Log($"Protocol : {protocol}");
        //Debug.Log("-------------------------");

        Managers.Scene.ChangeScene(Define.Scene.Lobby);
    }

}