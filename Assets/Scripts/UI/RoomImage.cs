using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86.Avx;

public class RoomImage : MonoBehaviour
{
    public TextMeshProUGUI roomNameField;
    public TextMeshProUGUI memberField;

    [HideInInspector]
    public string roomName;
    [HideInInspector]
    public int curMember;
    [HideInInspector]
    public int maxMember;

    Button roomButton;

    private void Awake()
    {
        roomButton = GetComponent<Button>();

        roomNameField.text = roomName;
        memberField.text = curMember + "/" + maxMember;
    }


}
