using UnityEngine;

public enum ForceType
{
    Infantry,
    Militia,
    Artillery,
    Cavalry,
    Tank
}

public struct ForceData
{
    public string ForcesName;
    public int Morale;
    public int MaxMorale;
    public int Soldiers;
    public int MaxSoldiers;
    public int Attack;
    public int Defense;
    public int Occupy;
    public int FundCost;
    public int ResourceCost;
    public ForceType ForceType;
    public int OwnerCountryIndex;
    public float MoveSpeed;
}

public enum ForceAction
{
    Idle,
    Move,
    Attack,
    Defend,
}

public class Force
{
    public string ForcesName { get; private set; }
    public int Morale { get; private set; }
    public int MaxMorale { get; private set; }
    public int Soldiers { get; private set; }
    public int MaxSoldiers { get; private set; }
    public int AttackPower { get; private set; }
    public int DefensePower { get; private set; }
    public int OccupingPower { get; private set; }
    public int FundCost { get; private set; }
    public int ResourceCost { get; private set; }
    public ForceType ForceType { get; private set; }
    public int OwnerCountryIndex { get; private set; } = -1;
    public bool IsFighting { get; private set; } = false;
    public float MoveSpeed { get; private set; } = 1;
    public ForceDataUI LinkedUI { get; private set; } = null;

    /// <summary>
    /// 現在攻撃を行っているステートのID
    /// </summary>
    public int CurrentTargetStateId { get; private set; } = -1;

    /// <summary>
    /// 現在駐屯を行っているステートのID
    /// </summary>
    public int CurrentGuarrisonStateId { get; private set; } = -1;

    public void SetForceData(ForceData data)
    {
        ForcesName = data.ForcesName;
        Morale = data.Morale;
        MaxMorale = data.MaxMorale;
        Soldiers = data.Soldiers;
        MaxSoldiers = data.MaxSoldiers;
        AttackPower = data.Attack;
        DefensePower = data.Defense;
        OccupingPower = data.Occupy;
        FundCost = data.FundCost;
        ResourceCost = data.ResourceCost;
        ForceType = data.ForceType;
        OwnerCountryIndex = data.OwnerCountryIndex;
        MoveSpeed = data.MoveSpeed;
    }

    public void SetTargetState(int i)
    {
        CurrentTargetStateId = i;
    }

    public void SetGuarrisonState(int i)
    {
        CurrentGuarrisonStateId = i;
    }

    public void SetLinkedUI(ForceDataUI ui)
    {
        LinkedUI = ui;
    }

    public void Conquest()
    {
        if (FieldManager.Instance.StateList[CurrentTargetStateId].GuarrisonDict.Count == 0)   //駐屯部隊が0部隊の場合
        {
            FieldManager.Instance.StateList[CurrentTargetStateId].Occupied(AttackPower, FieldManager.Instance.CountryList[OwnerCountryIndex]);  //侵攻を行う
        }
    }
}
