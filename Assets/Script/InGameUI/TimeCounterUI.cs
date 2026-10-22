using UnityEngine;
using UnityEngine.UI;

public class TimeCounterUI : MonoBehaviour
{
    [SerializeField] private Image m_NowStateImage;
    [SerializeField] private Sprite m_PlaySprite;
    [SerializeField] private Sprite m_StopSprite;

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
}
