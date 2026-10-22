
using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderMenu : TacticalMenu
{
    //ClickableProvをFieldManagerから取ってその分ボタンを生成
    //ボタンをリスト化し、それぞれのボタンに機能を持たせる
    //最後に、選択中の軍団のいる位置にボタンの持っている場所を持たせて、ポップアップを閉じ、選択中の軍団をnullにする


    [SerializeField] private GameObject m_BorderLabelPrehab;
    [SerializeField] private GameObject m_GuarrisonLabelPrehab;
    [SerializeField] private Transform m_ContentParent;
    [SerializeField] private OrderUI m_BorderButtonPrehab;
    [SerializeField] private OrderUI m_GuarrisonButtonPrehab;

    private List<GameObject> m_ButtonList = new List<GameObject>();

    public override void Init()
    {
        List<State> borderList = BattleSystemManager.Instance.PlayerCountry.BorderStateList;
        List<State> guarrisonList = BattleSystemManager.Instance.PlayerCountry.GuarrisonStateList;

        m_ButtonList.Add(Instantiate(m_BorderLabelPrehab, m_ContentParent));    //国境ラベル生成

        //====================国境ボタン生成===================
        for (int i = 0; i < borderList.Count; i++)
        {
            int index = i;

            OrderUI button = Instantiate(m_BorderButtonPrehab, m_ContentParent);
            button.SetBorderData(borderList[i]);
            button.ButtonText.SetText(borderList[i].StateNameJP + "戦線");
            button.OrderButton.onClick.AddListener(() => button.SetForcePositionForButton());
            button.OrderButton.onHover.AddListener(() => BattleSystemManager.Instance.HighLightPointingBorderOnUI(button.TargetState));
            button.OrderButton.onHoverExit.AddListener(() => FieldManager.Instance.HighLightProvinceOnUI());
            m_ButtonList.Add(button.gameObject);
        }
        //====================国境ボタン生成End===================

        m_ButtonList.Add(Instantiate(m_GuarrisonLabelPrehab, m_ContentParent));    //駐屯ラベル生成

        //====================駐屯ボタン生成===================
        for (int i = 0; i < guarrisonList.Count; i++)
        {
            int index = i;

            OrderUI button = Instantiate(m_GuarrisonButtonPrehab, m_ContentParent);
            button.SetGurrisontData(guarrisonList[i]);
            button.ButtonText.SetText(guarrisonList[i].StateNameJP + "駐屯");
            button.OrderButton.onClick.AddListener(() => button.SetForcePositionForButton());
            button.OrderButton.onHover.AddListener(() => BattleSystemManager.Instance.HighLightPointingBorderOnUI(button.TargetState));
            button.OrderButton.onHoverExit.AddListener(() => FieldManager.Instance.HighLightProvinceOnUI());
            m_ButtonList.Add(button.gameObject);
        }
        //====================駐屯ボタン生成End===================
    }

    public void ClearButtonList()
    {
        foreach(var button in m_ButtonList)
        {
            Destroy(button);
        }

        m_ButtonList.Clear();
    }
}
