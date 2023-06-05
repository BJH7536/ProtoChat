using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class RoomImage : MonoBehaviour
{
    public TextMeshProUGUI roomNameField;
    public TextMeshProUGUI memberField;
    public Button roomButton;

    public string roomName;
    public int curMemNum;
    public int maxMemNum;
    public List<string> MemberNames;

    private GameObject myChatPopup;

    private void Start()
    {
        roomNameField.text = roomName;
        memberField.text = curMemNum + "/" + maxMemNum;
    }

    public void ShowChatPopup()
    {
        if (myChatPopup != null) return;

        GameObject UI_Lobby = GameObject.Find("UI_Lobby");
        myChatPopup = Managers.Resource.Instantiate("UI/ChatPopup", UI_Lobby.transform);
    }

}
