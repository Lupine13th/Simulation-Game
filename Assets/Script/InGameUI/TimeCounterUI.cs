using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeCounterUI : MonoBehaviour
{
    [SerializeField] private Image m_NowStateImage;
    [SerializeField] private Image m_TimerGage;
    [SerializeField] private Sprite m_PlaySprite;
    [SerializeField] private Sprite m_StopSprite;
    [SerializeField] private TextMeshProUGUI m_TimerText;
    [SerializeField] private AudioSource m_ChaimSESource;
    [SerializeField] private AudioSource m_HandSESource;

    private const float kMaxWidth = 710f;
    private const int kMaxWidthValue = 24;

    private float m_Percent = 0.0f;

    public TextMeshProUGUI TimerText => m_TimerText;

    private void Start()
    {
        AudioManager.Instance.AddSEAudioSource(SEType.Chaim, m_ChaimSESource);
        AudioManager.Instance.AddBGMAudioSource(BGMType.Timer, m_HandSESource);
    }

    public void ChangeTimeImage(bool play)
    {
        switch(play)
        {
            case true:
                m_NowStateImage.sprite = m_PlaySprite;
                break;
            case false:
                m_NowStateImage.sprite = m_StopSprite;
                break;
        }
    }

    public void UpdateGage(int hour)
    {
        m_TimerGage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, kMaxWidth * ((float)hour / kMaxWidthValue));
    }
}
