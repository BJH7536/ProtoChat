using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System;
using System.IO;

public class TCPServer : MonoBehaviour
{
    public static TCPServer instance;
    void Awake() => instance = this;

    static ServerClient ServerInternalClient;

    //public InputField PortInput;
    public GameObject UI_Lobby;
    string ServerPort;

    List<ServerClient> tcpClients;                          // 연결된 모든 클라이언트
    List<ServerClient> tcpDisconnectClients;                // 연결이 끊긴 클라이언트

    List<Room> Rooms;                                       // 모든 Room들

    TcpListener tcpServer;                                  // 실제 서버?
    bool serverStarted;                                     // 서버가 열리면 True

    // 이미지를 저장할 경로
    private string imageSavePath = @"C:\ProtoChatData\Server";

    public void ServerCreate()
	{
        tcpClients = new List<ServerClient>();
        tcpDisconnectClients = new List<ServerClient>();    // 초기 설정
        Rooms = new List<Room>();                           // 모든 Room들
        ServerPort = Managers.Instance.Port.Trim();

        ServerInternalClient = new ServerClient(TCPClient.instance.tcpSocket);
        ServerInternalClient.clientName = Managers.Instance.ClientName;

        // 이미지 저장 폴더 생성
        if (!Directory.Exists(imageSavePath))
        {
            Directory.CreateDirectory(imageSavePath);
        }

        try             // 디버그를 위한 try-catch문
        {
            int port = ServerPort == "" ? 7777 : int.Parse(ServerPort);     // port 안정했으면 임의로 7777을 쓴다
            tcpServer = new TcpListener(IPAddress.Any, port);               // 새로운 Listener 소켓을 생성 (IPAddress.Any : 자기 컴퓨터의 IP 주소)
            tcpServer.Start();                                              // 서버가 열리면 Bind하고

            StartListening();                                               // Listen 시작
            serverStarted = true;
            ShowNoti($"서버가 {port}에서 시작되었습니다.");                     // 알림
        }
        catch (Exception e) 
        {
            ShowNoti($"Socket error: {e.Message}");
        }
	}

	void Update()
	{
        if (!serverStarted) return;                 // 서버가 열려있을 때만 Update가 돌아가도록

        foreach (ServerClient c in tcpClients)         // 연결되어 있는 client들에 대해...
        {
            // 클라이언트가 여전히 연결되있나?
            if (!IsConnected(c.tcp))                    // 클라이언트 연결이 끊겼다면
            {
                c.tcp.Close();                              // 소켓을 닫고
                tcpDisconnectClients.Add(c);                // disconnectList에 해당 클라이언트를 추가
                continue;
            }
            // 클라이언트로부터 체크 메시지를 받는다
            else                                        // 클라이언트 연결이 끊기지 않았다면
            {
                NetworkStream s = c.tcp.GetStream();                        // 데이터의 흐름을 담당하는 NetworkStream
                if (s.DataAvailable)                                        // 이 데이터가 존재한다면
                {
                    string data = new StreamReader(s, true).ReadLine();     // StreamReader를 통해서 읽어오고
                    if (data != null)
                        OnIncomingData(c, data);                            // 그 읽은 데이터가 null이 아니면 OnIncomingData를 호출한다. 
                }
            }
        }

		for (int i = 0; i < tcpDisconnectClients.Count - 1; i++)
		{
            ShowNoti($"{tcpDisconnectClients[i].clientName} 연결이 끊어졌습니다");

            tcpClients.Remove(tcpDisconnectClients[i]);
            tcpDisconnectClients.RemoveAt(i);
		}
	}

	bool IsConnected(TcpClient c)
	{
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);     // client에게 테스트용 1 byte를 보냈다가 정상적으로 잘 받으면 True

                return true;
            }
            else
                return false;
        }
        catch 
        {
            return false;
        }
	}

	void StartListening()
	{
        tcpServer.BeginAcceptTcpClient(AcceptTcpClient, tcpServer);           // 비동기로 듣기를 시작
	}

    void AcceptTcpClient(IAsyncResult ar)                               // Client의 연결 요청이 들어올 때 호출되는 함수
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        tcpClients.Add(new ServerClient(listener.EndAcceptTcpClient(ar))); // clients 리스트에 ServerClient 객체 추가
        StartListening();                                               // 다시 또 듣기 시작

        // 메시지를 연결된 모두에게 보냄
        Broadcast("%NAME", new List<ServerClient>() { tcpClients[tcpClients.Count - 1] });
    }

    void OnIncomingData(ServerClient c, string data)
    {
        if (data.StartsWith("&NAME"))                         // 수신한 데이터가 &NAME으로 시작한다면, (= 연결 성공 메세지)
        {
            c.clientName = data.Split('|')[1];
            Debug.Log($"{c.clientName}이(가) 연결되었습니다");

            if (c.clientName != "@Server")
                ShowNoti($"{c.clientName}이(가) 연결되었습니다");
        }
        else if (data.StartsWith("&CREATEROOM"))           // CREATEROOM 요청이 들어올 때 방 추가.
        {
            string[] roomData = data.Split('|');
            if (roomData.Length == 3)
            {
                string roomName = roomData[1];
                int maxPlayers;
                if (int.TryParse(roomData[2], out maxPlayers))
                {
                    CreateRoom(roomName, maxPlayers, c);
                }
            }
        }
        else if (data.StartsWith("&CHAT"))                  // 클라이언트가 채팅 메시지를 보냈을 때
        {
            string[] chatData = data.Substring(6).Split('|');
            string roomName = chatData[0];
            string message = chatData[1];

            Room clientRoom = GetClientRoom(c, roomName);
            if (clientRoom != null)
            {
                if (roomName != "")
                {
                    BroadcastToRoom($"%CHAT|{c.clientName}|{message}", clientRoom);
                }
            }
        }
        else if (data.StartsWith("&ENTER"))
        {
            string[] enterData = data.Substring(7).Split('|');
            string roomName = enterData[0];

            // 채팅방에 클라이언트 입장 처리
            EnterRoom(c, roomName);
        }
        else if (data.StartsWith("&LEAVE"))
        {
            string roomName = data.Split('|')[1];
            LeaveRoom(c, roomName);
        }
        else if (data.StartsWith("&IMAGE"))                  // 클라이언트가 이미지를 보냈을 때
        {
            string[] datas = data.Split('|');
            string roomName = datas[1];
            string imageString = datas[2];
            // 이미지 데이터 받기
            byte[] imageData = Convert.FromBase64String(imageString);

            // 이미지 파일로 저장
            string imageName = $"image_{DateTime.Now:yyyyMMddHHmmss}.png";
            string imagePath = Path.Combine(imageSavePath, imageName);
            File.WriteAllBytes(imagePath, imageData);

            //같은 방 내의 클라이언트들에게 멀티캐스트
            Room clientRoom = GetClientRoom(c, roomName);
            BroadcastToRoom($"%IMAGE|{c.clientName}|{imageString}", clientRoom);
        }
        else if (data == "&ROOMLIST")                       // 클라이언트가 채팅방 목록 요청을 보냈을 때
        {
            SendRoomList(c);                                // 채팅방 목록 정보를 해당 클라이언트에게 전송
        }
        else if (data == "&USERLIST")
        {
            SendUserList(c);
        }
        Debug.Log($"{c.clientName}({c.tcp.Client.RemoteEndPoint}) : {data}");

        return;
    }

    void Broadcast(string data, List<ServerClient> cl) 
    {
        foreach (var c in cl) 
        {
            try 
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());      // 쓰기 모드 활성화 StreamWriter
                writer.WriteLine(data);                                         // client들에게 Write한다
                writer.Flush();                                                 // 지금까지 쓴 data를 강제로 내보낸다
            }
            catch (Exception e) 
            {
                ShowNoti($"Error : message sending failed to '{c.clientName}'");
            }
        }
    }

    Room GetClientRoom(ServerClient client, string roomName)
    {
        foreach (Room room in Rooms)
        {
            if (room.getMembers().Contains(client) && room.roomName == roomName)
            {
                return room;
            }
        }
        return null;
    }

    void BroadcastToRoom(string data, Room room)
    {
        if (room != null)
        {
            Broadcast(data, room.getMembers());
        }
        else
        {
            ShowNoti($"'{room.roomName}'이라는 이름의 채팅방을 찾을 수 없습니다.");
            Debug.Log($"'{room.roomName}'이라는 이름의 채팅방을 찾을 수 없습니다.");
        }
    }

    void CreateRoom(string roomName, int maxPlayers, ServerClient client)
    {
        // 이미 같은 이름의 방이 존재하는지 확인
        if (Rooms.Exists(r => r.roomName == roomName))
        {
            ShowNoti($"'{roomName}'이라는 이름의 방이 이미 존재합니다.");
            SendNoti($"'{roomName}'이라는 이름의 방이 이미 존재합니다.", client);
            return;
        }

        // 새로운 Room 생성
        Room room = new Room(roomName, maxPlayers);
        Rooms.Add(room);

        ShowNoti($"'{roomName}' 방이 생성되었습니다.");
        SendNoti($"'{roomName}' 방이 생성되었습니다.", client);
    }

    void SendRoomList(ServerClient client)
    {
        string roomListData = "%ROOMLIST|"; // 채팅방 목록 데이터를 담을 문자열 변수 초기화

        // 모든 채팅방에 대한 정보를 문자열에 추가
        foreach (Room room in Rooms)
        {
            roomListData += $"{room.roomName}|{room.currentMemNum}|{room.MaxMemNum}|";

            // 채팅방 구성원 정보를 추가
            foreach (ServerClient member in room.getMembers())
            {
                roomListData += $"{member.clientName},";
            }

            // 마지막 구분자 제거
            if (room.getMembers().Count > 0)
                roomListData = roomListData.Remove(roomListData.Length - 1);

            roomListData += "|";
        }

        // 마지막 구분자 제거
        if (Rooms.Count > 0)
            roomListData = roomListData.Remove(roomListData.Length - 1);

        // 채팅방 목록 데이터를 클라이언트에 전송
        Broadcast(roomListData, new List<ServerClient>() { client });
    }

    void SendUserList(ServerClient client)
    {
        string userListData = "%USERLIST|"; // 유저 목록 데이터를 담을 문자열 변수 초기화

        // 모든 유저에 대한 정보를 문자열에 추가
        foreach (ServerClient _client in tcpClients)
        {
            userListData += $"{_client.clientName}|";
        }

        // 유저 목록 데이터를 클라이언트에 전송
        Broadcast(userListData, new List<ServerClient>() { client });
    }

    void EnterRoom(ServerClient client, string roomName)
    {
        // 클라이언트가 이미 채팅방에 있는지 확인
        Room existingRoom = GetClientRoom(client, roomName);
        if (existingRoom != null)
        {
            // 이미 채팅방에 있으면 해당 방을 나가고 새로운 방으로 입장
            LeaveRoom(client,roomName);
        }

        string data;

        // 채팅방 이름에 해당하는 방을 찾기
        Room targetRoom = Rooms.Find(room => room.roomName == roomName);
        if (targetRoom != null)
        {
            // 채팅방에 입장
            if (targetRoom.currentMemNum < targetRoom.MaxMemNum)
            {
                targetRoom.addMember(client);
                data = $"{client.clientName}님이 '{roomName}' 채팅방에 입장하셨습니다.";
            }
            else
                data = $"'{roomName}' 채팅방에 입장할 수 없습니다. 방이 가득 찼습니다.";
        }
        else
            data = $"'{roomName}' 채팅방을 찾을 수 없습니다.";

        ShowNoti(data);
        SendNoti(data, client);
    }

    void LeaveRoom(ServerClient client, string roomName)
    {
        Room clientRoom = GetClientRoom(client, roomName);
        if (clientRoom != null)
        {
            clientRoom.removeMember(client);
            BroadcastToRoom($"%CHAT|@Server|{client.clientName}님이 채팅방을 나갔습니다.", clientRoom);

            if (clientRoom.getMembers().Count == 0)
            {
                Rooms.Remove(clientRoom);
                ShowNoti($"{clientRoom.roomName} 방이 삭제되었습니다.");
            }
        }
        else
        {
            ShowNoti($"클라이언트 {client.clientName}은(는) 어떤 채팅방에도 속해있지 않습니다.");
        }
    }

    public void ShowNoti(string context)
    {
        GameObject noti = Managers.Resource.Instantiate("UI/Notification", null);
        if(context != "")
            noti.GetComponentInChildren<Notification>().context = context;

        Debug.Log(context);
    }
    
    public void SendNoti(string context, ServerClient client)
    {
        string data = $"%NOTI|{context}";

        Broadcast(data, new List<ServerClient>() { client });
    }
    
    public void ServerShutdown()
    {
        // 서버가 시작되었는지 확인
        if (!serverStarted) return;

        // 모든 클라이언트의 연결을 닫고 리스트 비우기
        foreach (var client in tcpClients)
        {
            client.tcp.Close();  // 각 클라이언트 소켓을 닫음
        }
        tcpClients.Clear();

        // TCP 리스너 종료
        tcpServer.Stop();  // 리스너를 중지하여 포트 반환
        serverStarted = false;

        Debug.Log("서버가 종료되었습니다. 포트가 반환되었습니다.");
    }
    
    void OnApplicationQuit()
    {
        ServerShutdown();  // 서버 종료 및 자원 해제
    }
}

public class ServerClient           // 서버에서 클라이언트에 대한 정보를 갖기위한 클래스
{
    public TcpClient tcp;           // tcp 통신이라서 tcpclient
    public string clientName;       // 클라이언트 이름

    public ServerClient(TcpClient clientSocket)     // 생성자
    {
        clientName = "Guest";                       // 초기 이름 Guest
        tcp = clientSocket;                         // tcp에는 들어온 소켓을 넣어준다
    }
}

public class Room                   // 서버에서 Room에 대한 정보를 유지하기 위한 클래스
{
    public string roomName;                     // Room 이름
    public List<ServerClient> clientsInRoom;    // 한 Room 안에 있는 Client들에 대한 리스트
    public int currentMemNum;
    public int MaxMemNum;

    public Room(string Name, int MaxNum)
    {
        roomName = Name;
        MaxMemNum = MaxNum;
        clientsInRoom = new List<ServerClient>();
    }
    public Room(string Name, int MaxNum, List<ServerClient> clients)
    {
        roomName = Name;
        MaxMemNum = MaxNum;
        clientsInRoom = clients;
    }

    public void                 addMember(ServerClient client)
    {
        if (currentMemNum < MaxMemNum)
        {
            clientsInRoom.Add(client);
            currentMemNum++;
        }
        else
        {
            TCPServer.instance.ShowNoti($"{roomName} 에 현재인원이 최대, 더이상 수용할 수 없음");
        }
    }
    public void                 removeMember(ServerClient client)
    {
        if (currentMemNum == 0)
        {
            TCPServer.instance.ShowNoti($"{roomName}에 현재인원이 아무도 없음, 제거할 수 없음");
            return;
        }
        clientsInRoom.Remove(client);
        currentMemNum--;
    }
    public List<ServerClient>   getMembers()
    {
        return clientsInRoom;
    }
    public void                 removeAllMember()
    {
        clientsInRoom.Clear();
    }

    ~Room()     //소멸자
    {
        removeAllMember();
    }

}