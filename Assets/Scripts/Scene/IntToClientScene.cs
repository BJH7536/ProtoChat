using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntToClientScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Scene = Define.Scene.IntToClient;

        Debug.Log("IntToClient");
        return true;
    }
}
