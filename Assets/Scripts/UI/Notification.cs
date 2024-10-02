using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string context = "[this is notification]";

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = context;
        StartCoroutine(FadeTextToZero());
    }

    public IEnumerator FadeTextToZero()  // 알파값 1에서 0으로 전환
    {
        yield return new WaitForSecondsRealtime(1.0f);
        
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a > 0.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / 2.0f));
            yield return null;
        }

        Destroy(gameObject);
    }
    
}