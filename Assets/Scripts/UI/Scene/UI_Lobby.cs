using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_Lobby : UI_Scene
{
    enum Buttons
    {
        
    }

    private void Start()
    {
        Init();

        Debug.Log("Load-------------------");
        Debug.Log(Managers.Instance.role);
        Debug.Log(Managers.Instance.protocol);
        Debug.Log("-----------------------");

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        //GetButton((int)Buttons.SubmitBtn).gameObject.BindEvent(submit);

        return true;
    }

}
