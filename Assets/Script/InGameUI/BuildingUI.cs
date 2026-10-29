using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [Header("‚±‚ÌUI‚Ì”wŒi")]
    [SerializeField] private Image m_BackGround;

    [Header("Œš’z•¨‚Ì–¼‘O")]
    [SerializeField] private TextMeshProUGUI m_BuidingNameText;

    [Header("Œš’z•¨‚ÌŒø‰Ê")]
    [SerializeField] private TextMeshProUGUI m_BuidingValueText;

    public Image BackGround => m_BackGround;
    public TextMeshProUGUI BuidingNameText => m_BuidingNameText;
    public TextMeshProUGUI BuidingValueText => m_BuidingValueText;
}
