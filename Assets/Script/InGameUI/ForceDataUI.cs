using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private List<TextMeshProUGUI> m_CommanderPowerTextList;
    [SerializeField] private Image m_CountryFlagImage;
    [SerializeField] private Image m_CommanderPortraitImage;
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
        m_CommanderPortraitImage.sprite = force.Commander.Portrait;

        m_CommanderPortraitImage.SetAllDirty();

        for (int i = 0; i < m_CommanderPowerTextList.Count; i++)
        {
            switch (i)
            {
                case 0:
                    m_CommanderPowerTextList[i].SetText(force.Commander.Attack.ToString());
                    break;
                case 1:
                    m_CommanderPowerTextList[i].SetText(force.Commander.Defence.ToString());
                    break;
                case 2:
                    m_CommanderPowerTextList[i].SetText(force.Commander.Logitics.ToString());
                    break;
                case 3:
                    m_CommanderPowerTextList[i].SetText(force.Commander.Supply.ToString());
                    break;
            }
        }

        var button = GetComponent<CustomButton>();  //ボタンを取得

        button.onHover.AddListener(() => WorldUIManager.Instance.ForceUIDict[force.BelongGroup].SetActiveArrowMark(true));      //マウスポインタが上にある時に大軍団のワールドUIの矢印をオン
        button.onHoverExit.AddListener(() => WorldUIManager.Instance.ForceUIDict[force.BelongGroup].SetActiveArrowMark(false)); //マウスポインタが外れた時に大軍団のワールドUIの矢印をオフ
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
        FieldManager.Instance.GetBorderAndStateData(BattleSystemManager.Instance.PlayerCountry);    //国境と自ステートをハイライト
        BattleSystemManager.Instance.OpenOrderMenu(ForceData);                                      //命令メニュー出現＆Manager内での編集ステートを変更
    }

    public void RefreshPositionName()
    {
        if (ForceData.CurrentTargetState != null)
        {
            m_PositionNameText.SetText(ForceData.CurrentTargetState.StateNameJP);
            m_PositionConstantText.SetText("にて戦闘中");
        }
        else if (ForceData.CurrentGuarrisonState != null)
        {
            m_PositionNameText.SetText(ForceData.CurrentGuarrisonState.StateNameJP);
            m_PositionConstantText.SetText("にて駐屯中");
        }
    }

    public void DeleteLink()
    {
        ForceData.SetLinkedUI(null);
    }
}
