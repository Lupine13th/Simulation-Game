using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUI_Force : MonoBehaviour
{
    [Header("大きいUI")]
    [SerializeField] private GameObject m_LargeUI;
    [Header("小さいUI")]
    [SerializeField] private GameObject m_SmallUI;

    [Header("カーソルポイント中の背景")]
    [SerializeField] private CustomButton m_LargeUIButton;
    [Header("カーソルポイントしてない時の背景")]
    [SerializeField] private CustomButton m_SmallUIButton;

    [Header("所有者国旗イメージ")]
    [SerializeField] private Image m_FlagImage;

    [Header("所有者国旗イメージ(小)")]
    [SerializeField] private Image m_SmallFlagImage;

    [Header("UI背景")]
    [SerializeField] private Image m_BackGround;

    [Header("UI背景(小)")]
    [SerializeField] private Image m_SmallBackGround;

    [Header("矢印マーク")]
    [SerializeField] private GameObject m_ArrowMark;

    [Header("軍団名")]
    [SerializeField] private TextMeshProUGUI m_ForceNameText;

    [Header("兵数")]
    [SerializeField] private TextMeshProUGUI m_SoldiersText;

    [Header("兵数(小)")]
    [SerializeField] private TextMeshProUGUI m_SmallSoldiersText;

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

        m_LargeUIButton.onHover.AddListener(() => ChangeUISize(true));
        m_LargeUIButton.onHoverExit.AddListener(() => ChangeUISize(false));
        m_SmallUIButton.onHover.AddListener(() => ChangeUISize(true));
        m_SmallUIButton.onHoverExit.AddListener(() => ChangeUISize(false));
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

    public void UpdateForceUI(Force force)
    {
        if (m_LargeUI.activeSelf)   //大きいほうのUIが展開中
        {
            FlagImage.sprite = force.OwnerCountry.CountryFlag;  //国旗データ入力
            ForceNameText.SetText(force.ForcesName);            //軍団の名前を入力
            SoldiersText.SetText(force.BelongGroup.AllSoldier.ToString());    //軍団の兵数を入力
            AttackText.SetText(force.AttackPower.ToString());   //軍団の攻撃力を入力
            DefenceText.SetText(force.DefensePower.ToString()); //軍団の防御力を入力

            if (force.OwnerCountry == BattleSystemManager.Instance.PlayerCountry)   //プレイヤー勢力の軍ならば
            {
                BackGround.color = Color.green;
            }
            else if (BattleSystemManager.Instance.PlayerCountry.EnemyCountryList.Contains(force.OwnerCountry))  //敵勢力の軍ならば
            {
                BackGround.color = Color.red;
            }
            else if (BattleSystemManager.Instance.PlayerCountry.AlliesCountryList.Contains(force.OwnerCountry)) //同盟勢力の軍ならば
            {
                BackGround.color = Color.blue;
            }
            else
            {
                BackGround.color = Color.gray;
            }
        }
        else
        {
            m_SmallFlagImage.sprite = force.OwnerCountry.CountryFlag;
            m_SmallSoldiersText.SetText(force.BelongGroup.AllSoldier.ToString());

            if (force.OwnerCountry == BattleSystemManager.Instance.PlayerCountry)   //プレイヤー勢力の軍ならば
            {
                m_SmallBackGround.color = Color.green;
            }
            else if (BattleSystemManager.Instance.PlayerCountry.EnemyCountryList.Contains(force.OwnerCountry))  //敵勢力の軍ならば
            {
                m_SmallBackGround.color = Color.red;
            }
            else if (BattleSystemManager.Instance.PlayerCountry.AlliesCountryList.Contains(force.OwnerCountry)) //同盟勢力の軍ならば
            {
                m_SmallBackGround.color = Color.blue;
            }
            else
            {
                m_SmallBackGround.color = Color.gray;
            }
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

    /// <summary>
    /// 大きいor小さいUIを表示
    /// </summary>
    /// <param name="isLarge"></param>
    public void ChangeUISize(bool isLarge)
    {
        switch (isLarge)
        {
            case false:
                m_SmallUI.SetActive(true);
                m_LargeUI.SetActive(false);
                break;
            case true:
                m_SmallUI.SetActive(false);
                m_LargeUI.SetActive(true);
                break;
        }
    }
}
