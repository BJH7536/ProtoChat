using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserImage : MonoBehaviour
{
    public TextMeshProUGUI userNameField;
    public TextMeshProUGUI lineField;

    public string Username;
    public bool Online = true;

    private void Start()
    {
        userNameField.text = Username;

        Online = true;

        if (Online)
            lineField.text = "Online";
        else
            lineField.text = "Offline";
    }
}
