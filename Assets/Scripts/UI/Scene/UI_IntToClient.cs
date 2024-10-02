using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_IntToClient : UI_Scene
{
    enum TMP_InputField
    {
        IPInputField,
        PortInputField,
        NameInputField,
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
        // Server를 찾아야겠지
        // IP, Port를 입력받아서 서버를 잡고 다음 씬으로 넘긴다

        BindInputField(typeof(TMP_InputField));
        BindButton(typeof(Button));

        GetInputField((int)TMP_InputField.IPInputField).onValueChanged.AddListener(
                (word) => GetInputField((int)TMP_InputField.IPInputField).text = Regex.Replace(word, @"[^0-9.]", "")
            );

        GetInputField((int)TMP_InputField.NameInputField).onValueChanged.AddListener(
                (word) => GetInputField((int)TMP_InputField.NameInputField).text = Regex.Replace(word, @"[^0-9a-zA-Z가-힣]", "")
            );




        GetButton((int)Button.SubmitBtn).gameObject.BindEvent(GotoLobby);

        return true;
    }

    public void GotoLobby()
    {
        Managers.Instance.IPaddress = GetInputField((int)TMP_InputField.IPInputField).text;
        Managers.Instance.Port = GetInputField((int)TMP_InputField.PortInputField).text;
        Managers.Instance.ClientName = GetInputField((int)TMP_InputField.NameInputField).text;
        Managers.Scene.ChangeScene(Define.Scene.Lobby);
    }
}
