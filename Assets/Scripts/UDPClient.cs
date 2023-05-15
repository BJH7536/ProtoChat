using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UDPClient : MonoBehaviour
{
    public InputField IPInput, PortInput, NickInput;
    string ip;
    int port;
    string clientName;

    private UdpClient client;
    private CancellationTokenSource cancellationTokenSource;

    public void InitializeClient()
    {
        if (cancellationTokenSource != null) // 이전 연결이 있으면 취소합니다.
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
        }

        ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text;
        port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);
        clientName = NickInput.text == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : NickInput.text;     // 정한 닉네임이 없으면 Guest(임의숫자)의 닉네임으로 설정
        Debug.Log($"Clients receive from {ip} : {port}");

        client = new UdpClient();

        cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveMessages(cancellationTokenSource.Token)); // 새로운 백그라운드 태스크를 시작합니다.
    }

    public void OnSendButton(InputField SendInput)
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        if (!Input.GetButtonDown("Submit")) return;
        SendInput.ActivateInputField();
#endif
        if (SendInput.text.Trim() == "") return;

        if (string.IsNullOrEmpty(SendInput.text))
            return;

        byte[] data = Encoding.UTF8.GetBytes($"{clientName}:{SendInput.text}");
        _ = Task.Run(() => SendAsync(data)); // 메시지를 비동기로 전송합니다.
        SendInput.text = "";
    }

    private async Task SendAsync(byte[] data)
    {
        try
        {
            await client.SendAsync(data, data.Length, ip, port);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending message: {ex.Message}");
        }
    }

    private async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult receivedResult = await client.ReceiveAsync(); // await과 함께 비동기 수신을 사용합니다.
                byte[] receivedData = receivedResult.Buffer;
                string receivedMessage = Encoding.UTF8.GetString(receivedData);

                // Ensure this is executed on the main thread using the MainThreadCaller
                UnityMainThreadDispatcher.Instance().Enqueue(() => Chat.instance.ShowMessage(receivedMessage));

                Debug.Log($"Client Received: {receivedMessage}");
            }
            catch (SocketException ex)
            {
                Debug.Log($"Socket exception: {ex.Message}");
                UnityMainThreadDispatcher.Instance().Enqueue(() => Chat.instance.ShowMessage("Server Offline"));
                // Add additional handling for SocketException if necessary
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Connection closed.");
                // Handle connection cancellation
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving message: {ex.Message}");
                // Handle any other exceptions that may occur
            }
        }
    }

    void OnApplicationQuit()
    {
        Exit();
    }

    public void OnExitButton()
    {
        Exit();
    }

    void Exit()
    {
        if (client != null)
        {
            // <DISCONNECT> 메시지를 전송.
            byte[] disconnectData = Encoding.UTF8.GetBytes("<DISCONNECT>");
            _ = Task.Run(() => SendAsync(disconnectData));

            client.Close();
        }

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}