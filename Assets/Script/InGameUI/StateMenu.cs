using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateMenu : TacticalMenu
{
    [SerializeField] private TextMeshProUGUI m_StateNameText;
    [SerializeField] private TextMeshProUGUI m_CountryNameText;
    [SerializeField] private TextMeshProUGUI m_ResourceText;
    [SerializeField] private TextMeshProUGUI m_IncomeText;
    [SerializeField] private TextMeshProUGUI m_PopulationText;
    [SerializeField] private TextMeshProUGUI m_CultureText;
    [SerializeField] private TextMeshProUGUI m_TerrainText;
    [SerializeField] private Image m_FlagImage;
    [SerializeField] private Image m_CultureImage;
    [SerializeField] private Image m_TerrainImage;

    public override void Init()
    {
        SetStateData(BattleSystemManager.Instance.SelectingState);
    }

    public async Task SetStateData(State state)
    {
        m_StateNameText.SetText(state.StateNameJP);   //ステート名
        m_ResourceText.SetText(state.Resource.ToString());  //資源
        m_IncomeText.SetText(state.Income.ToString());    //収入
        m_PopulationText.SetText(state.Population.ToString());  //人口
        m_CultureText.SetText(state.StateCultureJP.ToString());  //文化
        m_TerrainText.SetText(state.StateTypeJP);

        m_CultureImage.sprite = state.CultureMark;

        if (state.OwnedCountry != null)
        {
            m_CountryNameText.SetText(state.OwnedCountry.CountryName);  //国家名
            m_FlagImage.sprite = state.OwnedCountry.CountryFlag;        //国旗
        }
        else
        {
            m_CountryNameText.SetText("中立地域");  //国家が無ければ
            m_FlagImage.sprite = null;              //それぞれ空に
        }

        m_TerrainImage.sprite = state.TerrainUI;  //地形
    }
}
