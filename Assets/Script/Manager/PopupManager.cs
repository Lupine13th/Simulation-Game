using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PopupManager : SingletonManager<PopupManager>
{
    [Header("ポップアップの親")]
    [SerializeField] private Transform m_PopupParent;
    [Header("キャンバス")]
    [SerializeField] private Canvas m_PopupCanvas;
    [Header("ステートポップアップのプレハブ")]
    [SerializeField] private StatePopup m_StatePopupPrefab;

    private Camera m_MainCamera;

    public Canvas PopupCanvas => m_PopupCanvas;
    public Camera MainCamera => m_MainCamera;

    /// <summary>
    /// 現在展開中のポップアップのリスト
    /// </summary>
    public List<Popup> CurrentPopupList = new List<Popup>();

    private void Start()
    {
        m_MainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        
    }

    /// <summary>
    /// ステートポップアップをマウスの位置に生成
    /// </summary>
    /// <param name="state"></param>
    public void InstantiateStatePopupOnMousePosition(State state)
    {
        foreach (var pop in CurrentPopupList)
        {
            if (pop is StatePopup statepop)  //ステートポップアップのみ検索
            {
                if (statepop.StateData == state)    //同じステートのポップアップを作ろうとしたら
                {
                    pop.gameObject.transform.localPosition = GetMousePointerPosition(); //そのポップアップをマウス地点に出して
                    Debug.Log("同じステートに対してのポップアップを作成しようとしたのでキャンセルしました。");
                    return;                         //戻る
                }
            }
        }

        var popup = Instantiate(m_StatePopupPrefab, m_PopupParent);
        popup.SetStateData(state);
        popup.gameObject.transform.localPosition = GetMousePointerPosition();
        CurrentPopupList.Add(popup);
    }

    /// <summary>
    /// マウスの位置を取得
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMousePointerPosition()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_PopupCanvas.GetComponent<RectTransform>(), screenPos, m_MainCamera, out Vector2 localPos))
        {
            return localPos;
        }

        return Vector2.zero;
    }
}
