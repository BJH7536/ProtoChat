using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Define
{
    public enum UIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp,
    }

    public enum Scene
    {
        Init,
        IntToClient,
        IntToServer,
        Lobby,
    }

    public enum Role
    {
        unknown,
        Server,
        Client,
    }

    public enum Protocol
    {
        unknown,
        TCP,
        UDP,
    }
}
