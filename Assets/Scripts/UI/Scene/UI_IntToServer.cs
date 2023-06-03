using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_IntToServer : UI_Scene
{
    enum InputField
    {
        PortInputField,
    }

    enum Button
    {
        SubmitBtn,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // TODO
        // ���� ��Ʈ�� ��� ���������� �ѱ��
        BindButton(typeof(Button));
        BindInputField(typeof(InputField));

        GetButton((int)Button.SubmitBtn).gameObject.BindEvent(GotoServerLobby);

        return true;
    }

    public void GotoServerLobby()
    {
        Managers.Instance.IPaddress = "127.0.0.1";
        Managers.Instance.Port = GetInputField((int)InputField.PortInputField).text;
        Managers.Scene.ChangeScene(Define.Scene.Lobby);
    }
}
