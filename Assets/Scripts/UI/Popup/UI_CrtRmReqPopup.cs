using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class UI_CrtRmReqPopup : UI_Popup
{
    enum TMP_InputField
    {
        NameInputField,
        MaxMemInputField,
    }

    enum Buttons
    {
        CreateRoomBtn_1,
    }

    enum Images
    {
        Back,
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindInputField(typeof(TMP_InputField));
        BindImage(typeof(Images));

        GetButton((int)Buttons.CreateRoomBtn_1).gameObject.BindEvent(SendCreateRoomReq);
        GetImage((int)Images.Back).gameObject.BindEvent(Close);

        return true;
    }

    public void SendCreateRoomReq()
    {
        string RoomName = GetInputField((int)TMP_InputField.NameInputField).text;
        string MaxMem = GetInputField((int)TMP_InputField.MaxMemInputField).text;
        TCPClient.instance.SendCreateRoomReq(RoomName, MaxMem);
        Destroy(gameObject);
    }

    void Close()
    {
        Destroy(gameObject);
    }
}