using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public delegate void MessageDelegate(bool isSend, string text, string user, Texture texture);
public delegate void ImageDelegate(bool isSend, string text, string user, string Path);

public class TCPClient : MonoBehaviour
{
    public static TCPClient instance;
    void Awake() => instance = this;

    //public InputField IPInput, PortInput, NickInput;
    string ClientIPaddress, ClientPort, ClientName;
    public GameObject UI_Lobby;								// UI
	public RoomScrollViewController ScrollView_Room;        // UI : Scrollview_Room
    public UserScrollViewController ScrollView_Users;		// UI : ScrollView_Users
    public TcpClient tcpSocket;                             // 서버와 통신할 TcpClient

    List<Room> LocalRooms;

    public static event MessageDelegate textdelegate;
    public static event ImageDelegate imagedelegate;

    public bool ImageLoaded = false;
    public string LoadedImage = null; // 경로

    // 이미지를 저장할 경로
    private string imageSavePath = @"C:\ProtoChatData\Client";

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

        // 이미지 저장 폴더 생성
        if (!Directory.Exists(imageSavePath))
        {
            Directory.CreateDirectory(imageSavePath);
        }

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

    void Send(string data)
    {
        if (!socketReady) return;

        writer.WriteLine(data);
        writer.Flush();
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

    void OnIncomingData(string data)
	{
        if (data == "%NAME") 
		{
            ClientName = Managers.Instance.ClientName == "" ? "Guest" + UnityEngine.Random.Range(10, 99) : Managers.Instance.ClientName;	
            // 정한 닉네임이 없으면 Guest(임의숫자)의 닉네임으로 설정

    		Send($"&NAME|{ClientName}");
			return;
		}
		else if (data.StartsWith("%ROOMLIST"))	// 서버로부터 채팅방 목록 데이터를 받았을 때
        {
            ProcessRoomList(data);				// 채팅방 목록 데이터 처리
            return;
        }
        else if (data.StartsWith("%USERLIST"))	// 서버로부터 채팅방 목록 데이터를 받았을 때
        {
            ProcessUserList(data);				// 채팅방 목록 데이터 처리
            return;
        }
        else if (data.StartsWith("%CHAT"))		// 서버로부터 채팅방 내부 채팅 메세지를 받았을 때
		{
			ProcessChatMessage(data);
      		return;
		}
        else if(data.StartsWith("%IMAGE"))
        {
            ProcessChatImage(data);
            return;
        }
        else if (data.StartsWith("%NOTI"))
        {
            string context = data.Split('|')[1];
            ShowNoti(context);
            return;
        }

	}

    private void ProcessChatImage(string data)
    {
        string[] chatData = data.Split('|');
        string Sender = chatData[1];
        string imageString = chatData[2];

        // 이미지 데이터 받기
        byte[] imageData = Convert.FromBase64String(imageString);

        // 이미지 파일로 저장
        string imageName = $"image_{DateTime.Now:yyyyMMddHHmmss}.png";
        string imagePath = Path.Combine(imageSavePath, imageName);
        File.WriteAllBytes(imagePath, imageData);

        Debug.Log(imagePath);

        if (Sender == ClientName)
        {
            imagedelegate(true, "image", "나", imagePath);
        }
        else
        {
            imagedelegate(false, "image", Sender, imagePath);
        }

    }

    private void ProcessChatMessage(string data)        // 채팅 메세지 수신 후 처리
    {
        string[] chatData = data.Split('|');
        string Sender = chatData[1];
        string message = chatData[2];

        if(Sender == ClientName)
        {
            textdelegate(true, message, "나", null);
        }
        else
        {
            textdelegate(false, message, Sender, null);
        }
    }


    public void SendChatMessage(string roomName, string message)
    {
        // 요청 메시지 생성
        string data = $"&CHAT|{roomName}|{message}";

        // 메시지 전송
        Send(data);
    }

    public void SendImage(string roomName, byte[] imageData)
    {
        // 이미지 파일을 바이트 배열로 변환
        if (imageData != null)
        {
            // 이미지 데이터를 Base64 인코딩하여 문자열로 변환
            string imageString = Convert.ToBase64String(imageData);

            // 이미지 데이터를 서버로 전송
            string data = $"&IMAGE|{roomName}|{imageString}";
            Send(data);
        }
    }

    public Sprite MakeImageFromBytes(byte[] byteTexture)
    {
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(byteTexture);

        Rect rect = new Rect(0, 0, texture.width, texture.height);

        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
    }

    public void SendEnterRoom(string roomName)
    {
        string data = $"&ENTER|{roomName}";
        Send(data);
    }
    
    public void SendLeaveRoom(string roomName)
    {
        string data = $"&LEAVE|{roomName}";
        Send(data);
    }

    public void SendCreateRoomReq(string roomName, string maxPlayers)
    {
        // 요청 메시지 생성
        string message = $"&CREATEROOM|{roomName}|{maxPlayers}";

        // 메시지 전송
        Send(message);
    }

    public void SendRoomListReq()
    {
        Send("&ROOMLIST");
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
            RoomImage localRoom = ScrollView_Room.AddNewUiObject();
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

    public void SendUserListReq()
    {
        Send("&USERLIST");
    }

    private void ProcessUserList(string data)
    {
        Debug.Log(data);
        // 서버로부터 받은 유저 목록 데이터 파싱
        string[] userDataArray = data.Split('|');

        for (int i = 1; i < userDataArray.Length - 1; i++)
        {
            string roomName = userDataArray[i];

            // 유저 목록 정보 출력
            UserImage localUser = ScrollView_Users.AddNewUiObject();
            localUser.Username = roomName;
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

        Debug.Log(context);
    }
}
