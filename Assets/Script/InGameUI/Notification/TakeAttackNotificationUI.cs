using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TakeAttackNotificationUI : NotificationUI
{
    [Header("攻撃してきた勢力名")]
    [SerializeField] private TextMeshProUGUI m_AttackerNameText;
    [Header("攻撃してきた勢力国旗")]
    [SerializeField] private Image m_AttackerFlagImage;
    [Header("○○(勢力名)が")]
    [SerializeField] private TextMeshProUGUI m_NoticeScript1;
    [Header("○○(ステート)に対して")]
    [SerializeField] private TextMeshProUGUI m_NoticeScript2;

    /// <summary>
    /// プレイヤーの空ステートへの攻撃時のポップアップにテキストと旗を入力
    /// </summary>
    /// <param name="ATCountry"></param>
    /// <param name="DFCountry"></param>
    public void SetTextAndFlag(CountryData ATCountry, State DFState)
    {
        m_AttackerNameText.SetText(ATCountry.CountryName);
        m_AttackerFlagImage.sprite = ATCountry.CountryFlag;
        m_NoticeScript1.SetText(ATCountry.CountryName + "の軍団が");
        m_NoticeScript2.SetText("我らの所有する" + DFState.StateNameJP + "州に");
    }
}
