using System.Collections.Generic;
using UnityEngine;

public enum BattleResult
{
    None,
    Win,
    Lose,
    Draw
}

public class ForceGroup
{
    /// <summary>
    /// ‚±‚ÌƒXƒe[ƒg‹y‚Ñ‘‹«‚É‚¢‚éŒR’c
    /// </summary>
    public List<Force> ThisPlaceForceList { get; private set; } = new List<Force>();

    /// <summary>
    /// ‚±‚ÌƒXƒe[ƒg‹y‚Ñ‘‹«‚É‚¢‚éŒR’c
    /// </summary>
    public List<BattleResult> BattleResultList { get; private set; } = new List<BattleResult>();

    /// <summary>
    /// UŒ‚’†‚ÌƒXƒe[ƒg(UŒ‚‚µ‚Ä‚¢‚È‚¢‚Ínull)
    /// </summary>
    public State AttackingState { get; private set; }

    /// <summary>
    /// ’““Ô’†‚ÌƒXƒe[ƒg(UŒ‚‚µ‚Ä‚¢‚é‚Ínull)
    /// </summary>
    public State GuardingState { get; private set; }

    /// <summary>
    /// ‚±‚ÌŒR’c‚Ì‘•º—Í
    /// </summary>
    public int AllSoldier { get; private set; }

    public bool BattleCompleted { get; private set; } = false;

    public void UpdateDatas()
    {
        int allSoldier = 0;

        foreach (var force in ThisPlaceForceList)   //ŒR’c‚Ì‘•º—Í‚ğ‘«‚µZ
        {
            allSoldier += force.Soldiers;
        }

        AllSoldier = allSoldier;
    }

    /// <summary>
    /// UŒ‚‚·‚éƒXƒe[ƒg‚ğİ’è
    /// </summary>
    /// <param name="state"></param>
    public void SetAttackState(State state)
    {
        AttackingState = state;
    }

    /// <summary>
    /// ’““Ô‚·‚éƒXƒe[ƒg‚ğİ’è
    /// </summary>
    /// <param name="state"></param>
    public void SetGuarrisonState(State state)
    {
        GuardingState = state;
    }

    /// <summary>
    /// ‘SŒR’c‚ğ‰ñ•œ‚³‚¹‚é(m‹C‚à‰ñ•œ)
    /// </summary>
    public void HealAllForces()
    {
        foreach (var force in ThisPlaceForceList)
        {
            force.HealSoldier1Per();
            force.HealMorale(1);
        }
    }

    /// <summary>
    /// æ“ª‚ÌŒR’c
    /// </summary>
    public Force GetListHead()
    {
        return ThisPlaceForceList[0];
    }

    /// <summary>
    /// è—ÌŠ®—¹‚µ‚½ŒR’c‚ª‚±‚Ìƒƒ\ƒbƒh‚ğŒÄ‚ÑAƒ^[ƒQƒbƒg‚ÌƒXƒe[ƒg‚É‘SŒR’c‚ğ’““Ô‚³‚¹‚é
    /// </summary>
    /// <param name="target"></param>
    public void EndOccupy(State target)
    {
        for (int i = 0; i < ThisPlaceForceList.Count; i++)
        {
            FieldManager.Instance.SetForceOnState(target, ThisPlaceForceList[i]);   //ŒR’c‚ğ’€ŸˆÚ“®‚³‚¹‚é
        }
    }

    /// <summary>
    /// ‰ó–Å‚µ‚½ŒR’c‚ğíœ
    /// </summary>
    /// <param name="force"></param>
    public void RemoveForce(Force force)
    {
        var country = force.OwnerCountry;

        ThisPlaceForceList.Remove(force);   //ŒR’c‚ğíœ

        if (ThisPlaceForceList.Count == 0)  //‚à‚µŒR’c‚ª‚¢‚È‚­‚È‚ê‚Î
        {
            if (country.AttackingForceGroupDict.ContainsValue(this))    //UŒ‚’†‚Ì‘åŒR’c‚¾‚Á‚½‚ç
            {
                foreach (var pair in country.AttackingForceGroupDict)
                {
                    if (pair.Value.Equals(this))
                    {
                        ModelManager.Instance.ChangeBattleModel(false, AttackingState, country, this);  //ƒ‚ƒfƒ‹íœ
                        WorldUIManager.Instance.RemoveForceUI(this);        //UIíœ
                        country.AttackingForceGroupDict.Remove(pair.Key);   //‘åŒR’cíœ
                        break;  //ŠY“–‚·‚é‚à‚Ì‚ª•¡”‚È‚¯‚ê‚Î‚±‚±‚ÅI—¹
                    }
                }
            }
            else if (country.GuardingForceGroupDict.ContainsValue(this))//–h‰q’†‚Ì‘åŒR’c‚¾‚Á‚½‚ç
            {
                foreach (var pair in country.GuardingForceGroupDict)
                {
                    if (pair.Value.Equals(this))
                    {
                        ModelManager.Instance.ChangeBattleModel(false, GuardingState, country, this);  //ƒ‚ƒfƒ‹íœ
                        WorldUIManager.Instance.RemoveForceUI(this);        //UIíœ
                        country.GuardingForceGroupDict.Remove(pair.Key);    //‘åŒR’cíœ
                        break;  //ŠY“–‚·‚é‚à‚Ì‚ª•¡”‚È‚¯‚ê‚Î‚±‚±‚ÅI—¹
                    }
                }
            }
        }
    }

    /// <summary>
    /// í“¬‚ğs‚Á‚½ƒtƒ‰ƒO‚ğ•ÏX
    /// </summary>
    public void SetBattleComplete(bool on)
    {
        BattleCompleted = on;
    }

    /// <summary>
    /// í“¬Œ‹‰Ê‚ğ’Ç‰Á
    /// </summary>
    /// <param name="result"></param>
    public void AddBattleResult(BattleResult result)
    {
        BattleResultList.Add(result);
    }

    /// <summary>
    /// ‚»‚Ì“ú‚Ì‘‡í“¬Œ‹‰Ê‚ğo‚·
    /// </summary>
    public BattleResult CheckBattleResultOnDate()
    {
        var winCount = 0;
        var loseCount = 0;
        var drawCount = 0;
        var mostCountResult = BattleResult.None;
        var mostCount = 0;

        if (BattleResultList.Count <= 0)    //í“¬‚ğs‚Á‚Ä‚¢‚È‚¢‚È‚çNone‚ğ•Ô‚·
        {
            return BattleResult.None;
        }

        foreach (var result in BattleResultList)
        {
            switch (result)                     
            {
                case BattleResult.Win:
                    winCount++;
                    if (mostCount < winCount)
                    {
                        mostCountResult = result;
                        mostCount = winCount;
                    }
                    break;
                case BattleResult.Lose:
                    loseCount++;
                    if (mostCount < loseCount)
                    {
                        mostCountResult = result;
                        mostCount = loseCount;
                    }
                    break;
                case BattleResult.Draw:
                    drawCount++;
                    if (mostCount < drawCount)
                    {
                        mostCountResult = result;
                        mostCount = drawCount;
                    }
                    break;
            }

            
        }

        
        

        return mostCountResult;
    }

    /// <summary>
    /// í“¬Œ‹‰Ê‚ğƒNƒŠƒA
    /// </summary>
    /// <param name="result"></param>
    public void ResetBattleResult()
    {
        BattleResultList.Clear();
    }
}
