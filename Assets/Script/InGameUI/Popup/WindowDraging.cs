using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDraging : MonoBehaviour, IDragHandler
{
    //このコンポーネントを「ウィンドウを動かす時に摘まむ場所」に割り当ててください。

    [Header("動かしたいウィンドウ本体")]
    [SerializeField] private RectTransform m_WindowRect;  //動かしたいウィンドウ本体
    [Header("所属するCanvas")]
    [SerializeField] private Canvas m_ParentCanvas;       //所属するCanvas

    private void Awake()
    {
        if (m_WindowRect == null)
        {
            m_WindowRect = GetComponent<RectTransform>();
        }

        if (m_ParentCanvas == null)
        {
            m_ParentCanvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_WindowRect == null) return;

        float scaleFactor = m_ParentCanvas != null ? m_ParentCanvas.scaleFactor : 1f;
        m_WindowRect.anchoredPosition += eventData.delta / scaleFactor;
    }
}
