using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Province;

public enum Direction
{
    North,
    East,
    South,
    West,
    Count
}

public enum AboveModelState
{
    None,
    Guarrison,
    Occupied
}

public enum BuildingType
{
    None,
    Market, //市場
    Quarry  //採掘場
}

public struct StateData
{
    public string StateName;
    public string StateNameJP;
    public string StateCulture;
    public string StateCultureJP;
}

public enum StateType
{
    Plane,
    Mountain,
    Desert,
    River
}

public enum StateColorType
{
    Selecting,
    Border,
    Garrison
}

public struct ColorData
{
    public StateColorType ColorType;
    public Vector3 ColorValue;
}

public class State : MonoBehaviour
{
    [SerializeField] private List<Province> m_ProvinceList = new List<Province>();   //このステートが持つProvinceの配列
    [SerializeField] private int m_StateID;                 //このステートのID
    [SerializeField] private Province m_ProvincePrehab;   //Provinceのプレハブ
    [SerializeField] private Province m_BorderPrehab;     //Borderのプレハブ
    [SerializeField] private int m_Resource;    //このステートの資源
    [SerializeField] private int m_Income;      //このステートの収入
    [SerializeField] private int m_Population;  //このステートの人口

    [SerializeField] private GameObject m_StateIcon;            //アイコン
    [SerializeField] private SpriteRenderer m_IconBackGround;   //アイコン背景

    public CountryData OwnedCountry { get; private set; }

    public string StateName { get; private set; }
    public string StateNameJP { get; private set; }
    public string StateCulture { get; private set; }
    public string StateCultureJP { get; private set; }
    public StateType StateType { get; private set; }
    public string StateTypeJP { get; private set; }
    public Sprite CultureMark { get; private set; }
    public Sprite TerrainUI { get; private set; }
    public GameObject GuarrisonModel { get; private set; }
    
    public Dictionary<State, List<Province>> BorderProvDict { get; private set; } = new Dictionary<State, List<Province>>();
    public Dictionary<CountryData, List<Force>> GuarrisonDict {  get; private set; } = new Dictionary<CountryData, List<Force>>();

    /// <summary>
    /// このステートに存在する建物リスト
    /// </summary>
    public List<BuildingType> BuildingList { get; private set; } = new List<BuildingType>()
    {
        BuildingType.None, 
        BuildingType.None, 
        BuildingType.None
    };

    public int OccupiedValue { get; private set; }
    public int MaxOccupiedValue { get; private set; }
    public Vector3 MidModelPos { get; private set; }
    public bool IsIconAnimation { get; private set; }
    public CountryData OccupiedCountry { get; private set; }
    public AboveModelState ModelState { get; private set; }

    public int StateID => m_StateID;
    public int Resource => m_Resource;
    public int Income => m_Income;
    public int Population => m_Population;
    public List<Province> ProvinceList => m_ProvinceList;

    private Direction m_EditState = Direction.North; //Provinceを配置する際の方向を管理する変数

    public async Task Init()
    {
        m_StateID = FieldManager.Instance.GetStateList().Count;   //ステートIDを決定

        m_Resource = Random.Range(0, 300) * 10;     //資源を0～3000に
        m_Income = Random.Range(0, 300) * 10;       //収入を0～3000に
        m_Population = Random.Range(10000, 100000);   //人口を10000～100000に

        //===========ステートの種類を設定===========
        StateType = SetStateType();

        for (int i = 1; i <= 5; i++) //真ん中のProvinceを中心にして、外側に向かって順番に配置する
        {
            int provinceCount = (i * 2 - 1) + (i * 2 - 2) * 2 + (i * 2 - 3);
            int currentPositionX = 0;
            int currentPositionY = 0;
            Province province;

            if (i == 1)
            {
                provinceCount = 1; //中央だけ1に固定
            }
            else
            {
                currentPositionX = 1 - i;   //左上からスタートするため、X座標を調整
                currentPositionY = i - 1;   //左上からスタートするため、Y座標を調整
            }

            if (i < 5)
            {
                province = m_ProvincePrehab;
            }
            else
            {
                province = m_BorderPrehab; //外側のProvinceはBorderのプレハブを使用する
            }

            for (int j = 0; j < provinceCount; j++)
            {
                Vector3 provincePosition = new Vector3(currentPositionX, 0, currentPositionY) + this.transform.position;
                if (FieldManager.Instance != null)
                {
                    if (FieldManager.Instance.GetProvince((int)provincePosition.x, (int)provincePosition.z) == null)            //初めて生成されたプロヴィンスだったら
                    {
                        Province currentProvince = Instantiate(province, provincePosition, Quaternion.identity, this.transform);  //新しいプロヴィンス生成
                        m_ProvinceList.Add(currentProvince);                                       //このステートのプロヴィンスリストに追加
                        currentProvince.SetProvincePosition((int)provincePosition.x, (int)provincePosition.z);//座標登録 
                        FieldManager.Instance.SetProvince(currentProvince.ProvincePosition, currentProvince);   //マネージャ側で登録

                        if (currentProvince.IsBorderProvince)   //始めて生成された境界プロヴィンス
                        {
                            FieldManager.Instance.SetBorderProvince(currentProvince.ProvincePosition, currentProvince);
                            if (!currentProvince.CheckNeighborAllDirection()) //かつ、最も外側ならば外縁プロヴィンスになる
                            {
                                currentProvince.SetOutSideProvince(true);
                                FieldManager.Instance.SetOutSideProvince(currentProvince.ProvincePosition, currentProvince);    //マネージャに外縁プロヴィンスを登録
                            }
                        }
                    }
                    else
                    {
                        //Debug.Log("スルーしました");
                    }
                }

                switch (m_EditState)
                {
                    case Direction.North:
                        currentPositionX++;
                        if (currentPositionX == i - 1)
                        {
                            m_EditState = Direction.East;   //北に配置し終わったら東に向きを変える
                        }
                        break;
                    case Direction.East:
                        currentPositionY--;
                        if (currentPositionY == 1 - i)
                        {
                            m_EditState = Direction.South;  //東に配置し終わったら南に向きを変える
                        }
                        break;
                    case Direction.South:
                        currentPositionX--;
                        if (currentPositionX == 1 - i)
                        {
                            m_EditState = Direction.West;   //南に配置し終わったら西に向きを変える
                        }
                        break;
                    case Direction.West:
                        currentPositionY++;
                        if (currentPositionY == i - 1)
                        {
                            m_EditState = Direction.North;  //西に配置し終わったら北に向きを変える
                        }
                        break;
                }
            }
        }

        Vector3 provPos = new Vector3();

        foreach (var prov in m_ProvinceList)
        {
            provPos += prov.gameObject.transform.position;    //このステートを構成する全部のプロヴィンスの座標を足す
        }

        MidModelPos = provPos / m_ProvinceList.Count;   //プロヴィンス数で割ると中央値が出る(はず)

        m_StateIcon.transform.position = new Vector3(MidModelPos.x, m_StateIcon.transform.position.y, MidModelPos.z);   //アイコンの座標を調整

        //============最大占領ポイントを設定===========

        switch (StateType)
        {
            default:
                break;
            case StateType.Plane:
                MaxOccupiedValue = 1000;
                break;
            case StateType.Mountain:
                MaxOccupiedValue = 2000;
                break;
            case StateType.Desert:
                MaxOccupiedValue = 1500;
                break;
            case StateType.River:
                MaxOccupiedValue = 800;
                break;
        }

        MaxOccupiedValue = (int)(MaxOccupiedValue * Random.Range(0.8f, 1.2f));

        //============最大占領ポイントを設定End===========
    }

    /*bool SetIsBorder(int index)
    {
        bool isBorder = false;

        int slot = Random.Range(0, 4);

        if (index >= slot)
        {
            isBorder = true;
        }

        return isBorder;
    }*/

    private void Update()
    {
        if (IsIconAnimation)
        {

        }
    }

    public void SetStateColor(Vector3 color)
    {
        foreach (var prov in m_ProvinceList)
        {
            prov.SetProvinceColor(color);   //通常プロヴィンスを指定カラーに
        }
    }

    /// <summary>
    /// ステート内の国境プロヴィンスを探し出し、プレイヤーの場合は色の変更も行う
    /// </summary>
    /// <returns></returns>
    public List<Province> GetBorderProvinceList()
    {
        List<Province> borderProvinceList = new List<Province>();   //マークした国境プロヴィンスのリストを作成

        foreach (var prov in m_ProvinceList)    //このステートのプロヴィンスの中で
        {
            if (FieldManager.Instance.BorderProvinceDict.ContainsKey(prov.ProvincePosition))  //もし国境プロヴィンスだったら(この時点で正しい情報に書き換え済み？)
            {
                if (OwnedCountry == BattleSystemManager.Instance.PlayerCountry)
                {
                    prov.SetProvinceColor(FieldManager.Instance.StateColorDict[StateColorType.Border]);   //国境プロヴィンスを国境カラーに
                }
                borderProvinceList.Add(prov);
            }
            else
            {
                if (OwnedCountry == BattleSystemManager.Instance.PlayerCountry)
                {
                    prov.SetProvinceColor(FieldManager.Instance.StateColorDict[StateColorType.Garrison]);   //駐屯プロヴィンスを駐屯カラーに
                }
            }
        }

        return borderProvinceList;  //マークしたプロヴィンスを返す
    }

    public void ResetStateColor()
    {
        Vector3 color = Vector3.one;

        if (OwnedCountry != null)
        {
            color = OwnedCountry.CountryColor;
        }

        foreach (var prov in m_ProvinceList)
        {
            prov.SetProvinceColor(color);   //通常プロヴィンスを白に
        }
    }

    public void SetOwner(CountryData country)
    {
        OwnedCountry = country;
        SetStateColor(OwnedCountry.CountryColor);
    }

    public async Task SetStateData(StateData data)
    {
        StateName = data.StateName;
        StateNameJP = data.StateNameJP;
        StateCulture = data.StateCulture;
        StateCultureJP = data.StateCultureJP;

        CultureMark = await AddressableManager.Instance.LoadSpriteAsync(StateCulture.ToString());
        TerrainUI = await AddressableManager.Instance.LoadSpriteAsync(StateType.ToString() + "UI");
    }

    public StateType SetStateType()
    {
        if (m_Population < 20000)    //人口20000以下
        {
            if (m_Resource > 1500)  //資源が1500以上
            {
                StateTypeJP = "山岳";
                return StateType.Mountain;   //山岳地帯
            }
            else
            {
                StateTypeJP = "砂漠";
                return StateType.Desert;     //砂漠地帯
            }
        }
        else
        {
            if (m_Income > 2000)  //収入が2000以上
            {
                StateTypeJP = "河川";
                return StateType.River;   //河川地帯
            }
        }

        StateTypeJP = "平地";
        return StateType.Plane;  //平原地帯
    }

    /// <summary>
    /// このステートにとってどのプロヴィンスがどのステートに隣接しているかを登録
    /// </summary>
    public void AddBorderProv(Province prov)
    {
        List<NeighberProvince> neighberList = prov.NeighberStateList;

        foreach (var neighber in neighberList)
        {
            if (!BorderProvDict.ContainsKey(neighber.state))    //もし隣接ステートがこのステートの隣接ステートリストに登録されていなかったら
            {
                BorderProvDict[neighber.state] = new List<Province>();   //隣接ステートを登録
            }
            
            for (int i = 0; i < neighberList.Count; i++)
            {
                if (neighberList[i].direction == (Direction)i)  //方角ごとに判定
                {
                    BorderProvDict[neighber.state].Add(prov);   //隣接プロヴィンスを登録
                }
            }
        }
    }

    /// <summary>
    /// ステートの駐屯軍を配置
    /// </summary>
    public void AddGuarrisonForce(Force force)
    {
        CountryData country = force.OwnerCountry; //どの国所属か

        if (GuarrisonDict.ContainsKey(country))
        {
            GuarrisonDict[country].Add(force);
        }
        else
        {
            GuarrisonDict.Add(country, new List<Force>());
            GuarrisonDict[country].Add(force);
        }
    }

    /// <summary>
    /// 占領中
    /// </summary>
    /// <param name="value"></param>
    /// <param name="occupyingCountry"></param>
    public void Occupied(int value, CountryData occupyingCountry)
    {
        if (!m_StateIcon.activeSelf)
        {
            m_StateIcon.SetActive(true);    //占領されているアイコンをオン
            m_IconBackGround.color = new Color(occupyingCountry.CountryColor.x, occupyingCountry.CountryColor.y, occupyingCountry.CountryColor.z);  //アイコン背景を占領を行っている国家カラーに
        }

        if (OccupiedCountry != occupyingCountry) //占領中の国家と占領を行った国家が違う、もしくは占領中の国家がいなかった場合
        {
            OccupiedCountry = occupyingCountry;  //占領中の国家を変更
            OccupiedValue = value;              //占領ゲージを初期化＆一段階占領
        }
        else
        {
            OccupiedValue += value;
        }

        if (OccupiedValue > MaxOccupiedValue)   //最大占領よりも占領が大きい場合、ステートの支配権が移る
        {
            OccupiedCountry = null;
            OccupiedValue = 0;
            SetOwner(occupyingCountry);          //支配権を移行
        }
    }

    /// <summary>
    /// 駐屯モデルのポインタをセット
    /// </summary>
    /// <param name="obj"></param>
    public void SetGuarrisonModel(GameObject obj)
    {
        GuarrisonModel = obj;
    }

    /// <summary>
    /// 建物タイプに応じた建築を行う
    /// </summary>
    public void ArchitectBuilding(BuildingType type)
    {
        int buildingCount = 0;

        foreach (var build in BuildingList)
        {
            if (build != BuildingType.None) //何か建物がある場合
            {
                buildingCount++;
            }
        }

        if (buildingCount >= 3)
        {
            return;
        }

        BuildingList.Add(type);             //建物を追加
    }
}
