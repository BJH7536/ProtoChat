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

    private List<ClientInfo> clients;

    public void StartServer()
    {
        clients = new List<ClientInfo>();
        serverThread = new Thread(StartServerThread);
        serverThread.Start();
    }

    private void StartServerThread()
    {
        int port = GetPort();
        Debug.Log($"Server Started at port:{port}!");

        try
        {
            server = new UdpClient(port);

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
        string clientIp = clientEndPoint.Address.ToString();
        string nickname = receivedMessage.Split(':')[0];
        string context = receivedMessage.Split(':')[1];
        Debug.Log($"Received: {clientIp} ({nickname}) : {context}");

        byte[] responseData = Encoding.UTF8.GetBytes($"{nickname} : {context}");
        server.Send(responseData, responseData.Length, clientEndPoint);

        ClientInfo checkExistingClient = null;

        foreach (ClientInfo client in clients)
        {
            if (client.ClientEndPoint.Address == clientEndPoint.Address && client.ClientEndPoint.Port == clientEndPoint.Port)
            {
                checkExistingClient = client;
                break;
            }
        }

        if (checkExistingClient == null)
        {
            ClientInfo newClient = new ClientInfo(clientEndPoint);
            newClient.clientName = nickname;
            clients.Add(newClient);
            Debug.Log($"New client added: {clientEndPoint} ({nickname})");
        }

        clients = Broadcast($"{nickname} : {context}", clients);

    }

    private List<ClientInfo> Broadcast(string data, List<ClientInfo> cls)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        List<ClientInfo> updatedClients = new();

        foreach (ClientInfo client in cls)
        {
            try
            {
            server.Send(dataBytes, dataBytes.Length, client.ClientEndPoint);
            Debug.Log($"server sended to {client.ClientEndPoint.Address} : {client.ClientEndPoint.Port}");
            updatedClients.Add(client);
            }
            catch (Exception)
            {
                // 데이터를 보낼 수 없는 클라이언트는 리스트에서 제외.
            }
        }

        return updatedClients;
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