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
        string RoomName = GetInputField((int)TMP_InputField.NameInputField).text.Trim();
        string MaxMem = GetInputField((int)TMP_InputField.MaxMemInputField).text.Trim();

        if (string.IsNullOrEmpty(RoomName) || string.IsNullOrEmpty(MaxMem))
        {
            TCPClient.instance.ShowNoti("방을 만들 수 없습니다.");
            return;
        }
        
        TCPClient.instance.SendCreateRoomReq(RoomName, MaxMem);
        Destroy(gameObject);
    }

    void Close()
    {
        Destroy(gameObject);
    }
}