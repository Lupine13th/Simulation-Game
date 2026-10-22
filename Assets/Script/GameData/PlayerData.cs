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
    /// AI勢力の思考を更新
    /// </summary>
    public void UpdateAIThinking()
    {
        ClearActionValueDict();     //行動を決める辞書の中身をクリアする

        //---------行動評価フェーズ----------

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

        //---------行動評価フェーズEnd----------



        //---------行動選択フェーズ----------

        if (ThisActionIsMostValuable(AIActionType.Enlist))  //最も有効な行動が「徴兵」ならば
        {
            if (CountryData.Fund > 2000 && CountryData.Resource > 1000)
            {
                int soldiers = 1000;

                ForceData force = BattleSystemManager.Instance.SetEnlistData(ForceType.Infantry);

                force.Soldiers = soldiers;                  //兵数セット
                force.OwnerCountryIndex = m_PlayerIndex;    //所有勢力をセット

                CountryData.Enlist(force);
            }
        }

        //---------行動選択フェーズEnd----------
    }

    public void SetCountryData(CountryData data)
    {
        CountryData = data;
        m_PlayerIndex = data.CountryID;
    }

    public CountryData GetCountryData()
    {
        return FieldManager.Instance.GetCountryFromPlayer(this);
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
    public bool ThisActionIsMostValuable(AIActionType type)
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

        if (bestAction == type) //もし指定したタイプと同じならば正を返す
        {
            return true;
        }

        return false;           //そうじゃなければ負を返す
    }
}
