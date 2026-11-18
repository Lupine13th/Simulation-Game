using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeclareWarNotificationUI : NotificationUI
{
    [Header("宣戦布告した勢力名")]
    [SerializeField] private TextMeshProUGUI m_AttackerNameText;
    [Header("宣戦布告された勢力名")]
    [SerializeField] private TextMeshProUGUI m_DefenderNameText;
    [Header("宣戦布告した勢力国旗")]
    [SerializeField] private Image m_AttackerFlagImage;
    [Header("宣戦布告された勢力国旗")]
    [SerializeField] private Image m_DefenderFlagImage;
    [Header("○○が")]
    [SerializeField] private TextMeshProUGUI m_NoticeScript1;
    [Header("○○に対して")]
    [SerializeField] private TextMeshProUGUI m_NoticeScript2;

    /// <summary>
    /// 宣戦布告時のポップアップにテキストと旗を入力
    /// </summary>
    /// <param name="ATCountry"></param>
    /// <param name="DFCountry"></param>
    public void SetTextAndFlag(CountryData ATCountry, CountryData DFCountry)
    {
        m_AttackerNameText.SetText(ATCountry.CountryName);
        m_DefenderNameText.SetText(DFCountry.CountryName);
        m_NoticeScript1.SetText(ATCountry.CountryName + "が");
        m_NoticeScript2.SetText(DFCountry.CountryName + "に対して");

        m_AttackerFlagImage.sprite = ATCountry.CountryFlag;
        m_DefenderFlagImage.sprite = DFCountry.CountryFlag;
    }
}
