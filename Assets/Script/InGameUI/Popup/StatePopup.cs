using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatePopup : Popup
{
    [Header("ステート名テキスト")]
    [SerializeField] private TextMeshProUGUI m_StateNameText;
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

        InstantiateRayToWorldPosition(StateData.MidModelPos);
    }
}
    
