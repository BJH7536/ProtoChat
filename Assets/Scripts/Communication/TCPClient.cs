using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;

public class TCPClient : MonoBehaviour
{
	public InputField IPInput, PortInput, NickInput;
	string clientName;


    bool socketReady;			// socket이 준비되었는지
    TcpClient tcpSocket;			// 서버와 통신할 TcpClient
    NetworkStream stream;
	StreamWriter writer;
    StreamReader reader;

	public void ConnectToServer()
	{
		// 이미 연결되었다면 함수 무시
		if (socketReady) return;

		// 기본 호스트/ 포트번호
		string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text;			// 아무것도 안 썻다면 호스트에게 연결을 시도
		int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);

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
			Chat.instance.ShowMessage($"소켓에러 : {e.Message}");
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
			clientName = NickInput.text == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : NickInput.text;		// 정한 닉네임이 없으면 Guest(임의숫자)의 닉네임으로 설정
			Send($"&NAME|{clientName}");
			return;
		}

		Chat.instance.ShowMessage(data);
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
}
