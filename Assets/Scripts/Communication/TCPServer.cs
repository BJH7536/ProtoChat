using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;

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

    TcpListener tcpServer;     // 실제 서버?
    bool serverStarted;     // 서버가 열리면 True

    public void ServerCreate()
	{
        tcpClients = new List<ServerClient>();
        tcpDisconnectClients = new List<ServerClient>();    // 초기 설정
        Rooms = new List<Room>();                           // 모든 Room들
        ServerPort = Managers.Instance.Port.Trim();

        ServerInternalClient = new ServerClient(TCPClient.instance.tcpSocket);
        ServerInternalClient.clientName = Managers.Instance.ClientName;

        try             // 디버그를 위한 try-catch문
        {
            int port = ServerPort == "" ? 7777 : int.Parse(ServerPort);     // port 안정했으면 임의로 7777을 쓴다
            tcpServer = new TcpListener(IPAddress.Any, port);                          // 새로운 Listener 소켓을 생성 (IPAddress.Any : 자기 컴퓨터의 IP 주소)
            tcpServer.Start();                                                         // 서버가 열리면 Bind하고

            StartListening();                                                       // Listen 시작
            serverStarted = true;
            ShowNoti($"서버가 {port}에서 시작되었습니다.");          // 알림
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
                tcpDisconnectClients.Add(c);                      // disconnectList에 해당 클라이언트를 추가
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
            Broadcast($"{tcpDisconnectClients[i].clientName} 연결이 끊어졌습니다", tcpClients);

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
            Broadcast($"{c.clientName}이(가) 연결되었습니다", tcpClients);
            return;
        }
        else if (data.StartsWith("&CREATEROOM|"))           // CREATEROOM 요청이 들어올 때 방 추가.
        {
            string[] roomData = data.Split('|');
            if (roomData.Length == 3)
            {
                string roomName = roomData[1];
                int maxPlayers;
                if (int.TryParse(roomData[2], out maxPlayers))
                {
                    CreateRoom(roomName, maxPlayers, c);
                    return;
                }
            }
        }
        else if (data == "&ROOMLIST")                       // 클라이언트가 채팅방 목록 요청을 보냈을 때
        {
            SendRoomList(c);                                // 채팅방 목록 정보를 해당 클라이언트에게 전송
            return;
        }
        else if (data.StartsWith("&CHAT"))                  // 클라이언트가 채팅 메시지를 보냈을 때
        {
            Room clientRoom = GetClientRoom(c);
            if (clientRoom != null)
            {
                BroadcastToRoom($"{c.clientName} : {data.Substring(6)}", clientRoom);
            }
            return;
        }

        Broadcast($"{c.clientName} : {data}", tcpClients);     // client c가 보낸 data를 모든 client들에게 broadcast한다
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
                ShowNoti($"쓰기 에러 : {e.Message}를 클라이언트에게 {c.clientName}");
            }
        }
    }

    Room GetClientRoom(ServerClient client)
    {
        foreach (Room room in Rooms)
        {
            if (room.getMembers().Contains(client))
            {
                return room;
            }
        }
        return null;
    }

    void BroadcastToRoom(string data, Room room)
    {
        Broadcast(data, room.getMembers());
    }

    void CreateRoom(string roomName, int maxPlayers, ServerClient client)
    {
        // 이미 같은 이름의 방이 존재하는지 확인
        if (Rooms.Exists(r => r.roomName == roomName))
        {
            ShowNoti($"'{roomName}'이라는 이름의 방이 이미 존재합니다.");
            return;
        }

        // 새로운 Room 생성
        Room room = new Room(roomName, maxPlayers);
        room.addMember(client);
        Rooms.Add(room);

        ShowNoti($"'{roomName}' 방이 생성되었습니다.");
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

    public void ShowNoti(string context)
    {
        GameObject noti = Managers.Resource.Instantiate("UI/Notification", UI_Lobby.transform);
        if(context != "")
            noti.GetComponent<Notification>().context = context;
    }
    public void OnApplicationQuit()
    {
        tcpServer.Stop();
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