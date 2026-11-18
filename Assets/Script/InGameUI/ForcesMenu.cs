using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ForcesMenu : TacticalMenu
{
    [SerializeField] private Transform m_ListContent;
    [SerializeField] private ForceDataUI m_ForcePrehab;
    [SerializeField] private EnlistMenu m_EnlistMenu;

    private List<GameObject> m_ForceUIList = new List<GameObject>();

    private bool m_EditingForce = false;

    public override void Init()
    {
        UpdateForceUI(BattleSystemManager.Instance.SelectingCountry);   //軍隊メニューを更新
        CloseEnlistMenu();                                              //徴兵メニューを閉じる
        BattleSystemManager.Instance.ResetOwnedAllStateColor(BattleSystemManager.Instance.SelectingCountry);                        //ステート色を一旦リセット
        BattleSystemManager.Instance.SelectingState.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);  //選択ステートのみ色変更
    }

    /// <summary>
    /// この勢力が所有する軍団分のUIを生成する
    /// </summary>
    /// <param name="country"></param>
    public void InstanciateForceDataUI(CountryData country)
    {
        foreach (var force in country.ForceList)
        {
            ForceDataUI currentForceObj = Instantiate(m_ForcePrehab, m_ListContent);  //ListにForce情報が記入されたUIオブジェクトを配置していく
            currentForceObj.SetForceDataUI(force);
            currentForceObj.RefreshPositionName();
            m_ForceUIList.Add(currentForceObj.gameObject);
        }

        m_EditingForce = false;
    }

    public void OpenEnlistMenu()
    {
        m_EnlistMenu.gameObject.SetActive(true);
    }

    public void CloseEnlistMenu()
    {
        m_EnlistMenu.ResetEnlistMenu();
        m_EnlistMenu.gameObject.SetActive(false);
    }

    /// <summary>
    /// 軍団編集ボタン押したとき
    /// </summary>
    public void ChangeEditMenu()
    {
        var bsManager = BattleSystemManager.Instance;

        if (bsManager.CurrentEditState == EditState.None)    //編集モードになる前
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().OpenEditButton();                //編集ボタン出現
            }

            bsManager.ChangeEditingState(EditState.Force);   //編集モードに移行
        }
        else if (bsManager.CurrentEditState == EditState.Force)  //編集モード後
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().CloseEditButton();               //編集ボタン消去
            }

            bsManager.MenuDict[MenuType.Order].GetComponent<OrderMenu>().ClearButtonList();  //命令ボタンを全消去
            bsManager.CloseOrderMenu();                      //命令メニュー消去

            foreach (var state in FieldManager.Instance.GetOwnedStateList(bsManager.PlayerCountry).ToArray())
            {
                if (state == bsManager.SelectingState)   //選択中のステートの場合
                {
                    state.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);    //選択カラーに戻す
                }
                else
                {
                    state.SetStateColor(bsManager.PlayerCountry.CountryColor);           //国家カラーに戻す
                }
            }

            bsManager.ChangeEditingState(EditState.None);    //編集モードオフ
        } 
    }

    /// <summary>
    /// 軍団編集ボタン押したとき
    /// </summary>
    public void ChangeEditMenu(bool on)
    {
        var bsManager = BattleSystemManager.Instance;

        if (on)     //編集モード:オン
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().OpenEditButton();                //編集ボタン出現
            }

            bsManager.ChangeEditingState(EditState.Force);   //編集モードに移行
        }
        else if (!on)  //編集モード:オフ
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().CloseEditButton();               //編集ボタン消去
            }

            bsManager.MenuDict[MenuType.Order].GetComponent<OrderMenu>().ClearButtonList();  //命令ボタンを全消去
            bsManager.CloseOrderMenu();                      //命令メニュー消去

            foreach (var state in FieldManager.Instance.GetOwnedStateList(bsManager.PlayerCountry).ToArray())
            {
                if (state == bsManager.SelectingState)   //選択中のステートの場合
                {
                    state.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);    //選択カラーに戻す
                }
                else
                {
                    state.SetStateColor(bsManager.PlayerCountry.CountryColor);           //国家カラーに戻す
                }
            }

            bsManager.ChangeEditingState(EditState.None);    //編集モードオフ
        }
    }

    public void UpdateForceUI(CountryData country)
    {
        foreach (var obj in m_ForceUIList)
        {
            Destroy(obj);
        }

        m_ForceUIList.Clear();

        InstanciateForceDataUI(country);
    }
}
