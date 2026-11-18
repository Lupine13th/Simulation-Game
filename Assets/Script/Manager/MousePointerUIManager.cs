using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MousePointerUIManager : SingletonManager<MousePointerUIManager>
{
    [Header("マウスポインタUIの親")]
    [SerializeField] private Transform m_PointerUIParent;
    [Header("マウスポインタUI")]
    [SerializeField] private GameObject m_PointerUI;
    [Header("左クリックイメージ")]
    [SerializeField] private Image m_LeftClickImage;
    [Header("右クリックイメージ")]
    [SerializeField] private Image m_RightClickImage;
    [Header("左クリックイメージ")]
    [SerializeField] private TextMeshProUGUI m_LeftClickText;
    [Header("右クリックイメージ")]
    [SerializeField] private TextMeshProUGUI m_RightClickText;

    private Camera m_MainCamera;

    private string m_PointingObjectName;

    private void Start()
    {
        m_MainCamera = Camera.main;
        AudioManager.Instance.AddSEAudioSource(SEType.Click ,GetComponent<AudioSource>());
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();  //画面上でのマウスの位置

        PointerEventData pointerData = new PointerEventData(EventSystem.current)    //どの位置のデータを取るか
        {
            position = mousePos,
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);   //マウスポインタ地点直下に出したレイに触れたもの全部を取得

        bool isHit = false;

        foreach (var result in results) 
        {
            var button = result.gameObject.GetComponent<Button>();  //ボタンコンポーネントを取得

            if (button != null) //持っていれば
            {
                isHit = true;
                m_PointerUI.SetActive(true);    //ポップアップをオン
                m_PointingObjectName = result.gameObject.name;    //当たったオブジェクトの名前を取得

                RectTransform uiPos = m_PointerUI.GetComponent<RectTransform>();
                Vector2 localPoint;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_PointerUIParent as RectTransform, mousePos, m_MainCamera, out localPoint))
                {
                    uiPos.localPosition = localPoint;
                    uiPos.localRotation = Quaternion.identity;

                    // UI上のButtonに合わせたテキスト表示の設定
                    m_LeftClickText.SetText("開く");
                    m_RightClickImage.gameObject.SetActive(false);
                }
                break; // 最初に見つかったButtonで処理を確定
            }
            else
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI")) //ボタンには触れていないがUIには触れている
                {
                    m_RightClickImage.gameObject.SetActive(true);
                    m_LeftClickImage.gameObject.SetActive(true);
                    m_PointerUI.SetActive(false);
                    return;
                }
            }
        }

        if (isHit) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()); //カメラからのレイ
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))  //マウスポインタが何かに触れている
        {
            Vector3 screenPos = m_MainCamera.WorldToScreenPoint(hit.point);     //ポインタが当たった位置を取得
            m_PointerUI.SetActive(true);                                        //ポップアップをオン

            m_PointingObjectName = hit.collider.gameObject.name;                //当たったオブジェクトの名前を取得
            //Debug.Log(m_PointingObjectName);

            RectTransform uiPos = m_PointerUI.GetComponent<RectTransform>();

            Vector2 localPoint;                                                 //キャンバス上のローカル座標

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_PointerUIParent as RectTransform, screenPos, m_MainCamera, out localPoint)) //キャンバスのアンカーやスケールにフィットさせる
            {
                uiPos.localPosition = localPoint;
                uiPos.localRotation = Quaternion.identity;

                if (m_PointingObjectName == "BaseProvince(Clone)" || m_PointingObjectName == "Border(Clone)")   //プロヴィンスに触れている = ステートに触れている
                {
                    m_LeftClickText.SetText("勢力メニューを開く");
                    m_RightClickImage.gameObject.SetActive(true);
                    m_RightClickText.SetText("ステートメニューを開く");
                }
                else
                {
                    m_LeftClickText.SetText("");
                    m_RightClickText.SetText("");
                }
            }
        }
        else
        {
            m_RightClickImage.gameObject.SetActive(true);
            m_LeftClickImage.gameObject.SetActive(true);
            m_PointerUI.SetActive(false);
        }
    }
}
