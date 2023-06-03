using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_IntToClient : UI_Scene
{
    enum InputField
    {
        IPInputField,
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
        // Server�� ã�ƾ߰���
        // IP, Port�� �Է¹޾Ƽ� ������ ��� ���� ������ �ѱ��

        BindInputField(typeof(InputField));
        BindButton(typeof(Button));
        //GetButton((int)InputField.IPInputField).gameObject.

        GetButton((int)Button.SubmitBtn).gameObject.BindEvent(GotoLobby);

        return true;
    }

    public void GotoLobby()
    {
        Managers.Instance.IPaddress = GetInputField((int)InputField.IPInputField).text;
        Managers.Instance.Port = GetInputField((int)InputField.PortInputField).text;
        Managers.Scene.ChangeScene(Define.Scene.Lobby);
    }
}
