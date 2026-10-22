using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    [SerializeField] private CustomButton m_OrderButton;
    [SerializeField] private TextMeshProUGUI m_ButtonText;

    public ForcePositionType PosType { get; private set; }
    public State TargetState { get; private set; }

    public CustomButton OrderButton => m_OrderButton;
    public TextMeshProUGUI ButtonText => m_ButtonText;

    /// <summary>
    /// 命令メニュー内のボタンからユニット位置を設定する
    /// </summary>
    public void SetForcePositionForButton()
    {
        switch (PosType)
        {
            default:
                break;
            case ForcePositionType.Border:
                FieldManager.Instance.SetForceOnBorder(TargetState, BattleSystemManager.Instance.CurrentEditingForce);
                break;
            case ForcePositionType.Guarrison:
                FieldManager.Instance.SetForceOnState(TargetState, BattleSystemManager.Instance.CurrentEditingForce);
                break;
        }
    }

    /// <summary>
    /// UIオブジェクトに国境データを持たせる
    /// </summary>
    /// <param name="state"></param>
    public void SetBorderData(State state)
    {
        PosType = ForcePositionType.Border;
        TargetState = state;
    }

    /// <summary>
    /// UIオブジェクトに国境データを持たせる
    /// </summary>
    /// <param name="state"></param>
    public void SetGurrisontData(State state)
    {
        PosType = ForcePositionType.Guarrison;
        TargetState = state;
    }
}
