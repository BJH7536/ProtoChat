using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InitScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.Scene.Init;

        Debug.Log("Init");
        return true;
    }
}
