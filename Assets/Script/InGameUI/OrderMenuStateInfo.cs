using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderMenuStateInfo : MonoBehaviour
{
    [SerializeField] private Image m_FlagImage;
    [SerializeField] private TextMeshProUGUI m_CountryNameText;
    [SerializeField] private TextMeshProUGUI m_EnemyAllSoldierText;
    [SerializeField] private TextMeshProUGUI m_GeographText;

    public void RefreshInfoUI(CountryData data)
    {
        //‘‰Æ‚ÌŠø‚ğ“o˜^
        m_FlagImage.sprite = data.CountryFlag;

        //‘‰Æ‚Ì–¼‘O‚ğ“o˜^
        m_CountryNameText.SetText(data.CountryName);

        //‘‰Æ‚ÌŠ—L‚·‚éŒR—Ê‚ğ“o˜^
        m_EnemyAllSoldierText.SetText(data.AllSoldiers.ToString());
    }
}
