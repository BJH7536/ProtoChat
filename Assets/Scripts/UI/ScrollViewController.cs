using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScrollViewController : MonoBehaviour
{
    private ScrollRect ScrollRect;

    private float space = 0f;

    public GameObject uiPrefab;

    public List<RectTransform> uiObjects = new List<RectTransform>();

    void Start()
    {
        ScrollRect = GetComponent<ScrollRect>();
    }

    void Update()
    {
        
    }

    public void addNewUiObject()
    {
        RectTransform newUi = Instantiate(uiPrefab, ScrollRect.content).GetComponent<RectTransform>();
        uiObjects.Add(newUi);

        float y = 0f;
        for(int i = 0; i < uiObjects.Count; i++)
        {
            uiObjects[i].anchoredPosition = new Vector2(0f, -y);
            y += uiObjects[i].sizeDelta.y + space;

        }

        ScrollRect.content.sizeDelta = new Vector2(ScrollRect.content.sizeDelta.x, y);

    }
}
