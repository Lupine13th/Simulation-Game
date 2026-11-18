using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyMenu : TacticalMenu
{
    [Header("外交メニューで選択中の国家の旗")]
    [SerializeField] private Image m_SelectingCountryFlagImage;

    [Header("外交メニューで選択中の国家の名前テキスト")]
    [SerializeField] private TextMeshProUGUI m_SelectingCountryText;

    [Header("国家リストの親")]
    [SerializeField] private Transform m_CountryButtonParent;

    [Header("戦争中の国家リストの親")]
    [SerializeField] private Transform m_EnemyListParent;

    [Header("同盟中の国家リストの親")]
    [SerializeField] private Transform m_AlliesListParent;

    [Header("国家リストのボタンのプレファブ")]
    [SerializeField] private CountryButton m_CountryButtonPrefab;

    [Header("選択中国家の外交関係のUIプレファブ")]
    [SerializeField] private FlagAndNameUI m_FlagAndNamePrefab;

    public CountryData SelectingCountry { get; private set; }

    private List<FlagAndNameUI> m_FlagNamePrefabList = new List<FlagAndNameUI>();
    private List<CountryButton> m_CountryButtonList = new List<CountryButton>();

    public override void Init()
    {
        AudioManager.Instance.PlaySE(SEType.Main, SEType.Diplomacy);

        for (int i = 0; i < m_CountryButtonList.Count; i++)
        {
            Destroy(m_CountryButtonList[i].gameObject);
        }

        m_CountryButtonList.Clear();

        foreach (var country in FieldManager.Instance.CountryList)
        {
            CountryButton button = Instantiate(m_CountryButtonPrefab, m_CountryButtonParent);   //ボタン精製

            button.SetButtonData(country);              //国家データを入力
            button.SetCountryText(country.CountryName); //国家名登録
            button.SetFavorText(BattleSystemManager.Instance.CalculateFavorValue(BattleSystemManager.Instance.PlayerCountry, country)); //友好度を入力
            button.SetFlagImage(country.CountryFlag);   //旗も登録

            button.Button.onClick.AddListener(() => SetSelectingCountryOnDiplomacyMenu(country));   //ボタンをクリック時に「選択中の国家を設定する」機能を追加

            m_CountryButtonList.Add(button);            //リストに追加
        }

        SetSelectingCountryOnDiplomacyMenu(BattleSystemManager.Instance.SelectingCountry); //ステート上での選択中の国家を選択状態にする

        BattleSystemManager.Instance.ResetOwnedAllStateColor(BattleSystemManager.Instance.SelectingCountry);                        //ステート色を一旦リセット
        BattleSystemManager.Instance.ChangeOwnedAllStateColor(BattleSystemManager.Instance.SelectingCountry, FieldManager.Instance.StateColorDict[StateColorType.Selecting]);   //勢力に属するステート全ての色を変更
    }

    /// <summary>
    /// 選択中の国家をUIに表示
    /// </summary>
    /// <param name="data"></param>
    public void SetSelectingCountryOnDiplomacyMenu(CountryData data)
    {
        for (int i = 0; i < m_FlagNamePrefabList.Count; i++)
        {
            Destroy(m_FlagNamePrefabList[i].gameObject);    //UI削除
        }

        m_FlagNamePrefabList.Clear();                       //リストもクリア

        SelectingCountry = data;    //選択中の国家を設定

        m_SelectingCountryFlagImage.sprite = data.CountryFlag;  //国旗変更
        m_SelectingCountryText.SetText(data.CountryName);       //国名変更

        int enemyCount = 0;
        int alliesCount = 0;

        for (int i = 0; i < data.EnemyCountryList.Count; i++)   //この勢力の敵の数だけ回す
        {
            enemyCount++;

            if (enemyCount > 3) //三個以上は表示できないためキャンセル
            {
                continue;
            }

            FlagAndNameUI ui = Instantiate(m_FlagAndNamePrefab, m_EnemyListParent); //UI生成

            m_FlagNamePrefabList.Add(ui);   //リストに加える

            ui.FlagImage.sprite = data.EnemyCountryList[i].CountryFlag; //この国家の旗を設定
            ui.CountryText.SetText(data.EnemyCountryList[i].CountryName); //この国家の旗を設定
        }

        for (int i = 0; i < data.AlliesCountryList.Count; i++)   //この勢力の味方の数だけ回す
        {
            alliesCount++;

            if (alliesCount > 3) //三個以上は表示できないためキャンセル
            {
                continue;
            }

            FlagAndNameUI ui = Instantiate(m_FlagAndNamePrefab, m_AlliesListParent);

            m_FlagNamePrefabList.Add(ui);   //リストに加える

            ui.FlagImage.sprite = data.AlliesCountryList[i].CountryFlag; //この国家の旗を設定
            ui.CountryText.SetText(data.AlliesCountryList[i].CountryName); //この国家の旗を設定
        }
    }

    /// <summary>
    /// 宣戦布告ボタン
    /// </summary>
    public void DeclareWarButton()
    {
        if (BattleSystemManager.Instance.PlayerCountry.EnemyCountryList.Contains(SelectingCountry)) //既に宣戦布告していたら
        {
            return; //無視
        }

        BattleSystemManager.Instance.PlayerCountry.DeclareWar(SelectingCountry);    //プレイヤーが選択中の国家に対して宣戦布告

        SetSelectingCountryOnDiplomacyMenu(SelectingCountry);   //UI更新
    }

    /// <summary>
    /// 同盟ボタン
    /// </summary>
    public void FormAllianceButton()
    {
        if (BattleSystemManager.Instance.PlayerCountry.AlliesCountryList.Contains(SelectingCountry)) //既に同盟していたら
        {
            return; //無視
        }

        BattleSystemManager.Instance.PlayerCountry.FormAlliance(SelectingCountry);    //プレイヤーが選択中の国家に対して同盟

        SetSelectingCountryOnDiplomacyMenu(SelectingCountry);   //UI更新
    }
}
