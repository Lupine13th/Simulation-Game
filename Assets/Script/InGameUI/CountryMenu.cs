using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class CountryMenu : TacticalMenu
{
    [SerializeField] private TextMeshProUGUI m_CountryNameText;
    [SerializeField] private TextMeshProUGUI m_IdeoloryText;
    [SerializeField] private TextMeshProUGUI m_MainCultureText;
    [SerializeField] private TextMeshProUGUI m_AllResourceText;
    [SerializeField] private TextMeshProUGUI m_AllIncomeText;
    [SerializeField] private Image m_CountryFlag;
    [SerializeField] private Image m_LeaderPortrait;
    [SerializeField] private Image m_MainCultureImage;

    public override void Init()
    {
        SetCountryDataforMenu(BattleSystemManager.Instance.SelectingCountry);
        BattleSystemManager.Instance.ResetOwnedAllStateColor(BattleSystemManager.Instance.SelectingCountry);                        //ステート色を一旦リセット
        BattleSystemManager.Instance.ChangeOwnedAllStateColor(BattleSystemManager.Instance.SelectingCountry, FieldManager.Instance.StateColorDict[StateColorType.Selecting]);   //勢力に属するステート全ての色を変更
    }

    public async Task SetCountryDataforMenu(CountryData country)
    {
        m_CountryNameText.SetText(country.CountryName);   //ステート名
        m_IdeoloryText.SetText(country.ForceIdeology);  //資源
        m_IdeoloryText.color = SetTextColor(country.LargeIdeology);
        m_MainCultureText.SetText(country.MainCountryCultureJP);

        country.CalculateDateGDP();

        m_AllResourceText.SetText(country.AllResource.ToString());
        m_AllIncomeText.SetText(country.AllIncome.ToString());

        m_CountryFlag.sprite = country.CountryFlag;
        m_LeaderPortrait.sprite = country.LeaderPortrait;
        m_MainCultureImage.sprite = country.CultureImage;
    }

    public Color SetTextColor(LIdeology ideology)
    {
        switch (ideology)
        {
            case LIdeology.Liberalism:
                return Color.yellow;
            case LIdeology.Nationalism:
                return Color.purple;
            case LIdeology.Socialism:
                return Color.red;
            case LIdeology.Militarism:
                return Color.gray;
            default:
                return Color.black;
        }
    }
}
