using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum MenuType
{ 
    None,
    State = 0,
    Country = 1,
    Forces = 2,
    Diplomacy = 3,
    Enlist = 4,
    Order = 5,
    Building = 6
}

public enum EditState
{
    None,
    Force
}

public class BattleSystemManager : SingletonManager<BattleSystemManager>
{
    [Serializable]
    public struct MenuData
    {
        public MenuType type;
        public TacticalMenu menu;
    }

    
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private GameObject m_TacticalMenu;

    [SerializeField] private GameObject m_SoldierPrefab;  //兵士のプレファブ

    [SerializeField] private List<MenuData> m_MenuDataList = new List<MenuData>();

    public Dictionary<ForceType, EnlistData> EnlistDataDict { get; private set; } = new Dictionary<ForceType, EnlistData>();

    [SerializeField] private List<GameObject> m_LabelList = new List<GameObject>();

    [SerializeField] private TimeCounterUI m_TimeCounterUI;

    [SerializeField] private InventryUI m_InventryUI;

    [SerializeField] private AudioSource m_MainBGMPlayer;
    [SerializeField] private AudioSource m_MainSEPlayer;

    [Header("")]
    [SerializeField] private ResultUI m_ResultUI;

    private Dictionary<Direction, float>  m_CameraMoveSpeed = new Dictionary<Direction, float>()
    {
        { Direction.North, 0.0f },
        { Direction.South, 0.0f },
        { Direction.East, 0.0f },
        { Direction.West, 0.0f }
    };

    public CamaraState CurrentCameraState { get; private set; } = CamaraState.None;

    public GameObject SoldierPrefab => m_SoldierPrefab;

    public InventryUI InventryUI => m_InventryUI;

    public TimeCounterUI TimeCounterUI => m_TimeCounterUI;

    public State SelectingState { get; private set; }
    public Force CurrentEditingForce { get; private set; }
    public CountryData SelectingCountry { get; private set; }
    public CountryData PlayerCountry { get; private set; }
    public MenuType MenuStatus { get; private set; } = MenuType.None;

    public EditState CurrentEditState { get; private set; } = EditState.None;

    private State CurrentSelectingStateBorder;

    public Dictionary<MenuType, TacticalMenu> MenuDict { get; private set; } = new Dictionary<MenuType, TacticalMenu>();

    /// <summary>
    /// 部隊の指揮官の画像データリスト
    /// </summary>
    public List<Sprite> CommanderPortraitList { get; private set; } = new List<Sprite>();

    private const float kOpenLabelPositionX = -185.0f; //付箋を少し出す位置
    private const float kCloseLabelPositionX = -269.0f; //付箋の元の位置

    private const string kCommanderPortraitFolderPath = "Assets/Texture/Portrait/Commander";

    /// <summary>
    /// ポインタがUIに触れていて、別の処理でその影響をかき消させないようにするフラグ
    /// </summary>
    private bool m_IsPointerOverUI = false;

    async Task Start()
    {
        foreach (var data in m_MenuDataList)
        {
            MenuDict.Add(data.type, data.menu);
        }
        CurrentCameraState = CamaraState.TopDown;
        SetCameradefaultPosition(); //カメラを初期値(見下ろし)に
        CloseTacticalMenu();    //戦術メニュークローズ

        AddressableManager addressable = AddressableManager.Instance;

        string statusCsv = await addressable.LoadCsvTextAsync(addressable.MasterAddress[MasterData.ForceStatus]);   //マスターデータから情報取得

        addressable.ParseCsvStatusData(statusCsv, EnlistDataDict);  //データ分解後、EnlistDataListに入れる

        //----------指揮官の画像を読み取り----------

        int maxIndex = AddressableManager.Instance.GetTextureCount(kCommanderPortraitFolderPath);

        for (int i = 0; i < maxIndex; i++)
        {
            string leaderId = "Commander" + i;
            CommanderPortraitList.Add(await AddressableManager.Instance.LoadSpriteAsync(leaderId));
        }

        FieldManager.Instance.StopTimeCount();
        m_TimeCounterUI.ChangeTimeImage(false);

        AudioManager.Instance.AddBGMAudioSource(BGMType.Main, m_MainBGMPlayer); //メインBGM
        AudioManager.Instance.AddSEAudioSource(SEType.Main, m_MainSEPlayer);    //メインSE
        AudioManager.Instance.PlayBGM(BGMType.Main, BGMType.Main_Peace); //メインBGMを再生
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("SampleScene");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        if (Input.GetKeyDown(KeyCode.Tab))    //TAB入力
        {
            ChangeCameraOrder();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (FieldManager.Instance.IsPlaying)
            {
                case true:
                    FieldManager.Instance.StopTimeCount();
                    m_TimeCounterUI.ChangeTimeImage(false);
                    break;
                case false:
                    FieldManager.Instance.PlayTimeCount();
                    m_TimeCounterUI.ChangeTimeImage(true);
                    break;
            }
        }

        if (Input.GetKey(KeyCode.W))    //W入力
        {
            m_CameraMoveSpeed[Direction.North] += 0.01f;
        }
        else
        {
            m_CameraMoveSpeed[Direction.North] -= 0.01f;
        }

        if (Input.GetKey(KeyCode.S))    //S入力
        {
            m_CameraMoveSpeed[Direction.South] += 0.01f;
        }
        else
        {
            m_CameraMoveSpeed[Direction.South] -= 0.01f;
        }

        if (Input.GetKey(KeyCode.A))    //A入力
        {
            m_CameraMoveSpeed[Direction.West] += 0.01f;
        }
        else
        {
            m_CameraMoveSpeed[Direction.West] -= 0.01f;
        }

        if (Input.GetKey(KeyCode.D))    //D入力
        {
            m_CameraMoveSpeed[Direction.East] += 0.01f;
        }
        else
        {
            m_CameraMoveSpeed[Direction.East] -= 0.01f;
        }

        if (CheckMoveValue())
        {
            UpdateCameraPosition();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)  //右クリック時
        {
            switch (CurrentEditState)
            {
                default:
                    break;
                case EditState.None:
                    StateClick(MouseButton.Right);
                    break;
                case EditState.Force:
                    //StateClick(MouseButton.Right);
                    break;
            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)   //左クリック時
        {
            switch (CurrentEditState)
            {
                default:
                    break;
                case EditState.None:
                    StateClick(MouseButton.Left);
                    break;
                case EditState.Force:
                    //StateClick(MouseButton.Left);
                    break;
            }
        }

        if (CurrentEditState == EditState.Force && CurrentEditingForce != null) //軍隊配置の編集モードかつ編集する部隊を選択済みのとき
        {
            CheckMouseOverOnBorder();

            if (Mouse.current.leftButton.wasPressedThisFrame)   //左クリック時
            {
                ForceEditProvinceLeftClick();   //軍団の編集をマップ上で行う
            }
        }
        //Debug.Log(Instance);
    }

    public void ChangeCameraOrder() //カメラ状態を順番に切り替える
    {
        switch (CurrentCameraState)
        {
            default:
                break;
            case CamaraState.TopDown:
                CurrentCameraState = CamaraState.Tactical;
                SetCameradefaultPosition();
                break;
            case CamaraState.Tactical:
                CurrentCameraState = CamaraState.Dramatic;
                SetCameradefaultPosition();
                break;
            case CamaraState.Dramatic:
                CurrentCameraState = CamaraState.TopDown;
                SetCameradefaultPosition();
                break;
        }
    }

    /// <summary>
    /// 現在の状況に応じてカメラ位置を更新
    /// </summary>
    public void SetCameradefaultPosition()  
    {
        Quaternion cameraRotation = new Quaternion();

        switch (CurrentCameraState)
        {
            default:
                break;
            case CamaraState.TopDown:
                cameraRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                m_MainCamera.transform.SetPositionAndRotation(new Vector3(0.0f, 50.0f, 0.0f), cameraRotation);  //直上からの見下ろし位置へカメラを移動
                WorldUIManager.Instance.ForceUIParent.gameObject.SetActive(false);
                WorldUIManager.Instance.StateUIParent.gameObject.SetActive(true);
                break;
            case CamaraState.Tactical:
                cameraRotation = Quaternion.Euler(60.0f, 0.0f, 0.0f);
                m_MainCamera.transform.SetPositionAndRotation(new Vector3(0.0f, 20.0f, -20.0f), cameraRotation);//斜め上からの戦術カメラに移動
                WorldUIManager.Instance.ForceUIParent.gameObject.SetActive(true);
                WorldUIManager.Instance.StateUIParent.gameObject.SetActive(true);
                break;
            case CamaraState.Dramatic:
                cameraRotation = Quaternion.Euler(15.0f, 0.0f, 0.0f);
                m_MainCamera.transform.SetPositionAndRotation(new Vector3(0.0f, 5.0f, -20.0f), cameraRotation); //真横からのドラマティックカメラに移動
                WorldUIManager.Instance.ForceUIParent.gameObject.SetActive(true);
                WorldUIManager.Instance.StateUIParent.gameObject.SetActive(true);
                break;
        }
    }

    public void UpdateCameraPosition()
    {
        Vector3 cameraPos = m_MainCamera.transform.position;

        cameraPos.z += m_CameraMoveSpeed[Direction.North] - m_CameraMoveSpeed[Direction.South];
        cameraPos.x += m_CameraMoveSpeed[Direction.East] - m_CameraMoveSpeed[Direction.West];

        m_MainCamera.transform.SetPositionAndRotation(cameraPos, m_MainCamera.transform.rotation);
    }

    public void StateClick(MouseButton click)
    {
        if (EventSystem.current != null && IsPointerOverUI())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        Province selectingProvince;

        if (Physics.Raycast(ray, out hit))  //Stateに触っていれば
        {
            switch (click)
            {
                default:
                    break;
                case MouseButton.Right:                             //=========================右クリック時==========================
                    if (SelectingState != null) SelectingState.ResetStateColor();   //選択ステートの色をリセット

                    selectingProvince = hit.collider.GetComponent<Province>();      //クリックしたプロヴィンス
                    SelectingState = selectingProvince.OwnedState;                  //選択ステートに設定
                    SelectingCountry = SelectingState.OwnedCountry;
                    OpenStateMenu();                                                //戦術メニューオープン
                    break;
                case MouseButton.Left:                              //=========================左クリック時==========================
                    if (hit.collider.GetComponent<Province>().OwnedState.OwnedCountry == null) return;

                    if (SelectingState != null) SelectingState.ResetStateColor();   //選択ステートの色をリセット

                    selectingProvince = hit.collider.GetComponent<Province>();      //クリックしたプロヴィンス
                    SelectingState = selectingProvince.OwnedState;                  //選択ステートに設定
                    SelectingCountry = SelectingState.OwnedCountry;
                    OpenCountryMenu();                                              //国家メニューオープン
                    break;
            }

            AudioManager.Instance.PlaySE(SEType.Click, SEType.Click);
        }
    }

    /// <summary>
    /// 軍団編成中に国境を左クリックした場合
    /// </summary>
    public void ForceEditProvinceLeftClick()
    {
        if (EventSystem.current != null && IsPointerOverUI())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        Province selectingProvince;

        if (Physics.Raycast(ray, out hit))  //Stateに触っていれば
        {
            selectingProvince = hit.collider.GetComponent<Province>(); //クリックしたプロヴィンス

            foreach (var state in PlayerCountry.RelateStateDict)  
            {
                if (CurrentSelectingStateBorder == state.Key)   //クリックしたプロヴィンスが現在ハイライト中のステートと同じか？
                {
                    if (state.Value[0].type == ForcePositionType.Border)    //クリックしたのが国境ハイライトだった場合
                    {
                        FieldManager.Instance.SetForceOnBorder(CurrentSelectingStateBorder, CurrentEditingForce);  //現在ハイライト中のステートに軍団を配置
                    }
                    if (state.Value[0].type == ForcePositionType.Guarrison)    //クリックしたのが駐屯ハイライトだった場合
                    {
                        FieldManager.Instance.SetForceOnState(CurrentSelectingStateBorder, CurrentEditingForce);  //現在ハイライト中のステートに軍団を配置
                    }
                }
            }
        }
    }

    /// <summary>
    /// 国境上にマウスオーバーしたか？
    /// </summary>
    public void CheckMouseOverOnBorder()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        Province pointProv = null;

        if (m_IsPointerOverUI)   //UI側で目指すステートを選択中だったら
        {
            return;
        }

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.GetComponent<Province>() != null)
            {
                pointProv = hit.collider.GetComponent<Province>();

                foreach (var list in PlayerCountry.RelateStateDict.Values)
                {
                    foreach (var prov in list)
                    {
                        if (prov.province == pointProv) //現在ポイントしているプロヴィンスが含まれている辞書の値が見つかった
                        {
                            if (prov.selectableState != null && prov.selectableState.Count > 0) //nullチェック
                            {
                                if (CurrentSelectingStateBorder == null)    //初回
                                {
                                    CurrentSelectingStateBorder = prov.selectableState[0];
                                    HighLightPointingBorder(prov.selectableState[0]);  //国境上にマウスオーバーした場合、国境をハイライト
                                    //Debug.Log("初めてこの選択プロヴィンスに触れたよ");
                                    return;
                                }
                                else if (CurrentSelectingStateBorder == prov.selectableState[0])  //もし前回の国境と同じなら
                                {
                                    HighLightPointingBorder(prov.selectableState[0]);  //国境上にマウスオーバーした場合、国境をハイライト
                                    //Debug.Log("今は「" + prov.selectableState[0].StateNameJP + "」に関連するステートに触れているよ");
                                    return;
                                }
                                else    //前回とポイントしてるモノが違ったら
                                {
                                    CurrentSelectingStateBorder = prov.selectableState[0];
                                    FieldManager.Instance.HighLightProvince();    //マウスオーバーされてなければ元のハイライトに戻す
                                    //Debug.Log("「" + prov.selectableState[0].StateNameJP + "」に関連するステートに変わったよ");
                                    return;
                                }
                            }
                        }
                    }
                }

                CurrentSelectingStateBorder = null;
                FieldManager.Instance.HighLightProvince();    //マウスオーバーされてなければ元のハイライトに戻す
                //Debug.Log("自国のプロヴィンスに当たってないよ");
                return;
            }
            else    //プロヴィンス以外のモノに当たれば
            {
                CurrentSelectingStateBorder = null;
                FieldManager.Instance.HighLightProvince();    //マウスオーバーされてなければ元のハイライトに戻す
                //Debug.Log("プロヴィンスに当たってないよ");
                return;
            }
        }
        else    //もしなんにも当たらなければ
        {
            CurrentSelectingStateBorder = null;
            FieldManager.Instance.HighLightProvince();    //マウスオーバーされてなければ元のハイライトに戻す
            //Debug.Log("何にも当たってないよ");
            return;
        }

        //Debug.Log(pointProv.OwnedState.StateNameJP + "  " + CurrentSelectingStateBorder.StateNameJP);
    }

    public void OpenTacticalMenu()
    {
        m_TacticalMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// 統合版のメニュー展開メソッド
    /// </summary>
    /// <param name="menuIndex"></param>
    public void OpenTacticalMenu(int menuIndex)
    {
        if (menuIndex == 2 && SelectingCountry != PlayerCountry)
        {
            return;
        }

        if ((menuIndex == 1 || menuIndex == 3) && SelectingCountry == null)
        {
            return;
        }

        MenuType menu = (MenuType)menuIndex;

        SetLabel(menuIndex);    //メニューラベル変更

        m_TacticalMenu.gameObject.SetActive(true);  //戦術メニューのベースを開く

        ResetAllMenu();                             //メニューを一旦全部閉じる

        MenuDict[menu].gameObject.SetActive(true);  //対象のメニューを展開

        MenuDict[menu].Init();                      //メニューの準備をさせる
    }

    public void OpenStateMenu()
    {
        OpenTacticalMenu();  //戦術メニューオープン
        ResetAllMenu();
        MenuDict[MenuType.State].gameObject.SetActive(true);
        MenuDict[MenuType.State].GetComponent<StateMenu>().SetStateData(SelectingState);   //戦術メニューテキスト入力

        SetLabel(0);

        ResetOwnedAllStateColor(SelectingCountry);

        SelectingState.SetStateColor(FieldManager.Instance.StateColorDict[StateColorType.Selecting]);   //新しい選択ステートの色をセット
    }

    public void OpenCountryMenu()
    {
        OpenTacticalMenu();  //戦術メニューオープン
        ResetAllMenu();
        MenuDict[MenuType.Country].gameObject.SetActive(true);

        if (SelectingCountry != null)
        {
            MenuDict[MenuType.Country].GetComponent<CountryMenu>().SetCountryDataforMenu(SelectingCountry);   //戦術メニューテキスト入力
        }

        SetLabel(1);

        ChangeOwnedAllStateColor(SelectingCountry, FieldManager.Instance.StateColorDict[StateColorType.Selecting]);
    }

    public void OpenForcesMenu()
    {
        if (SelectingCountry != PlayerCountry) return;  //プレイヤー国家だった場合のみ展開可能

        OpenTacticalMenu();  //戦術メニューオープン
        ResetAllMenu();
        MenuDict[MenuType.Forces].gameObject.SetActive(true);

        if (SelectingCountry != null)
        {
            MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().UpdateForceUI(SelectingCountry);   //戦術メニューテキスト入力
            MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().CloseEnlistMenu();   //徴兵メニュークローズ
        }

        SetLabel(2);
    }

    public void OpenDiplomacyMenu()
    {
        if (SelectingCountry != PlayerCountry) return;  //プレイヤー国家だった場合のみ展開可能

        OpenTacticalMenu();  //戦術メニューオープン
        ResetAllMenu();
        MenuDict[MenuType.Diplomacy].gameObject.SetActive(true);

        if (SelectingCountry != null)
        {
            MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().UpdateForceUI(SelectingCountry);   //戦術メニューテキスト入力
            MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().CloseEnlistMenu();   //徴兵メニュークローズ
        }

        SetLabel(2);
    }

    public void CloseTacticalMenu()
    {
        ResetAllMenu();
        m_TacticalMenu.gameObject.SetActive(false); //戦術メニュークローズ

        if (SelectingState != null)
        {
            ResetOwnedAllStateColor(SelectingCountry);  //ステートの色を戻す                  
        }

        SelectingState = null;                      //選択ステートを消去
        SelectingCountry = null;                    //選択カントリーを消去
    }

    /// <summary>
    /// 全メニューを閉じる
    /// </summary>
    public void ResetAllMenu()
    {
        foreach(var menu in MenuDict)
        {
            menu.Value.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 全付箋を基の位置へ
    /// </summary>
    public void ResetAllLabel()
    {
        foreach(var label in m_LabelList)
        {
            Transform labelPos = label.transform;

            label.transform.SetLocalPositionAndRotation(new Vector3(kCloseLabelPositionX, labelPos.localPosition.y, labelPos.localPosition.z) , Quaternion.identity);
        }
    }

    /// <summary>
    /// 付箋をインデックス指定で少し出す
    /// </summary>
    public void SetLabel(int index)
    {
        ResetAllLabel();

        Transform labelPos = m_LabelList[index].transform;

        labelPos.SetLocalPositionAndRotation(new Vector3(kOpenLabelPositionX, labelPos.localPosition.y, labelPos.localPosition.z), Quaternion.identity);
    }

    /// <summary>
    /// UI上にマウスポインタがあるか
    /// </summary>
    /// <returns></returns>
    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();    //レイに触れたモノのリスト
        EventSystem.current.RaycastAll(eventData, results);         //レイに触れたものを移植

        foreach (var result in results)
        {
            //もし当たったオブジェクトのレイヤーがUIなら触れている
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                Debug.Log(result.gameObject.name + "をクリックしたため、レイをキャンセルしました。");
                return true;
            }
        }

        return false;
    }

    public bool CheckMoveValue()
    {
        m_CameraMoveSpeed[Direction.North] = Mathf.Clamp(m_CameraMoveSpeed[Direction.North], 0.0f, 0.1f);
        m_CameraMoveSpeed[Direction.South] = Mathf.Clamp(m_CameraMoveSpeed[Direction.South], 0.0f, 0.1f);
        m_CameraMoveSpeed[Direction.West] = Mathf.Clamp(m_CameraMoveSpeed[Direction.West], 0.0f, 0.1f);
        m_CameraMoveSpeed[Direction.East] = Mathf.Clamp(m_CameraMoveSpeed[Direction.East], 0.0f, 0.1f);

        if (m_CameraMoveSpeed[Direction.North] > 0.0f)
        {
            return true;
        }
        if (m_CameraMoveSpeed[Direction.South] > 0.0f)
        {
            return true;
        }
        if (m_CameraMoveSpeed[Direction.West] > 0.0f)
        {
            return true;
        }
        if (m_CameraMoveSpeed[Direction.East] > 0.0f)
        {
            return true;
        }

        return false;
    }

    public Dictionary<MenuType, TacticalMenu> GetMenuDict()
    {
        return MenuDict;
    }

    public void SetPlayerCountry(CountryData data)
    {
        PlayerCountry = data;
    }

    /// <summary>
    /// 軍団の配置切り替えモードへ移行
    /// </summary>
    /// <param name="force"></param>
    public void OpenOrderMenu(Force force)
    {
        CurrentEditingForce = force;
        CurrentEditState = EditState.Force;
        MenuDict[MenuType.Order].gameObject.SetActive(true);        //命令メニュー展開
        MenuDict[MenuType.Order].GetComponent<OrderMenu>().Init();  //命令メニューのUI配置
    }

    /// <summary>
    /// 軍団の配置切り替えモードへ移行
    /// </summary>
    public void CloseOrderMenu()
    {
        CurrentEditingForce = null;
        CurrentEditState = EditState.None;
        MenuDict[MenuType.Order].gameObject.SetActive(false);        //命令メニュー消去
    }

    /// <summary>
    /// 侵攻先のステートに隣接しているプロヴィンスをハイライトする
    /// </summary>
    /// <param name="target"></param>
    public void HighLightPointingBorder(State target)
    {
        var list = PlayerCountry.RelateStateDict;

        if (list.ContainsKey(target))
        {
            foreach (var prov in list[target])   //そのステートに関与しうるプロヴィンスを全部
            {
                prov.province.SetProvinceColor(new Vector3(1.0f, 1.0f, 0.6f));  //色を変える
            }
        }
    }

    /// <summary>
    /// 侵攻先のステートに隣接しているプロヴィンスをハイライトする(UIのボタンに触れた時用)
    /// </summary>
    /// <param name="target"></param>
    public void HighLightPointingBorderOnUI(State target)
    {
        var list = PlayerCountry.RelateStateDict;

        if (list.ContainsKey(target))
        {
            foreach (var prov in list[target])   //そのステートに関与しうるプロヴィンスを全部
            {
                prov.province.SetProvinceColor(new Vector3(1.0f, 1.0f, 0.6f));  //色を変える
            }
        }

        m_IsPointerOverUI = true;
    }

    /// <summary>
    /// 現在徴兵を行おうとしている部隊のデータを辞書から取得
    /// </summary>
    /// <param name="type"></param>
    public ForceData SetEnlistData(ForceType type)
    {
        EnlistData data = EnlistDataDict[type];

        ForceData force = new ForceData();

        force.ForceType = type;                     //兵種
        force.MaxSoldiers = data.MaxSoldiers;       //最大兵数
        force.MaxMorale = data.MaxMorales;          //最大士気
        force.Attack = data.Attack;                 //攻撃力
        force.Defense = data.Defence;               //防御力
        force.Occupy = data.Occupy;                 //占領力
        force.MoveSpeed = data.MoveSpeed;           //移動スピード
        force.FundCost = data.FundCost;             //一人当たりの資金コスト
        force.ResourceCost = data.ResourceCost;     //一人当たりの資源コスト

        force.Soldiers = 0;                         //初期の兵数

        return force;
    }

    public void ChangeEditingState(EditState state)
    {
        CurrentEditState = state;
    }

    /// <summary>
    /// その勢力に属するステートを総て色替えする
    /// </summary>
    /// <param name="country"></param>
    public void ChangeOwnedAllStateColor(CountryData country, Vector3 color)
    {
        var states = FieldManager.Instance.GetOwnedStateList(country);

        foreach (var state in states)
        {
            state.SetStateColor(color);
        }
    }

    /// <summary>
    /// その勢力に属するステートを総て元に戻す
    /// </summary>
    /// <param name="country"></param>
    public void ResetOwnedAllStateColor(CountryData country)
    {
        var states = FieldManager.Instance.GetOwnedStateList(country);

        if (states == null || states.Count <= 0)
        {
            return;
        }

        foreach (var state in states)
        {
            state.SetStateColor(country.CountryColor);
        }
    }

    /// <summary>
    /// ポイント済みか検知
    /// </summary>
    /// <param name="value"></param>
    public void ChangeIsPointingUI(bool value)
    {
        m_IsPointerOverUI = value;
    }

    /// <summary>
    /// 自国と相手国との間で友好度を計算する
    /// </summary>
    /// <param name="myData"></param>
    /// <param name="targetData"></param>
    /// <returns></returns>
    public int CalculateFavorValue(CountryData myData, CountryData targetData)
    {
        int favorValue = 0; //友好度

        if (myData.LargeIdeology == targetData.LargeIdeology)   //大枠イデオロギーが同じの場合
        {
            favorValue += 50;

            if (myData.ForceIdeology == targetData.ForceIdeology)   //イデオロギーが同じ場合
            {
                favorValue += 30;
            }
        }
        else                                                    //イデオロギーが違う場合
        {
            favorValue -= 30;
        }

        if (myData.AlliesCountryList.Contains(targetData))  //相手国と同盟関係である
        {
            favorValue += 100;
        }

        if (myData.EnemyCountryList.Contains(targetData))   //相手国と戦争状態である
        {
            favorValue -= 100;
        }

        return favorValue;
    }

    /// <summary>
    /// ゲーム終了時に呼ぶメソッド
    /// </summary>
    public void GameOver(bool win)
    {
        FieldManager.Instance.StopTimeCount();  //シミュレーション終了
        m_TimeCounterUI.ChangeTimeImage(false);   //一応、UIも変更

        var player = PlayerCountry;

        m_ResultUI.gameObject.SetActive(true);  //リザルト画面を表示

        m_ResultUI.SetTextAndFlag(player.CountryFlag, player.StateGetCount, player.CountryKillCount, player.DeathCount, TimeCounterUI.TimerText.text);   //
    }
}

public enum CamaraState
{
    None,
    TopDown,
    Tactical,
    Dramatic
}

public enum MouseButton
{
    Right,
    Left
}

public enum MasterData
{
    Ideology,
    State,
    FOGoverment,
    InfantryPreset,
    ForceStatus,
    Building,
}