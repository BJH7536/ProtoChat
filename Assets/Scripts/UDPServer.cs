using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDPServer : MonoBehaviour
{
    public InputField PortInput;
    private UdpClient server;
    private Thread serverThread;

    private Dictionary<IPEndPoint, string> clients;

    public void StartServer()
    {
        clients = new Dictionary<IPEndPoint, string>();
        serverThread = new Thread(StartServerThread);
        serverThread.Start();
    }

    private void StartServerThread()
    {
        int port = GetPort();

        try
        {
            server = new UdpClient(port);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Chat.instance.ShowMessage($"Server Started at port:{port}!");
                Debug.Log($"Server Started at port:{port}!");
            });

            while (true)
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedData = server.Receive(ref clientEndPoint);
                OnIncomingData(receivedData, clientEndPoint);
            }
        }
        catch (SocketException ex)
        {
            Debug.Log($"Socket exception: {ex.Message}");
        }
    }

    private int GetPort()
    {
        int port = 7777;

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        MainThreadCaller.EnqueueAction(() =>
        {
            port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);
            autoResetEvent.Set();
        });

        autoResetEvent.WaitOne();
        return port;
    }

    private void OnIncomingData(byte[] data, IPEndPoint clientEndPoint)
    {
        string receivedMessage = Encoding.UTF8.GetString(data);
        if (receivedMessage == "<DISCONNECT>")
        {
            clients.Remove(clientEndPoint);
            Debug.Log($"Client disconnected: {clientEndPoint} ({GetClientNameByEndPoint(clientEndPoint, clients)})");

            return;
        }
        string clientIp = clientEndPoint.Address.ToString();
        string nickname = receivedMessage.Split(':')[0];
        string context = receivedMessage.Split(':')[1];
        Debug.Log($"Server Received: {clientIp} ({nickname}) : {context}");

        // 클라이언트 추가 또는 업데이트
        if (clients.ContainsKey(clientEndPoint))
        {
            // 기존 클라이언트 정보 업데이트
            clients[clientEndPoint] = nickname;
        }
        else
        {
            // 새로운 클라이언트 정보 추가
            clients.Add(clientEndPoint, nickname);
            Debug.Log($"New client added: {clientEndPoint} ({nickname})");
        }

        Broadcast($"{nickname} : {context}", clients);


    }

    private void Broadcast(string data, Dictionary<IPEndPoint, string> cls)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        List<IPEndPoint> disconnectedClients = new List<IPEndPoint>();

        foreach (IPEndPoint clientEndPoint in cls.Keys)
        {
            try
            {
                server.Send(dataBytes, dataBytes.Length, clientEndPoint);
                Debug.Log($"server send message to {clientEndPoint}");
            }
            catch (Exception)
            {
                // 데이터를 보낼 수 없는 클라이언트는 딕셔너리에서 제외.
                disconnectedClients.Add(clientEndPoint);
            }
        }

        foreach (IPEndPoint client in disconnectedClients)
        {
            clients.Remove(client);
            Debug.Log($"Client disconnected: {client} ({GetClientNameByEndPoint(client, clients)})");
        }
    }

    public string GetClientNameByEndPoint(IPEndPoint endPoint, Dictionary<IPEndPoint, string> clients)
    {
        string nickname = null;

        if (clients.TryGetValue(endPoint, out nickname))
        {
            return nickname;
        }

        // 해당 endPoint에 해당하는 클라이언트가 없는 경우
        return "Unknown";
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    private void StopServer()
    {
        if (server != null)
            server.Close();
        if (serverThread != null)
            serverThread.Abort();
    }

    public void debug_showClients()
    {
        Debug.Log("--------------------------");
        foreach(var c in clients)
        {
            Debug.Log($"{c.Key} | {c.Value}");
        }
        Debug.Log("--------------------------");
    }
}

public class ClientInfo
{
    public IPEndPoint ClientEndPoint;
    public string clientName;

    public ClientInfo(IPEndPoint clientEndPoint)
    {
        ClientEndPoint = clientEndPoint;
        clientName = "Guest";
    }

}