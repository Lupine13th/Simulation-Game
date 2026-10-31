using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using static Province;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public enum ProvinceColor
{
    Border,
    Default,
    Selecting
}

public enum TimeType
{
    Year,
    Month,
    Day,
    Hour
}

public enum Culture
{
    Volnia,
    Aurelia,
    Erden,
    Kasara,
    Zarif,
    Varnadi,
    Longhua,
    Manahiri
}

public enum ForcePositionType
{
    Border,
    Guarrison,
}

public struct ClickableProvinceData
{
    public ForcePositionType type;
    public List<State> selectableState;
    public Province proviince;
}

public struct BuildingData
{
    public BuildingType type;
    public string dispName;
    public string value;
    public int fundCost;
    public int resourceCost;
}


public class FieldManager : SingletonManager<FieldManager>
{
    [Serializable]
    public struct MaterialKeyValue
    {
        public ProvinceColor color;
        public Material Material;
    }

    [Header("生成されたステートのリスト")]
    [SerializeField] private List<State> m_StateList  = new List<State>();                                              //全ステートリスト

    public List<CountryData> CountryList { get; private set; } = new List<CountryData>();

    [Header("ステートのプレファブ")]
    [SerializeField] private State m_StatePrefab;

    [Header("プレイヤーデータのプレファブ")]
    [SerializeField] private PlayerData m_PlayerDataPrefab;

    [Header("データオブジェクト置き場")]
    [SerializeField] private Transform m_PlayerDataListParent;

    [Header("ステートの親")]
    [SerializeField] private Transform m_Continent;

    [Header("生成されるステート数(調整用)")]
    [SerializeField] private int m_StateCount;

    [Header("生成される勢力数(調整用)")]
    [SerializeField] private int m_CountryCount;

    [SerializeField] List<MaterialKeyValue> m_ProvinceMaterialSetting = new List<MaterialKeyValue>();

    [SerializeField] private TextMeshProUGUI m_TimeText;

    public List<State> StateList => m_StateList;

    public List<string> UsedFlagID { get; private set; } = new List<string>(); //そのイデオロギーの国家のカウント

    private List<float> m_UpdateDuration = new List<float>()
    {
        0.1f,
        0.5f,
        1.0f
    };


    /// <summary>
    /// プロヴィンスの座標をキーに入っているディクショナリー
    /// </summary>
    public Dictionary<Vector2Int, Province> ProvinceDict { get; private set; } = new Dictionary<Vector2Int, Province>();

    /// <summary>
    /// プロヴィンスの座標をキーに入っているディクショナリー
    /// </summary>
    public Dictionary<Vector2Int, Province> BorderProvinceDict { get; private set; } = new Dictionary<Vector2Int, Province>();

    /// <summary>
    /// 外縁プロヴィンスが入っているディクショナリー
    /// </summary>
    public Dictionary<Vector2Int, Province> OutSideProvinceDict { get; private set; } = new Dictionary<Vector2Int, Province>();

    /// <summary>
    /// プロヴィンス色をキーに入っているディクショナリー
    /// </summary>
    public Dictionary<ProvinceColor, Material> ProvinceMaterialDict { get; private set; } = new Dictionary<ProvinceColor, Material>();

    public Dictionary<int, StateData> StateDataDict { get; private set; } = new Dictionary<int, StateData>();   //ステートデータ
    public Dictionary<LIdeology, List<string>> FOGovermentDict { get; private set; } = new Dictionary<LIdeology, List<string>>();
    public Dictionary<BuildingType, BuildingData> BuildingDataDict { get; private set; } = new Dictionary<BuildingType, BuildingData>();
    public Dictionary<TimeType, int> FieldTime { get; private set; } = new Dictionary<TimeType, int>()
    {
        { TimeType.Year, 2026 },
        { TimeType.Month, 1 },
        { TimeType.Day, 1 },
        { TimeType.Hour, 0 }
    };

    public Dictionary<StateColorType, Vector3> StateColorDict { get; private set; } = new Dictionary<StateColorType, Vector3>()
    {
        { StateColorType.Selecting, new Vector3(0.16f, 1.0f, 0.22f) },
        { StateColorType.Border, new Vector3(1.0f, 0.2f, 0.2f) },
        { StateColorType.Garrison, new Vector3(0.2f, 0.2f, 1.0f) },
    };

    public Dictionary<PlayerData, CountryData> CountryDictPlayerKey { get; private set; } = new Dictionary<PlayerData, CountryData>();

    private float m_Duration = 0;

    /// <summary>
    /// 経過時間計算用
    /// </summary>
    public int TimeCount { get; private set; }

    public bool IsPlaying { get; private set; } = false;

    async void Start()
    {
        m_TimeText.SetText(FieldTime[TimeType.Hour].ToString() + ". " + FieldTime[TimeType.Day].ToString() + ". " + FieldTime[TimeType.Month].ToString() + ". " + FieldTime[TimeType.Year].ToString());

        //=====================ステート生成フェーズ====================
        List<int> instancedId = new List<int>();
        string stateMasterData = await AddressableManager.Instance.LoadCsvTextAsync(AddressableManager.Instance.MasterAddress[MasterData.State]);
        AddressableManager.Instance.ParseCsvStateData(stateMasterData, StateDataDict);  //stateDataDictにintをキーにデータを入れる

        Vector3 stateBasePosition = Vector3.zero;
        State currentState = null;
        currentState = Instantiate(m_StatePrefab, stateBasePosition, Quaternion.identity, m_Continent);  //原点のステートを生成
        await currentState.Init();                                                                       //ステートの初期化
        await currentState.SetStateData(AddressableManager.Instance.GetRandomStateData(instancedId, StateDataDict)); //ステートデータを入力

        m_StateList.Add(currentState);

        for (int i = 1; i < m_StateCount; i++)
        {
            CheckOutSideProvince();

            int random = UnityEngine.Random.Range(0, OutSideProvinceDict.Count);    //ランダム数を出す
            stateBasePosition = OutSideProvinceDict.ElementAt(random).Value.transform.position; //インデックスからランダムで取得
            currentState = Instantiate(m_StatePrefab, stateBasePosition, Quaternion.identity, m_Continent);  //周りのステートを生成
            await currentState.Init();
            await currentState.SetStateData(AddressableManager.Instance.GetRandomStateData(instancedId, StateDataDict)); //ステートデータを入力

            m_StateList.Add(currentState);
        }

        //ステートが隣接する部分に壁を設置
        CheckBorderDirection(); 

        //ステート隣接データを再精査
        CheckAllBorder();

        string buildMasterData = await AddressableManager.Instance.LoadCsvTextAsync(AddressableManager.Instance.MasterAddress[MasterData.Building]);
        AddressableManager.Instance.ParseCsvBuildingData(buildMasterData, BuildingDataDict);  //stateDataDictにintをキーにデータを入れる

        //=====================ステート生成フェーズEnd====================

        //=====================国家生成フェーズ=================

        string foGMasterData = await AddressableManager.Instance.LoadCsvTextAsync(AddressableManager.Instance.MasterAddress[MasterData.FOGoverment]);
        AddressableManager.Instance.ParseCsvIdeologyType(foGMasterData, FOGovermentDict);  //stateDataDictにintをキーにデータを入れる

        for (int i = 0; i < m_CountryCount; i++)
        {
            CountryData country = new CountryData();
            await country.Init(i);
            CountryList.Add(country);

            //ランダムなステートに国家を割り当て
            List<int> emptyStateIDList = new List<int>();
            for (int j = 0; j < m_StateList.Count; j++)
            {
                if (m_StateList[j].OwnedCountry == null) emptyStateIDList.Add(j);
            }
            int randomIndex = UnityEngine.Random.Range(0, emptyStateIDList.Count);
            int randomEmptyStateID = emptyStateIDList[randomIndex];

            m_StateList[randomEmptyStateID].SetOwner(country);  //そのステートに国家を割り当て
            country.SetCapital(randomEmptyStateID);             //国家の首都を割り当て

            PlayerData playerData = Instantiate(m_PlayerDataPrefab, m_PlayerDataListParent);    //プレイヤー生成

            if (i == 0)
            {
                playerData.SetPlayer(true, i);  //インデックス1番にプレイヤーを当てはめる

                country.SetInventryUI();        //プレイヤー国家のデータをUIに反映
            }
            else
            {
                playerData.SetPlayer(false, i); //それ以外はAI
            }

            playerData.SetCountryData(country);     //プレイヤーに国家データを持たせる
            country.SetPlayerData(playerData);      //国家にもプレイヤーを参照できるように

            CountryDictPlayerKey.Add(playerData, country);  //辞書型に登録

            await country.SetCountryData();     //csvデータから国家のデータを登録
        }

        BattleSystemManager.Instance.SetPlayerCountry(CountryList[0]);  //初代インデックスにプレイヤーを入れる
        //=====================国家生成フェーズEnd=================

        //ステートUIの初期化
        WorldUIManager.Instance.Init();
    }

    void Update()
    {
        if (IsPlaying)
        {
            if (m_UpdateDuration[0] > m_Duration)  //再生中
            {
                m_Duration += Time.deltaTime;
            }
            else
            {
                m_Duration = 0.0f;
                UpdateBattleField();
            }
        }
    }

    public Province GetProvince(int x, int y)
    {
        if (!ProvinceDict.ContainsKey(new Vector2Int(x, y)))
        {
            return null;
        }
        return ProvinceDict[new Vector2Int(x, y)];
    }

    public void SetProvince(Vector2Int position, Province province)
    {
        ProvinceDict[position] = province;
    }

    public void SetBorderProvince(Vector2Int position, Province province)
    {
        BorderProvinceDict[position] = province;
    }

    public void SetOutSideProvince(Vector2Int position, Province province)
    {
        OutSideProvinceDict[position] = province;
    }

    public List<State> GetStateList()
    {
        return m_StateList;
    }

    /// <summary>
    /// 現在登録中の外縁プロヴィンスに外縁じゃなくなっているのが要れば排除する
    /// </summary>
    /// <returns></returns>
    public void CheckOutSideProvince()
    {
        List<Vector2Int> removeList = new List<Vector2Int>();

        foreach (var prov in OutSideProvinceDict)       //排除リストにブチ込む
        {
            if (prov.Value.CheckNeighborAllDirection())
            {
                prov.Value.SetOutSideProvince(false);   //外縁プロヴィンス剥奪
                removeList.Add(prov.Key);               //排除リストへ
            }
        }

        foreach (var prov in removeList)                //排除開始！
        {
            OutSideProvinceDict.Remove(prov);
        }
    }

    public void CheckBorderDirection()
    {
        foreach (var prov in BorderProvinceDict)
        {
            for (int i = 0; i < (int)Direction.Count; i++)
            {
                Direction direction = (Direction)i;
                if (prov.Value.CheckNeighborProvinceIsDifferentState(direction))
                    prov.Value.SetBorderObject(direction);
            }
        }
    }

    /// <summary>
    /// そのプロヴィンスが本当に他のステートに接しているか
    /// </summary>
    public void CheckAllBorder()
    {
        BorderProvinceDict.Clear(); //一旦これまでの国境プロヴィンスをクリア

        foreach (var prov in ProvinceDict)
        {
            for (int i = 0; i < (int)Direction.Count; i++)  //方角ことにチェック
            {
                Direction direction = (Direction)i;
                if (prov.Value.GetNeighborProvinceState(direction) != null) //隣のステートがnullじゃない＆自分のステートじゃない
                {
                    prov.Value.SetNeighberState(direction, prov.Value.GetNeighborProvinceState(direction)); //そのプロヴィンスの隣接ステートをセット
                    BorderProvinceDict[prov.Key] = prov.Value; //国境プロヴィンスとして登録
                }
            }

            prov.Value.OwnedState.AddBorderProv(prov.Value);    //それぞれの方角の隣接ステートをセットする
        }
    }

    public void AddUsedFlag(string flagId)
    {
        UsedFlagID.Add(flagId);
    }

    /// <summary>
    /// 戦場を更新する(ゲーム内で言うと一時間ごとに更新)
    /// </summary>
    private void UpdateBattleField()
    {
        TimeCount++;                        //時間カウント(おそらくイベントのカウントとかで使う用)

        FieldTime[TimeType.Hour]++;         //1時間更新

        foreach (var country in CountryList)
        {
            country.UpdateCountryDataOnHour();    //国家ごとの情報更新(時毎)

            if (!country.OwnedPlayerData.IsPlayer)
            {
                //country.OwnedPlayerData.UpdateAIThinking();
            }
        }

        WorldUIManager.Instance.UpdateStateUI();    //ステートUI更新
        WorldUIManager.Instance.UpdateForceUI();    //軍団情報更新

        if (FieldTime[TimeType.Hour] >= 24) //24時以降になると
        {
            FieldTime[TimeType.Hour] = 0;   //0時に
            FieldTime[TimeType.Day]++;      //1日更新

            foreach (var country in CountryList)
            {
                country.UpdateCountryDataOnDate();//国家ごとの情報更新(日毎)
            }
        }

        if (FieldTime[TimeType.Day] >= 31)  //31日以降になると
        {
            FieldTime[TimeType.Day] = 1;    //1日に
            FieldTime[TimeType.Month]++;    //1月更新
        }

        if (FieldTime[TimeType.Month] >= 13)//13月以降になると
        {
            FieldTime[TimeType.Month] = 1;  //1月に
            FieldTime[TimeType.Year]++;     //1年更新
        }

        //行進した時間をテキストにセット
        m_TimeText.SetText(FieldTime[TimeType.Hour].ToString() + ". " + FieldTime[TimeType.Day].ToString() + ". " + FieldTime[TimeType.Month].ToString() + ". " + FieldTime[TimeType.Year].ToString()); 
    }

    public CountryData GetCountryFromPlayer(PlayerData player)
    {
        if (CountryDictPlayerKey.TryGetValue(player, out var country))
        {
            return country;
        }
        return null;    //見つからなかった場合
    }

    public PlayerData GetPlayerFromCountry(CountryData country)
    {
        //Valueが一致する最初の要素（KeyValuePair）を探し、そのKeyを返す
        return CountryDictPlayerKey.FirstOrDefault(pair => pair.Value == country).Key;  //見つからない場合は型に応じたデフォルト値（null）が返る
    }

    public void PlayTimeCount()
    {
        IsPlaying = true;
    }

    public void StopTimeCount()
    {
        IsPlaying = false;
    }

    /// <summary>
    /// この国家にとって駐屯可能なステートと侵攻先の国境のデータをCountryDataに取得させる
    /// </summary>
    public void GetBorderAndStateData(CountryData country)
    {
        SearchNeighborStateForDict(country.NeighborStateDict, country);

        GetSettableBorderAndStateList(country.BorderStateList, country.GuarrisonStateList, country.NeighborStateDict);
    }

    public void HighLightProvince()
    {
        foreach (var state in m_StateList)
        {
            if (state.OwnedCountry == null)
                continue;

            if (GetPlayerFromCountry(state.OwnedCountry).IsPlayer)  //国境プロヴィンスの中でもプレイヤーのステートをマーク
            {
                state.GetBorderProvinceList();
            }
        }
    }

    public void HighLightProvinceOnUI()
    {
        //トリガー消去
        BattleSystemManager.Instance.ChangeIsPointingUI(false);

        foreach (var state in m_StateList)
        {
            if (state.OwnedCountry == null)
                continue;

            if (GetPlayerFromCountry(state.OwnedCountry).IsPlayer)  //国境プロヴィンスの中でもプレイヤーのステートをマーク
            {
                state.GetBorderProvinceList();
            }
        }
    }

    /// <summary>
    /// 隣接プロヴィンスを取得
    /// </summary>
    /// <param name="neighberList"></param>
    /// <returns></returns>
    public List<State> PickUpNeighberStates(List<NeighberProvince> neighberList)
    {
        List<State> list = new List<State>();

        foreach (var prov in neighberList)
        {
            list.Add(prov.state);
        }

        return list;
    }

    /// <summary>
    /// 国境赴任任務を設定
    /// </summary>
    /// <param name="state">BorderStateListでのインデックス</param>
    /// <param name="force">移動する軍団</param>
    public void SetForceOnBorder(State state, Force force)
    {
        CountryData country = force.OwnerCountry; //国のデータ
        int beforeStateId = force.CurrentGuarrisonStateId;  //この軍団が前にいた駐屯ステート
        int beforeBorderId = force.CurrentTargetStateId;    //この軍団が前にいた国境

        force.SetTargetState(state.StateID);    //攻撃目標を設定
        force.SetGuarrisonState(-1);            //駐屯ステートを初期化

        force.LinkedUI.RefreshPositionName();   //UIの駐屯地テキストを更新

        country.RefreshBattleModelDict();       //モデル用の辞書型を更新

        if (beforeStateId != -1)
        {
            if (country.CheckForceOnThisState(StateList[beforeStateId]).Count == 0)                 //移動前の駐屯ステートに兵が居なければ
            {
                ModelManager.Instance.ChangeGuarrisonModel(false, StateList[beforeStateId], force);        //マス上のモデルを削除
            }
        }

        if (beforeBorderId != -1)
        {
            if (country.CheckForceOnThisStatesBorder(StateList[beforeBorderId]).Count == 0)         //移動前の国境に兵が居なければ
            {
                ModelManager.Instance.ChangeBattleModel(false, StateList[beforeBorderId], country, force); //国境のモデルを削除
            }
        }

        if (country.BattleModelDict.ContainsKey(state))
        {
            if (country.BattleModelDict[state] == null) //モデルが配置されて居なかったら
            {
                ModelManager.Instance.ChangeBattleModel(true, state, country, force);  //国境沿いに侵攻アニメーションのモデルを配置
            }
        }

        if (BattleSystemManager.Instance.PlayerCountry == force.OwnerCountry)
        {
            BattleSystemManager.Instance.MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().ChangeEditMenu(false); //メニューを閉じる
        }
    }

    /// <summary>
    /// 駐屯任務を設定
    /// </summary>
    /// <param name="state">GurrisonStateListでのインデックス</param>
    /// /// <param name="force">移動する軍団</param>
    public void SetForceOnState(State state, Force force)
    {
        CountryData country = force.OwnerCountry; //国のデータ
        int beforeStateId = force.CurrentGuarrisonStateId;  //この軍団が前にいた駐屯ステート
        int beforeBorderId = force.CurrentTargetStateId;    //この軍団が前にいた国境

        force.SetTargetState(-1);                   //攻撃目標を設定
        force.SetGuarrisonState(state.StateID);     //駐屯ステートを初期化

        force.LinkedUI.RefreshPositionName();       //UIの現在地を更新

        country.RefreshBattleModelDict();       //モデル用の辞書型を更新

        if (beforeStateId != -1)
        {
            if (country.CheckForceOnThisState(StateList[beforeStateId]).Count == 0)         //移動前の駐屯ステートに兵が居なければ
            {
                ModelManager.Instance.ChangeGuarrisonModel(false, StateList[beforeStateId], force);//マス上のモデルを削除
            }
        }

        if (beforeBorderId != -1)
        {
            if (country.CheckForceOnThisStatesBorder(StateList[beforeBorderId]).Count == 0)  //移動前の国境に兵が居なければ
            {
                ModelManager.Instance.ChangeBattleModel(false, StateList[beforeBorderId], country, force); //国境のモデルを削除
            }
        }

        if (state.GuarrisonModel == null)       //駐屯モデルが居なければ
        {
            ModelManager.Instance.ChangeGuarrisonModel(true, state, force);    //マス上に駐屯モデルアニメーションを配置
        }

        if (BattleSystemManager.Instance.PlayerCountry == force.OwnerCountry)
        {
            BattleSystemManager.Instance.MenuDict[MenuType.Forces].GetComponent<ForcesMenu>().ChangeEditMenu(false); //メニューを閉じる
        }
    }

    /// <summary>
    /// 国家が持っている州のリストを返す
    /// </summary>
    /// <param name="country"></param>
    /// <returns></returns>
    public List<State> GetOwendStateList(CountryData country)
    {
        List<State> states = new List<State>(); //この国家の持つ州を一旦入れるバッファ

        foreach (var state in m_StateList)      //州ごとに検索
        {
            if (state.OwnedCountry == country)
            {
                states.Add(state);  //「所有しているステート」に入れる
            }
        }

        if (states.Count == 0)  //所有している州が無ければ
        {
            Debug.LogWarning("所有している州を発見できず");
            return null;
        }
        else                    //所有している州があれば
        {
            return states;      //リストを返す
        }
    }

    /// <summary>
    /// ステートをキー、隣接プロヴィンスデータを値とする辞書型に勢力を指定してデータを入れる
    /// /// </summary>
    /// <param name="dict"></param>
    /// <param name="country"></param>
    public void SearchNeighborStateForDict(Dictionary<State, List<ClickableProvinceData>> dict, CountryData country)
    {
        dict.Clear();   //一旦クリア

        //国境の種類を列挙
        var borderProvinceList = new List<Province>();

        foreach (var state in m_StateList)  //ステートリストからcountryが持つステートを割り出す
        {
            if (state.OwnedCountry == null)
                continue;

            if (state.OwnedCountry == country)  //現在のステートが指定した勢力に属している
            {
                if (country == BattleSystemManager.Instance.PlayerCountry)  //呼び出したのがプレイヤーの時のみ
                {
                    //自分のステートをマークする
                    state.SetStateColor(new Vector3(0.8f, 0.8f, 0.8f));
                }

                //国境プロヴィンスの色を変える()
                borderProvinceList = state.GetBorderProvinceList();

                foreach (var prov in state.ProvinceList)
                {
                    if (borderProvinceList.Contains(prov))  //国境プロヴィンスは国境として登録
                    {
                        foreach (var nState in prov.NeighberStateList)  //そのプロヴィンスが隣接しているステートの数分、回す(複数のステートと隣接している場合にしっかりとUIとデータに反映するため)
                        {
                            /*if (!state.OwnedCountry.EnemyCountryList.Contains(nState.state.OwnedCountry) || nState.state.OwnedCountry != null)  //敵対している勢力と空ステート以外は無視
                            {
                                continue;
                            }*/

                            if (nState.state.OwnedCountry != null)  //そのステートはどこかの勢力に領有されている
                            {
                                if (!state.OwnedCountry.EnemyCountryList.Contains(nState.state.OwnedCountry))   //かつ、その隣接しているステートの勢力と現在のステートの勢力が戦争状態に無い
                                {
                                    continue;   //戦争状態じゃない勢力には攻め込めないため、無視
                                }
                            }

                            if (dict.ContainsKey(nState.state))    //既にそのステートデータで辞書が作られているか
                            {
                                dict[nState.state].Add(new ClickableProvinceData() { type = ForcePositionType.Border, selectableState = PickUpNeighberStates(prov.NeighberStateList), proviince = prov });
                            }
                            else
                            {
                                dict.Add(nState.state, new List<ClickableProvinceData>() { new ClickableProvinceData() { type = ForcePositionType.Border, selectableState = PickUpNeighberStates(prov.NeighberStateList), proviince = prov } });
                            }
                        }
                    }
                    else                                    //国境プロヴィンスじゃない場合は駐屯地として登録
                    {
                        if (dict.ContainsKey(prov.OwnedState))
                        {
                            dict[prov.OwnedState].Add(new ClickableProvinceData() { type = ForcePositionType.Guarrison, selectableState = new List<State>() { prov.OwnedState }, proviince = prov });
                        }
                        else
                        {
                            dict.Add(prov.OwnedState, new List<ClickableProvinceData>() { new ClickableProvinceData() { type = ForcePositionType.Guarrison, selectableState = PickUpNeighberStates(prov.NeighberStateList), proviince = prov } });
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 部隊配置時に必要なデータを取得させる
    /// </summary>
    /// <param name="borders"></param>
    /// <param name="guarrison"></param>
    public void GetSettableBorderAndStateList(List<State> borders, List<State> guarrison, Dictionary<State, List<ClickableProvinceData>> neighborDict)
    {
        borders.Clear();
        guarrison.Clear();

        //====================フィールドマネージャからデータを取得===================
        foreach (var state in neighborDict.Keys)
        {
            if (GetPlayerFromCountry(state.OwnedCountry) != null)
            {
                if (!GetPlayerFromCountry(state.OwnedCountry).IsPlayer)  //他ステートをターゲットとする場合
                {
                    borders.Add(state);
                }
                else                                            //自分が保有するステートをターゲットとする場合
                {
                    guarrison.Add(state);
                }
            }
            else                                            //自分が保有するステート以外をターゲットとする場合
            {
                borders.Add(state);
            }
        }
        //====================フィールドマネージャからデータを取得End===================
    }
}
