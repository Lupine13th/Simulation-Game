using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceUI_State : MonoBehaviour
{
    [Header("所有者国旗イメージ")]
    [SerializeField] private Image m_FlagImage;

    [Header("UI背景")]
    [SerializeField] private Image m_BackGround;

    [Header("ステート名")]
    [SerializeField] private TextMeshProUGUI m_StateNameText;

    public Image FlagImage => m_FlagImage;
    public Image BackGround => m_BackGround;
    public TextMeshProUGUI StateNameText => m_StateNameText;
}
