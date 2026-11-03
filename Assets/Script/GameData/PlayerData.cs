using System.Collections.Generic;
using UnityEngine;

public enum AITendency
{
    Default,
    Aggressive,
    Defencive,
    Expansion
}

public enum AIActionType
{
    None,
    Build,
    Attack,
    Guarrison,
    DeclareWar,
    FormAlliance,
    Enlist
}

/// <summary>
/// AIの行動データ
/// </summary>
public class AIActionData
{

}

public class PlayerData : MonoBehaviour
{
    [SerializeField] private int m_PlayerIndex = -1;
    [SerializeField] private bool m_IsPlayer = false;

    public int PlayerIndex => m_PlayerIndex;
    public bool IsPlayer => m_IsPlayer;

    /// <summary>
    /// AIの行動を決める値
    /// </summary>
    public Dictionary<AIActionType, float> ActionValueDict { get; private set; } = new Dictionary<AIActionType, float>()
    {
        { AIActionType.Build, 0f },
        { AIActionType.Attack, 0f },
        { AIActionType.Guarrison, 0f },
        { AIActionType.DeclareWar, 0f },
        { AIActionType.FormAlliance, 0f },
        { AIActionType.Enlist, 0f }
    };

    public CountryData CountryData { get; private set; }

    public AITendency Tendency { get; private set; }

    /// <summary>
    /// 防衛的なAIの思考を行う
    /// </summary>
    public void UpdateAIThinkingOnDefencive()
    {
        ClearActionValueDict();     //行動を決める辞書の中身をクリアする

        //---------行動評価フェーズ----------

        var stateDict = CountryData.NeighborStateDict;  //隣接ステートの辞書型

        FieldManager.Instance.GetBorderAndStateData(CountryData);

        FieldManager.Instance.SearchNeighborStateForDict(stateDict, CountryData);   //隣接ステートの情報を得る

        if (CountryData.EnemyCountryList.Count > 0) //どこかの勢力と戦争中である
        {
            ActionValueDict[AIActionType.Enlist] += 100;    //100点追加

            if (Tendency == AITendency.Aggressive)              //攻撃的なAIの場合
            {
                ActionValueDict[AIActionType.Enlist] += 50; //50点追加
            }
            else if (Tendency == AITendency.Expansion)          //拡張的なAIの場合
            {
                ActionValueDict[AIActionType.Enlist] += 100;//100点追加
            }
        }
        else    //平時である
        {
            ActionValueDict[AIActionType.Enlist] -= 100;
        }

        if (stateDict.Count >= CountryData.ForceList.Count) //国境の数よりも軍団が少ない場合
        {
            ActionValueDict[AIActionType.Enlist] += 200;    //200点追加
        }
        else                                                //軍団数が足りている場合
        {
            ActionValueDict[AIActionType.Attack] += 100;    //100点追加
        }


        //---------行動評価フェーズEnd----------



        //---------行動選択フェーズ----------

        switch (MostValuableAction())
        {
            case AIActionType.Enlist:   //最も有効な行動が「徴兵」ならば
                if (CountryData.Fund > 2000 && CountryData.Resource > 1000)
                {
                    int soldiers = 1000;

                    ForceData force = BattleSystemManager.Instance.SetEnlistData(ForceType.Infantry);

                    force.Soldiers = soldiers;                  //兵数セット
                    force.OwnerCountry = CountryData;    //所有勢力をセット

                    CountryData.Enlist(force);
                }
                break;
            case AIActionType.Attack:   //最も有効な行動が「攻撃」ならば

                State bestState = null;        //最も攻撃が有効なステート
                int bestValue = 0;
                
                foreach (var state in stateDict.Keys)
                {
                    int value = 0;

                    if (state.OwnedCountry == null)     //まず、空ステートならば
                    {
                        value = 500;   //200点からスタート
                    }

                    int stateValue = (state.Income + state.Resource) / 10;  //ステート自身の評価

                    value += stateValue;   //加点

                    if (bestValue < value)
                    {
                        bestState = state;  //有効なステートを更新
                        bestValue = value;
                    }
                }

                var randForce = CountryData.GetRandomFreeForce();   //ランダムな軍団を選択

                if (randForce != null)
                {
                    FieldManager.Instance.SetForceOnBorder(bestState, randForce);   //選択したステートに軍団を割り当て
                }

                break;
        }

        //---------行動選択フェーズEnd----------
    }

    /// <summary>
    /// このインスタンスに情報を入力
    /// </summary>
    /// <param name="data"></param>
    public void SetCountryData(CountryData data)
    {
        CountryData = data;
        m_PlayerIndex = data.CountryID;
    }

    public void SetPlayer(bool isPlayer, int index)
    {
        m_IsPlayer = isPlayer;
        m_PlayerIndex = index;
    }

    /// <summary>
    /// 辞書型の中をクリア
    /// </summary>
    public void ClearActionValueDict()
    {
        foreach (var key in new List<AIActionType>(ActionValueDict.Keys))
        {
            ActionValueDict[key] = default;
        }
    }

    /// <summary>
    /// 辞書型の中で指定したキーが一番大きい値かどうかを判定する
    /// </summary>
    public AIActionType MostValuableAction()
    {
        AIActionType bestAction = AIActionType.None;
        float bestValue = 0f;

        foreach (var action in ActionValueDict)
        {
            if (bestValue < action.Value)   //一番大きな値を持つアクションの種類を割り出す
            {
                bestAction = action.Key;    //一番大きな値は何だったのか
                bestValue = action.Value;   //一番大きな値を更新
            }
        }

        return bestAction;  //一番評価の高い行動を返す
    }
}
