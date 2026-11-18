using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public enum ForceType
{
    Infantry,
    Militia,
    Artillery,
    Cavalry,
    Tank
}

public enum BattleType
{
    None,
    Offensive,  //攻略戦
    Encounter   //遭遇戦
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
    public CountryData OwnerCountry;
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
    public CountryData OwnerCountry { get; private set; }
    public float MoveSpeed { get; private set; } = 1;
    public ForceDataUI LinkedUI { get; private set; } = null;
    public CommanderData Commander { get; private set; } = null;

    /// <summary>
    /// 所属している軍団
    /// </summary>
    public ForceGroup BelongGroup { get; private set; }

    /// <summary>
    /// 現在攻撃を行っているステートのID
    /// </summary>
    public State CurrentTargetState { get; private set; }

    /// <summary>
    /// 現在駐屯を行っているステートのID
    /// </summary>
    public State CurrentGuarrisonState { get; private set; }

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
        OwnerCountry = data.OwnerCountry;
        MoveSpeed = data.MoveSpeed;
    }

    public void SetTargetState(State state)
    {
        CurrentTargetState = state;
    }

    public void SetGuarrisonState(State state)
    {
        CurrentGuarrisonState = state;
    }

    public void SetLinkedUI(ForceDataUI ui)
    {
        LinkedUI = ui;
    }

    /// <summary>
    /// 所属している大軍団をセット
    /// </summary>
    /// <param name="force"></param>
    public void SetBelongGroup(ForceGroup force)
    {
        BelongGroup = force;
    }

    /// <summary>
    /// 侵攻を行う際に呼ばれるメソッド
    /// </summary>
    public void Conquest()
    {
        var targetCountry = CurrentTargetState.OwnedCountry;

        DamagedMorale(1);

        if (targetCountry != null)    //ターゲットのステートがどこかの勢力に所有されていれば
        {
            var frontEnemy = IsEnemyAttackingThisForcePlace(targetCountry);     //目の前にいる敵

            if (frontEnemy != null)                                             //目の前に敵がいる(この軍団がいるステートを敵が攻撃中)
            {
                CalculateBattle(BattleType.Encounter, frontEnemy);              //遭遇戦によるダメージ処理
                return;
            }

            if (CurrentTargetState == null)                                     //なぜかこのタイミングでCurrentTargetStateがnullになることがあるため一旦nullチェック
            {
                return;
            }

            if (targetCountry.GuardingForceGroupDict.ContainsKey(CurrentTargetState))   //ヌルチェック
            {
                frontEnemy = targetCountry.GuardingForceGroupDict[CurrentTargetState].GetListHead();    //こちらに攻撃してきている敵が居ないなら、駐屯部隊を取る
            }

            if (frontEnemy != null)                                             //目の前に敵がいる(ターゲットのステートに部隊が駐屯中)
            {
                CalculateBattle(BattleType.Offensive, frontEnemy);              //攻略戦によるダメージ処理
                return;
            }
        }

        //==========ここ以下の処理は敵と戦争が起こらずに、占領だけが行われているときに通る=========

        if (CurrentTargetState.OccupiedCountry == OwnerCountry)         //自勢力によって占領が行われている
        {
            CurrentTargetState.Occupied(AttackPower, OwnerCountry, BelongGroup);     //ターゲットのステートで侵略されているメソッドを呼ぶ
        }
        else if (CurrentTargetState.OccupiedCountry == null)            //ステートが何処の占領も受けていない
        {
            CurrentTargetState.Occupied(AttackPower, OwnerCountry, BelongGroup);     //ターゲットのステートで侵略されているメソッドを呼ぶ
        }
    }

    /// <summary>
    /// この軍団が攻撃中のステートにいる敵軍団がこの軍団のいるステートに向かって攻撃してきているか、いるならば軍団を返す
    /// </summary>
    /// <returns></returns>
    public Force IsEnemyAttackingThisForcePlace(CountryData enemyCountry)
    {
        var thisForcePlaceStateList = OwnerCountry.GetForceGroupAttackingBase(CurrentTargetState);  //この軍団がいるステート(跨いでいる場合は複数)

        if (thisForcePlaceStateList == null)
        {
            return null;
        }

        var enemyAttackingStateList = enemyCountry.AttackingForceGroupDict;                    //敵勢力が攻撃中のステート

        if (enemyCountry == OwnerCountry)                                   //もし、味方のステートを攻撃してしまって居たら
        {
            FieldManager.Instance.SetForceOnState(CurrentTargetState, this);//そのステートに駐屯させる
            return null;
        }

        foreach (var enemyAttackingState in enemyAttackingStateList.ToArray())
        {
            if (thisForcePlaceStateList.Contains(enemyAttackingState.Key))
            {
                return enemyAttackingState.Value.GetListHead();    //もし、敵が攻撃中のステートにこの軍団がいるステートが含まれていればその大軍団の先頭を返す
            }
        }

        return null;            //敵は攻撃をしていないorこの軍団がいるステートに対して攻撃をしていない
    }

    /// <summary>
    /// ダメージ計算(戦闘の種類によってダメージ計算方法を変更)
    /// </summary>
    /// <param name="type"></param>
    public void CalculateBattle(BattleType type, Force enemyForce)
    {
        int thisForceDice = Random.Range(Commander.Attack, 10 + Commander.Attack);    //こちらの部隊用の乱数
        int thisForceDefenceDice = Random.Range(Commander.Defence, 10 + Commander.Defence);   //敵部隊用の乱数
        int enemyForceDice = Random.Range(enemyForce.Commander.Attack, 10 + enemyForce.Commander.Attack);   //敵部隊用の乱数
        int enemyForceDefenceDice = Random.Range(enemyForce.Commander.Defence, 10 + enemyForce.Commander.Defence);   //敵部隊用の乱数
        float ownDamage = 0;
        float enemyDamage = 0;

        float thisSoldiersCorrection = 1.0f + BelongGroup.AllSoldier / 500f;
        float enemySoldiersCorrection = 1.0f + enemyForce.BelongGroup.AllSoldier / 500f;

        switch (type)
        {
            case BattleType.Encounter:  //遭遇戦(お互い攻撃中)

                ownDamage += enemyForce.AttackPower * enemyForceDice - DefensePower * thisForceDefenceDice;         //この軍団がくらうダメージ
                enemyDamage += AttackPower * thisForceDice - enemyForce.DefensePower * enemyForceDefenceDice;       //敵軍団がくらうダメージ

                ownDamage *= enemySoldiersCorrection;   //軍量による補正値
                enemyDamage *= thisSoldiersCorrection;  //軍量による補正値

                ownDamage /= 5;
                enemyDamage /= 5;

                if (ownDamage < 0)
                {
                    ownDamage = 0;
                }

                if (enemyDamage < 0)
                {
                    enemyDamage = 0;
                }

                break;
            case BattleType.Offensive:  //攻略戦(この軍団が駐屯中の部隊に攻撃中)

                ownDamage += enemyForce.AttackPower * enemyForceDice - DefensePower * thisForceDefenceDice;     //この軍団がくらうダメージ
                enemyDamage += AttackPower * thisForceDice - enemyForce.DefensePower * enemyForceDefenceDice;       //敵軍団がくらうダメージ
                                                                                                                    //
                ownDamage *= enemySoldiersCorrection;   //軍量による補正値
                enemyDamage *= thisSoldiersCorrection;  //軍量による補正値

                ownDamage /= 5;
                enemyDamage /= 5;

                if (ownDamage < 0)
                {
                    ownDamage = 0;
                }

                if (enemyDamage < 0)
                {
                    enemyDamage = 0;
                }

                break;
        }

        Damaged((int)ownDamage);
        enemyForce.Damaged((int)enemyDamage);

        Debug.Log(ForcesName + "へ" + ownDamage +"ダメージ　　　　　" + enemyForce.ForcesName + "へ" + enemyDamage + "ダメージ");

        if (thisForceDice > enemyForceDice)
        {
            BelongGroup.AddBattleResult(BattleResult.Win);              //この部隊は勝利
            enemyForce.BelongGroup.AddBattleResult(BattleResult.Lose);  //相手部隊は敗北
        }
        else if (thisForceDice < enemyForceDice)
        {
            BelongGroup.AddBattleResult(BattleResult.Lose);             //この部隊は敗北
            enemyForce.BelongGroup.AddBattleResult(BattleResult.Win);   //相手部隊は勝利
        }
        else
        {
            BelongGroup.AddBattleResult(BattleResult.Draw);             //この部隊は引き分け
            enemyForce.BelongGroup.AddBattleResult(BattleResult.Draw);  //相手部隊は引き分け
        }

        BelongGroup.SetInBattle(true);
        enemyForce.BelongGroup.SetInBattle(true);
    }

    /// <summary>
    /// 軍団にダメージ(指定した値)
    /// </summary>
    /// <param name="damage"></param>
    public void Damaged(int damage)
    {
        Soldiers -= damage;

        if (Soldiers <= 0)
        {
            Debug.Log(OwnerCountry.CountryName + "の軍団が壊滅しました");

            BelongGroup.UpdateDatas();  //大軍団の情報更新

            BelongGroup.RemoveForce(this);
                
            OwnerCountry.ForceList.Remove(this);            //勢力のリストからも削除
        }
    }

    /// <summary>
    /// 軍団にダメージ(10パーセント)
    /// </summary>
    /// <param name="damage"></param>
    public void Damaged10Per()
    {
        Soldiers -= MaxSoldiers / 10;

        if (Soldiers <= 0)
        {
            Debug.Log(OwnerCountry.CountryName + "の軍団が壊滅しました");

            BelongGroup.UpdateDatas();  //大軍団の情報更新

            BelongGroup.RemoveForce(this);
                
            OwnerCountry.ForceList.Remove(this);            //勢力のリストからも削除
        }
    }

    /// <summary>
    /// 軍団を回復(1/100)
    /// </summary>
    public void HealSoldier1Per()
    {
        Soldiers += MaxSoldiers / 100;

        if (Soldiers > MaxSoldiers)
        {
            Soldiers = MaxSoldiers;
        }
    }

    /// <summary>
    /// 軍団の士気を消耗(値)
    /// </summary>
    public void DamagedMorale(int value)
    {
        Morale -= value;

        if (Morale < 0)
        {
            Morale = 0;
        }
    }

    /// <summary>
    /// 軍団の士気を回復(値)
    /// </summary>
    public void HealMorale(int value)
    {
        Morale += value;

        if (Morale > MaxMorale)
        {
            Morale = MaxMorale;
        }
    }

    /// <summary>
    /// ランダムな指揮官をセット
    /// </summary>
    public async Task SetRandomCommander()
    {
        Commander = new CommanderData();                                //指揮官データ作成
        Commander.Init();                                               //ステータス決定
        Commander.SetPortrait(OwnerCountry.GetRandomCommander());       //ランダムなポートレートを割り当て
    }
}
