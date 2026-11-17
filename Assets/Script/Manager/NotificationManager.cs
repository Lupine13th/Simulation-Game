using UnityEngine;

public class NotificationManager : SingletonManager<NotificationManager>
{
    [Header("宣戦布告通知のプレファブ")]
    [SerializeField] private DeclareWarNotificationUI m_DeclareWarNoticePrefab;
    [Header("被攻撃通知のプレファブ")]
    [SerializeField] private TakeAttackNotificationUI m_TakeAttackNoticePrefab;
    [Header("通知ポップアップの親")]
    [SerializeField] private Transform m_NoticeParent;

    /// <summary>
    /// 宣戦布告通知を生成
    /// </summary>
    /// <param name="ATCountry"></param>
    /// <param name="DFCountry"></param>
    public void InstanciateDeclareWarNotice(CountryData ATCountry, CountryData DFCountry)
    {
        var notice = Instantiate(m_DeclareWarNoticePrefab, m_NoticeParent);     //UI生成
        notice.SetTextAndFlag(ATCountry, DFCountry);                            //テキストと国旗をセット
    }

    /// <summary>
    /// 被攻撃通知を生成
    /// </summary>
    /// <param name="ATCountry"></param>
    /// <param name="DFCountry"></param>
    public void InstanciateTakeAttackNotice(CountryData ATCountry, State DFState)
    {
        var notice = Instantiate(m_TakeAttackNoticePrefab, m_NoticeParent);     //UI生成
        notice.SetTextAndFlag(ATCountry, DFState);                              //テキストと国旗をセット
    }
}
