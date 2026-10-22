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
    }

    public void SetCountryForce(CountryData country)
    {
        foreach (var force in country.ForceList)
        {
            ForceDataUI currentForceObj = Instantiate(m_ForcePrehab, Vector3.zero, Quaternion.identity, m_ListContent);  //ListにForce情報が記入されたUIオブジェクトを配置していく
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
        if (BattleSystemManager.Instance.CurrentEditState == EditState.None)    //編集モードになる前
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().OpenEditButton();                //編集ボタン出現
            }

            BattleSystemManager.Instance.ChangeEditingState(EditState.Force);   //編集モードに移行
        }
        else if (BattleSystemManager.Instance.CurrentEditState == EditState.Force)  //編集モード後
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().CloseEditButton();               //編集ボタン消去
            }

            BattleSystemManager.Instance.MenuDict[MenuType.Order].GetComponent<OrderMenu>().ClearButtonList();  //命令ボタンを全消去
            BattleSystemManager.Instance.CloseOrderMenu();                      //命令メニュー消去
            BattleSystemManager.Instance.SelectingState.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);      //編集モードになった際に変更したプロヴィンスの色を修正
            BattleSystemManager.Instance.ChangeEditingState(EditState.None);    //編集モードオフ
        } 
    }

    /// <summary>
    /// 軍団編集ボタン押したとき
    /// </summary>
    public void ChangeEditMenu(bool on)
    {
        if (on)     //編集モード:オン
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().OpenEditButton();                //編集ボタン出現
            }

            BattleSystemManager.Instance.ChangeEditingState(EditState.Force);   //編集モードに移行
        }
        else if (!on)  //編集モード:オフ
        {
            foreach (var UI in m_ForceUIList)
            {
                UI.GetComponent<ForceDataUI>().CloseEditButton();               //編集ボタン消去
            }

            BattleSystemManager.Instance.MenuDict[MenuType.Order].GetComponent<OrderMenu>().ClearButtonList();  //命令ボタンを全消去
            BattleSystemManager.Instance.CloseOrderMenu();                      //命令メニュー消去
            BattleSystemManager.Instance.SelectingState.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);      //編集モードになった際に変更したプロヴィンスの色を修正
            BattleSystemManager.Instance.ChangeEditingState(EditState.None);    //編集モードオフ
        }
    }

    public void UpdateForceUI(CountryData country)
    {
        foreach (var obj in m_ForceUIList)
        {
            Destroy(obj);
        }

        m_ForceUIList.Clear();

        SetCountryForce(country);
    }
}
