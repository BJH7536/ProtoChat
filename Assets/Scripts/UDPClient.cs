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
        Debug.Log($"Client is set to {ip} : {port}");

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

                Chat.instance.ShowMessage(receivedMessage);

                Debug.Log($"Received: {receivedMessage}");
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionReset) // 연결이 강제로 차단되지 않은 경우에만 예외가 기록됩니다.
                {
                    Debug.Log($"Socket exception: {ex.Message}");
                }
            }
            catch (OperationCanceledException) // 연결 취소 시 오류 메시지를 피하기 위해 처리합니다.
            {
                Debug.Log("connect Error");
                // 연결 취소 시 아무 것도 수행되지 않고 루프가 종료됩니다.
            }
        }
    }

    void OnApplicationQuit()
    {
        if (client != null)
            client.Close();

        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}
