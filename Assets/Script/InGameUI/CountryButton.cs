using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountryButton : MonoBehaviour
{
    [Header("ボタンの中に描かれる旗マーク")]
    [SerializeField] private Image m_FlagImage;

    [Header("勢力の名前")]
    [SerializeField] private TextMeshProUGUI m_CountryNameText;

    [Header("評価の値")]
    [SerializeField] private TextMeshProUGUI m_FavorValueText;

    [Header("このボタンクリック時に反応する部分")]
    [SerializeField] private CustomButton m_Button;

    public CountryData ButtonsData { get; private set; }

    /// <summary>
    /// このボタンクリック時に反応する部分
    /// </summary>
    public CustomButton Button => m_Button;

    /// <summary>
    /// 国家情報をボタンに持たせる
    /// </summary>
    /// <param name="data"></param>
    public void SetButtonData(CountryData data)
    {
        ButtonsData = data;
    }

    /// <summary>
    /// 旗のイメージを付ける
    /// </summary>
    /// <param name="flag"></param>
    public void SetFlagImage(Sprite flag)
    {
        m_FlagImage.sprite = flag;
    }

    /// <summary>
    /// 勢力の名前を勢力ボタンに入力する
    /// </summary>
    /// <param name="name"></param>
    public void SetCountryText(string name)
    {
        m_CountryNameText.SetText(name);
    }

    /// <summary>
    /// プレイヤー国家に対する評価値を勢力ボタンに入力する
    /// </summary>
    /// <param name="favor"></param>
    public void SetFavorText(int favor)
    {
        Image background = GetComponent<Image>();

        if (background == null)
        {
            return;
        }

        if (favor >= 0)
        {
            m_FavorValueText.SetText("+" + favor.ToString()); //テキストに入力
        }
        else
        {
            m_FavorValueText.SetText(favor.ToString()); //テキストに入力
        }
        

        //TODO:同盟の場合、色を緑色にする
        //TODO:戦争中の場合、色を黒目の赤色にする

        if (favor > 20)         //評価値が20以上の場合
        {
            background.color = Color.skyBlue;   //水色にする
        }
        else if (favor < -20)   //評価値が-20以下の場合
        {
            background.color = Color.lightPink; //桃色にする
        }
        else
        {
            background.color = Color.gray;      //中立の場合、灰色に
        }
    }
}
