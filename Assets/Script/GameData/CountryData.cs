using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CountryData
{
    /// <summary>
    /// 国家のID
    /// </summary>
    public int CountryID { get; private set; }

    /// <summary>
    /// 首都州のID
    /// </summary>
    public int CapitalStateID {  get; private set; }

    /// <summary>
    /// 全兵力
    /// </summary>
    public int AllSoldiers {  get; private set; }

    /// <summary>
    /// 最大総資金
    /// </summary>
    public int MaxFund { get; private set; } = 10000;

    /// <summary>
    /// 現在の総資金
    /// </summary>
    public int Fund { get; private set; } = 5000;

    /// <summary>
    /// 最大総資源
    /// </summary>
    public int MaxResource {  get; private set; } = 10000;

    /// <summary>
    /// 現在の総資源
    /// </summary>
    public int Resource {  get; private set; } = 5000;

    /// <summary>
    /// 大枠のイデオロギー
    /// </summary>
    public LIdeology LargeIdeology { get; private set; }

    /// <summary>
    /// 大枠内の細かいイデオロギー
    /// </summary>
    public string ForceIdeology { get; private set; }

    /// <summary>
    /// 国家名
    /// </summary>
    public string CountryName { get; private set; }

    /// <summary>
    /// 主要文化
    /// </summary>
    public string MainCountryCulture { get; private set; }

    /// <summary>
    /// 主要文化(日本語名)
    /// </summary>
    public string MainCountryCultureJP { get; private set; }

    /// <summary>
    /// 国旗ポートレート
    /// </summary>
    public Sprite CountryFlag { get; private set; }

    /// <summary>
    /// リーダーポートレート
    /// </summary>
    public Sprite LeaderPortrait { get; private set; }

    /// <summary>
    /// 文化のイメージ
    /// </summary>
    public Sprite CultureImage { get; private set; }

    /// <summary>
    /// 国家のステート上の色
    /// </summary>
    public Vector3 CountryColor { get; private set; }

    /// <summary>
    /// 軍団のリスト
    /// </summary>
    public List<Force> ForceList { get; private set; } = new List<Force>();

    /// <summary>
    /// 戦争中の勢力のリスト
    /// </summary>
    public List<CountryData> EnemyCountryList { get; private set; } = new List<CountryData>();

    /// <summary>
    /// 同盟中の勢力のリスト
    /// </summary>
    public List<CountryData> AlliesCountryList { get; private set; } = new List<CountryData>();

    /// <summary>
    /// 部隊を配置可能な国境のリスト(プレイヤーが使用)
    /// </summary>
    public List<State> BorderStateList { get; private set; } = new List<State>();

    /// <summary>
    /// 部隊を配置可能なステートのリスト
    /// </summary>
    public List<State> GuarrisonStateList { get; private set; } = new List<State>();

    /// <summary>
    /// 国境ごとの戦闘モデル用辞書型
    /// </summary>
    public Dictionary<State, GameObject> BattleModelDict { get; private set; } = new Dictionary<State, GameObject>();

    /// <summary>
    /// key:ターゲットとするステート value:そのステートに隣接しているプロヴィンスのリスト
    /// </summary>
    public Dictionary<State, List<ClickableProvinceData>> NeighborStateDict { get; private set; } = new Dictionary<State, List<ClickableProvinceData>>();

    /// <summary>
    /// この国家のプレイヤー
    /// </summary>
    public PlayerData OwnedPlayerData { get; private set; }

    /// <summary>
    /// 隣のステート
    /// </summary>
    public List<State> NeighborStateList { get; private set; } = new List<State>();

    private const string kLeaderPortraitFolderPath = "Assets/Texture/Portrait/Leader";

    /// <summary>
    /// インスタンス生成時に一番最初に呼ばれる
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<bool> Init(int index)
    {
        CountryID = index;

        int ideologyIndex = Random.Range(0, (int)LIdeology.Count);
        LargeIdeology = (LIdeology)ideologyIndex;                   //ランダムでイデオロギー設定

        Dictionary<LIdeology, List<string>> ideoloryTextDict = new Dictionary<LIdeology, List<string>>();   //イデオロギーごとの文字列

        string csvText = await AddressableManager.Instance.LoadCsvTextAsync(AddressableManager.Instance.MasterAddress[MasterData.Ideology]);    //勢力イデオロギーをcsvからリスト化して取得
        AddressableManager.Instance.ParseCsvIdeologyType(csvText, ideoloryTextDict);                        //勢力イデオロギーをcsvからリスト化して取得

        ForceIdeology = AddressableManager.Instance.GetRandomTextIdeoloryType(LargeIdeology, ideoloryTextDict); //勢力イデオロギーを入力

        //====================ステート上での色を設定======================
        float redValue = 0.1f * Random.Range(0, 10);
        float greenValue = 0.1f * Random.Range(0, 10);
        float blueValue = 0.1f * Random.Range(0, 10);

        CountryColor = new Vector3(redValue, greenValue, blueValue);
        //====================ステート上での色を設定End======================

        Debug.Log("この国家IDは" + CountryID + "で、イデオロギーは" + ForceIdeology + "です。");

        return true;
    }

    /// <summary>
    /// 国家データを取得
    /// </summary>
    /// <returns></returns>
    public async Task SetCountryData()
    {
        //====================イデオロギーに応じた設定======================
        string foGoverment = AddressableManager.Instance.GetRandomTextIdeoloryType(LargeIdeology, FieldManager.Instance.FOGovermentDict); //国号を取得
        string Name = FieldManager.Instance.StateList[CapitalStateID].StateNameJP;    //国家の固有名を取得
        if (LargeIdeology == LIdeology.Nationalism)
        {
            Name = FieldManager.Instance.StateList[CapitalStateID].StateCultureJP;    //民族主義国家なら民族に
        }

        CountryName = Name + foGoverment;   //固有名＋政府形態

        CountryFlag = await GetRandomFlag(LargeIdeology);   //イデオロギーに応じた国旗を選択
        //====================イデオロギーに応じた設定End======================

        LeaderPortrait = await GetRandomLeader();   //リーダー割り当て

        MainCountryCulture = FieldManager.Instance.StateList[CapitalStateID].StateCulture;        //主要文化を首都に合わせる
        MainCountryCultureJP = FieldManager.Instance.StateList[CapitalStateID].StateCultureJP;

        CultureImage = FieldManager.Instance.StateList[CapitalStateID].CultureMark;
    }

    /// <summary>
    /// ランダムな国旗を取得
    /// </summary>
    /// <param name="ideology"></param>
    /// <returns></returns>
    public async Task<Sprite> GetRandomFlag(LIdeology ideology)
    {
        string flagId = "";
        do
        {
            string flagIndex = Random.Range(0, 4).ToString();

            switch (ideology)
            {
                case LIdeology.Liberalism:
                    flagId = "L" + flagIndex;
                    break;
                case LIdeology.Nationalism:
                    flagId = "N" + flagIndex;
                    break;
                case LIdeology.Socialism:
                    flagId = "S" + flagIndex;
                    break;
                case LIdeology.Militarism:
                    flagId = "M" + flagIndex;
                    break;
            }
        }
        while (FieldManager.Instance.UsedFlagID.Contains(flagId));    //もし既にこのIDが使用されていればdoに戻る

        FieldManager.Instance.AddUsedFlag(flagId);    //既に使用されている国旗リストに入れる

        return await AddressableManager.Instance.LoadSpriteAsync(flagId);
    }

    /// <summary>
    /// ランダムなリーダーを取得
    /// </summary>
    /// <returns></returns>
    public async Task<Sprite> GetRandomLeader()
    {
        int maxIndex = AddressableManager.Instance.GetTextureCount(kLeaderPortraitFolderPath);
        int randomIndex = Random.Range(0, maxIndex - 1);
        string leaderId = "Leader" + randomIndex;
        return await AddressableManager.Instance.LoadSpriteAsync(leaderId);
    }

    /// <summary>
    /// 首都を設定
    /// </summary>
    /// <param name="id"></param>
    public void SetCapital(int id)
    {
        CapitalStateID = id;
    }

    /// <summary>
    /// 構造体データをもとに徴兵を行う
    /// </summary>
    /// <param name="data"></param>
    public void Enlist(ForceData data)
    {
        FieldManager manager = FieldManager.Instance;   //マネージャのインスタンス取得

        Force force = new Force();  //軍団インスタンス生成
        force.SetForceData(data);   //軍団情報をセット
        ForceList.Add(force);       //リストに追加

        Fund -= data.FundCost * data.Soldiers;          //資金消費
        Resource -= data.ResourceCost * data.Soldiers;  //資源消費

        if (OwnedPlayerData.IsPlayer)   //プレイヤーの時だけ
        {
            SetInventryUI();    //所持リソースUI更新
        }

        BattleSystemManager.Instance.GetMenuDict()[MenuType.Forces].GetComponent<ForcesMenu>().UpdateForceUI(this); //プレイヤーの軍団メニューを更新

        manager.SetForceOnState(manager.StateList[CapitalStateID], force);  //首都に駐屯部隊として出す

        Debug.Log("徴兵完了:  兵種:" + data.ForceType + "  兵数:" + data.Soldiers + "  使用資金(残り):" + data.FundCost * data.Soldiers + "(" + Fund + ")" +"  使用資源(残り):" + data.ResourceCost * data.Soldiers + "(" + Resource + ")");
    }

    /// <summary>
    /// 対象の勢力と戦争状態になる
    /// </summary>
    /// <param name="enemydata"></param>
    public void DeclareWar(CountryData enemydata)
    {
        if (EnemyCountryList.Contains(enemydata) || enemydata.EnemyCountryList.Contains(this))  //お互いに既に戦争状態だった場合
        {
            return; //無視
        }

        EnemyCountryList.Add(enemydata);        //この勢力の「戦争中の勢力リスト」に対象の勢力を加える
        enemydata.EnemyCountryList.Add(this);   //対象の勢力の「戦争中の勢力リスト」にこの勢力を加える
    }

    /// <summary>
    /// 対象の勢力と同盟状態になる
    /// </summary>
    /// <param name="targetdata"></param>
    public void FormAlliance(CountryData targetdata)
    {
        if (AlliesCountryList.Contains(targetdata) || targetdata.AlliesCountryList.Contains(this))  //お互いに既に同盟状態だった場合
        {
            return; //無視
        }

        AlliesCountryList.Add(targetdata);        //この勢力の「同盟中の勢力リスト」に対象の勢力を加える
        targetdata.AlliesCountryList.Add(this);   //対象の勢力の「同盟中の勢力リスト」にこの勢力を加える
    }

    /// <summary>
    /// プレイヤーのデータを取得
    /// </summary>
    /// <param name="data"></param>
    public void SetPlayerData(PlayerData data)
    {
        OwnedPlayerData = data;
    }

    /// <summary>
    /// 隣のステートを取得
    /// </summary>
    /// <param name="state"></param>
    public void SetNeighberState(State state)
    {
        if (!NeighborStateList.Contains(state))
        {
            NeighborStateList.Add(state);
        }
    }

    /// <summary>
    /// 1時間ごとの国家情報を更新
    /// </summary>
    public void UpdateCountryDataOnHour()
    {
        //====================「兵力」に関する情報の更新===================

        int currentAllSoldiers = 0;                         //現在の勢力の総兵数

        foreach (var force in ForceList)
        {
            currentAllSoldiers += force.Soldiers;           //軍団ごとの値を入力

            if (force.CurrentTargetStateId != -1)           //現在侵略中のステートがあれば
            {
                force.Conquest();                           //侵略を行わせる
            }                              
        }

        AllSoldiers = currentAllSoldiers;                   //国家の全兵力のステータスに更新

        //====================「兵力」に関する情報の更新End===================
    }

    /// <summary>
    /// 1日ごとの国家情報を更新
    /// </summary>
    public void UpdateCountryDataOnDate()
    {
        //====================「リソース」に関する情報の更新===================

        var stateList = FieldManager.Instance.GetOwendStateList(this);    //所有している州を取得

        foreach (var state in stateList)
        {
            Fund += state.Income;   //収入を追加

            if (Fund > MaxFund)     //最大資金を越えると
            {
                Fund = MaxFund;     //最大資金と同じ量になる
            }

            Resource += state.Resource; //資源を追加

            if (Resource > MaxResource) //最大資源を越えると
            {
                Resource = MaxResource; //最大資源と同じ量になる
            }
        }

        if (OwnedPlayerData.IsPlayer)    //もしプレイヤー国家だったら
        {
            SetInventryUI();
        }

        //====================「リソース」に関する情報の更新End===================


        //====================「AI思考」の更新===================

        if (!OwnedPlayerData.IsPlayer)    //もしプレイヤー国家じゃなかったら
        {
            OwnedPlayerData.UpdateAIThinkingOnDefencive();  //AIの思考を更新
        }

        //====================「AI思考」の更新End===================
    }

    /// <summary>
    /// インベントリUIに反映
    /// </summary>
    public void SetInventryUI()
    {
        InventryUIContainer container = new InventryUIContainer();  //リソースUIに反映する情報を作る

        container.fundRatio = (float)Fund / (float)MaxFund;
        container.resourceRatio = (float)Resource / (float)MaxResource;
        container.fund = Fund + "/" + MaxFund;
        container.resource = Resource + "/" + MaxResource;

        BattleSystemManager.Instance.InventryUI.SetUI(container);   //UIに反映
    }

    /// <summary>
    /// 戦闘モデルのポインタをセット
    /// </summary>
    /// <param name="obj"></param>
    public void SetBattleModel(GameObject obj, State target)
    {
        BattleModelDict[target] = obj;
    }

    /// <summary>
    /// 隣接しているステート情報を取得するタイミングで辞書型のサイズを変更する
    /// </summary>
    public void RefreshBattleModelDict()
    {
        List<State> garbageStateList = new List<State>();

        foreach (var state in BattleModelDict.Keys)     //まずは要らないキーを排除する
        {
            if (BorderStateList.Contains(state))      //現在のステートが隣接ステートリスト内にある
            {
                continue;                               //無視
            }
            else                                        //現在のステートが隣接ステートリスト内にない
            {
                garbageStateList.Add(state);            //ゴミ箱に入れる
            }
        }

        for (int i = 0; i < garbageStateList.Count; i++)
        {
            BattleModelDict.Remove(garbageStateList[i]);//中身を削除
        }

        foreach (var state in BorderStateList)
        {
            if (BattleModelDict.ContainsKey(state)) //もし国境用のキーがあれば
            {
                continue;
            }
            else                                    //なければ
            {
                BattleModelDict.Add(state, null);   //空のキーを作る
            }
        }
    }

    /// <summary>
    /// この国家がこのステート上に駐屯部隊をおいているか
    /// </summary>
    /// <param name="state"></param>
    public List<Force> CheckForceOnThisState(State state)
    {
        List<Force> guarrisonList = new List<Force>();

        foreach (var force in ForceList)
        {
            if (force.CurrentGuarrisonStateId == state.StateID) //駐屯ステートと一致する
            {
                guarrisonList.Add(force);
            }
        }

        return guarrisonList;
    }

    /// <summary>
    /// この国家がこの国境上に侵攻部隊をおいているか
    /// </summary>
    /// <param name="state"></param>
    public List<Force> CheckForceOnThisStatesBorder(State state)
    {
        List<Force> forceList = new List<Force>();

        foreach (var force in ForceList)
        {
            if (force.CurrentTargetStateId == state.StateID)    //侵攻ステートと一致する
            {
                forceList.Add(force);
            }
        }

        return forceList;
    }

    /// <summary>
    /// 駐屯中の軍団でランダムな軍団を返す
    /// </summary>
    /// <returns></returns>
    public Force GetRandomFreeForce()
    {
        List<Force> freeForceList = new List<Force>();  //現在、駐屯中の軍団

        foreach(var force in ForceList)
        {
            if (force.CurrentTargetStateId == -1)   //ターゲットが無い軍団の場合
            {
                freeForceList.Add(force);   //リストに追加
            }
        }

        if (freeForceList.Count == 0)   //自由な部隊が無ければ
        {
            return null;
        }

        var randomindex = Random.Range(0, freeForceList.Count); //インデックス内で乱数を取る

        return freeForceList[randomindex];
    }
}

public enum LIdeology
{
    Liberalism = 0,
    Nationalism = 1,
    Socialism = 2,
    Militarism = 3,
    Count = 4
}
