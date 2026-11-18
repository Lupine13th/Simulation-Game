using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("プレイヤーの国旗")]
    [SerializeField] private Image m_PlayerFlag;

    [Header("占領したステート数テキスト")]
    [SerializeField] private TextMeshProUGUI m_OccupiedStateCountText;

    [Header("降伏させて勢力数テキスト")]
    [SerializeField] private TextMeshProUGUI m_SurrenderCountryCountText;

    [Header("死傷者数テキスト")]
    [SerializeField] private TextMeshProUGUI m_CasualtiesCountText;

    [Header("署名日のテキスト")]
    [SerializeField] private TextMeshProUGUI m_SignatureDataText;

    [Header("署名GIF")]
    [SerializeField] private UniGifImage m_SignatureGIF;

    [Header("ゲーム終了ボタン")]
    [SerializeField] private GameObject m_SignatureButton;

    private float m_GameEndCount = 0.0f;

    private bool m_GameEnded = false;

    private void Update()
    {
        if (m_GameEnded)
        {
            m_GameEndCount += Time.deltaTime;   //時間を進める

            if (m_GameEndCount > 5.0f)
            {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    /// <summary>
    /// 国旗、占領したステート数、降伏させた勢力数、死傷者数をUIに反映する
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="stateCount"></param>
    /// <param name="surrenderCount"></param>
    /// <param name="casualtiesCount"></param>
    public void SetTextAndFlag(Sprite flag, int stateCount, int surrenderCount, int casualtiesCount, string date)
    {
        m_PlayerFlag.sprite = flag;                                     //国旗を反映
        m_OccupiedStateCountText.SetText(stateCount.ToString());        //占領したステート数
        m_SurrenderCountryCountText.SetText(surrenderCount.ToString()); //降伏させた勢力数
        m_CasualtiesCountText.SetText(casualtiesCount.ToString());      //死傷者数
        m_SignatureDataText.SetText(date);                              //日付
    }

    /// <summary>
    /// ゲーム終了ボタン押下時
    /// </summary>
    public void GameOver()
    {
        m_SignatureButton.gameObject.SetActive(false);
        m_SignatureGIF.SetGifFromUrl("https://www.image2url.com/r2/default/gifs/1794300973505-06871898-c53c-45a0-8263-1284a95cf365.gif", true);
        m_GameEnded = true;
    }
}
