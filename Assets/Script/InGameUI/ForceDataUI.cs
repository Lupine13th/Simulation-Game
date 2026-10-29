using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForceDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_ForceNameText;
    [SerializeField] private TextMeshProUGUI m_SoldiersText;
    [SerializeField] private TextMeshProUGUI m_MoraleText;
    [SerializeField] private TextMeshProUGUI m_AttackText;
    [SerializeField] private TextMeshProUGUI m_DefenseText;
    [SerializeField] private TextMeshProUGUI m_PositionNameText;
    [SerializeField] private TextMeshProUGUI m_PositionConstantText;
    [SerializeField] private Image m_CountryFlagImage;
    [SerializeField] private GameObject Buttons;

    public Force ForceData { get; private set; }

    /// <summary>
    /// この軍団のステータス
    /// </summary>
    /// <param name="force"></param>
    public void SetForceDataUI(Force force)
    {
        ForceData = force;
        ForceData.SetLinkedUI(this);
        m_ForceNameText.SetText(force.ForcesName);
        m_SoldiersText.SetText(force.Soldiers + "/" + force.MaxSoldiers);
        m_MoraleText.SetText(force.Morale + "/" + force.MaxMorale);
        m_AttackText.SetText(force.AttackPower.ToString());
        m_DefenseText.SetText(force.DefensePower.ToString());
        m_CountryFlagImage.sprite = force.OwnerCountry.CountryFlag;
    }

    public void OpenEditButton()
    {
        Buttons.SetActive(true);
    }

    public void CloseEditButton()
    {
        Buttons.SetActive(false);
    }

    /// <summary>
    /// プレイヤー用部隊配置メソッド
    /// </summary>
    public void PlaceForce()
    {
        FieldManager.Instance.HighLightBorderAndOwnState();   //国境と自ステートをハイライト
        BattleSystemManager.Instance.OpenOrderMenu(ForceData);    //命令メニュー出現＆Manager内での編集ステートを変更
    }

    public void RefreshPositionName()
    {
        if (ForceData.CurrentTargetStateId != -1)
        {
            m_PositionNameText.SetText(FieldManager.Instance.StateList[ForceData.CurrentTargetStateId].StateNameJP);
            m_PositionConstantText.SetText("にて戦闘中");
        }
        else if (ForceData.CurrentGuarrisonStateId != -1)
        {
            m_PositionNameText.SetText(FieldManager.Instance.StateList[ForceData.CurrentGuarrisonStateId].StateNameJP);
            m_PositionConstantText.SetText("にて駐屯中");
        }
    }

    public void DeleteLink()
    {
        ForceData.SetLinkedUI(null);
    }
}
