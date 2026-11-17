using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUI_Force : MonoBehaviour
{
    [Header("所有者国旗イメージ")]
    [SerializeField] private Image m_FlagImage;

    [Header("UI背景")]
    [SerializeField] private Image m_BackGround;

    [Header("矢印マーク")]
    [SerializeField] private GameObject m_ArrowMark;

    [Header("軍団名")]
    [SerializeField] private TextMeshProUGUI m_ForceNameText;

    [Header("兵数")]
    [SerializeField] private TextMeshProUGUI m_SoldiersText;

    [Header("攻撃力")]
    [SerializeField] private TextMeshProUGUI m_AttackText;

    [Header("防御力")]
    [SerializeField] private TextMeshProUGUI m_DefenceText;

    [Header("勝敗判定")]
    [SerializeField] private TextMeshProUGUI m_ResultText;

    [Header("勝敗判定テキストの親")]
    [SerializeField] private Animator m_ResultTextAnim;

    [SerializeField] public int ID;    //デバッグ用　識別番号

    /// <summary>
    /// 軍団のアニメオブジェクト
    /// </summary>
    public GameObject ForceObject { get; private set; }

    /// <summary>
    /// 軍団のデータ
    /// </summary>
    public ForceGroup ForceData { get; private set; }

    public Image FlagImage => m_FlagImage;
    public Image BackGround => m_BackGround;
    public TextMeshProUGUI ForceNameText => m_ForceNameText;
    public TextMeshProUGUI SoldiersText => m_SoldiersText;
    public TextMeshProUGUI AttackText => m_AttackText;
    public TextMeshProUGUI DefenceText => m_DefenceText;

    public static int TotalCount = 0;

    private void Awake()
    {
        ID = TotalCount++;  //識別番号を持たせて、デバッガで追跡可能に
    }


    public void SetDatas(ForceGroup forcedata, GameObject forceobject)
    {
        ForceData = forcedata;
        ForceObject = forceobject;
    }

    public void SetResultText(BattleResult result)
    {
        m_ResultText.gameObject.SetActive(true);//テキストをオン
        m_ResultTextAnim.Play("ResultText");    //アニメーション開始

        switch (result)
        {
            case BattleResult.Win:
                m_ResultText.SetText("WIN!");
                m_ResultText.color = Color.gold;
                break;
            case BattleResult.Lose:
                m_ResultText.SetText("LOSE!");
                m_ResultText.color = Color.purple;
                break;
            case BattleResult.Draw:
                m_ResultText.SetText("DRAW");
                m_ResultText.color = Color.white;
                break;
        }
    }

    /// <summary>
    /// 矢印マークをオンオフ
    /// </summary>
    /// <param name="on"></param>
    public void SetActiveArrowMark(bool on)
    {
        m_ArrowMark.SetActive(on);
    }
}
