using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

[Serializable]
public struct EnlistData
{
    public ForceType ForceType;
    public int MaxSoldiers;
    public int MaxMorales;
    public int Attack;
    public int Defence;
    public int Occupy;
    public float MoveSpeed;
    public int FundCost;
    public int ResourceCost;
}

public class EnlistMenu : TacticalMenu
{
    [SerializeField] private TMP_InputField m_ForcesNameInput;
    [SerializeField] private TextMeshProUGUI m_ForcesTypeText;
    [SerializeField] private TextMeshProUGUI m_MaxSoldierValueText;
    [SerializeField] private TextMeshProUGUI m_NeedFundText;
    [SerializeField] private TextMeshProUGUI m_NeedResourceText;
    [SerializeField] private TMP_InputField m_SoldierValueInput;
    [SerializeField] private Slider m_SoldierSlider;

    [SerializeField] private ForcesMenu m_ForcesMenu;

    public int SoldierValue { get; private set; }
    /*public int m_MaxSoldierValue { get; private set; } = 1000;
    public int m_MaxMoraleValue { get; private set; }
    public int m_AttackValue { get; private set; }
    public int m_DefenseValue { get; private set; }
    public int m_OccupyValue { get; private set; }
    public float m_MoveSpeed { get; private set; }
    public int m_FundCostValue { get; private set; }
    public int m_ResourceCostValue { get; private set; }*/

    public ForceData NewForcesData { get; private set; } = new ForceData();

    private ForceType m_SelectingType;

    private bool m_IsUpdating = false;

    private void Start()
    {
        m_SoldierValueInput.onEndEdit.AddListener(OnSoldierInputEndEdit);  //兵数インプットをチェック
        m_SoldierValueInput.onValueChanged.AddListener(OnSoldierInputChanged);  //兵数インプットの値をスライダーに適応
        m_SoldierSlider.onValueChanged.AddListener(OnSliderValueChanged);     //スライダーの値を兵数インプットに適応
    }

    private void Update()
    {
        //m_SoldierValueInput.text = m_SoldierValue.ToString();
        m_MaxSoldierValueText.SetText("/" + NewForcesData.MaxSoldiers);
    }

    public void SelectInfantry()
    {
        NewForcesData = BattleSystemManager.Instance.SetEnlistData(ForceType.Infantry);
        Debug.Log("歩兵選択中");
        m_ForcesTypeText.SetText("歩兵");
    }
    public void SelectMilitia()
    {
        NewForcesData = BattleSystemManager.Instance.SetEnlistData(ForceType.Militia);
        Debug.Log("民兵選択中");
        m_ForcesTypeText.SetText("民兵");
    }
    public void SelectArtillery()
    {
        NewForcesData = BattleSystemManager.Instance.SetEnlistData(ForceType.Artillery);
        Debug.Log("砲兵選択中");
        m_ForcesTypeText.SetText("砲兵");
    }
    public void SelectCavalry()
    {
        NewForcesData = BattleSystemManager.Instance.SetEnlistData(ForceType.Cavalry);
        Debug.Log("騎兵選択中");
        m_ForcesTypeText.SetText("騎兵");
    }
    public void SelectTank()
    {
        NewForcesData = BattleSystemManager.Instance.SetEnlistData(ForceType.Tank);
        Debug.Log("戦車兵選択中");
        m_ForcesTypeText.SetText("戦車兵");
    }

    //-----------------|ボタン用|プレイヤーの調整項目に応じて徴兵を行うコマンド------------------
    public void EnlistCustomForce()
    {
        CountryData countryData = BattleSystemManager.Instance.SelectingCountry;

        ForceData newforce = NewForcesData; //新しい軍団のバッファ

        if (m_ForcesNameInput.text == "")
        {
            m_ForcesNameInput.text = "第" + countryData.ForceList.Count.ToString() + "軍団";
        }

        newforce.ForcesName = m_ForcesNameInput.text;   //軍団名入力
        newforce.Soldiers = SoldierValue;               //兵数入力
        newforce.OwnerCountry = countryData;

        NewForcesData = newforce;

        countryData.Enlist(NewForcesData);       //徴兵コマンド実行

        m_ForcesMenu.CloseEnlistMenu(); //徴兵メニュークローズ
    }

    /// <summary>
    /// メニュー初期化
    /// </summary>
    public void ResetEnlistMenu()
    {
        m_ForcesNameInput.text = "";
        SelectInfantry();
        m_SoldierSlider.value = 0;
    }

    /// <summary>
    /// スライダー変更時にテキストと連動する
    /// </summary>
    /// <param name="value"></param>
    private void OnSliderValueChanged(float value)
    {
        if (m_IsUpdating || m_SoldierValueInput.isFocused) 
            return;

        SoldierValue = (int)(value * NewForcesData.MaxSoldiers);  //最大値に対する割合をデータとして取得
        UpdateSliderAndInputText(SoldierValue);   //それをテキストに入力
    }

    /// <summary>
    /// 兵数インプットを入力完了時に発動
    /// 最大値、最小値以内かを確認
    /// </summary>
    /// <param name="text"></param>
    private void OnSoldierInputEndEdit(string text)
    {
        //もし何も入力されて無ければ、返す
        if (string.IsNullOrEmpty(text)) return;

        //数字しか取らないようにしてるけど一応数字化可能かチェック
        if (int.TryParse(text, out int value))
        {
            //兵数が最大よりも大きくないかクランプ
            int clampedValue = Mathf.Clamp(value, 0, NewForcesData.MaxSoldiers);

            //もし最大or0未満だったら最大か最小にする
            UpdateSliderAndInputText(clampedValue);
        }
    }

    /// <summary>
    /// 兵数インプット入力中に発動
    /// スライダー側に反映
    /// </summary>
    /// <param name="text"></param>
    private void OnSoldierInputChanged(string text)
    {
        if (m_IsUpdating || string.IsNullOrEmpty(text)) return;

        if (int.TryParse(text, out int value))
        {
            m_IsUpdating = true;
            m_SoldierSlider.value = (float)value / (float)NewForcesData.MaxSoldiers;
            SoldierValue = value;
            m_IsUpdating = false;
        }
    }

    /// <summary>
    /// スライダーとインプットを同期　valueには実数値
    /// </summary>
    /// <param name="value"></param>
    private void UpdateSliderAndInputText(int value)
    {
        m_IsUpdating = true;

        m_SoldierValueInput.text = ((int)value).ToString(); //兵数テキスト変更
        m_NeedFundText.SetText(((int)value * NewForcesData.FundCost).ToString());          //必要資金テキスト変更
        m_NeedResourceText.SetText(((int)value * NewForcesData.ResourceCost).ToString());  //必要資源テキスト変更

        m_IsUpdating = false;
    }
}
