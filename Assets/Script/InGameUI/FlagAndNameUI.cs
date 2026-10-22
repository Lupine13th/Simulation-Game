using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlagAndNameUI : MonoBehaviour
{
    [Header("同盟中or戦争中の旗のイメージ")]
    [SerializeField] private Image m_FlagImage;

    [Header("同盟中or戦争中の国家の名前")]
    [SerializeField] private TextMeshProUGUI m_CountryText;

    public Image FlagImage => m_FlagImage;
    public TextMeshProUGUI CountryText => m_CountryText;
}
