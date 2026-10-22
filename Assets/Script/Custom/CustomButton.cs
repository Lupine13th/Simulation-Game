using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomButton : Button, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// ポインタが触れているイベントのトリガー
    /// </summary>
    public ButtonClickedEvent onHover = new ButtonClickedEvent();

    /// <summary>
    /// ポインタが離れたイベントのトリガー
    /// </summary>
    public ButtonClickedEvent onHoverExit = new ButtonClickedEvent();

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        onHover.Invoke();   //カーソルが乗ったときに実行
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        onHoverExit.Invoke();   //カーソルが離れたときに実行
    }
}
