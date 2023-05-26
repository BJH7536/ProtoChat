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
        Lobby,
    }

    public enum Role
    {
        Server,
        Client,
    }

    public enum Protocol
    {
        TCP,
        UDP,
    }
}
