using System.Collections.Generic;
using UnityEngine;

public class ForceGroup
{
    /// <summary>
    /// このステート及び国境にいる軍団
    /// </summary>
    public List<Force> ThisPlaceForceList { get; private set; } = new List<Force>();

    /// <summary>
    /// 攻撃中のステート(攻撃していない時はnull)
    /// </summary>
    public State AttackingState { get; private set; }

    /// <summary>
    /// 駐屯中のステート(攻撃している時はnull)
    /// </summary>
    public State GuardingState { get; private set; }

    /// <summary>
    /// この軍団の総兵力
    /// </summary>
    public int AllSoldier { get; private set; }

    public int UpdateDatas()
    {
        int allSoldier = 0;

        foreach (var force in ThisPlaceForceList)
        {
            allSoldier += force.Soldiers;
        }

        AllSoldier = allSoldier;

        return AllSoldier;
    }

    /// <summary>
    /// 攻撃するステートを設定
    /// </summary>
    /// <param name="state"></param>
    public void SetAttackState(State state)
    {
        AttackingState = state;
    }

    /// <summary>
    /// 駐屯するステートを設定
    /// </summary>
    /// <param name="state"></param>
    public void SetGuarrisonState(State state)
    {
        GuardingState = state;
    }

    /// <summary>
    /// 先頭の軍団
    /// </summary>
    public Force GetListHead()
    {
        return ThisPlaceForceList[0];
    }
}
