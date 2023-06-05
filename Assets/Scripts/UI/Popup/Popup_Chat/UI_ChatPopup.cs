using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_ChatPopup : UI_Popup
{
    //public Vector3 initPos;
    public GameObject ChatManager;
    ChatManager chatManager;

    public RoomImage roomImage;
    public string roomName;

    enum TMP_InputField
    {
        ChatInputField,
    }

    enum Buttons
    {
        Send,
        Media,
        Close,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //initPos = gameObject.GetComponent<RectTransform>().anchoredPosition;
        //Debug.Log(initPos);

        roomName = roomImage.roomName;

        chatManager = ChatManager.GetComponent<ChatManager>();

        BindInputField(typeof(TMP_InputField));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.Send).gameObject.BindEvent(send);
        GetButton((int)Buttons.Media).gameObject.BindEvent(media);
        GetButton((int)Buttons.Close).gameObject.BindEvent(close);

        return true;

    }

    // TODO
    // ���� ��ư�� ���ε� �� �Լ� �����
    // �̵�� ��ư�� ���ε� �� �Լ� �����
    // close�� �ּ�ȭ ��ư�� �Ȱ���.

    public void send()
    {
        string text = GetInputField((int)TMP_InputField.ChatInputField).text.Trim();
        GetInputField((int)TMP_InputField.ChatInputField).text = "";

        if (text == "")
        {
            GetInputField((int)TMP_InputField.ChatInputField).text = "";
            return;
        }
        else
        {
            TCPClient.instance.SendCreateRoomReq(roomName, text);
            return;
        }

    }

    public void media()
    {

    }

    public void close()
    {
        DestroyImmediate(gameObject);
    }
}