using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomBtn : MonoBehaviour
{
    public void instanceUI_CrtRmReqPopup()
    {
        Managers.Resource.Instantiate("UI/UI_CrtRmReqPopup", transform.parent);
    }

}
