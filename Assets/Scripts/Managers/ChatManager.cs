using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public GameObject MyArea, YourArea, DataArea;
    public RectTransform ContentRect;
    public Scrollbar scrollbar;
    AreaScript LastArea;

    public void Chat(bool isSend, string text, string user, Texture picture)
    {
        if (text.Trim() == "") return;

        bool isBottom = scrollbar.value <= 0.00001f;

        // 보내는 사람, 받는 사람의 영역을 크게 만들고 텍스트 대입
        AreaScript Area = Instantiate(isSend ? MyArea : YourArea).GetComponent<AreaScript>();
        Area.transform.SetParent(ContentRect.transform, false);
        Area.BoxRect.sizeDelta = new Vector2(280, Area.BoxRect.sizeDelta.y);
        Area.TextRect.GetComponent<TextMeshProUGUI>().text = text;
        Fit(Area.BoxRect);                              // 텍스트만 대입하면 버그가 있다. 이를 해결하기 위해 Fit()


        // 두 줄 이상이면 크기를 줄여가면서, 한 줄이 아래로 내려가면 바로 전 크기를 대입
        float X = Area.TextRect.sizeDelta.x + 42;
        float Y = Area.TextRect.sizeDelta .y;
        if (Y > 49)
        {
            for (int i = 0; i < 200; i++)
            {
                Area.BoxRect.sizeDelta += new Vector2(X - i * 2, Area.BoxRect.sizeDelta.y);
                Fit(Area.BoxRect);

                if (Y != Area.TextRect.sizeDelta.y) { Area.BoxRect.sizeDelta = new Vector2(X - (i * 2) + 2, Y); break; }
            }
        }
        else Area.BoxRect.sizeDelta = new Vector2(X, Y);
    }

    void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
}
