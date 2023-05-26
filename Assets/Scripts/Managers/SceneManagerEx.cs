using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    Define.Scene _Scene = Define.Scene.Init;

    public Define.Scene Scene { get { return _Scene; } set { _Scene = value; } }

    public Define.Scene CurrentSceneType
    {
        get
        {
            if (_Scene != Define.Scene.Init)
                return _Scene;
            return CurrentScene.Scene;
        }
        set { _Scene = value; }
    }

    public BaseScene CurrentScene { get { return GameObject.Find("@Scene").GetComponent<BaseScene>(); } }

    public void ChangeScene(Define.Scene type)
    {
        CurrentScene.Clear();

        _Scene = type;
        SceneManager.LoadScene(GetSceneName(type));
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        char[] letters = name.ToLower().ToCharArray();
        letters[0] = char.ToUpper(letters[0]);
        return new string(letters);
    }
}