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
    Guarrison,
    AttackDisable,
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

   

    [SerializeField] private GameObject m_StateIcon;            //アイコン
    [SerializeField] private SpriteRenderer m_IconBackGround;   //アイコン背景

    public CountryData OwnedCountry { get; private set; }

    public int Yield { get; private set; }    //このステートの資源
    public int Income { get; private set; }    //このステートの収入
    public int Population { get; private set; } //このステートの人口
    public int Ration { get; private set; }     //このステートの糧食

    public float Stability { get; private set; } = 100.0f;  //このステートの安定度

    public const float kMaxStability = 100.0f;  //このステートの安定度



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
    public List<Province> ProvinceList => m_ProvinceList;

    private Direction m_EditState = Direction.North; //Provinceを配置する際の方向を管理する変数

    public async Task Init()
    {
        m_StateID = FieldManager.Instance.GetStateList().Count;   //ステートIDを決定

        Yield = Random.Range(50, 300) * 10;         //資源を0～3000に
        Income = Random.Range(50, 300) * 10;        //収入を0～3000に
        Population = Random.Range(10000, 100000);   //人口を10000～100000に
        Ration = Random.Range(1000, 10000);         //糧食を1000～10000に

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

    /// <summary>
    /// 1日ごとのステート内処理
    /// </summary>
    public void UpdateStateOnDate()
    {
        ChangeStateStability(1.0f);
    }

    /// <summary>
    /// 所有する全プロヴィンスの色を変える
    /// </summary>
    /// <param name="color"></param>
    public void SetStateColor(Vector3 color)
    {
        foreach (var prov in m_ProvinceList)
        {
            prov.SetProvinceColor(color);   //通常プロヴィンスを指定カラーに
        }
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

    /// <summary>
    /// 所有勢力を変更
    /// </summary>
    /// <param name="country"></param>
    public void SetOwner(CountryData country)
    {
        var lateCountry = OwnedCountry; //過去に所有されていた勢力

        OwnedCountry = country;         //所有勢力を切り替え

        country.AddStateGetCount();     //最大ステート領有数を更新

        SetStateColor(OwnedCountry.CountryColor);

        if (lateCountry != null)        //空ステートを占領した時に不具合が出ないように
        {
            var states = FieldManager.Instance.GetOwnedStateList(lateCountry);

            if (states == null)                                 //占領されていた勢力が全ステートを失えば勢力ごと削除
            {
                country.AddCountryKillCount();                  //崩壊させた勢力数を追加
                FieldManager.Instance.DeleteCountry(country);   //国家を削除
            }
            else
            {
                if (lateCountry.GuardingForceGroupDict.ContainsKey(this))   //占領が完了したステートに敵の駐屯軍がいれば
                {
                    foreach (var force in lateCountry.GuardingForceGroupDict[this].ThisPlaceForceList)
                    {
                        FieldManager.Instance.SetForceOnState(FieldManager.Instance.GetOwnedStateList(lateCountry)[0], force);  //その勢力の所有するステートのインデックス0(多分首都？)に移動させる
                    }
                }
            }
        }
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
        if (Population < 20000)    //人口20000以下
        {
            if (Yield > 1500)  //資源が1500以上
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
            if (Income > 2000)  //収入が2000以上
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
    /// 占領中
    /// </summary>
    /// <param name="value"></param>
    /// <param name="occupyingCountry"></param>
    public void Occupied(int value, CountryData occupyingCountry, ForceGroup forceGroup)
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
            SetOwner(occupyingCountry);     //支配権を移行
            Stability = 0.0f;
            m_StateIcon.SetActive(false);   //占領されているアイコンをオフ
            forceGroup.EndOccupy(this);     //占領を終了する
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

    public void ChangeStateStability(float percent)
    {
        Stability += percent;

        if (Stability >= kMaxStability)
        {
            Stability = kMaxStability;
        }
    }
}
