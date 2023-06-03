using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Scene = Define.Scene.Lobby;

        Debug.Log("Init");
        return true;
    }
}
