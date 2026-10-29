using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BuildingButton : MonoBehaviour
{
    [Header("建築物名のテキスト")]
    [SerializeField] private TextMeshProUGUI m_NameText;

    [Header("建築物の効果のテキスト")]
    [SerializeField] private TextMeshProUGUI m_ValueText;

    Button m_Button;

    /// <summary>
    /// このボタンを押したときに建築されるもののデータ
    /// </summary>
    public BuildingData Data { get; private set; }

    /// <summary>
    /// データをセット＆UIに記入
    /// </summary>
    /// <param name="data"></param>
    public void SetData(BuildingData data)
    {
        Data = data;
        m_NameText.SetText(data.dispName);
        m_ValueText.SetText(data.value);

        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(() => ArchitectBuildingThisState());
    }

    /// <summary>
    /// 建築メソッドをボタンから実行する
    /// </summary>
    public void ArchitectBuildingThisState()
    {
        BattleSystemManager.Instance.SelectingState.ArchitectBuilding(Data.type);
        Debug.Log("建築完了" + Data.type);
    }
}
