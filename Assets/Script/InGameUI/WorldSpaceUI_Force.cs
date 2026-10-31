using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUI_Force : MonoBehaviour
{
    [Header("所有者国旗イメージ")]
    [SerializeField] private Image m_FlagImage;

    [Header("UI背景")]
    [SerializeField] private Image m_BackGround;

    [Header("軍団名")]
    [SerializeField] private TextMeshProUGUI m_ForceNameText;

    [Header("兵数")]
    [SerializeField] private TextMeshProUGUI m_SoldiersText;

    [Header("攻撃力")]
    [SerializeField] private TextMeshProUGUI m_AttackText;

    [Header("防御力")]
    [SerializeField] private TextMeshProUGUI m_DefenceText;

    [SerializeField] public int ID;    //デバッグ用　識別番号

    /// <summary>
    /// 軍団のアニメオブジェクト
    /// </summary>
    public GameObject ForceObject { get; private set; }

    /// <summary>
    /// 軍団のデータ
    /// </summary>
    public Force ForceData { get; private set; }

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


    public void SetDatas(Force forcedata, GameObject forceobject)
    {
        ForceData = forcedata;
        ForceObject = forceobject;
    }
}
