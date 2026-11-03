using UnityEngine;

public class BuildingMenu : TacticalMenu
{
    [Header("建築物ボタンの親")]
    [SerializeField] private Transform m_BuildingButtonParent;

    [Header("建築物ボタンのプレハブ")]
    [SerializeField] private BuildingButton m_BuildingButtonPrefab;

    public override void Init()
    {
        foreach (var build in FieldManager.Instance.BuildingDataDict.Values)   //建物のデータ数だけ回す
        {
            BuildingButton button = Instantiate(m_BuildingButtonPrefab, m_BuildingButtonParent);
            button.SetData(build);
        }
    }
}
