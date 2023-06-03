using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public GameObject MyArea, YourArea, DateArea;
    public RectTransform ContentRect;
    public Scrollbar scrollbar;
    AreaScript LastArea;

    public void Chat(bool isSend, string text, string user, Texture picture)
    {
        if (text.Trim() == "") return;

        bool isBottom = scrollbar.value <= 0.00001f;

        // ������ ���, �޴� ����� ������ ũ�� ����� �ؽ�Ʈ ����
        AreaScript Area = Instantiate(isSend ? MyArea : YourArea).GetComponent<AreaScript>();
        Area.transform.SetParent(ContentRect.transform, false);
        Area.BoxRect.sizeDelta = new Vector2(280, Area.BoxRect.sizeDelta.y);
        Area.TextRect.GetComponent<TextMeshProUGUI>().text = text;
        Fit(Area.BoxRect);                              // �ؽ�Ʈ�� �����ϸ� ���װ� �ִ�. �̸� �ذ��ϱ� ���� Fit()


        // �� �� �̻��̸� ũ�⸦ �ٿ����鼭, �� ���� �Ʒ��� �������� �ٷ� �� ũ�⸦ ����
        float X = Area.TextRect.sizeDelta.x + 42;
        float Y = Area.TextRect.sizeDelta.y;
        if (Y > 49)
        {
            for (int i = 0; i < 200; i++)
            {
                Area.BoxRect.sizeDelta += new Vector2(X - i * 2, Area.BoxRect.sizeDelta.y);
                Fit(Area.BoxRect);

                if (Y >= Area.TextRect.sizeDelta.y) { Area.BoxRect.sizeDelta = new Vector2(X - (i * 2) + 2, Y); break; }
            }
        }
        else Area.BoxRect.sizeDelta = new Vector2(X, Y);

        // ���� Area�� �� ������ ��¥�� ���� �̸� ����
        DateTime t = DateTime.Now;
        Area.Time = t.ToString("yyyy-MM-dd-HH-mm");
        Area.User = user;

        // ���� Area�� �׻� ���ο� �ð� ����
        int hour = t.Hour;
        if (t.Hour == 0) hour = 12;
        else if (t.Hour > 12) hour -= 12;
        Area.TimeText.text = (t.Hour > 12 ? "PM" : "AM") + hour + ":" + t.Minute.ToString("D2");

        // ���� Area�� ������ ���� �ð�, ���� ���ֱ�
        bool isSame = LastArea != null && LastArea.Time == Area.Time && LastArea.User == Area.User;
        if (isSame) LastArea.TimeText.text = "";
        Area.Tail.SetActive(!isSame);

        // Ÿ���� ���� �Ͱ� ������ �̹���, �̸� ���ֱ�
        if(!isSend)
        {
            Area.UserImage.gameObject.SetActive(!isSame);
            Area.UserText.gameObject.SetActive(!isSame);
            Area.UserText.text = Area.User;
        }

        // ���� Area�� ��¥�� �ٸ��� ��¥ ���� ���̱�
        if(LastArea != null && LastArea.Time.Substring(0,10) != Area.Time.Substring(0,10))
        {
            Transform CurDateArea = Instantiate(DateArea).transform;
            CurDateArea.SetParent(ContentRect.transform, false);
            CurDateArea.SetSiblingIndex(CurDateArea.GetSiblingIndex() - 1);

            string week = "";

            switch (t.DayOfWeek)
            {
                case DayOfWeek.Sunday: week = "��"; break;
                case DayOfWeek.Monday: week = "��"; break;
                case DayOfWeek.Tuesday: week = "ȭ"; break;
                case DayOfWeek.Wednesday: week = "��"; break;
                case DayOfWeek.Thursday: week = "��"; break;
                case DayOfWeek.Friday: week = "��"; break;
                case DayOfWeek.Saturday: week = "��"; break;
            }
            CurDateArea.GetComponent<AreaScript>().DateText.text = t.Year + "��" + t.Month + "��" + t.Day + "��" + week + "����";
        }



        Fit(Area.BoxRect);
        Fit(Area.AreaRect);
        Fit(ContentRect);
        LastArea = Area;

        if (!isSend && !isBottom) return;
        Invoke("ScrollDelay", 0.03f);
    }

    void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

    void ScrollDelay() => scrollbar.value = 0;
}
