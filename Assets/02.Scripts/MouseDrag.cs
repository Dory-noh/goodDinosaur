using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDrag : MonoBehaviour, IDragHandler
{
    public RectTransform content;  // 스크롤할 UI (Grid가 적용된 Content)
    public float scrollSpeed = 0.5f;  // 스크롤 속도 조절

    public void OnDrag(PointerEventData eventData)
    {
        content.anchoredPosition += new Vector2(0, eventData.delta.y * scrollSpeed);
    }
}
