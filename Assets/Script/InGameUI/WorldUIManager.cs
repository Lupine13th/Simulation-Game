using System.Collections.Generic;
using UnityEngine;

public class WorldUIManager : SingletonManager<WorldUIManager>
{
    [Header("ステート名UIのプレファブ")]
    [SerializeField] private WorldSpaceUI_State m_StateUIPrefab;

    [Header("軍団UIのプレファブ")]
    [SerializeField] private WorldSpaceUI_Force m_ForceUIPrefab;

    [Header("ステート上アイコンのプレファブ")]
    [SerializeField] private GameObject m_StateIconPrefab;

    [Header("ステート名UIの親")]
    [SerializeField] private Transform m_StateUIParent;

    [Header("軍団UIの親")]
    [SerializeField] private Transform m_ForceUIParent;

    /// <summary>
    /// ステートのリスト
    /// </summary>
    private List<State> m_StateList= new List<State>();

    /// <summary>
    /// ステートUIのリスト
    /// </summary>
    private List<WorldSpaceUI_State> m_StateUIList= new List<WorldSpaceUI_State>();

    /// <summary>
    /// 軍団UIの辞書型
    /// </summary>
    private Dictionary<Force, WorldSpaceUI_Force> m_ForceUIDict= new Dictionary<Force, WorldSpaceUI_Force>();

    /// <summary>
    /// ステート上のアイコンの辞書型
    /// </summary>
    private Dictionary<State, SpriteRenderer> m_StateIconDict= new Dictionary<State, SpriteRenderer>();

    private Camera m_MainCamera;

    private Canvas m_UICanvas;

    private void Start()
    {
        m_MainCamera = Camera.main;
        m_UICanvas = GetComponent<Canvas>();    //キャンバス取得
    }

    /// <summary>
    /// 最初に呼び出される
    /// </summary>
    public void Init()
    {
        m_StateList = FieldManager.Instance.StateList; //ステートリストを取得

        foreach (var state in m_StateList)
        {
            Vector3 screenPos = m_MainCamera.WorldToScreenPoint(state.MidModelPos);   //カメラから見たステートの位置をキャンバス上の座標に変換

            WorldSpaceUI_State ui = Instantiate(m_StateUIPrefab, screenPos, Quaternion.identity ,m_StateUIParent);  //UI生成

            m_StateUIList.Add(ui);  //リストに追加
        }

        UpdateStateUI();

        RefreshUIPosition();
    }

    private void LateUpdate()
    {
        RefreshUIPosition();
    }

    public void RefreshUIPosition()
    {
        switch (BattleSystemManager.Instance.CurrentCameraState)    //カメラの状態によって描画距離を切り替える
        {
            case CamaraState.TopDown:
                m_UICanvas.planeDistance = 20;
                break;
            case CamaraState.Tactical:
                m_UICanvas.planeDistance = 10;
                break;
            case CamaraState.Dramatic:
                m_UICanvas.planeDistance = 5;
                break;
        }

        //==================ステートUI更新====================

        for (int i = 0; i < m_StateList.Count; i++)
        {
            Vector3 screenPos = m_MainCamera.WorldToScreenPoint(m_StateList[i].MidModelPos);   //カメラから見たステートの位置をキャンバス上の座標に変換

            RectTransform stateUiPos = m_StateUIList[i].GetComponent<RectTransform>();

            Vector2 localPoint; //キャンバス上のローカル座標

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_StateUIParent as RectTransform, screenPos, m_MainCamera, out localPoint)) //ワールドUI用キャンバスのアンカーやスケールにフィットさせる
            {
                stateUiPos.localPosition = localPoint;
                stateUiPos.localRotation = Quaternion.identity;
            }
        }

        //==================ステートUI更新End====================


        //==================軍団UI更新====================

        if (m_ForceUIDict.Count > 0)
        {
            foreach (var ui in m_ForceUIDict.Values)
            {
                Vector3 screenPos = m_MainCamera.WorldToScreenPoint(ui.ForceObject.transform.position);   //カメラから見た軍団モデルの位置をキャンバス上の座標に変換

                RectTransform forceUiPos = ui.GetComponent<RectTransform>();

                Vector2 localPoint; //キャンバス上のローカル座標

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ForceUIParent as RectTransform, screenPos, m_MainCamera, out localPoint)) //ワールドUI用キャンバスのアンカーやスケールにフィットさせる
                {
                    forceUiPos.localPosition = localPoint;
                    forceUiPos.localRotation = Quaternion.identity;
                }
            }
        }

        //==================軍団UI更新End====================
    }

    /// <summary>
    /// 軍団が出来た時にこのメソッドを呼び、UIを作る
    /// </summary>
    /// <param name="force">その軍団のデータ</param>
    /// <param name="model">軍団のアニメオブジェクト</param>
    public void InstantiateForceUI(Force force, GameObject model)
    {
        WorldSpaceUI_Force ui = Instantiate(m_ForceUIPrefab, m_ForceUIParent);  //UI生成
        m_ForceUIDict[force] = ui;                                              //辞書型に追加

        ui.SetDatas(force, model);  //データをセット

        UpdateForceUI();
    }

    /// <summary>
    /// 軍団が消えた時or移動時にUIを消す
    /// </summary>
    /// <param name="force">その軍団のデータ</param>
    public void RemoveForceUI(Force force)
    {
        if (m_ForceUIDict.ContainsKey(force))
        {
            Destroy(m_ForceUIDict[force].gameObject);
            m_ForceUIDict.Remove(force);
        }
    }

    /// <summary>
    /// ステートUIの情報を更新
    /// </summary>
    public void UpdateStateUI()
    {
        for (int i = 0; i < m_StateList.Count; i++)
        {
            if (m_StateList[i].OwnedCountry == null)    //誰も持っていないステートの場合
            {
                m_StateUIList[i].BackGround.color = Color.white;    //背景を白色に
                m_StateUIList[i].FlagImage.sprite = null;           //国旗は無しで

                m_StateUIList[i].StateNameText.color = Color.black;
            }
            else
            {
                Vector3 color = m_StateList[i].OwnedCountry.CountryColor;

                m_StateUIList[i].BackGround.color = new Color(color.x, color.y, color.z);       //背景を勢力の色に
                m_StateUIList[i].FlagImage.sprite = m_StateList[i].OwnedCountry.CountryFlag;    //国旗もロード

                if (color.x > 0.5f && color.y > 0.5f && color.z > 0.5f) //背景が白に近い場合テキストを黒色にする
                {
                    m_StateUIList[i].StateNameText.color = Color.black;
                }
                else
                {
                    m_StateUIList[i].StateNameText.color = Color.white;
                }
            }

            m_StateUIList[i].StateNameText.SetText(m_StateList[i].StateNameJP);             //ステート名をテキストに入力
        }
    }

    /// <summary>
    /// 軍団UIの情報を更新
    /// </summary>
    public void UpdateForceUI()
    {
        foreach (var force in m_ForceUIDict)
        {
            force.Value.FlagImage.sprite = FieldManager.Instance.CountryList[force.Key.OwnerCountryIndex].CountryFlag;  //国旗データ入力
            force.Value.ForceNameText.SetText(force.Key.ForcesName);            //軍団の名前を入力
            force.Value.SoldiersText.SetText(force.Key.Soldiers.ToString());    //軍団の兵数を入力
            force.Value.AttackText.SetText(force.Key.AttackPower.ToString());   //軍団の攻撃力を入力
            force.Value.DefenceText.SetText(force.Key.DefensePower.ToString()); //軍団の防御力を入力

            Vector3 color = FieldManager.Instance.CountryList[force.Key.OwnerCountryIndex].CountryColor;    //国家カラー取得
            force.Value.BackGround.color = new Color(color.x, color.y, color.z);                            //背景に適応
        }
    }
}
