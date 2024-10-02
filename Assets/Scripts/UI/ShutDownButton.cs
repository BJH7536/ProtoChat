using UnityEngine;

public class ShutDownButton : MonoBehaviour
{
    void Start()
    {
        if(Managers.Instance.role != Define.Role.Server) gameObject.SetActive(false);
    }
    
    public void ShutDownAndBackToInitScreen()
    {
        TCPServer.instance.ServerShutdown();
        Managers.Scene.ChangeScene(Define.Scene.Init);
    }
}
