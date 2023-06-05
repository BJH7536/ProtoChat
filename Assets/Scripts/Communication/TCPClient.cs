using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using System.Linq;

public class TCPClient : MonoBehaviour
{
    public static TCPClient instance;
    void Awake() => instance = this;

    //public InputField IPInput, PortInput, NickInput;
    string ClientIPaddress, ClientPort, ClientName;
    public GameObject UI_Lobby;								// UI
	public RoomScrollViewController Scrollview_Room;        // UI : Scrollview_Room
    public RoomScrollViewController ScrollView_Users;		// UI : ScrollView_Users
    public TcpClient tcpSocket;                             // 서버와 통신할 TcpClient

    List<Room> LocalRooms;

    #region 통신 관련
    bool socketReady;			// socket이 준비되었는지
    NetworkStream stream;
	StreamWriter writer;
    StreamReader reader;
    #endregion 

    public void ConnectToServer()
	{
		// 이미 연결되었다면 함수 무시
		if (socketReady) return;

		// 기본 호스트/ 포트번호
		ClientIPaddress = Managers.Instance.IPaddress;
		ClientPort = Managers.Instance.Port;

        string ip = ClientIPaddress == "" ? "127.0.0.1" : ClientIPaddress;			// 아무것도 안 썻다면 호스트에게 연결을 시도
		int port = ClientPort == "" ? 7777 : int.Parse(ClientPort);

		// 소켓 생성
		try
		{
            tcpSocket = new TcpClient(ip, port);		// 소켓 생성
			stream = tcpSocket.GetStream();			// 네트워크의 스트림을 관찰할 수 있도록
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			socketReady = true;

		}
		catch (Exception e) 
		{
            ShowNoti($"소켓에러 : {e.Message}");
		}
	}

	void Update()
	{
		if (socketReady && stream.DataAvailable)	// 연결되고, 데이터가 오고 있으면?
		{
			string data = reader.ReadLine();
			if (data != null)
				OnIncomingData(data);				// 스트림을 리더로 읽어서 데이터가 유효하면 OnIncomingData 실행
		}
	}

	void OnIncomingData(string data)
	{

		if (data == "%NAME") 
		{
            ClientName = Managers.Instance.ClientName == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : Managers.Instance.ClientName;		// 정한 닉네임이 없으면 Guest(임의숫자)의 닉네임으로 설정
			Send($"&NAME|{ClientName}");
			return;
		}
		else if (data.StartsWith("%ROOMLIST")) // 서버로부터 채팅방 목록 데이터를 받았을 때
        {
            ProcessRoomList(data); // 채팅방 목록 데이터 처리
            return;
        }

        //Chat.instance.ShowMessage(data);
	}

	void Send(string data)
	{
		if (!socketReady) return;

		writer.WriteLine(data);
		writer.Flush();
	}

	public void SendRoomListReq()
	{
		Send("&ROOMLIST");
	}

	public void OnSendButton(InputField SendInput)
	{
#if (UNITY_EDITOR || UNITY_STANDALONE)
		if (!Input.GetButtonDown("Submit")) return;
		SendInput.ActivateInputField();
#endif
		if (SendInput.text.Trim() == "") return;

		string message = SendInput.text;
		SendInput.text = "";
		Send(message);
	}

    public void SendCreateRoomReq(string roomName, string maxPlayers)
    {
        // 요청 메시지 생성
        string message = $"&CREATEROOM|{roomName}|{maxPlayers}";

        // 메시지 전송
        Send(message);
    }

    void ProcessRoomList(string data)
    {
        // 서버로부터 받은 채팅방 목록 데이터 파싱
        string[] roomDataArray = data.Split('|');

        for (int i = 1; i < roomDataArray.Length; i += 4)
        {
            string roomName = roomDataArray[i];
            int currentMemNum = int.Parse(roomDataArray[i + 1]);
            int maxMemNum = int.Parse(roomDataArray[i + 2]);
            string[] members = roomDataArray[i + 3].Split(',');

			// 채팅방 목록 정보를 출력하거나 처리하는 로직을 추가
            //Debug.Log($"Room: {roomName}, Current Members: {currentMemNum}/{maxMemNum}");
            RoomImage localRoom = Scrollview_Room.AddNewUiObject();
			localRoom.roomName = roomName;
			localRoom.curMemNum = currentMemNum;
			localRoom.maxMemNum = maxMemNum;

            foreach (string member in members)
            {
                //Debug.Log($"- {member}");
                localRoom.MemberNames.Add(member);
            }
        }
    }

    void OnApplicationQuit()
	{
		CloseSocket();
	}
	void CloseSocket()
	{
		if (!socketReady) return;

		writer.Close();
		reader.Close();
        tcpSocket.Close();
		socketReady = false;
	}
    public void ShowNoti(string context)
    {
        GameObject noti = Managers.Resource.Instantiate("UI/Notification", UI_Lobby.transform);
        if (context != "")
            noti.GetComponent<Notification>().context = context;
    }
}
