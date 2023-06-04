using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoom : MonoBehaviour
{
    public GameObject CreateRoomReqPrefab;
    public GameObject UI_Lobby;

    public void instan()
    {
        Managers.Resource.Instantiate(CreateRoomReqPrefab, UI_Lobby.transform);
    }

}
