using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] protected LineRenderer LineRenderer;

    protected RectTransform OwnRectTransform;

    private void Awake()
    {
        OwnRectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// ポップアップのキャンバス内のローカル座標をワールド座標に変換する
    /// </summary>
    protected Vector2 GetScreenPosition()
    {
        var rect = GetComponent<RectTransform>();
        return RectTransformUtility.WorldToScreenPoint(PopupManager.Instance.MainCamera, rect.position);
    }

    /// <summary>
    /// ポップアップからワールド座標に向けてレイを出す
    /// </summary>
    /// <param name="targetPos"></param>
    protected void InstantiateRayToWorldPosition(Vector3 targetPos)
    {
        LineRenderer.enabled = true;      //レイをオン
        LineRenderer.positionCount = 2;   //始点と終点
        LineRenderer.SetPosition(0, OwnRectTransform.position);   //始点を設定
        LineRenderer.SetPosition(1, targetPos);   //終点を設定
    }

    /// <summary>
    /// レイの座標更新
    /// </summary>
    /// <param name="targetPos"></param>
    protected void UpdataRayPosition(Vector3 targetPos)
    {
        LineRenderer.SetPosition(0, OwnRectTransform.position);   //始点を設定
        LineRenderer.SetPosition(1, targetPos);   //終点を設定
    }

    /// <summary>
    /// このウィンドウを削除
    /// </summary>
    public void CloseThisWindow()
    {
        if (PopupManager.Instance.CurrentPopupList.Contains(this))
        {
            PopupManager.Instance.CurrentPopupList.Remove(this);
            Destroy(gameObject);
        }
    }
}
