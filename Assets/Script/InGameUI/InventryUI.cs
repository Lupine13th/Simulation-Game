using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct InventryUIContainer
{
    public float fundRatio;
    public float resourceRatio;
    public string fund;
    public string resource;
}

public class InventryUI : MonoBehaviour
{
    [Header("総資金のバー")]
    [SerializeField] private RectTransform m_FundBarValue;

    [Header("総資源のバー")]
    [SerializeField] private RectTransform m_ResourceBarValue;

    [Header("総資金のテキスト")]
    [SerializeField] private TextMeshProUGUI m_FundText;

    [Header("総資源のテキスト")]
    [SerializeField] private TextMeshProUGUI m_ResourceText;

    private const float kMaxBarWidth = 400f;

    public void SetUI(InventryUIContainer container)
    {
        m_FundBarValue.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, kMaxBarWidth * container.fundRatio);        //資金ゲージを更新
        m_ResourceBarValue.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, kMaxBarWidth * container.resourceRatio);//資源ゲージを更新
        m_FundText.SetText(container.fund);         //資金テキスト更新
        m_ResourceText.SetText(container.resource); //資源テキスト更新
    }
}
