using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntToServerScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Scene = Define.Scene.IntToServer;

        Debug.Log("IntToServer");
        return true;
    }
}
