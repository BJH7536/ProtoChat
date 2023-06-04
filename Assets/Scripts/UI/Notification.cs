using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    TextMeshProUGUI text;
    public string context = "[this is notification]";

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = context;
        StartCoroutine(FadeTextToZero());
    }

    public IEnumerator FadeTextToZero()  // ���İ� 1���� 0���� ��ȯ
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a > 0.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / 2.0f));
            yield return null;
        }
        //StartCoroutine(FadeTextToFullAlpha());
        Destroy(gameObject);
    }

    //public IEnumerator FadeTextToFullAlpha() // ���İ� 0���� 1�� ��ȯ
    //{
    //    text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
    //    while (text.color.a < 1.0f)
    //    {
    //        text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / 2.0f));
    //        yield return null;
    //    }
    //    StartCoroutine(FadeTextToZero());
    //}

}