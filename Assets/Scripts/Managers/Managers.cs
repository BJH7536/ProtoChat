using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class Managers : MonoBehaviour
{
    private static Managers s_instance = null;
    public static Managers Instance { get { return s_instance; } }

    private static ResourceManager s_resourceManager = new ResourceManager();
    private static UIManager s_uiManager = new UIManager();
    private static SceneManagerEx s_sceneManager = new SceneManagerEx();

    public static ResourceManager Resource { get { Init(); return s_resourceManager; } }
    public static UIManager UI { get { Init(); return s_uiManager; } }
    public static SceneManagerEx Scene { get { Init(); return s_sceneManager; } }

    public Define.Role role;
    public Define.Protocol protocol;

    private void Start()
    {
        role = Define.Role.unknown;
        protocol = Define.Protocol.unknown;
        Init();
    }

    private static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
                go = new GameObject { name = "@Managers" };

            s_instance = Utils.GetOrAddComponent<Managers>(go);
            DontDestroyOnLoad(go);

            Application.targetFrameRate = 60;
        }
    }
}
