using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatePopup : Popup
{
    [Header("ステート名テキスト")]
    [SerializeField] private TextMeshProUGUI m_StateNameText;
    [Header("安定度のゲージ")]
    [SerializeField] private Image m_StabilityGageImage;
    [Header("安定度のアイコン周りのフェード")]
    [SerializeField] private Image m_StabilityFadeImage;
    [Header("領有勢力の国旗イメージ")]
    [SerializeField] private Image m_OwnedCountryFlagImage;
    [Header("ステート地形イメージ")]
    [SerializeField] private Image m_GeographImage;
    [Header("民族解説テキスト")]
    [SerializeField] private TextMeshProUGUI m_CultureText;
    [Header("民族マークイメージ")]
    [SerializeField] private Image m_CultureMarkImage;
    [Header("資金収入テキスト")]
    [SerializeField] private TextMeshProUGUI m_IncomeText;
    [Header("資源産出テキスト")]
    [SerializeField] private TextMeshProUGUI m_YieldText;
    [Header("徴兵人口テキスト")]
    [SerializeField] private TextMeshProUGUI m_PopulationText;
    [Header("糧食テキスト")]
    [SerializeField] private TextMeshProUGUI m_RationText;

    public State StateData {  get; private set; }

    public const float kGageMaxHeight = 150.0f;

    private void LateUpdate()
    {
        if (StateData != null)
        {
            SetStateData(StateData);
            if (LineRenderer.enabled)
            {
                UpdataRayPosition(StateData.MidModelPos);   //ステート中央座標にレイを出す
            }
        }
    }

    /// <summary>
    /// ステートデータをポップアップに反映
    /// </summary>
    /// <param name="state"></param>
    public void SetStateData(State state)
    {
        if (StateData == null)
        {
            StateData = state;
        }

        m_StateNameText.SetText(state.StateNameJP);
        if (state.OwnedCountry != null)
        {
            m_OwnedCountryFlagImage.sprite = state.OwnedCountry.CountryFlag;
        }
        m_GeographImage.sprite = state.TerrainUI;
        m_CultureText.SetText(state.StateCultureJP + "族");
        m_CultureMarkImage.sprite = state.CultureMark;
        m_IncomeText.SetText(state.Income.ToString());
        m_YieldText.SetText(state.Yield.ToString());
        m_PopulationText.SetText(state.Population.ToString());
        m_RationText.SetText(state.Ration.ToString());

        var stabilityPercent = state.Stability / 100.0f;

        if (stabilityPercent < 0.3f)
        {
            SetStabilityColor(Color.red);
        }
        else if (stabilityPercent < 0.7f)
        {
            SetStabilityColor(Color.yellow);
        }
        else
        {
            SetStabilityColor(Color.green);
        }

        m_StabilityGageImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, stabilityPercent * kGageMaxHeight);   //ゲージの高さを更新

        InstantiateRayToWorldPosition(StateData.MidModelPos);
    }

    public void SetStabilityColor(Color color)
    {
        m_StabilityGageImage.color = color;
        m_StabilityFadeImage.color = color;
    }
}
    
